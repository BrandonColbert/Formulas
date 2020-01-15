using System;
using System.Collections.Generic;

namespace Formulas {
	/// <summary>Helper class for formulas</summary>
	public static class Formulizer {
		/// <summary>The current provider that formulas will use to operate with</summary>
		public static IFormulaProvider Provider { get; set; } = new DefaultFormulaProvider();

		//Cache of all the functions methods
		internal static HashSet<string> functions { get; } = new HashSet<string>() {
			"sin", "asin", "cos", "acos", "tan", "atan",
			"sqrt", "ln", "log", "sgn", "rnd",
			"abs", "nml",
			"rvs", "lvs", "uvs", "dvs", "fvs", "bvs",
			"qtn", "vec"
		};

		/// <summary>Checks whether a formula is valid for interpretation</summary>
		/// <returns>Valdity of the formula and potential error message</returns>
		public static (bool, string) Validate(string formula) => throw new NotImplementedException();
	}
}