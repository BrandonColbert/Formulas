using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Formulas {
	/// <summary>Stores the transformation functions that can be used within formulas</summary>
	public class Transforms : IEnumerable<MethodInfo> {
		private Dictionary<string, Dictionary<Type, Function>> registry = new Dictionary<string, Dictionary<Type, Function>>(StringComparer.OrdinalIgnoreCase);

		internal Function this[string name] {
			set {
				if(!registry.TryGetValue(name, out var flavors)) {
					flavors = new Dictionary<Type, Function>();
					registry.Add(name, flavors);
				}

				flavors[value.method.GetParameters().First().ParameterType] = value;
			}
		}

		/// <param name="name">Transform name</param>
		/// <param name="inputType">Input type</param>
		/// <returns>The corresponding transform</returns>
		public Function Get(string name, Type inputType) {
			if(registry.TryGetValue(name, out var flavors)) {
				if(flavors.TryGetValue(inputType, out var transform))
					return transform;

				foreach(var flavor in flavors)
					if(TypeMap.Castable(flavor.Key, inputType))
						return flavor.Value;
			}

			return flavors.FirstOrDefault().Value;
		}

		/// <summary>Applys the transform to the input value</summary>
		/// <param name="name">Transform name</param>
		/// <param name="input">Input value</param>
		/// <returns>Output value</returns>
		public object Apply(string name, object input) => Get(name, input.GetType()).Apply(input);

		/// <summary>Adds a transform function</summary>
		/// <param name="name">Transform name</param>
		/// <param name="function">Function defining its behavior</param>
		/// <typeparam name="T">Input type</typeparam>
		/// <typeparam name="TResult">Output type</typeparam>
		public void Add<T, TResult>(string name, Func<T, TResult> function) {
			if(!registry.TryGetValue(name, out var flavors)) {
				flavors = new Dictionary<Type, Function>();
				registry.Add(name, flavors);
			}

			flavors.Add(typeof(T), new Function{
				target = function.Target,
				method = function.Method
			});
		}

		/// <summary>
		/// Removes the transform with the name and input type
		/// 
		/// Removes all transforms with the name if the input type is unspecified
		/// </summary>
		/// <param name="name">Transform name</param>
		/// <param name="inputType">Input type or null</param>
		public void Remove(string name, Type inputType = null) {
			if(inputType != null) {
				if(registry.TryGetValue(name, out var flavors))
					flavors.Remove(inputType);
			} else
				registry.Remove(name);
		}

		/// <summary>Removes the transform with the name and input type</summary>
		/// <param name="name">Transform name</param>
		/// <typeparam name="T">Input type</typeparam>
		public void Remove<T>(string name) => Remove(name, typeof(T));

		/// <summary>The added methods</summary>
		public IEnumerator<MethodInfo> GetEnumerator() {
			foreach(var entry in registry)
				foreach(var flavor in entry.Value)
					yield return flavor.Value.method;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>Transform function</summary>
		public class Function {
			/// <summary>Method target</summary>
			public object target;

			/// <summary>Method info</summary>
			public MethodInfo method;

			/// <summary>Applys this transform to the input value</summary>
			/// <param name="input">Input value</param>
			/// <returns>Output value</returns>
			public object Apply(object input) => method.Invoke(target, new[]{Number.From(input, out var n) ? n : input});

			/// <summary>Deconstructor</summary>
			public void Deconstruct(out object target, out MethodInfo method) {
				target = this.target;
				method = this.method;
			}
		}

		/// <summary>Generic transform function</summary>
		/// <typeparam name="T">Input type</typeparam>
		/// <typeparam name="TResult">Output type</typeparam>
		public class Function<T, TResult> : Function {
			/// <summary>Input type</summary>
			public Type inputType;

			/// <summary>Output type</summary>
			public Type outputType;

			/// <param name="func">Transform function delegate</param>
			public Function(Func<T, TResult> func) {
				inputType = typeof(T);
				outputType = typeof(TResult);
				target = func.Target;
				method = func.Method;
			}

			/// <summary>Deconstructor</summary>
			public void Deconstruct(out object target, out MethodInfo method, out Type inputType, out Type outputType) {
				base.Deconstruct(out target, out method);
				inputType = this.inputType;
				outputType = this.outputType;
			}
		}
	}
}