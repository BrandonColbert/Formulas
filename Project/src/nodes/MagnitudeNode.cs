namespace Formulas {
	/// <summary>Represents an enclosed section of the subtree whose result is an absolute value</summary>
	class MagnitudeNode : GroupNode {
		public const string TransformName = "abs";
		public MagnitudeNode(string value) : base(value) {}
		public override string ToString() => $"|{value}|";
		public override string ToDisplayString() => $"(magnitude {value}){base.ToDisplayString()}";

		public override bool Amend(out Node node) {
			if(!base.Amend(out var group)) {
				node = group;
				return false;
			}

			//Magnitude of the grouped simplification
			node = new OpNode(Operation.Transform);
			node.Usurp(new FunctionNode(TransformName));
			node.Append(group);

			return true;
		}
	}
}