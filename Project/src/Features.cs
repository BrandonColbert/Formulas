using System;
using System.Diagnostics;
using System.Reflection;
using static Formulas.Transforms;

namespace Formulas {
	/// <summary>Allows functions to be added and types to be enabled for formulas</summary>
	public static class Features {
		/// <summary>Transform function registry</summary>
		public static Transforms Transforms { get; } = new Transforms{
			[MagnitudeNode.TransformName] = new Function<Number, Number>(v => Math.Abs(v))
		};

		/// <summary>Types registry</summary>
		public static TypeMap Types { get; } = new TypeMap();

		/// <summary>The type who is calling a function is this library</summary>
		/// <remarks>Used to provide context dynamic compilation binding</remarks>
		internal static Type GetContext() {
			var depth = 0;
			var context = typeof(object);
			var assembly = Assembly.GetExecutingAssembly();

			//Backtrack through callers to leave this assembly
			do {
				var frame = new StackFrame(depth++, false);
				context = frame.GetMethod().DeclaringType;
			} while(context.Assembly == assembly);

			return context;
		}
	}
}