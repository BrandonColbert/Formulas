using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Formulas {
	/// <summary>Manages usable types within formulas</summary>
	public class TypeMap {
		/// <summary>Whether formula type description should be deduced from any types in the loaded assemblies</summary>
		public bool typeDeduction = true;

		private Dictionary<string, Type> registry = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		private const string OpImplicit = "op_Implicit";

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

		/// <param name="from">Original type</param>
		/// <param name="to">Desired type</param>
		/// <returns>Whether the first type can be casted to the second type</returns>
		internal static bool Castable(Type from, Type to) =>
			to.IsAssignableFrom(from) || //Check can assign from into to
			from.GetTypeInfo().GetMethod(OpImplicit, new[]{to}) != null || //Check if from implictly converts into to
			to.GetTypeInfo().GetMethod(OpImplicit, new[]{from}) != null; //Check if to implictly converts into from

		/// <summary>Attempts to coerce the given types into one's compatible for the specified operation</summary>
		/// <param name="op">Operation to undergo between the lhs and rhs types</param>
		/// <param name="lhs">Type on the left hand side of the operator</param>
		/// <param name="rhs">Type on the right hand side of the operator</param>
		/// <param name="leftType">Type that the left hand side is being coerced into</param>
		/// <param name="rightType">Type that the right hand side is being coerced into</param>
		/// <returns>Whether coercion is possible between the two types</returns>
		internal static bool Coerce(string op, Type lhs, Type rhs, out Type leftType, out Type rightType) {
			bool Check(Type instigator, Type target, out Type requiredType) {
				foreach(var method in instigator.GetTypeInfo().GetDeclaredMethods(op)) {
					requiredType = method.GetParameters()[1].ParameterType;
					if(Castable(target, requiredType))
						return true;
				}

				requiredType = null;
				return false;
			}

			if(Check(lhs, rhs, out var result)) {
				leftType = lhs;
				rightType = result;
				return true;
			}

			if(Check(rhs, lhs, out result)) {
				leftType = result;
				rightType = rhs;
				return true;
			}

			leftType = null;
			rightType = null;
			return false;
		}
	}
}