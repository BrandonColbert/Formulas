using System;
using static Formulas.Transforms;

namespace Formulas {
	/// <summary>Represents a function within at tree</summary>
	class FunctionNode : Node {
		private string value;
		public FunctionNode(string name) => this.value = name;
		public override string ToString() => value;
		public override string ToDisplayString() => $"(function {value}){base.ToDisplayString()}";

		/// <summary>Applys the stored function to the input value</summary>
		/// <param name="input">Input value</param>
		/// <param name="output">Output value</param>
		/// <returns>Whether the function could be applied to the input value</returns>
		public bool Apply(object input, out object output) {
			if(!Match(input.GetType(), out var function)) {
				output = null;
				return false;
			}

			output = function.Apply(input);
			return true;
		}

		/// <summary>Finds the method based on the input type</summary>
		/// <param name="type">Expected input type</param>
		/// <param name="function">Transform function</param>
		/// <returns>Whether a matching function could be found</returns>
		public bool Match(Type type, out Function function) {
			function = Features.Transforms.Get(value, type);
			return function != null;
		}
	}
}