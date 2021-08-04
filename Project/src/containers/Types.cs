using System;
using System.Collections.Generic;
using System.Linq;

namespace Formulas {
	/// <summary>Manages usable types within formulas</summary>
	public class Types {
		/// <summary>Whether formula type description should be deduced from any types in the loaded assemblies</summary>
		public bool typeDeduction = true;

		private Dictionary<string, Type> registry = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

		/// <param name="name">Typename</param>
		/// <param name="type">Type for the given typename</param>
		/// <returns>Whether a matching type was found</returns>
		public bool Find(string name, out Type type) {
			if(registry.TryGetValue(name, out type))
				return true;

			if(typeDeduction) {
				var fullName = name.Contains(".");
				var matches = AppDomain
					.CurrentDomain
					.GetAssemblies()
					.SelectMany(d => d.GetTypes())
					.Where(t => Parser.GetTypename(t, fullName) == name);

				if(matches.Count() == 1) {
					type = matches.First();
					return true;
				}
			}

			return false;
		}

		/// <summary>Enable a type to be explicity used in formulas</summary>
		/// <param name="type">The type</param>
		/// <param name="names">Aliases that will be recognized as the type</param>
		public void Enable(Type type, params string[] names) {
			foreach(var name in names)
				registry[name] = type;
		}

		/// <summary>Enable a type to be explicity used in formulas</summary>
		/// <param name="names">Aliases that will be recognized as the type</param>
		/// <typeparam name="T">The type</typeparam>
		public void Enable<T>(params string[] names) => Enable(typeof(T), names);

		/// <summary>Disables a type from being used in formulas</summary>
		/// <param name="type">The type</param>
		public void Disable(Type type) => registry = registry.Where(p => p.Value != type).ToDictionary(e => e.Key, e => e.Value);

		/// <summary>Disables a type from being used in formulas</summary>
		/// <typeparam name="T">The type</typeparam>
		public void Disable<T>() => Disable(typeof(T));
	}
}