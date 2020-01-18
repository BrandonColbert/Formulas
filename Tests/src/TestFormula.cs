using Formulas;
using NUnit.Framework;

[TestFixture]
class TestFormula : IFormulaTest {
	protected override IFormula Build(string formula, params object[] inputs) => new Formula(formula, inputs);
}