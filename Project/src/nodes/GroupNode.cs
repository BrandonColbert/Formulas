namespace Formulas {
	/// <summary>Represents a parenthesis enclosed section of the subtree</summary>
	class GroupNode : Node {
		public readonly string value;
		public GroupNode(string value) => this.value = value;
		public override string ToString() => $"({value})";
		public override string ToDisplayString() => $"(group {value}){base.ToDisplayString()}";

		public override bool Reduce(out Node node) {
			node = new Parser(value).CreateDefinition();
			return true;
		}
	}
}