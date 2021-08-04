using System;

namespace Formulas {
	/// <summary>Describes a way to convert input values into an output value.</summary>
	public interface IFormula {
		/// <param name="input">Inputs for the function</param>
		/// <returns>Solution to the function based on given inputs</returns>
		object Solve(params object[] input);
	}

	/// <summary>Extensions of the IFormula interface</summary>
	public static class IFormulaExtensions {		
		/// <param name="formula">Formula instance</param>
		/// <param name="input">Inputs for the function</param>
		/// <typeparam name="T">Type to cast solution to</typeparam>
		/// <returns>Solution casted to T</returns>
		public static T Solve<T>(this IFormula formula, params object[] input) {
			var result = formula.Solve(input);

			if(result is T v)
				return v;

			try {
				return (T)System.Convert.ChangeType(result, typeof(T));
			} catch(InvalidCastException) {}

			throw new SolveException($"Solution '{result}' of type {result?.GetType().ToString() ?? "unknown"} could not be converted to {typeof(T)}");
		}
	}
}