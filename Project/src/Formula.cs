using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Formulas
{
	/// <summary>Standard implementation for creating a formula</summary>
	public sealed class Formula : IFormula {
		/// <summary>The original string provided as the formula</summary>
		public string description { get; private set; }

		/// <summary>Variables in the formula</summary>
		public char[] variables => mapping.Select(v => v.Key).ToArray();

		private object[] symbols;
		private int[] order;
		private List<KeyValuePair<char, object>> mapping; 
		private readonly int initialInputCount;
		private static Capacity capacity = new Capacity(100);

		/// <param name="description">Formula to create</param>
		public Formula(string description) : this(description, new object[0]) {}

		/// <param name="other">Formula to copy</param>
		/// <param name="input">Initial inputs to use instead</param>
		public Formula(Formula other, params object[] input) {
			//Copy the formula
			description = other.description;
			//Copy the required components
			order = other.order;
			symbols = other.symbols;

			//Recreate mapping with the same variables, but use this initial input instead
			mapping = new List<KeyValuePair<char, object>>();
			for(var i = 0; i < other.mapping.Count; i++) {
				if(i < input.Length) {
					mapping.Add(new KeyValuePair<char, object>(other.mapping[i].Key, input[i]));
					initialInputCount++;
				} else
					mapping.Add(new KeyValuePair<char, object>(other.mapping[i].Key, null));
			}
		}

		/// <param name="description">Formula to create</param>
		/// <param name="input">Initial inputs to use</param>
		public Formula(string description, params object[] input) {
			this.description = description;
			(symbols, order, mapping, initialInputCount) = Formulizer.Build(description, input);
		}

		/// <param name="input">Inputs for the function</param>
		/// <returns>Solution to the function based on given inputs</returns>
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
			var symbols = this.symbols.Select(symbol => {
				switch(symbol) {
					case StringSymbol v: return v.value; //Function or member/indexer access
					case CharSymbol v:
						if(char.IsLetter(v.value)) { //Variable value
							symbol = solutionMapping[v.value];
							goto default;
						} else //Operator
							return v.value;
					default: return Number.TryParse(symbol, out var n) ? n : symbol;
				}
			}).ToArray();

			//If there is only one symbol, return its value
			if(symbols.Length == 1)
				return symbols[0];

			//Iteratively reduce the symbol count with the result operations in the calculated order
			foreach(var index in order) {
				try {
					//Calculate the operation between the left and right sides then store in symbols
					var op = (char)symbols[index];
					dynamic lhs = symbols[index - 1], rhs = symbols[index + 1];
					object result;

					//Calculate the operation between them
					switch(op) {
						case '^': result = Math.Pow(lhs, rhs); break;
						case '*': result = lhs * rhs; break;
						case '/': result = lhs / rhs; break;
						case '%': result = lhs % rhs; break;
						case '+': result = lhs + rhs; break;
						case '-': result = lhs - rhs; break;
						case '@': result = Apply(lhs, rhs); break;
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

					symbols[index - 1] = Number.TryParse(result, out var n) ? n : result;

					//Replace the lhs, op, and rhs with their result in the symbol list
					Array.Copy(symbols, index + 2, symbols, index, symbols.Length - 2 - index);
				} catch(System.Exception e) {
					throw new Formula.Exception(this, solutionMapping, symbols, index, e);
				}
			}

			return symbols[0];
		}

		/// <returns>A string that represents the current object.</returns>
		public override string ToString() => description;

		/// <summary>Converts a description into a formula</summary>
		/// <param name="description">The description to create a formula with</param>
		public static implicit operator Formula(string description) => new Formula(description);

		private static object Apply(string function, Number value) => applyNumber[function](value);
		private static Dictionary<string, Func<Number, object>> applyNumber = new Dictionary<string, Func<Number, object>>() {
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
			["nml"] = v => Formulizer.Provider.Nml(v)
		};

		private static object Apply(string function, object value) => applyObject[function](value);
		private static Dictionary<string, Func<object, object>> applyObject = new Dictionary<string, Func<object, object>>() {
			["abs"] = v => Formulizer.Provider.Abs(v),
			["nml"] = v => Formulizer.Provider.Nml(v),
			["qtn"] = v => Formulizer.Provider.Qtn(v),
			["vec"] = v => Formulizer.Provider.Vec(v)
		};

		internal class Exception : FormulaException {
			public Exception(Formula formula, ICollection<KeyValuePair<char, object>> mapping, object[] symbols, int index, System.Exception e) : base(
				string.Join("\n", new[]{
					$"{e.Message}\n{formula.description}",
					"Mappings",
					string.Join("\n", mapping.Select(map => $"\t{map.Key} = {map.Value} | {map.Value?.GetType()}")),
					"Symbols",
					string.Join("\n", symbols.Select((s, i) => $"\t'{s}': {s.GetType()}{((i >= index - 1 && index + 1 >= i) ? "\t<-" : string.Empty)}"))
				})
			, e) {}
		}
	}
}