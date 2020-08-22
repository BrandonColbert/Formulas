using System;
using System.Collections.Generic;
using System.Linq;

namespace Formulas {
	/// <summary>Specifies formula qualities</summary>
	public class Description {
		/// <summary>Name of the formula</summary>
		public readonly string name;

		/// <summary>List of variables in order specified upon creation</summary>
		public readonly List<string> variables;

		/// <summary>Mapping between variables and their types</summary>
		public readonly Dictionary<string, Type> types;

		/// <param name="name">Formula name</param>
		public Description(string name) : this(name, new List<string>(), new Dictionary<string, Type>()) {}

		/// <param name="name">Formula name</param>
		/// <param name="variables">Formula variables</param>
		/// <param name="types">Formula variable types</param>
		public Description(string name, List<string> variables, Dictionary<string, Type> types) {
			this.name = name;
			this.variables = variables;
			this.types = types;
		}

		/// <returns>User friendly string</returns>
		public override string ToString() => $"{name}({string.Join(", ", types.Select(v => v.Value == typeof(object) ? v.Key : $"{v.Key}: {Parser.GetTypename(v.Value)}"))})";
	}
}