using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Formulas {
	/// <summary>Allows functions to be added and types to be enabled for formulas</summary>
	public static class Features {
		/// <summary>Whether formula type specification should be deduced from any types in the loaded assemblies</summary>
		public static bool deduceTypeFromAssemblies = true;
		internal static Func<Number, Number, Number> pow = (l, r) => Math.Pow(l, r);
		internal static readonly HashSet<string> functionNames = new HashSet<string>();
		private static Dictionary<string, List<(object, MethodInfo)>> functions = new Dictionary<string, List<(object, MethodInfo)>>();
		private static Dictionary<string, Type> typemap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		private const string OpImplicit = "op_Implicit";

		static Features() => Features.AddFunction("abs", new Func<Number, Number>(v => Math.Abs(v)));

		/// <param name="name">Typename</param>
		/// <returns>Type for the given typename</returns>
		public static Type FindType(string name) {
			if(typemap.TryGetValue(name, out var type))
				return type;
			else if(deduceTypeFromAssemblies) {
				var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(d => d.GetTypes());

				var matches = (name.Contains(".") ? types.Where(t => t.FullName.ToLower() == name.ToLower()) : types.Where(t => t.Name.ToLower() == name.ToLower())).ToArray();

				if(matches.Length == 0)
					throw new Exception($"Type not found for variable '{name}'");
				else if(matches.Length > 1)
					throw new Exception($"Type for variable '{name}' ambiguous between {string.Join(", ", matches.Select(t => t.FullName))}");

				return matches[0];
			} else
				return null;
		}

		/// <summary>Enable a type to be explicity used in formulas</summary>
		/// <param name="type">The type</param>
		/// <param name="names">Aliases that will be recognized as the type</param>
		public static void EnableType(Type type, params string[] names) {
			foreach(var name in names)
				typemap[name] = type;
		}

		/// <summary>Disables a type from being used in formulas</summary>
		/// <param name="type">The type</param>
		public static void DisableType(Type type) => typemap = typemap.Where(p => p.Value != type).ToDictionary(e => e.Key, e => e.Value);

		/// <summary>Add a function that formulas can use</summary>
		/// <param name="name">Name of the function</param>
		/// <param name="function">Function delegate</param>
		/// <typeparam name="T">Input type</typeparam>
		/// <typeparam name="TResult">Output type</typeparam>
		public static void AddFunction<T, TResult>(string name, Func<T, TResult> function) {
			name = name.ToLower();

			if(!functions.TryGetValue(name, out var v)) {
				functionNames.Add(name);
				functions[name] = v = new List<(object, MethodInfo)>();
			}

			v.Add((function.Target, function.Method));
		}

		/// <summary>Remove a function</summary>
		/// <param name="name">Name of the function</param>
		public static void RemoveFunctions(string name) {
			name = name.ToLower();
			functionNames.Remove(name);
			functions.Remove(name);
		}

		/// <summary>Executes an added function</summary>
		/// <param name="name">Function name</param>
		/// <param name="value">Function input</param>
		/// <returns>Function output</returns>
		public static object Function(string name, object value) {
			var (target, func) = MatchFunction(name, value.GetType());
			// Console.Error.WriteLine($"For {name} {functions[name].Count}: {op?.ToString() ?? "null"}");
			return func.Invoke(target, new[]{Number.From(value, out var n) ? n : value});
		}

		internal static (object, MethodInfo) MatchFunction(string name, Type type) {
			var methods = functions[name];

			switch(methods.Count) {
				case 0: return (null, null);
				case 1: return methods[0];
				default:
					foreach(var method in methods)
						if(Castable(type, method.Item2.GetParameters().First().ParameterType))
							return method;

					return methods[0];
			}
		}

		internal static bool Castable(Type from, Type to) =>
			to.IsAssignableFrom(from) || //Check can assign from into to
			from.GetTypeInfo().GetMethod(OpImplicit, new[]{to}) != null || //Check if from specify conversion into to
			to.GetTypeInfo().GetMethod(OpImplicit, new[]{from}) != null; //Check if to specify conversion into from

		internal static (Type, Type) CompatibleType(Type lhs, string op, Type rhs) {
			Type Method(Type a, Type b) {
				// Console.Error.WriteLine($"{a.FullName} - {op}");
				foreach(var method in a.GetTypeInfo().GetDeclaredMethods(op)) {
					var type = method.GetParameters()[1].ParameterType;
					var castable = Castable(b, type);
					// Console.Error.WriteLine($"\t{b} -> {type} = {castable}");

					if(castable)
						return type;
				}

				return null;
			}

			Type result;

			if((result = Method(lhs, rhs)) != null)
				return (lhs, result);
			else if((result = Method(rhs, lhs)) != null)
				return (result, rhs);

			return (null, null);
		}
	}
}