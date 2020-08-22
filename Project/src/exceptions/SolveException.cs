using System;

namespace Formulas {
	/// <summary>Error when a formula fails to solve</summary>
	public class SolveException : FormulaException {
		/// <param name="message">Message describing the error</param>
		public SolveException(string message) : base(message) {}

		/// <param name="message">Message describing the error</param>
		/// <param name="innerException">Exception causing the formula exception</param>
		public SolveException(string message, Exception innerException) : base(message, innerException) {}
	}
}