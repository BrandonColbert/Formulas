using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Formulas {
	static class Glue {
		private const string OpImplicit = "op_Implicit";

		/// <param name="from">Original type</param>
		/// <param name="to">Desired type</param>
		/// <returns>Whether the first type can be casted to the second type</returns>
		public static bool Castable(Type from, Type to) {
			//Check can assign from into to
			if(to.IsAssignableFrom(from))
				return true;

			//Check if from implictly converts into to
			if(from.GetTypeInfo().GetMethod(OpImplicit, new[]{to}) != null)
				return true;

			//Check if to implictly converts into from
			if(to.GetTypeInfo().GetMethod(OpImplicit, new[]{from}) != null)
				return true;

			return false;
		}

		/// <summary>Attempts to coerce the given types into one's compatible for the specified operation</summary>
		/// <param name="op">Operation to undergo between the lhs and rhs types</param>
		/// <param name="lhs">Type on the left hand side of the operator</param>
		/// <param name="rhs">Type on the right hand side of the operator</param>
		/// <param name="leftType">Type that the left hand side is being coerced into</param>
		/// <param name="rightType">Type that the right hand side is being coerced into</param>
		/// <returns>Whether coercion is possible between the two types</returns>
		public static bool Coerce(string op, Type lhs, Type rhs, out Type leftType, out Type rightType) {
			bool Convertible(Type instigator, Type target, out Type requiredType) {
				foreach(var method in instigator.GetTypeInfo().GetDeclaredMethods(op)) {
					requiredType = method.GetParameters()[1].ParameterType;

					if(Castable(target, requiredType))
						return true;
				}

				requiredType = null;
				return false;
			}

			if(Convertible(lhs, rhs, out var result)) {
				leftType = lhs;
				rightType = result;
				return true;
			}

			if(Convertible(rhs, lhs, out result)) {
				leftType = result;
				rightType = rhs;
				return true;
			}

			leftType = null;
			rightType = null;
			return false;
		}

		/// <summary>The type who is calling a function is this library</summary>
		/// <remarks>Used to provide context dynamic compilation binding</remarks>
		public static Type GetContext() {
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