using Formulas;
using NUnit.Framework;

[TestFixture]
class TimeBasic : FormulaTester {
	const int count = 10000;

	[Test]
	public void Run() {
		var content = "f(x: int, y: int) = (134 + x) / (2x + y^4) * x^y - y * x";
		var input = new object[]{2, 4};

		for(var i = 0; i < count; i++)
			TimeBuild(content);

		var formula = Build(content);
		for(var i = 0; i < count; i++)
			TimeSolve(formula, input);
	}

	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest);
}

[TestFixture]
class TimeCompileBasic : TimeBasic {
	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest).Compile();
}