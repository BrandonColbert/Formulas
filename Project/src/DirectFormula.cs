using System;

namespace Formulas {
	/// <summary>Solves directly using a delegate</summary>
	public sealed class DirectFormula : IFormula {
		private Func<object[], object> solver;

		/// <param name="solver">Function to use for solving</param>
		public DirectFormula(Func<object[], object> solver) => this.solver = solver;

		/// <param name="input">Inputs for the function</param>
		/// <returns>Solution to the function based on given inputs</returns>
		public object Solve(params object[] input) => solver(input);
	}
}