namespace Formulas {
	/// <summary>Describes a way to convert input values into an output value.</summary>
	public interface IFormula {
		/// <summary>The original string provided as the formula</summary>
		string description { get; }

		/// <summary>Variables in the formula</summary>
		char[] variables { get; }

		/// <param name="input">Inputs for the function</param>
		/// <returns>Solution to the function based on given inputs</returns>
		object Solve(params object[] input);
	}

	public static class IFormulaExtensions {
		/// <param name="input">Inputs for the function</param>
		/// <typeparam name="T">Type to cast solution to</typeparam>
		/// <returns>Solution casted to T</returns>
		public static T Solve<T>(this IFormula formula, params object[] input) {
			var result = formula.Solve(input);

			if(result is T v)
				return v;

			throw new FormulaException("Solution '" + result + "' of type " + result?.ToString() + " could not be converted to " + typeof(T));
		}
	}
}