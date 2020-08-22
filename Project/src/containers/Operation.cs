using System.Collections.Generic;

namespace Formulas {
	/// <summary>Types of operations that can be done within a formula</summary>
	enum Operation {
		Add,
		Subtract,
		Multiply,
		Divide,
		Modulo,
		Negate,
		Power,
		Property,
		Index,
		Transform
	}

	/// <summary>Operators for operations in a formula</summary>
	static class Ops {
		public const char
			Add = '+', //Add
			Sub = '-', //Subtract
			Mul = '*', //Multiply
			Div = '/', //Divide
			Mod = '%', //Modulo
			Pow = '^', //Power
			Idx = ':', //Index
			Prp = '.', //Property
			Gpo = '(', //Group Open
			Gpc = ')', //Group Close
			Mag = '|'; //Magnitude

		/// <param name="value">Character to check</param>
		/// <returns>Whether the character is an operator</returns>
		public static bool Is(char value) => operators.Contains(value);

		/// <summary>Precedence relative to other operations</summary>
		public static int Precedence(this Operation op) => precedence[op];

		public static string MethodName(this Operation op) => methodNames[op];

		private static Dictionary<Operation, int> precedence = new Dictionary<Operation, int>() {
			[Operation.Add] = 0,
			[Operation.Subtract] = 0,
			[Operation.Multiply] = 1,
			[Operation.Divide] = 1,
			[Operation.Modulo] = 1,
			[Operation.Negate] = 2,
			[Operation.Power] = 3,
			[Operation.Property] = 4,
			[Operation.Index] = 4,
			[Operation.Transform] = 4
		};

		private static readonly HashSet<char> operators = new HashSet<char>(){
			Add, Sub,
			Mul, Div, Mod,
			Pow,
			Idx, Prp,
			Gpo, Gpc, Mag
		};
		
		private static Dictionary<Operation, string> methodNames = new Dictionary<Operation, string>(){
			[Operation.Add] = "op_Addition",
			[Operation.Subtract] = "op_Subtraction",
			[Operation.Multiply] = "op_Multiply",
			[Operation.Divide] = "op_Division",
			[Operation.Modulo] = "op_Modulus"
		};
	}
}