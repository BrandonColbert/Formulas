using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Formulas {
	public sealed class Formula : IFormula {
		public string description { get; private set; }
		public char[] variables => mapping.Select(v => v.Key).ToArray();
		private object[] symbols;
		private int[] order;
		private List<KeyValuePair<char, object>> mapping; 
		private readonly int initialInputCount;
		private static Capacity capacity = new Capacity(100);

		/// <param name="formula">Formula to create</param>
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

		/// <param name="formula">Formula to create</param>
		/// <param name="input">Initial inputs to use</param>
		public Formula(string description, params object[] input) {
			this.description = description;
			(symbols, order, mapping, initialInputCount) = Formulizer.Build(description, input);
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