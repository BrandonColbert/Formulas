using System.Collections.Generic;

namespace Formulas {
	/// <summary>Represents a parenthesis enclosed section of the subtree</summary>
	class GroupNode : Node {
		public readonly string value;
		public GroupNode(string value) => this.value = value;
		public override string ToString() => $"({value})";
		public override string DisplayString() => $"{ToString()}{base.DisplayString()}";

		public override bool Reduce(out Node node) {
			//Create a tree from the description
			node = Parser.CreateTree(new LinkedList<char>(value));
			return true;
		}
	}
}