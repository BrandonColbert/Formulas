using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;

namespace Formulas {
	public static class NativeFormula {
		static readonly string folder ="natforms";

		static NativeFormula() {
			//Create directory to store compiled formulas
			if(!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
		}

		/// <summary>Deletes the compiled formulas</summary>
		public static void Cleanup() {
			if(Directory.Exists(folder))
				Directory.Delete(folder, true);
		}

		/// <param name="other">Formula to compile from</param>
		/// <returns>Compiles the formula into a direct formula</returns>
		public static IFormula CompileDirect(Formula other, (object[], int[], List<KeyValuePair<char, object>>, int) data) {
			//Copy description and variables
			var description = String.Copy(other.description);
			var variables = other.variables.ToArray();

			//Destructure data into components
			var (symbols, order, mappings, inputCount) = data;
			symbols = symbols.ToArray();

			//Construct solution function body
			foreach(var index in order) {
				symbols[index - 1] = $@"({Express(
					symbols[index - 1].ToString(),
					((CharSymbol)symbols[index]).value,
					symbols[index + 1].ToString()
				)})";

				Array.Copy(symbols, index + 2, symbols, index, symbols.Length - 2 - index);
			}

			//Compile
			var solver = Compile($@"
				using Formulas;

				public static class Solver {{
					{(inputCount > 0 ? $"public static dynamic {string.Join(", ", mappings.Take(inputCount).Select(p => p.Key))};" : string.Empty)}

					public static void Init(dynamic[] input) {{
						{string.Join("\n", mappings.Take(inputCount).Select((p, i) => $"{p.Key} = input[{i}];"))}
					}}

					public static object Solve(dynamic[] input) {{
						{string.Join("\n", mappings.Skip(inputCount).Select((p, i) => $"var {p.Key} = input[{i}];"))}

						return {symbols[0].ToString()};
					}}
				}}
			", "Solver");
			
			//Get functions from compilation
			var init = Delegate.CreateDelegate(typeof(Action<dynamic[]>), solver.GetMethod("Init")) as Action<dynamic[]>;
			var solve = Delegate.CreateDelegate(typeof(Func<dynamic[], object>), solver.GetMethod("Solve")) as Func<dynamic[], object>;

			//Update solver using compiled formula
			init(mappings.Take(inputCount).Select(p => p.Value).ToArray());

			return new DirectFormula(solve);
		}

		/// <param name="other">Formula to compile</param>
		/// <returns>Compiles the formula into a custom IFormula class</returns>
		public static IFormula Compile(Formula other, (object[], int[], List<KeyValuePair<char, object>>, int) data) {
			//Destructure data into components
			var (symbols, order, mappings, inputCount) = data;
			symbols = symbols.ToArray();

			//Construct solution function body
			foreach(var index in order) {
				symbols[index - 1] = $@"({Express(
					symbols[index - 1].ToString(),
					((CharSymbol)symbols[index]).value,
					symbols[index + 1].ToString()
				)})";

				Array.Copy(symbols, index + 2, symbols, index, symbols.Length - 2 - index);
			}

			//Compile
			var formula = Compile($@"
				using Formulas;

				public sealed class NFormula : IFormula {{
					public string description {{ get; private set; }}
					public char[] variables {{ get; private set; }}
					{(inputCount > 0 ? $"public dynamic {string.Join(", ", mappings.Take(inputCount).Select(p => p.Key))};" : string.Empty)}

					public NFormula(string description, char[] variables, dynamic[] input) {{
						this.description = description;
						this.variables = variables;
						{string.Join("\n", mappings.Take(inputCount).Select((p, i) => $"{p.Key} = input[{i}];"))}
					}}

					public object Solve(dynamic[] input) {{
						{string.Join("\n", mappings.Skip(inputCount).Select((p, i) => $"var {p.Key} = input[{i}];"))}

						return {symbols[0].ToString()};
					}}
				}}
			", "NFormula");

			return Activator.CreateInstance(
				formula,
				String.Copy(other.description),
				other.variables.ToArray(),
				mappings.Take(inputCount).Select(p => p.Value).ToArray()
			) as IFormula;
		}

		static string Express(string lhs, char op, string rhs) {
			switch(op) {
				case '^': return $"(float)Math.Pow({lhs}, {rhs})";
				case '*': return $"NativeFormula.Multiply({lhs}, {rhs})";
				case '/': return $"NativeFormula.Divide({lhs}, {rhs})";
				case '%': return $"NativeFormula.Modulus({lhs}, {rhs})";
				case '+': return $"NativeFormula.Add({lhs}, {rhs})";
				case '-': return $"NativeFormula.Subtract({lhs}, {rhs})";
				//case '*': case '/': case '%': case '+': case '-': return $"{lhs} {op} {rhs}";
				case '.': return $"{lhs}.{rhs}";
				case ':': return $"{lhs}[\"{rhs}\"]";
				case '@': return functions[lhs](rhs);
				default: throw new FormulaException("Operator '" + op + "' not supported");
			}
		}

		static Dictionary<string, Func<string, string>> functions = new Dictionary<string, Func<string, string>>() {
			["sin"] = v => $"Formulizer.Provider.Sin({v})",
			["asin"] = v => $"Formulizer.Provider.Asin({v})",
			["cos"] = v => $"Formulizer.Provider.Cos({v})",
			["acos"] = v => $"Formulizer.Provider.Acos({v})",
			["tan"] = v => $"Formulizer.Provider.Tan({v})",
			["atan"] = v => $"Formulizer.Provider.Atan({v})",
			["sqrt"] = v => $"Formulizer.Provider.Sqrt({v})",
			["ln"] = v => $"Formulizer.Provider.Ln({v})",
			["log"] = v => $"Formulizer.Provider.Log({v})",
			["sgn"] = v => $"Formulizer.Provider.Sgn({v})",
			["rvs"] = v => $"Formulizer.Provider.Rvs({v})",
			["lvs"] = v => $"Formulizer.Provider.Lvs({v})",
			["uvs"] = v => $"Formulizer.Provider.Uvs({v})",
			["dvs"] = v => $"Formulizer.Provider.Dvs({v})",
			["fvs"] = v => $"Formulizer.Provider.Fvs({v})",
			["bvs"] = v => $"Formulizer.Provider.Bvs({v})",
			["rnd"] = v => $"Formulizer.Provider.Rnd({v})",
			["abs"] = v => $"NativeFormula.Abs({v})",
			["nml"] = v => $"NativeFormula.Normalize({v})",
			["qtn"] = v => $"Formulizer.Provider.Qtn({v})",
			["vec"] = v => $"Formulizer.Provider.Vec({v})"
		};

		/// <param name="source">C# source to compile</param>
		/// <returns>The created type</returns>
		static Type Compile(string source, string typename) {
			var id = Guid.NewGuid().ToString();
			var fileName = $"{folder}/formula_{id}";
			// var assemblyName = $"assembly_{id}";
			// var moduleName = $"module_{id}";
			// var domainName = $"domain_{id}";

			var provider = new CSharpCodeProvider();
			var parameters = new CompilerParameters(
				Assembly.GetExecutingAssembly().GetReferencedAssemblies()
					.Select(a => Assembly.Load(a).Location)
					.Concat(new []{Assembly.GetExecutingAssembly().Location})
					.ToArray(), //AppDomain.CurrentDomain.GetAssemblies().Select(a => a.Location).ToArray(),
				fileName
			);
			parameters.GenerateExecutable = false;
			parameters.GenerateInMemory = false;

			var result = provider.CompileAssemblyFromSource(parameters, source);

			if(result.Errors.Count > 0) {
				var errors = new List<string>();

				foreach(CompilerError error in result.Errors)
					errors.Add($"Error ({error.ErrorNumber}): {error.ErrorText}");

				throw new FormulaException(string.Join("\n", errors));
			}

			//var builder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndCollect);
			//var module = builder.DefineDynamicModule(moduleName, fileName);

			// var domain = AppDomain.CreateDomain("Solver", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
			// foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
			// 	domain.Load(File.ReadAllBytes(assembly.Location));
			// domain.Load(File.ReadAllBytes(fileName));

			return Assembly.Load(File.ReadAllBytes(fileName)).GetType(typename);
		}

		public static dynamic Abs(dynamic v) => Formulizer.Provider.Abs(float.TryParse(v.ToString(), out float f) ? f : v);
		public static dynamic Normalize(dynamic v) => Formulizer.Provider.Nml(float.TryParse(v.ToString(), out float f) ? f : v);

		public static dynamic Multiply(dynamic lhs, dynamic rhs) => lhs * rhs;
		public static dynamic Multiply(double lhs, dynamic rhs) => (float)lhs * rhs;
		public static dynamic Multiply(dynamic lhs, double rhs) => lhs * (float)rhs;
		public static dynamic Multiply(double lhs, double rhs) => (float)lhs * (float)rhs;

		public static dynamic Divide(dynamic lhs, dynamic rhs) => lhs / rhs;
		public static dynamic Divide(double lhs, dynamic rhs) => (float)lhs / rhs;
		public static dynamic Divide(dynamic lhs, double rhs) => lhs / (float)rhs;
		public static dynamic Divide(double lhs, double rhs) => (float)lhs / (float)rhs;

		public static dynamic Add(dynamic lhs, dynamic rhs) => lhs + rhs;
		public static dynamic Add(double lhs, dynamic rhs) => (float)lhs + rhs;
		public static dynamic Add(dynamic lhs, double rhs) => lhs + (float)rhs;
		public static dynamic Add(double lhs, double rhs) => (float)lhs + (float)rhs;

		public static dynamic Subtract(dynamic lhs, dynamic rhs) => lhs - rhs;
		public static dynamic Subtract(double lhs, dynamic rhs) => (float)lhs - rhs;
		public static dynamic Subtract(dynamic lhs, double rhs) => lhs - (float)rhs;
		public static dynamic Subtract(double lhs, double rhs) => (float)lhs - (float)rhs;

		public static dynamic Modulus(dynamic lhs, dynamic rhs) => lhs % rhs;
		public static dynamic Modulus(double lhs, dynamic rhs) => (float)lhs % rhs;
		public static dynamic Modulus(dynamic lhs, double rhs) => lhs % (float)rhs;
		public static dynamic Modulus(double lhs, double rhs) => (float)lhs % (float)rhs;
	}
}