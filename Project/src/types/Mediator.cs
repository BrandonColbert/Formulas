using System.Linq;

namespace Formulas {
	/// <summary>Solves a formula while taking into consideration predfined input</summary>
	public sealed class Mediator : IFormula {
		private IFormula formula;
		private object[] initialInput;

		/// <param name="formula">Formula to solve with</param>
		/// <param name="input">Initial inputs to use</param>
		public Mediator(IFormula formula, params object[] input) {
			this.formula = formula;			
			this.initialInput = input;
		}

		/// <param name="input">Inputs for the function</param>
		/// <returns>Solution to the function based on given inputs</returns>
		public object Solve(params object[] input) => formula.Solve(initialInput.Concat(input).ToArray());

		/// <returns>Mediated formula string</returns>
		public override string ToString() => formula.ToString();
	}
}