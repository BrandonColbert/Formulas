using System;
using System.Collections.Generic;
using System.Text;

namespace Formulas {
	/// <summary>An exception thrown when a formula fails</summary>
	public class FormulaException : Exception {
		public FormulaException() : base() {}
		public FormulaException(string message) : base(message) {}
		public FormulaException(string message, Exception innerException) : base(message, innerException) {}

		public static string FormatMapping(Dictionary<char, object> mapping) {
			var sb = new StringBuilder("Mappings\n");
			foreach(var map in mapping)
				sb.Append("\t" + map.Key + " = " + map.Value + " | " + map.Value?.GetType() + "\n");

			return sb.ToString();
		}

		public static string FormatStatus(object[] symbols, int index) {
			var sb = new StringBuilder("Symbols\n");
			for(var i = 0; i < symbols.Length; i++) {
				var s = symbols[i];
				sb.Append("\t'" + s + "': " + s.GetType());
				if(i >= index - 1 && index + 1 >= i) sb.Append("\t<-");
				sb.Append("\n");
			}

			return sb.ToString();
		}
	}
}