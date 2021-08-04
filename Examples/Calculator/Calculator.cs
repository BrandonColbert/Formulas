using Formulas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class Calculator {
	private static readonly Regex solveRegex = new Regex(string.Join("", new[]{
		"^", //Line start
		@"[a-zA-Z0-9_]+", //Formula name
		@"\(", //Input start
		@"((-?[a-zA-Z0-9_\.]+,)*(-?[a-zA-Z0-9_\.]+))?", //Input
		@"\)", //Input end
		"$" //Line end
	}));

	private Dictionary<string, Formula> formulas = new Dictionary<string, Formula>();
	private List<string> lines = new List<string>();

	public Calculator() {
		Prompt();
		while(Parse(Console.ReadLine()));
	}

	public bool Parse(string line) {
		switch(line) {
			case "":
				break;
			case "q":
			case "quit":
			case "exit":
				return false;
			case "?":
			case "help":
				Console.WriteLine(
@"	list: Displays a list of all declared functions.
	q | quit | exit: Exit the program.
	? | help: Describe available commands."
				);
				break;
			case "list":
				if(formulas.Count > 0)
					Console.WriteLine(string.Join("\n", formulas.Values.OrderBy(v => v.ToString())));

				Console.WriteLine();
				break;
			default:
				if(line.Contains('=')) {
					var incomplete = line.EndsWith(";");

					lines.AddRange(line.Split(';').Where(s => !string.IsNullOrWhiteSpace(s)));

					if(incomplete)
						break;
				}

				if(lines.Count > 0) {
					try {
						Formula f = lines.ToArray();
						formulas[f.description.name] = f;
						Console.WriteLine(f);
						Console.WriteLine();
					} catch(FormulaException e) {
						if(e.InnerException != null)
							Console.WriteLine(e.InnerException.Message);
						else
							Console.WriteLine(e.Message);

						Console.WriteLine();
					}

					lines.Clear();
					break;
				}

				Formula formula = null;
				var input = new object[0];

				line = line.Replace(" ", "");

				if(solveRegex.IsMatch(line)) {
					var parts = line.Split('(');
					var name = parts.First();
					var pars = parts.Last().Substring(0, parts.Last().Length - 1).Split(',');

					if(formulas.TryGetValue(parts.First(), out formula)) {
						input = pars.Select<string, object>(v => {
							if(double.TryParse(v, out var n))
								return n;

							return v;
						}).ToArray();
					}
				}

				if(formula == null) {
					try {
						formula = new Formula(line);
						input = new object[0];
					} catch(FormulaException e) {
						if(e.InnerException != null)
							Console.WriteLine(e.InnerException.Message);
						else
							Console.WriteLine(e.Message);

						Console.WriteLine();
						return true;
					}
				}

				try {
					Console.WriteLine(formula.Solve(input));
				} catch(FormulaException e) {
					if(e.InnerException != null)
						Console.WriteLine(e.InnerException.Message);
					else
						Console.WriteLine(e.Message);
				}

				Console.WriteLine();
				break;
		}

		return true;
	}

	static void Prompt() {
		Console.WriteLine(
@"Type an <expression> to solve.

To create a reusable function, try: <name>(<...variables>) = <expression>.
Semicolon-separated placeholder variables may be defined on lines preceding a function.

To solve the function, use: <name>(<...input>).

For commands, type '?'.
"
		);
	}
}