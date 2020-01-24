using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Formulas {
	/// <summary>Standard implementation for creating a formula</summary>
	/// <typeparam name="Provider">IFormulaProvider implementation to specify operations</typeparam>
	/// <typeparam name="Number">Numeric type</typeparam>
	/// <typeparam name="Vector">Vector type</typeparam>
	/// <typeparam name="Quaternion">Quaternion type</typeparam>
	public sealed class Formula<Provider, Number, Vector, Quaternion> : IFormula where Provider : IFormulaProvider<Number, Vector, Quaternion>, new() {
		/// <summary>Provides operations for the formula</summary>
		public static IFormulaProvider<Number, Vector, Quaternion> provider { get; } = new Provider();

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
		public Formula(Formula<Provider, Number, Vector, Quaternion> other, params object[] input) {
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
			(symbols, order, mapping, initialInputCount) = provider.Build(description, input);
		}

		/// <param name="input">Inputs for the function</param>
		/// <returns>Solution to the function based on given inputs</returns>
		public object Solve(params object[] input) {
			//Check if too few inputs supplied
			if(mapping.Count > initialInputCount + input.Length)
				throw new FormulaException($"Expected {mapping.Count} inputs, received {initialInputCount + input.Length}");

			var solutionMapping = new Dictionary<char, object>();

			//Pull initial input values
			for(var i = initialInputCount - 1; i >= 0; i--) {
				var e = mapping[i];
				solutionMapping[e.Key] = e.Value;
			}

			//Pull provided input values
			for(var i = mapping.Count - initialInputCount - 1; i >= 0; i--)
				solutionMapping[mapping[initialInputCount + i].Key] = input[i];

			//Create the symbol list
			var symbols = new object[this.symbols.Length];
			
			for(var i = this.symbols.Length - 1; i >= 0; i--) {
				var symbol = this.symbols[i];

				switch(symbol) {
					case StringSymbol v: //Function or member/indexer access
						symbols[i] = v.value; 
						break;
					case CharSymbol v:
						if(char.IsLetter(v.value)) { //Variable value
							symbol = solutionMapping[v.value];
							goto default;
						} else //Operator
							symbols[i] = v.value;
						break;
					default:
						symbols[i] = provider.ToNumber(symbol, out var n) ? n : symbol;
						break;
				}
			}

			//Iteratively reduce the symbol count with the result operations in the calculated order
			foreach(var index in order) {
				try {
					//Calculate the operation between the left and right sides then store in symbols
					var lhs = symbols[index - 1];
					var rhs = symbols[index + 1];
					object result;

					//Calculate the operation between them
					switch((char)symbols[index]) {
						case '^': result = provider.Pow((Number)lhs, (Number)rhs); break;
						case '*': result = (dynamic)lhs * (dynamic)rhs; break;
						case '/': result = (dynamic)lhs / (dynamic)rhs; break;
						case '%': result = (dynamic)lhs % (dynamic)rhs; break;
						case '+': result = (dynamic)lhs + (dynamic)rhs; break;
						case '-': result = (dynamic)lhs - (dynamic)rhs; break;
						case '@': result = Apply((string)lhs, (dynamic)rhs); break;
						case ':': result = ((dynamic)lhs)[(string)rhs]; break;
						case '.': {
							var type = lhs.GetType();

							//Get next value from members
							if(!capacity.Retrieve(type, (string)rhs, out Func<object, object> getMember)) {
								var members = type.GetMember((string)rhs);

								//Check if unambiguous valid member exists
								if(members.Length == 1) {
									switch(members[0]) {
										case FieldInfo info: getMember = info.GetValue; break;
										case PropertyInfo info: getMember = info.GetValue; break;
										default: throw new FormulaException($"Member '{members[0].Name}' for '{lhs}' of type '{type.Name}' is not a field or property");
									}

									capacity.Store(type, (string)rhs, getMember);
								} else
									throw new FormulaException($"Member '{rhs}' for '{lhs}' of type '{type.Name}' {(members.Length == 0 ? "does not exist" : "is ambiguous")}");
							}

							//Get the value based through field or property access
							result = getMember(lhs);
							break;
						}
						default: throw new FormulaException($"Operator '{symbols[index]}' not supported");
					}

					symbols[index - 1] = provider.ToNumber(result, out var n) ? n : result;

					//Replace the lhs, op, and rhs with their result in the symbol list
					Array.Copy(symbols, index + 2, symbols, index, symbols.Length - 2 - index);
				} catch(System.Exception e) {
					throw new Exception(this, solutionMapping, symbols, index, e);
				}
			}

			return symbols[0];
		}

		internal class Exception : FormulaException {
			public Exception(Formula<Provider, Number, Vector, Quaternion> formula, ICollection<KeyValuePair<char, object>> mapping, object[] symbols, int index, System.Exception e) : base(
				string.Join("\n", new[]{
					$"{e.Message}\n{formula.description}",
					"Mappings",
					string.Join("\n", mapping.Select(map => $"\t{map.Key} = {map.Value} | {map.Value?.GetType()}")),
					"Symbols",
					string.Join("\n", symbols.Select((s, i) => $"\t'{s}': {s.GetType()}{((i >= index - 1 && index + 1 >= i) ? "\t<-" : string.Empty)}"))
				})
			, e) {}
		}

		/// <returns>A string that represents the current object.</returns>
		public override string ToString() => description;

		/// <summary>Converts a description into a formula</summary>
		/// <param name="description">The description to create a formula with</param>
		public static implicit operator Formula<Provider, Number, Vector, Quaternion>(string description) => new Formula<Provider, Number, Vector, Quaternion>(description);

		private object Apply(string name, Number value) => numericFunctions[name](value);
		private object Apply(string name, Vector value) => vectorFunctions[name](value);
		private object Apply(string name, Quaternion value) => quaternionFunctions[name](value);

		private static Dictionary<string, Func<Number, object>> numericFunctions = new Dictionary<string, Func<Number, object>>() {
			["sin"] = v => provider.Sin(v),
			["asin"] = v => provider.Asin(v),
			["cos"] = v => provider.Cos(v),
			["acos"] = v => provider.Acos(v),
			["tan"] = v => provider.Tan(v),
			["atan"] = v => provider.Atan(v),
			["sqrt"] = v => provider.Sqrt(v),
			["ln"] = v => provider.Ln(v),
			["log"] = v => provider.Log(v),
			["sgn"] = v => provider.Sgn(v),
			["rvs"] = v => provider.Rvs(v),
			["lvs"] = v => provider.Lvs(v),
			["uvs"] = v => provider.Uvs(v),
			["dvs"] = v => provider.Dvs(v),
			["fvs"] = v => provider.Fvs(v),
			["bvs"] = v => provider.Bvs(v),
			["rnd"] = v => provider.Rnd(v),
			["abs"] = v => provider.Abs(v),
			["nml"] = v => provider.Nml(v)
		};

		private static Dictionary<string, Func<Vector, object>> vectorFunctions = new Dictionary<string, Func<Vector, object>>() {
			["abs"] = v => provider.Abs(v),
			["nml"] = v => provider.Nml(v),
			["qtn"] = v => provider.Qtn(v)
		};

		private static Dictionary<string, Func<Quaternion, object>> quaternionFunctions = new Dictionary<string, Func<Quaternion, object>>() {
			["abs"] = v => provider.Abs(v),
			["nml"] = v => provider.Nml(v),
			["vec"] = v => provider.Vec(v),
			["inq"] = v => provider.Inq(v)
		};
	}
}