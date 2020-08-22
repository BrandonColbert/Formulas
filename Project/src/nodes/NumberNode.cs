using System.Collections.Generic;
using System.Linq.Expressions;

namespace Formulas {
	/// <summary>Represents a number within the tree</summary>
	class NumberNode : Node {
		public readonly Number value;
		public NumberNode(Number number) => value = number;
		public override string ToString() => value.ToString();
		public override string ToDisplayString() => $"(number {value}){base.ToDisplayString()}";
		public override Expression Compile(Description desc, ParameterExpression args) => Expression.Constant(value);

		public override bool Calculate(Description desc, Dictionary<string, object> inputs, out object result) {
			result = value;
			return true;
		}
	}
}