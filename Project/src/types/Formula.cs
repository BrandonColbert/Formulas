using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Formulas {
	/// <summary>Standard implementation for creating a formula</summary>
	public sealed class Formula : IFormula {
		/// <summary>Specifies the variables and their types</summary>
		public readonly Description description;

		/// <summary>Defines the steps to achieve a solution</summary>
		public readonly Node definition;

		/// <param name="source">Definition and optional description of the formula</param>
		public Formula(string source) {
			try {
				var parser = new Parser(source);
				definition = parser.CreateDefinition();
				description = parser.HasDescription ? parser.CreateDescription() : Parser.CreateDescription(definition);
			} catch(Exception e) {
				throw new ParseException($"Unable to parse '{source}'", e);
			}
		}

		/// <param name="source">Definition and optional description of the formula</param>
		/// <param name="rest">Definition and optional description of the formula</param>
		public Formula(string source, params string[] rest) : this(rest.Length == 0 ? source : rest[rest.Length - 1]) {
			if(rest.Length == 0)
				return;

			Array.Copy(rest, 0, rest, 1, rest.Length - 1);
			rest[0] = source;

			for(var i = rest.Length - 1; i >= 0; i--) {
				var formula = new Formula(rest[i]);

				if(formula.description.name.Length == 0)
					throw new ParseException($"Declaration '{formula}' must have a name to be used in '{this}'");

				if(formula.description.variables.Count > 0)
					throw new ParseException($"Declaration '{formula}' must not include paramters to be used in '{this}'");

				foreach(var node in definition) {
					if(!(node is VariableNode variable))
						continue;

					if(variable.value == formula.description.name) {
						formula.definition.Replace(variable);
						definition = formula.definition.Top;
					}
				}
			}
		}

		/// <param name="input">Inputs for the function</param>
		/// <returns>Solution to the function based on given inputs</returns>
		/// <remarks>Formulas accessing a subclass member of an indexed value will not compile</remarks>
		public object Solve(params object[] input) {
			var map = new Dictionary<string, object>();

			if(input.Length < description.variables.Count)
				throw new SolveException($"{input.Length}/{description.variables.Count} inputs supplied for '{this}'");

			for(var i = description.variables.Count - 1; i >= 0; i--)
				map.Add(description.variables[i], input[i]);

			try {
				if(definition.Calculate(description, map, out var result))
					return result;
			} catch(Exception e) {
				throw new SolveException($"Unable to solve '{this}'\n{definition.ToDisplayString()}", e);
			}

			throw new SolveException($"Unable to solve '{this}'\n{definition.ToDisplayString()}");
		}

		/// <summary>Compiles this formula to improve solve performance. Should be used when formula is accessed frequently</summary>
		/// <returns>A new formula with the solver compiled from this formula's information</returns>
		public IFormula Compile() {
			try {
				var parameters = Expression.Parameter(typeof(object[]), "args");
				var body = definition.Compile(description, parameters);
				body = Expression.Convert(body, typeof(object));

				var function = Expression.Lambda<Func<object[], object>>(body, parameters);
				var result = function.Compile();

				return new Procedure(result);
			} catch(Exception e) {
				throw new CompileException($"Unable to compile '{this}'\n{definition.ToDisplayString()}", e);
			}
		}

		/// <returns>Interpretation of original formula string based on definition tree</returns>
		public override string ToString() => $"{description} = {definition}";

		/// <summary>Converts a definition into a formula</summary>
		/// <param name="source">The definition to create a formula with</param>
		public static implicit operator Formula(string source) => new Formula(source);

		/// <summary>Converts a definition into a formula</summary>
		/// <param name="sources">The definition to create a formula with</param>
		public static implicit operator Formula(string[] sources) {
			switch(sources.Length) {
				case 0:
					return new Formula("");
				case 1:
					return new Formula(sources.First());
				default:
					return new Formula(sources.First(), sources.Skip(1).ToArray());
			}
		}
	}
}