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

	/// <summary>Error when a formula fails to solve</summary>
	public class FormulaSolveException : FormulaException {
		/// <param name="formula">Formula that failed</param>
		/// <param name="index">Solution step</param>
		/// <param name="e">Nested exception</param>
		public FormulaSolveException(Formula formula, int index, System.Exception e) : base($"{e.Message}\n{Reason(formula, index)}", e) {}

		/// <param name="formula">Formula that failed</param>
		/// <param name="index">Solution step</param>
		public FormulaSolveException(Formula formula, int index) : base(Reason(formula, index)) {}

		/// <param name="message">Message describing the error</param>
		public FormulaSolveException(string message) : base(message) {}

		private static string Reason(Formula formula, int index) => string.Join(
				"\n",
				new []{
					formula.description,
					"Mappings",
					string.Join("\n", formula.mapping.Select(map => $"\t{map.Key} = {map.Value} | {map.Value?.GetType()}")),
					"Symbols",
					string.Join("\n", formula.symbols.Select((s, i) => $"\t'{s}': {s.GetType()}{((i >= index - 1 && index + 1 >= i) ? "\t<-" : string.Empty)}"))
				}
			);
	}

	/// <summary>Error when a formula fails to be parsed</summary>
	public class FormulaParseException : FormulaException {
		/// <param name="message">Message describing the error</param>
		public FormulaParseException(string message) : base(message) {}

		/// <param name="message">Message describing the error</param>
		/// <param name="innerException">Exception causing the formula exception</param>
		public FormulaParseException(string message, Exception innerException) : base(message, innerException) {}
	}

	/// <summary>Error when a formula fails to compile</summary>
	public class FormulaCompileException : FormulaException {
		/// <param name="message">Message describing the error</param>
		public FormulaCompileException(string message) : base(message) {}

		/// <param name="message">Message describing the error</param>
		/// <param name="innerException">Exception causing the formula exception</param>
		public FormulaCompileException(string message, Exception innerException) : base(message, innerException) {}
	}
}