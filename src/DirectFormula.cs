using System;

namespace Formulas {
	/// <summary>Solves directly using a delegate</summary>
	public sealed class DirectFormula : IFormula {
		public string description { get; private set; } = string.Empty;
		public char[] variables { get; private set; } = new char[0];
		private Func<object[], object> solver;
		public DirectFormula(Func<object[], object> solver) => this.solver = solver;
		public object Solve(params object[] input) => solver(input);
	}
}