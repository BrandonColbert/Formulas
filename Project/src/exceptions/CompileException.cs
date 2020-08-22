using System;

namespace Formulas {
	/// <summary>Error when a formula fails to compile</summary>
	public class CompileException : FormulaException {
		/// <param name="message">Message describing the error</param>
		public CompileException(string message) : base(message) {}

		/// <param name="message">Message describing the error</param>
		/// <param name="innerException">Exception causing the formula exception</param>
		public CompileException(string message, Exception innerException) : base(message, innerException) {}
	}
}