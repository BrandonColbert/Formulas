using System.Collections.Generic;
using System.Linq.Expressions;

namespace Formulas {
	/// <summary>
	/// Represents a variable within the tree
	/// 
	/// Calculation coerces this node to act as a variable
	/// </summary>
	class VariableNode : TextNode {
		public VariableNode(string text) : base(text) {}
		public override string ToDisplayString() => $"(variable {value}){base.ToDisplayString()}";
		public override bool Calculate(Description desc, Dictionary<string, object> inputs, out object result) => inputs.TryGetValue(value, out result);

		public override Expression Compile(Description desc, ParameterExpression args) {
			var index = desc.variables.IndexOf(value);

			if(index == -1)
				throw new CompileException($"Unknown variable '{this}'");

			if(!desc.types.TryGetValue(value, out var type))
				throw new CompileException($"Type not descified for variable '{this}'");

			Expression body = Expression.ArrayIndex(args, Expression.Constant(index));

			if(body.Type != type) {
				if(type.IsValueType)
					body = Expression.Unbox(body, type);

				body = Expression.Convert(body, type);
			}

			if(Number.Is(body.Type) && body.Type != typeof(Number))
				body = Expression.Convert(body, typeof(Number));

			return body;
		}
	}
}