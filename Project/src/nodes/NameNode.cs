using System.Linq.Expressions;

namespace Formulas {
	/// <summary>Represents a property or function name within a tree</summary>
	class NameNode : TextNode {
		public NameNode(string text) : base(text) {}
		public override Expression Compile(Specification spec, ParameterExpression args) => Expression.Constant(value);
		public override string DisplayString() => $"{ToString()} (name){base.DisplayString()}";
	}
}