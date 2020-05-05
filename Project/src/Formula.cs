using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Formulas {
	/// <summary>Standard implementation for creating a formula</summary>
	public sealed class Formula : IFormula {
		/// <summary>The original string provided as the formula</summary>
		public readonly string description;

		/// <summary>Variables in the formula</summary>
		public char[] variables => mapping.Select(v => v.Key).ToArray();

		internal object[] symbols;
		internal List<KeyValuePair<char, object>> mapping; 
		private int[] order;
		private readonly int initialInputCount;
		private static Capacity<Type, string, Func<object, object>> capacity = new Capacity<Type, string, Func<object, object>>(100);
		private const string IndexerProperty = "Item";

		/// <param name="description">Formula to create</param>
		public Formula(string description) : this(description, new object[0]) {}

		/// <param name="other">Formula to copy</param>
		/// <param name="input">Initial inputs to use instead</param>
		public Formula(Formula other, params object[] input) {
			//Copy the components
			(description, order, symbols) = (other.description, other.order.ToArray(), other.symbols.ToArray());

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
			(symbols, order, mapping, initialInputCount) = Builder.Parse(description, input);
		}

		/// <param name="input">Inputs for the function</param>
		/// <returns>Solution to the function based on given inputs</returns>
		/// <remarks>Formulas accessing a subclass member of an indexed value will not compile</remarks>
		public object Solve(params object[] input) {
			//Check if too few inputs supplied
			if(mapping.Count > initialInputCount + input.Length)
				throw new FormulaSolveException($"Expected {mapping.Count} inputs, received {initialInputCount + input.Length}\n{description}");

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
						symbols[i] = Number.From(symbol, out var n) ? n : symbol;
						break;
				}
			}

			//Iteratively reduce the symbol count with the result operations in the calculated order
			foreach(var index in order) {
				try {
					//Calculate the operation between the left and right sides then store in symbols
					var lhs = symbols[index - 1];
					var rhs = symbols[index + 1];

					//Calculate the operation between them
					switch((char)symbols[index]) {
						case '+': symbols[index - 1] = (dynamic)lhs + (dynamic)rhs; break;
						case '-': symbols[index - 1] = (dynamic)lhs - (dynamic)rhs; break;
						case '*': symbols[index - 1] = (dynamic)lhs * (dynamic)rhs; break;
						case '/': symbols[index - 1] = (dynamic)lhs / (dynamic)rhs; break;
						case '%': symbols[index - 1] = (dynamic)lhs % (dynamic)rhs; break;
						case '^': symbols[index - 1] = Features.pow((Number)lhs, (Number)rhs); break;
						case '@': symbols[index - 1] = Features.Function(lhs as string, rhs); break;
						case ':': {
							var result = ((dynamic)lhs)[rhs as string];
							symbols[index - 1] = Number.From(result, out Number n) ? n : result;
							break;
						}
						case '.': {
							var type = lhs.GetType();

							//Get next value from members
							if(!capacity.Retrieve(type, rhs as string, out var getMember)) {
								var members = type.GetMember(rhs as string);

								//Check if unambiguous valid member exists
								if(members.Length == 1) {
									switch(members[0]) {
										case FieldInfo info: getMember = info.GetValue; break;
										case PropertyInfo info: getMember = info.GetValue; break;
										default: throw new FormulaSolveException($"Member '{members[0].Name}' for '{lhs}' of type '{type.Name}' is not a field or property\n{description}");
									}

									capacity.Store(type, rhs as string, getMember);
								} else
									throw new FormulaSolveException($"Member '{rhs}' for '{lhs}' of type '{type.Name}' {(members.Length == 0 ? "does not exist" : "is ambiguous")}\n{description}");
							}

							//Get the value based through field or property access
							var result = getMember(lhs);
							symbols[index - 1] = Number.From(result, out var n) ? n : result;
							break;
						}
						default: throw new FormulaSolveException($"Operator '{symbols[index]}' not supported\n{description}");
					}

					//Replace the lhs, op, and rhs with their result in the symbol list
					Array.Copy(symbols, index + 2, symbols, index, symbols.Length - 2 - index);
				} catch(Exception e) {
					throw new FormulaSolveException(this, index, e);
				}
			}

			return Number.From(symbols[0], out var n1) ? n1 : symbols[0];
		}

		/// <summary>Compiles this formula to improve solve performance. Should be used when formula is accessed frequently</summary>
		/// <returns>A new formula with the solver compiled from this formula's information</returns>
		public IFormula Compile() {
			//Prepare expression arguments, type specification, and symbol list
			var args = Expression.Parameter(typeof(object[]), "args");
			var types = Builder.ParseTypes(description);
			var symbols = new object[this.symbols.Length];

			//Expresion at, to type, and at to type
			Expression At(int j) => (Expression)symbols[j];
			Expression Ext(Expression value, Type type) => value.Type == type ? value : type.IsValueType ? Expression.Unbox(value, type) : Expression.Convert(value, type);
			Expression Extat(int j, Type type) => Ext(At(j), type);

			//Fill symbols and acquire expressions
			for(var i = this.symbols.Length - 1; i >= 0; i--) {
				var symbol = this.symbols[i];

				switch(symbol) {
					case StringSymbol v: //Function or member/indexer access
						symbols[i] = v.value; 
						break;
					case CharSymbol v:
						if(char.IsLetter(v.value)) { //Variable
							var index = mapping.FindIndex(m => m.Key == v.value);

							if(index == -1)
								throw new FormulaCompileException($"Reference to unknown variable '{v.value}'");

							var entry = mapping[index];

							if(entry.Value != null) //Add the value if initial input exists
								symbols[i] = Expression.Constant(entry.Value);
							else //Pull from parameters otherwise
								symbols[i] = Ext(Expression.ArrayIndex(args, Expression.Constant(index - initialInputCount)), types[index]);
						} else //Operator
							symbols[i] = v.value;
						break;
					default:
						symbols[i] = Expression.Constant(Number.TryParse(symbol.ToString(), out var n) ? n : symbol);
						break;
				}
			}

			//Construct solution function body
			foreach(var index in order) {
				var op = (char)symbols[index];

				switch(op) {
					case '^': //Call Feature's power function
						symbols[index - 1] = Expression.Call(
							Expression.Constant(Features.pow.Target), Features.pow.Method,
							Expression.Convert(At(index - 1), typeof(Number)),
							Expression.Convert(At(index + 1), typeof(Number))
						);
						break;
					case ':': { //Try to access indexer property, otherwise use runtime version
						var (instance, parameter) = (At(index - 1), Expression.Constant(symbols[index + 1]));
						var indexer = instance.Type.GetProperty(IndexerProperty);
						symbols[index - 1] = indexer != null ?
							Expression.Property(instance, indexer, parameter) as Expression :
							Expression.Call(((Func<object, string, object>)GetIndexerValue).Method, Extat(index - 1, typeof(object)), parameter) as Expression;
						break;
					}
					case '.': { //Try to access member property/field, otherwise use runtime version
						var (instance, member) = (At(index - 1), symbols[index + 1].ToString());

						try {
							symbols[index - 1] = Expression.PropertyOrField(instance, member);
						} catch(ArgumentException) {
							var ex = Expression.Call(
								((Func<object, string, object>)GetPropertyOrFieldValue).Method,
								Ext(instance, typeof(object)),
								Expression.Constant(member)
							) as Expression;

							var memberType = instance.Type.GetField(member)?.FieldType ?? instance.Type.GetProperty(member)?.PropertyType;
							symbols[index - 1] = memberType != null ? Expression.Unbox(ex, memberType) : ex;
						}
						break;
					}
					case '@': //Call the matching feature defined function
						var value = At(index + 1);
						var (target, func) = Features.MatchFunction(symbols[index - 1].ToString().ToLower(), value.Type);
						var parameterType = func.GetParameters().First().ParameterType;
						symbols[index - 1] = Expression.Call(Expression.Constant(target), func, value.Type == parameterType ? value : Expression.Convert(value, parameterType)); break;
					default: {
						Func<Expression, Expression, BinaryExpression> opEx;
						string opName;

						switch(op) {
							case '*': (opEx, opName) = (Expression.Multiply, "op_Multiply"); break;
							case '/': (opEx, opName) = (Expression.Divide, "op_Division"); break;
							case '%': (opEx, opName) = (Expression.Modulo, "op_Modulus"); break;
							case '+': (opEx, opName) = (Expression.Add, "op_Addition"); break;
							case '-': (opEx, opName) = (Expression.Subtract, "op_Subtraction"); break;
							default: throw new FormulaCompileException($"Operator '{op}' not supported");
						}

						var (lv, rv) = (At(index - 1), At(index + 1));

						try {
							symbols[index - 1] = opEx(lv, rv);
						} catch(InvalidOperationException e) {
							var (ltype, rtype) = (lv.Type, rv.Type);
							var (cltype, crtype) = Features.CompatibleType(ltype, opName, rtype);

							if(cltype == null || crtype == null)
								throw new FormulaCompileException($"Unable to '{op}' bewteen types {ltype.Name} and {rtype.Name}", e);

							symbols[index - 1] = opEx(
								ltype == cltype ? lv : Expression.Convert(lv, cltype),
								rtype == crtype ? rv : Expression.Convert(rv, crtype)
							);
						}

						break;
					}
				}

				Array.Copy(symbols, index + 2, symbols, index, symbols.Length - 2 - index);
			}

			return new DirectFormula(Expression.Lambda<Func<object[], object>>(Extat(0, typeof(object)), args).Compile());
		}

		/// <returns>A string that represents the current object.</returns>
		public override string ToString() => description;

		/// <summary>Converts a description into a formula</summary>
		/// <param name="description">The description to create a formula with</param>
		public static implicit operator Formula(string description) => new Formula(description);

		private static object GetPropertyOrFieldValue(object instance, string member) => instance.GetType().GetField(member)?.GetValue(instance) ?? instance.GetType().GetProperty(member)?.GetValue(instance);
		private static object GetIndexerValue(object instance, object index) => instance.GetType().GetProperty(IndexerProperty).GetValue(instance, new []{index});
	}
}