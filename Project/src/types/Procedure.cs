using System;

namespace Formulas {
	/// <summary>Solves directly using a delegate</summary>
	public sealed class Procedure : IFormula {
		private Func<object[], object> value;

		/// <param name="function">Function to use for solving</param>
		public Procedure(Func<object[], object> function) => this.value = function;

		/// <param name="input">Inputs for the function</param>
		/// <returns>Solution to the function based on given inputs</returns>
		public object Solve(params object[] input) => value(input);
	}
}