using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Formulas {
	/// <summary>Helper class for formulas</summary>
	public static class Formulizer {
		/// <summary>The current provider that formulas will use to operate with</summary>
		public static IFormulaProvider Provider { get; set; } = new DefaultFormulaProvider();

		/// <summary>Checks whether a formula is valid for interpretation</summary>
		/// <returns>Valdity of the formula and potential error message</returns>
		public static (bool, string) Validate(string formula) => throw new NotImplementedException();

		//Cache of all the functions methods
		static HashSet<string> functions { get; } = new HashSet<string>() {
			"sin", "asin", "cos", "acos", "tan", "atan",
			"sqrt", "ln", "log", "sgn", "rnd",
			"abs", "nml",
			"rvs", "lvs", "uvs", "dvs", "fvs", "bvs",
			"qtn", "vec"
		};

		/// <summary>Builds the required data for a formula using the description and initial inputs</summary>
		/// <returns>
		/// A tuple consisting of:
		/// The values (floats), variables (chars/strings), functions (strings), and operations (chars)
		/// Array of indices of the next lhs to operate on
		/// The mappings for each variable
		/// Number of initial inputs provided
		/// </returns>
		internal static (object[], int[], List<KeyValuePair<char, object>>, int) Build(string description, params object[] input) {
			//Temporary variables for interpreting the formula
			var symbols = new List<object>(); //Symbols in the formula
			var groups = new List<(int, int)>(); //Each group's index and range
			var groupIndices = new Stack<(int, bool)>(); //Each group's open index and whether it was'|' true or '(' false
			var value = new Queue<char>(); //The values and variables or the function being built
			var initialInputCount = 0;

			var mapping = new List<KeyValuePair<char, object>>();

			//Check for mappings and create if possible
			var mappingProvided = description.Contains('=');

			if(mappingProvided) {
				var parts = description.Split('=');
				description = parts[1];

				//Get the variables
				var variables = parts[0].Take(parts[0].Length - 1) //Get everything within f(...)
										.Skip(2)
										.Where(c => char.IsLetter(c)) //Get letters
										.ToList();

				//Map the variables to currently available inputs
				for(var i = 0; i < variables.Count; i++) {
					if(i < input.Length) {
						mapping.Add(new KeyValuePair<char, object>(variables[i], input[i]));
						initialInputCount++;
					} else
						mapping.Add(new KeyValuePair<char, object>(variables[i], null));
				}
			}

			//Push value/variable/access to symbol list and return whether a value was pushed
			Func<bool> pushValue = () => {
				//Only push a value if one exists
				if(value.Count > 0) {
					//Check if previous symbol was an access operator and possibly add everything
					if(symbols.LastOrDefault() is CharSymbol cs && (cs.value == '.' || cs.value == ':')) {
						symbols.Add(new StringSymbol(value.ToArray()));
						value.Clear();

						return true;
					}

					//Multiply if preceded by a group close
					if(groups.Count > 0) {
						var last = groups.Last();
						
						//Checks if the last group close index is the same as the values future index in symbols
						if(last.Item1 + last.Item2 == symbols.Count)
							symbols.Add(new CharSymbol('*'));
					}				

					//Account for
					//	(+/-)number, variable/+number, ...
					//	(+/-)number, variable with member accesses
					//	variable, +number/variable, ...
					//	variable/accesses, accesses
					//	(+/-)number
					var subvalue = new StringBuilder();
					var chained = false;

					//Push the number to symbols if it exists
					Action pushNumber = () => {
						if(subvalue.Length > 0) {
							if(chained)
								symbols.Add(new CharSymbol('*'));

							symbols.Add(float.Parse(subvalue.ToString()));
							subvalue.Clear();

							chained = true;
						}
					};

					//Check if first number is negative
					if(value.Peek() == '-')
						subvalue.Append(value.Dequeue());

					//Look for variables, numbers, and accesses
					while(value.Count > 0) {
						var c = value.Dequeue();

						if(char.IsDigit(c) || c == '.') //Number or fractional portion of a number
							subvalue.Append(c);
						else { //Variable
							pushNumber();

							//Chain with previous symbol if needed
							if(chained)
								symbols.Add(new CharSymbol('*'));

							//Add variable
							symbols.Add(new CharSymbol(c));
							chained = true;

							//If no mappings provided, register variables as required
							if(!mappingProvided && !mapping.Any(m => m.Key == c)) {
								if(mapping.Count < input.Length) {
									mapping.Add(new KeyValuePair<char, object>(c, input[mapping.Count]));
									initialInputCount++;
								} else
									mapping.Add(new KeyValuePair<char, object>(c, null));
							}
						}
					}

					//Push the last number if it exists since numbers are only pushed when proceeded by chars in the loop
					pushNumber();

					return true;
				}

				return false;
			};

			//Pull data from formula
			for(var i = 0; i < description.Length; i++) {
				var c = description[i];

				switch(c) {
					case '@': throw new FormulaException("Reserved operator @ not allowed"); //Function application (lhs is function and rhs is value)
					case '(': { //Group open
						//Adjacent groups get multiplied
						if(groups.Count > 0) {
							var last = groups.Last();

							if(last.Item1 + last.Item2 == symbols.Count)
								symbols.Add(new CharSymbol('*'));
						}

						var v = new string(value.ToArray());

						//If a function or value/variable preceded, add the appropriate symbol
						if(functions.Contains(v)) {
							symbols.Add(new StringSymbol(v));
							symbols.Add(new CharSymbol('@'));
							value.Clear();
						} else if(pushValue())
							symbols.Add(new CharSymbol('*'));

						//Remember the group open index based on the current symbol index
						//Also use magnitude status which can be set by '|' prior
						groupIndices.Push((symbols.Count, false));
						break;
					}
					case ')': { //Group close
						pushValue();

						//Create the group
						var index = groupIndices.Pop().Item1;
						groups.Add((index, symbols.Count - index));
						break;
					}
					case '|': { //Absolute value
						var shouldClose = false;

						var v = new string(value.ToArray());
						if(functions.Contains(v)) {
							//If a function precedes, add and open
							symbols.Add(new StringSymbol(v));
							symbols.Add(new CharSymbol('@'));
							value.Clear();
						} else if(pushValue()) {
							//Close if last group is magnitude open and the symbol is not an operator or function
							if(groupIndices.Count > 0 && groupIndices.Peek().Item2) {
								switch(symbols.Last()) {
									case CharSymbol s: shouldClose = char.IsLetterOrDigit(s.value); break;
									case StringSymbol s: shouldClose = !functions.Contains(s.value); break;
									default: shouldClose = true; break;
								}
							}

							if(!shouldClose)
								symbols.Add(new CharSymbol('*'));
						} else if((groupIndices.Count == 0 || groupIndices.Peek().Item2) && groups.Count > 0) {
							//Close if a non magnitude group closed before this
							var last = groups.Last();
							shouldClose = last.Item1 + last.Item2 == symbols.Count;
						}

						if(shouldClose) { //Close magnitude group
							for(var j = 0; j < 2; j++) {
								var index = groupIndices.Pop().Item1;
								groups.Add((index, symbols.Count - index));
							}
						} else { //Open magnitude group
							//Applying magnitude value function the group
							groupIndices.Push((symbols.Count, false));
							symbols.Add(new StringSymbol("abs"));
							symbols.Add(new CharSymbol('@'));
							groupIndices.Push((symbols.Count, true));
						}
						break;
					}
					case '-': { //Negation/subtraction
						//Subtraction occured if a value was pushed or a group ended previously
						if(pushValue() || (groups.Count > 1 && groups.Last().Item1 + groups.Last().Item2 == symbols.Count))
							symbols.Add(new CharSymbol('-'));
						else { //Negation occured
							if(char.IsDigit(description[i + 1])) //Negation can be applied directly if upcoming symbol is a value
								value.Enqueue(c);
							else { //Apply negation through multiplication since upcoming symbol must be function/variable/group
								symbols.Add(-1f);
								symbols.Add(new CharSymbol('*'));
							}
						}
						break;
					}
					case '.':
						if(char.IsDigit(value.Peek()))
							value.Enqueue(c);
						else {
							pushValue();
							symbols.Add(new CharSymbol(c));
						}
						break;
					default:
						if(char.IsLetterOrDigit(c)) //Must be part of a function/value/variable if alphanumeric
							value.Enqueue(c);
						else if(!char.IsWhiteSpace(c)) { //Otherwise assumed to be an operator if non-whitespace
							pushValue();
							symbols.Add(new CharSymbol(c));
						}
						break;
				}
			}

			//Push the potential final value
			pushValue();

			//Assign the symbols and set initial input count
			var resultSymbols = symbols.ToArray();

			//Determine the solving order
			var order = new List<int>();

			//Pushes the order for the symbol into the list and nullifies the symbol and returns the index of the symbol
			Action<int> pushOrder = index => {
				symbols[index] = null;
				order.Add(index);
			};

			//Compute basic order
			groups.Add((0, symbols.Count));
			foreach(var group in groups) {
				var (start, range) = group;
				int index;

				//Find the operands within each group and order them
				while((index = symbols.FindIndex(start, range, s => s is CharSymbol c ? c.value == '.' || c.value == ':' : false)) != -1) pushOrder(index);
				while((index = symbols.FindIndex(start, range, s => s is CharSymbol c ? c.value == '@' : false)) != -1) pushOrder(index);
				while((index = symbols.FindIndex(start, range, s => s is CharSymbol c ? c.value == '^' : false)) != -1) pushOrder(index);
				while((index = symbols.FindIndex(start, range, s => s is CharSymbol c ? c.value == '*' || c.value == '/' || c.value == '%' : false)) != -1) pushOrder(index);
				while((index = symbols.FindIndex(start, range, s => s is CharSymbol c ? c.value == '+' || c.value == '-' : false)) != -1) pushOrder(index);
			}

			//Compute order accounting for symbol removals while 
			var resultOrder = order.ToArray();

			for(var i = 0; i < order.Count; i++) {
				var index = order[i];

				for(var j = i + 1; j < order.Count; j++)
					if(order[j] > index)
						resultOrder[j] -= 2;
			}

			return (resultSymbols, resultOrder, mapping, initialInputCount);
		}
	}
}