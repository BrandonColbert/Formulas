using Formulas;
using NUnit.Framework;

[TestFixture]
class TestFormulaCompile : TestFormula {
	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest).Compile();
}