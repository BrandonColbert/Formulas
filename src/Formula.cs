using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Formulas {
	public sealed class Formula : IFormula {
		public string description { get; private set; }
		public char[] variables => mapping.Select(v => v.Key).ToArray();
		private object[] symbols; //The values (floats), variables (chars/strings), functions (strings), and operations (chars)
		private int[] order; //Array of indices of the next lhs to operate on
		private List<KeyValuePair<char, object>> mapping = new List<KeyValuePair<char, object>>(); //The mappings for each variable
		private readonly int initialInputCount; //Number of initial inputs provided
		private static Capacity capacity = new Capacity(100);

		/// <param name="formula">Formula to create</param>
		public Formula(string formula) : this(formula, new object[0]) {}

		/// <param name="other">Formula to copy</param>
		/// <param name="input">Initial inputs to use instead</param>
		public Formula(Formula other, params object[] input) {
			//Copy the formula
			description = other.description;
			//Copy the required components
			order = other.order;
			symbols = other.symbols;

			//Recreate mapping with the same variables, but use this initial input instead
			for(var i = 0; i < other.mapping.Count; i++) {
				if(i < input.Length) {
					mapping.Add(new KeyValuePair<char, object>(other.mapping[i].Key, input[i]));
					initialInputCount++;
				} else
					mapping.Add(new KeyValuePair<char, object>(other.mapping[i].Key, null));
			}
		}

		/// <param name="formula">Formula to create</param>
		/// <param name="input">Initial inputs to use</param>
		public Formula(string formula, params object[] input) {
			this.description = formula;

			//Temporary variables for interpreting the formula
			var symbols = new List<object>(); //Symbols in the formula
			var groups = new List<(int, int)>(); //Each group's index and range
			var groupIndices = new Stack<(int, bool)>(); //Each group's open index and whether it was'|' true or '(' false
			var value = new Queue<char>(); //The values and variables or the function being built
			var initialInputCount = 0;

			//Check for mappings and create if possible
			var mappingProvided = formula.Contains('=');

			if(mappingProvided) {
				var parts = formula.Split('=');
				formula = parts[1];

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

			/// <summary>Push value/variable/access to symbol list</summary>
			/// <returns>Whether a value was pushed</returns>
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

					/// <summary>Push the number to symbols if it exists</summary>
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
			for(var i = 0; i < formula.Length; i++) {
				var c = formula[i];

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
						if(Formulizer.functions.Contains(v)) {
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
						if(Formulizer.functions.Contains(v)) {
							//If a function precedes, add and open
							symbols.Add(new StringSymbol(v));
							symbols.Add(new CharSymbol('@'));
							value.Clear();
						} else if(pushValue()) {
							//Close if last group is magnitude open and the symbol is not an operator or function
							if(groupIndices.Count > 0 && groupIndices.Peek().Item2) {
								switch(symbols.Last()) {
									case CharSymbol s: shouldClose = char.IsLetterOrDigit(s.value); break;
									case StringSymbol s: shouldClose = !Formulizer.functions.Contains(s.value); break;
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
							if(char.IsDigit(formula[i + 1])) //Negation can be applied directly if upcoming symbol is a value
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
			this.symbols = symbols.ToArray();
			this.initialInputCount = initialInputCount;

			//Determine the solving order
			var order = new List<int>();

			/// <summary>Pushes the order for the symbol into the list and nullifies the symbol</summary>
			/// <value>Index of the symbol</value>
			Action<int> pushOrder = index => {
				symbols[index] = null;
				order.Add(index);
			};

			//Compute basic order
			groups.Add((0, symbols.Count));
			foreach(var group in groups) {
				var start = group.Item1;
				var range = group.Item2;
				int index;

				//Find the operands within each group and order them
				while((index = symbols.FindIndex(start, range, s => s is CharSymbol c ? c.value == '.' || c.value == ':' : false)) != -1) pushOrder(index);
				while((index = symbols.FindIndex(start, range, s => s is CharSymbol c ? c.value == '@' : false)) != -1) pushOrder(index);
				while((index = symbols.FindIndex(start, range, s => s is CharSymbol c ? c.value == '^' : false)) != -1) pushOrder(index);
				while((index = symbols.FindIndex(start, range, s => s is CharSymbol c ? c.value == '*' || c.value == '/' || c.value == '%' : false)) != -1) pushOrder(index);
				while((index = symbols.FindIndex(start, range, s => s is CharSymbol c ? c.value == '+' || c.value == '-' : false)) != -1) pushOrder(index);
			}

			//Compute order accounting for symbol removals while 
			this.order = order.ToArray();

			for(var i = 0; i < order.Count; i++) {
				var index = order[i];

				for(var j = i + 1; j < order.Count; j++)
					if(order[j] > index)
						this.order[j] -= 2;
			}
		}

		public object Solve(params object[] input) {
			//Check if too few inputs supplied
			if(mapping.Count > initialInputCount + input.Length)
				throw new FormulaException("Expected " + mapping.Count + " inputs, received " + (initialInputCount + input.Length));

			var solutionMapping = new Dictionary<char, object>();

			//Pull initial input values
			for(var i = 0; i < initialInputCount; i++) {
				var e = mapping[i];
				solutionMapping[e.Key] = e.Value;
			}

			//Pull provided input values
			for(var i = 0; i < mapping.Count - initialInputCount; i++)
				solutionMapping[mapping[initialInputCount + i].Key] = input[i];

			//Create the symbol list
			var symbols = this.symbols.ToArray();

			//If there is only one symbol, return its value
			if(symbols.Length == 1)
				return GetSymbolValue(symbols[0], solutionMapping);

			//Iteratively reduce the symbol count with the result operations in the calculated order
			foreach(var index in order) {
				try {
					//Calculate the operation between the left and right sides then store in symbols
					Operate(
						GetSymbolValue(symbols[index - 1], solutionMapping),
						((CharSymbol)symbols[index]).value,
						GetSymbolValue(symbols[index + 1], solutionMapping),
						out symbols[index - 1]
					);

					//Replace the lhs, op, and rhs with their result in the symbol list
					Array.Copy(symbols, index + 2, symbols, index, symbols.Length - 2 - index);
				} catch(Exception e) {
					throw new FormulaException(e.Message + "\n" + description + "\n" + FormulaException.FormatMapping(solutionMapping) + FormulaException.FormatStatus(symbols, index), e);
				}
			}

			return symbols[0];
		}

		private void Operate(dynamic lhs, char op, dynamic rhs, out object result) {
			//Calculate the operation between them
			switch(op) {
				case '^': result = (float)Math.Pow(lhs, rhs); break;
				case '*': result = lhs * rhs; break;
				case '/': result = lhs / rhs; break;
				case '%': result = lhs % rhs; break;
				case '+': result = lhs + rhs; break;
				case '-': result = lhs - rhs; break;
				case '@': result = apply[lhs](rhs); break;
				case ':': result = lhs[rhs]; break;
				case '.': {
					Type type = lhs.GetType();

					//Get next value from members
					if(!capacity.Retrieve(type, rhs, out Func<object, object> memberGetter)) {
						var members = type.GetMember(rhs);

						//Check if unambiguous valid member exists
						if(members.Length == 1) {
							switch(members[0]) {
								case FieldInfo info: memberGetter = instance => info.GetValue(instance); break;
								case PropertyInfo info: memberGetter = instance => info.GetValue(instance); break;
								default: throw new FormulaException("Member '" + members[0].Name + "' for '" + lhs + "' of type '" + type.Name + "' is not a field or property");
							}

							capacity.Store(type, rhs, memberGetter);
						} else if(members.Length == 0)
							throw new FormulaException("Member '" + rhs + "' for '" + lhs + "' of type '" + type.Name + "' does not exist");
						else
							throw new FormulaException("Member '" + rhs + "' for '" + lhs + "' of type '" + type.Name + "' is ambiguous");
					}

					//Get the value based through field or property access
					result = memberGetter(lhs);
					break;
				}
				default: throw new FormulaException("Operator '" + op + "' not supported");
			}
		}

		/// <param name="symbol">The variable/value symbol to get the value of</param>
		/// <param name="mappings">The mappings between variables and inputs</param>
		/// <returns>The value at an index based on the mappings</returns>
		private object GetSymbolValue(object symbol, IDictionary<char, object> mapping) {
			switch(symbol) {
				case StringSymbol v: return v.value; //Function or member/indexer access
				case CharSymbol v: return GetSymbolValue(mapping[v.value], mapping); //Variable value
				case double v: return (float)v; //Auto convert alternative numeric types to floats
				case int v: return (float)v;
				case decimal v: return (float)v;
				case long v: return (float)v;
				case Func<object> v: return v(); //Lambda without arguments/getter to evaluate
				default: return symbol; //Must be some non-number
			}
		}

		/// <returns>A Formula created with the native scripting language</returns>
		public IFormula Compile() => NativeFormula.Compile(this, (symbols, order, mapping, initialInputCount));

		public override string ToString() => description;
		public static implicit operator Formula(string formula) => new Formula(formula);

		/// <summary>Parses a string as the specified primative value</summary>
		/// <param name="value">String to parse</param>
		/// <param name="type">Type to parse as</param>
		private static object ParseString(string value, Type type) => parseFunctions[type](value);
		private static Dictionary<Type, Func<string, object>> parseFunctions = new Dictionary<Type, Func<string, object>>() {
			[typeof(bool)] = v => bool.Parse(v),
			[typeof(byte)] = v => byte.Parse(v),
			[typeof(sbyte)] = v => sbyte.Parse(v),
			[typeof(char)] = v => char.Parse(v),
			[typeof(decimal)] = v => decimal.Parse(v),
			[typeof(double)] = v => double.Parse(v),
			[typeof(float)] = v => float.Parse(v),
			[typeof(int)] = v => int.Parse(v),
			[typeof(long)] = v => long.Parse(v),
			[typeof(ulong)] = v => ulong.Parse(v),
			[typeof(short)] = v => short.Parse(v),
			[typeof(ushort)] = v => ushort.Parse(v),
			[typeof(string)] = v => v
		};

		private static Dictionary<string, Func<object, object>> apply = new Dictionary<string, Func<object, object>>() {
			["sin"] = v => Formulizer.Provider.Sin(v),
			["asin"] = v => Formulizer.Provider.Asin(v),
			["cos"] = v => Formulizer.Provider.Cos(v),
			["acos"] = v => Formulizer.Provider.Acos(v),
			["tan"] = v => Formulizer.Provider.Tan(v),
			["atan"] = v => Formulizer.Provider.Atan(v),
			["sqrt"] = v => Formulizer.Provider.Sqrt(v),
			["ln"] = v => Formulizer.Provider.Ln(v),
			["log"] = v => Formulizer.Provider.Log(v),
			["sgn"] = v => Formulizer.Provider.Sgn(v),
			["rvs"] = v => Formulizer.Provider.Rvs(v),
			["lvs"] = v => Formulizer.Provider.Lvs(v),
			["uvs"] = v => Formulizer.Provider.Uvs(v),
			["dvs"] = v => Formulizer.Provider.Dvs(v),
			["fvs"] = v => Formulizer.Provider.Fvs(v),
			["bvs"] = v => Formulizer.Provider.Bvs(v),
			["rnd"] = v => Formulizer.Provider.Rnd(v),
			["abs"] = v => Formulizer.Provider.Abs(v),
			["nml"] = v => Formulizer.Provider.Nml(v),
			["qtn"] = v => Formulizer.Provider.Qtn(v),
			["vec"] = v => Formulizer.Provider.Vec(v)
		};
	}
}