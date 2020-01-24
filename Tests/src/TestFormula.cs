using System.Numerics;
using Formulas;
using NUnit.Framework;

[TestFixture]
class TestFormula : IFormulaTest {
	protected override IFormula Build(string formula, params object[] inputs) => new Formula<FormulaProvider, Number, Vector3, Quaternion>(formula, inputs);
}