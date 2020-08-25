using Formulas;
using NUnit.Framework;

[TestFixture]
class TestDeclarations : FormulaTester {
	[TestCase(1, 2, ExpectedResult=2)]
	[TestCase(2, 5, ExpectedResult=5)]
	public object Declare1(int x, int y) => TimeSolve(TimeBuild(
		"z = y - x",
		"f(x: int, y: int) = x + z"
	), x, y);

	[TestCase(1, 2, 3, ExpectedResult=10)]
	[TestCase(2, -4, 3, ExpectedResult=-15)]
	public object Declare2(int a, int b, int c) => TimeSolve(TimeBuild(
		"e = b * c - b",
		"d = a * e",
		"f(a: int, b: int, c: int) = a + b + c + d"
	), a, b, c);

	[TestCase(1, 2, 3, ExpectedResult=24)]
	public object Declare3(int a, int b, int c) => TimeSolve(TimeBuild(
		"e = abc",
		"v = sqrt|e + c|",
		"g = sgn(e)^(2v)",
		"f(a: int, b: int, c: int) = e * 2(v - a) * a^g"
	), a, b, c);

	[TestCase(1, 2, 3, ExpectedResult=6)]
	public object Declare3Ignore2(int a, int b, int c) => TimeSolve(TimeBuild(
		"e = abc",
		"v = sqrt|e + b|",
		"g = sgn(e)^(2v)",
		"f(a: int, b: int, c: int) = e"
	), a, b, c);

	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest);
}

[TestFixture]
class TestCompileDeclarations : TestDeclarations {
	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest).Compile();
}