using System.Linq.Expressions;

namespace Formulas {
	/// <summary>Represents a property or function name within a tree</summary>
	class NameNode : TextNode {
		public NameNode(string text) : base(text) {}
		public override Expression Compile(Description desc, ParameterExpression args) => Expression.Constant(value);
		public override string ToDisplayString() => $"(name {value}){base.ToDisplayString()}";

		public override bool Amend(out Node node) {
			switch(Parent) {
				case OpNode parentOpNode:
					switch(parentOpNode.value) {
						case Operation.Transform:
							if(ReferenceEquals(Parent.Left, this)) {
								node = new FunctionNode(value);
								return true;
							}
							break;
					}
					break;
			}

			node = null;
			return base.Amend(out node);
		}
	}
}