using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace Formulas {
	/// <summary>Represents some kind of operation between its children</summary>
	class OpNode : Node {
		public readonly Operation value;
		public OpNode(Operation op) => value = op;
		public override string ToDisplayString() => $"(operation {value}){base.ToDisplayString()}";

		public override bool Amend(out Node node) {
			var left = Left && Left is NumberNode l ? l : null;
			var right = Right && Right is NumberNode r ? r : null;

			//Ensure both left and right sides are numbers
			if(!left || !right)
				return base.Amend(out node);

			//If the operation is numeric, return a new number node with the computed value
			switch(value) {
				case Operation.Add:
					node = new NumberNode(left.value + right.value);
					return true;
				case Operation.Subtract:
					node = new NumberNode(left.value - right.value);
					return true;
				case Operation.Multiply:
					node = new NumberNode(left.value * right.value);
					return true;
				case Operation.Divide:
					node = new NumberNode(left.value / right.value);
					return true;
				case Operation.Modulo:
					node = new NumberNode(left.value % right.value);
					return true;
				case Operation.Negate:
					node = new NumberNode(-right.value);
					return true;
				case Operation.Power:
					node = new NumberNode(Math.Pow(left.value, right.value));
					return true;
				default:
					return base.Amend(out node);
			}
		}

		public override bool Calculate(Description desc, Dictionary<string, object> inputs, out object result) {
			object ValueOf(Node node) {
				if(!node.Calculate(desc, inputs, out var r))
					throw new SolveException($"Unable to calculate '{node}' in '{this}'");

				return r;
			}

			switch(value) {
				case Operation.Add:
					result = (dynamic)ValueOf(Left) + (dynamic)ValueOf(Right);
					return true;
				case Operation.Subtract:
					result = (dynamic)ValueOf(Left) - (dynamic)ValueOf(Right);
					return true;
				case Operation.Multiply:
					result = (dynamic)ValueOf(Left) * (dynamic)ValueOf(Right);
					return true;
				case Operation.Divide:
					result = (dynamic)ValueOf(Left) / (dynamic)ValueOf(Right);
					return true;
				case Operation.Modulo:
					result = (dynamic)ValueOf(Left) % (dynamic)ValueOf(Right);
					return true;
				case Operation.Negate:
					result = -(dynamic)ValueOf(Right);
					return true;
				case Operation.Power:
					result = (Number)Math.Pow((Number)ValueOf(Left), (Number)ValueOf(Right));
					return true;
				case Operation.Transform:
					if(!(Left is FunctionNode leftFunctionNode))
						throw new SolveException($"Transform function must be provided, but '{Left}' was supplied for '{this}'");

					if(!leftFunctionNode.Apply(ValueOf(Right), out result))
						throw new SolveException($"Unable to transform '{Right}' with function '{Left}'");

					return true;
				case Operation.Index: {
					var left = ValueOf(Left);

					switch(Right) {
						case TextNode rightTextNode: {
							result = ((dynamic)left)[rightTextNode.value];

							if(Number.From(result, out var n))
								result = n;

							break;
						}
						case NumberNode rightNumberNode: {
							var index = rightNumberNode.value;

							if(left.GetType().IsArray)
								result = ((dynamic)left)[(int)rightNumberNode.value];
							else
								result = ((dynamic)left)[index];

							if(Number.From(result, out var n))
								result = n;

							break;
						}
						default:
							throw new SolveException($"Transform parameter must be text or a number, but '{Right}' was supplied for '{this}'");
					}
					return true;
				}
				case Operation.Property: {
					var left = ValueOf(Left);

					if(!(Right is NameNode rightNameNode))
						throw new SolveException($"Property must be a name, but '{Right}' was supplied for '{this}'");

					var type = left.GetType();
					var members = type.GetMember(rightNameNode.value);

					if(members.Length != 1)
						throw new SolveException($"Property '{Right}' is ambiguous on type '{type.Name}' within '{this}'");

					switch(members[0]) {
						case FieldInfo info:
							result = info.GetValue(left);
							break;
						case PropertyInfo info:
							result = info.GetValue(left);
							break;
						default:
							throw new SolveException($"Member '{members[0].Name}' of type '{type.Name}' descified by '{Left}' is not a field or property");
					}

					if(Number.From(result, out var n))
						result = n;

					return true;
				}
				default:
					throw new ParseException($"'{value}' within '{this}' is not a supported operation");
			}
		}

		public override Expression Compile(Description desc, ParameterExpression args) {
			switch(value) {
				case Operation.Negate:
					return Expression.Negate(Right.Compile(desc, args));
				case Operation.Power: {
					var left = Left.Compile(desc, args);
					var right = Right.Compile(desc, args);

					if(left.Type != typeof(double))
						left = Expression.Convert(left, typeof(double));
					if(right.Type != typeof(double))
						right = Expression.Convert(right, typeof(double));

					return Expression.Convert(
						Expression.Call(((Func<double, double, double>)Math.Pow).Method, left, right),
						typeof(Number)
					);
				} case Operation.Property: {
					var instance = Left.Compile(desc, args);

					if(!(Right is NameNode rightNameNode))
						throw new CompileException($"Property must be a name, but '{Right}' was supplied for '{this}'");

					try {
						return Expression.PropertyOrField(instance, rightNameNode.value);
					} catch(ArgumentException) {
						var getter = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
							CSharpBinderFlags.None,
							rightNameNode.value,
							Features.GetContext(),
							new[]{CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)} //Instance
						);

						return Expression.Dynamic(getter, typeof(object), instance);
					}
				}
				case Operation.Index: {
					var instance = Left.Compile(desc, args);

					if(instance.Type.IsArray) {
						if(!(Right is NumberNode))
							throw new CompileException($"Arrays can only be indexed with numbers, but '{Right}' was supplied for '{this}'");

						var index = Right.Compile(desc, args);
						if(index.Type != typeof(int))
							index = Expression.Convert(index, typeof(int));

						return Expression.ArrayIndex(instance, index);
					}

					var indexers = instance.Type
						.GetProperties()
						.Where(v => {
							var pars = v.GetIndexParameters();

							if(pars.Length != 1)
								return false;

							switch(Right) {
								case NameNode _:
									return pars.First().ParameterType == typeof(string);
								case NumberNode _:
									return Number.Is(pars.First().ParameterType);
								default:
									return false;
							}
						})
						.ToArray();

					switch(indexers.Length) {
						case 0: { //No indexer found, use dynamic index
							var indexer = Microsoft.CSharp.RuntimeBinder.Binder.GetIndex(
								CSharpBinderFlags.None,
								Features.GetContext(),
								new[]{
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), //Instance
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) //Parameter
								}
							);

							var parameter = Right.Compile(desc, args);
							return Expression.Dynamic(indexer, typeof(object), instance, parameter);
						}
						case 1: {
							var indexer = indexers.First();

							var parameter = Right.Compile(desc, args);
							var parameterType = indexer.GetIndexParameters().First().ParameterType;
							if(parameter.Type != parameterType)
								parameter = Expression.Convert(parameter, parameterType);

							return Expression.Property(
								instance,
								indexer,
								parameter
							);
						}
						default:
							throw new CompileException($"Ambiguous indexers found for '{Left}' of type '{instance.Type.Name}' accepting '{Right}'");
					}
				}
				case Operation.Transform: {
					if(!(Left is FunctionNode leftFunctionNode))
						throw new CompileException($"Transform function must be provided, but '{Left}' was supplied for '{this}'");

					var parameter = Right.Compile(desc, args);
					if(!leftFunctionNode.Match(parameter.Type, out var function))
						throw new CompileException($"Input of type {parameter.Type} can not be transformed by function '{Left}'");

					var (target, method) = function;
					var parameterType = method.GetParameters().First().ParameterType;
					if(parameter.Type != parameterType)
						parameter = Expression.Convert(parameter, parameterType);

					return Expression.Call(
						Expression.Constant(target),
						method,
						parameter
					);
				}
				default: {
					Func<Expression, Expression, BinaryExpression> operation;

					switch(value) {
						case Operation.Add:
							operation = Expression.Add;
							break;
						case Operation.Subtract:
							operation = Expression.Subtract;
							break;
						case Operation.Multiply:
							operation = Expression.Multiply;
							break;
						case Operation.Divide:
							operation = Expression.Divide;
							break;
						case Operation.Modulo:
							operation = Expression.Modulo;
							break;
						default:
							throw new CompileException($"Operator '{value}' in '{this}' does not support compilation");
					}

					var left = Left.Compile(desc, args);
					var right = Right.Compile(desc, args);

					if(Number.Is(left.Type) && left.Type != typeof(Number))
						left = Expression.Convert(left, typeof(Number));
					if(Number.Is(right.Type) && right.Type != typeof(Number))
						right = Expression.Convert(right, typeof(Number));

					try {
						return operation(left, right);
					} catch(InvalidOperationException) {
						if(!TypeMap.Coerce(value.MethodName(), left.Type, right.Type, out var leftType, out var rightType))
							throw new CompileException($"Unable to '{value}' between types {Parser.GetTypename(right.Type)} and '{Parser.GetTypename(left.Type)}'");

						return operation(
							Expression.Convert(left, leftType),
							Expression.Convert(right, rightType)
						);
					}
				}
			}
		}

		public override string ToString() {
			var builder = new StringBuilder();

			bool grouped;

			if(Parent && Parent is OpNode parentOpNode) {
				switch(parentOpNode.value) {
					case Operation.Transform:
						grouped = false;
						break;
					default:
						grouped = parentOpNode.value.Precedence() > value.Precedence();
						break;
				}
			} else
				grouped = false;
			
			if(grouped)
				builder.Append("(");

			switch(value) {
				case Operation.Add:
					builder.Append($"{Left} + {Right}");
					break;
				case Operation.Subtract:
					builder.Append($"{Left} - {Right}");
					break;
				case Operation.Multiply:
					builder.Append($"{Left} * {Right}");
					break;
				case Operation.Divide:
					builder.Append($"{Left} / {Right}");
					break;
				case Operation.Modulo:
					builder.Append($"{Left} % {Right}");
					break;
				case Operation.Negate:
					builder.Append($"-{Right}");
					break;
				case Operation.Power:
					switch(Right) {
						case OpNode rightOpNode:
							if(rightOpNode.value.Precedence() >= value.Precedence())
								builder.Append($"{Left}^({Right})");
							else
								goto default;
							break;
						default:
							builder.Append($"{Left}^{Right}");
							break;
					}
					break;
				case Operation.Property:
					builder.Append($"{Left}.{Right}");
					break;
				case Operation.Index:
					builder.Append($"{Left}:{Right}");
					break;
				case Operation.Transform:
					builder.Append($"{Left}({Right})");
					break;
			}

			if(grouped)
				builder.Append(")");

			return builder.ToString();
		}
	}
}