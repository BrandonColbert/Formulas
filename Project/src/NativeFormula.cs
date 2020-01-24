using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;

namespace Formulas {
	/// <summary>Compiles specialized IFormula class</summary>
	public static class NativeFormula {
		static readonly string folder = "natforms";

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

		/// <param name="description">Description with formula notation</param>
		/// <param name="input">Initial formula inputs</param>
		/// <returns>A specialized IFormula compiled in the native scripting language</returns>
		public static IFormula Compile<Provider, Number, Vector, Quaternion>(string description, params object[] input) where Provider : IFormulaProvider<Number, Vector, Quaternion>, new() {
			//Destructure data into components
			var (symbols, order, mapping, inputCount) = new Provider().Build(description, input);

			//Construct solution function body
			var lines = new Queue<string>();
			for(var i = 0; i < order.Length; i++) {
				var index = order[i];

				Func<int, string> sof = j => {
					if(double.TryParse(symbols[j].ToString(), out var _))
						return $"(Number)({symbols[j].ToString()})";

					return $"(provider.ToNumber({symbols[j].ToString()}, out temp) ? temp : {symbols[j].ToString()})";
				};

				string result;
				string lhs = symbols[index - 1].ToString(), rhs = symbols[index + 1].ToString();
				var op = ((CharSymbol)symbols[index]).value;

				switch(op) {
					case '^': result = $"provider.Pow({sof(index - 1)}, {sof(index + 1)})"; break;
					case '*': case '/': case '%': case '+': case '-': result = $"{sof(index - 1)} {op} {sof(index + 1)}"; break;
					case '.': result = $"{lhs}.{rhs}"; break;
					case ':': result = $"{lhs}[\"{rhs}\"]"; break;
					case '@': result = $"provider.{lhs.First().ToString().ToUpper()}{lhs.Substring(1)}({rhs})"; break;
					default: throw new FormulaException("Operator '" + op + "' not supported");
				}

				lines.Enqueue($@"dynamic n{i} = {result};");
				symbols[index - 1] = $"n{i}";

				Array.Copy(symbols, index + 2, symbols, index, symbols.Length - 2 - index);
			}

			//Compile
			var formula = Compile($@"using Formulas;

public sealed class SpecializedFormula : IFormula {{
	private IFormulaProvider<{typeof(Number).FullName}, {typeof(Vector).FullName}, {typeof(Quaternion).FullName}> provider = new {typeof(Provider).FullName}();

	public string description {{ get; private set; }}
	public char[] variables {{ get; private set; }}
	{(inputCount > 0 ? $"public dynamic {string.Join(", ", mapping.Take(inputCount).Select(p => p.Key))};" : string.Empty)}

	public SpecializedFormula(string description, char[] variables, dynamic[] input) {{
		this.description = description;
		this.variables = variables;
		{string.Join("\n\t\t", mapping.Take(inputCount).Select((p, i) => $"{p.Key} = input[{i}];"))}
	}}

	public object Solve(dynamic[] input) {{
		{string.Join("\n\t\t", mapping.Skip(inputCount).Select((p, i) => $"var {p.Key} = input[{i}];"))}

		{typeof(Number).FullName} temp;

		{string.Join("\n\t\t", lines)}

		return {symbols[0]};
	}}
}}", "SpecializedFormula", new HashSet<string>{
				Assembly.GetAssembly(typeof(Provider)).Location,
				Assembly.GetAssembly(typeof(Number)).Location,
				Assembly.GetAssembly(typeof(Vector)).Location,
				Assembly.GetAssembly(typeof(Quaternion)).Location
			});

			return Activator.CreateInstance(
				formula,
				description,
				mapping.Select(v => v.Key).ToArray(),
				mapping.Take(inputCount).Select(p => p.Value).ToArray()
			) as IFormula;
		}

		/// <param name="source">C# source to compile</param>
		/// <param name="typename">Name of the desired type in the assembly</param>
		/// <param name="additionalAssemblies">Additional assemblies to include</param>
		/// <returns>The newly compiled type</returns>
		static Type Compile(string source, string typename, IEnumerable<string> additionalAssemblies) {
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
					.Concat(additionalAssemblies)
					.ToArray(), //AppDomain.CurrentDomain.GetAssemblies().Select(a => a.Location).ToArray(),
				fileName
			);

			parameters.GenerateExecutable = false;
			parameters.GenerateInMemory = false;

			var result = provider.CompileAssemblyFromSource(parameters, source);

			if(result.Errors.Count > 0) {
				var errors = new List<string>();

				foreach(CompilerError error in result.Errors)
					errors.Add($"Error ({error.ErrorNumber} at Line {error.Line}, Column: {error.Column}): {error.ErrorText}");

				throw new FormulaException($"{string.Join("\n", errors)}\n{string.Join("\n", source.Split('\n').Select((line, index) => $"{index + 1} {line}"))}");
			}

			//var builder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndCollect);
			//var module = builder.DefineDynamicModule(moduleName, fileName);

			// var domain = AppDomain.CreateDomain("Solver", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
			// foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
			// 	domain.Load(File.ReadAllBytes(assembly.Location));
			// domain.Load(File.ReadAllBytes(fileName));

			return Assembly.Load(File.ReadAllBytes(fileName)).GetType(typename);
		}
	}
}