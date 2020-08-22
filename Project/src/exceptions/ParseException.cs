using System;

namespace Formulas {
	/// <summary>Error when a formula fails to be parsed</summary>
	public class ParseException : FormulaException {
		/// <param name="message">Message describing the error</param>
		public ParseException(string message) : base(message) {}

		/// <param name="message">Message describing the error</param>
		/// <param name="innerException">Exception causing the formula exception</param>
		public ParseException(string message, Exception innerException) : base(message, innerException) {}
	}
}