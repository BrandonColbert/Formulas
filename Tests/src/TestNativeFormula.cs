using System.Numerics;
using Formulas;
using NUnit.Framework;

[TestFixture]
class TestNativeFormula : IFormulaTest {
	protected override IFormula Build(string formula, params object[] inputs) => NativeFormula.Compile<FormulaProvider, Number, Vector3, Quaternion>(formula, inputs);
}