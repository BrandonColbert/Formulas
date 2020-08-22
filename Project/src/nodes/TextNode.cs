using System.Collections.Generic;

namespace Formulas {
	/// <summary>Represents a variable, property, or function within the tree</summary>
	abstract class TextNode : Node {
		public readonly string value;
		public TextNode(string text) => value = text;
		public override string ToString() => value;
	}
}