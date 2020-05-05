using Formulas;
using NUnit.Framework;

[TestFixture]
class TestCompiledFormula : TestFormula {
	protected override IFormula Build(string formula, params object[] inputs) => new Formula(formula, inputs).Compile();
}