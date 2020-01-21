using System;

namespace Formulas {
	/// <summary>An exception thrown when a formula fails</summary>
	public class FormulaException : Exception {
		/// <summary>New formula exception instance</summary>
		public FormulaException() : base() {}

		/// <param name="message">Message describing the error</param>
		public FormulaException(string message) : base(message) {}

		/// <param name="message">Message describing the error</param>
		/// <param name="innerException">Exception causing the formula exception</param>
		public FormulaException(string message, Exception innerException) : base(message, innerException) {}
	}
}