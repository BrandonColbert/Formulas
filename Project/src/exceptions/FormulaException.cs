using System;
using System.Linq;

namespace Formulas {
	/// <summary>Error when some stage of a formula fails</summary>
	public abstract class FormulaException : Exception {
		/// <param name="message">Message describing the error</param>
		public FormulaException(string message) : base(message) {}

		/// <param name="message">Message describing the error</param>
		/// <param name="innerException">Exception causing the formula exception</param>
		public FormulaException(string message, Exception innerException) : base(message, innerException) {}
	}
}