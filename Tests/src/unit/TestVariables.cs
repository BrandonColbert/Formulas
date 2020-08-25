using Formulas;
using NUnit.Framework;

[TestFixture]
class TestVariables : FormulaTester {
	[TestCase(-1, ExpectedResult=-1)]
	[TestCase(0, ExpectedResult=0)]
	[TestCase(1, ExpectedResult=1)]
	public object OneInt(int a) => TimeSolve(TimeBuild("f(x: int) = x"), a);
	
	[TestCase(1, 2, ExpectedResult=3)]
	[TestCase(2, 1, ExpectedResult=3)]
	[TestCase(-1, -2, ExpectedResult=-3)]
	[TestCase(-2, -1, ExpectedResult=-3)]
	public object TwoInt(int a, int b) => TimeSolve(TimeBuild("f(x: int, y: int) = x + y"), a, b);

	[TestCase(1, 2, ExpectedResult=1)]
	[TestCase(2, 1, ExpectedResult=-1)]
	[TestCase(-1, -2, ExpectedResult=-1)]
	[TestCase(-2, -1, ExpectedResult=1)]
	public object TwoIntReversed(int a, int b) => TimeSolve(TimeBuild("f(y: int, x: int) = x - y"), a, b);

	[TestCase(1, "charlie", ExpectedResult=8)]
	[TestCase(-1, "susan", ExpectedResult=4)]
	public object OneIntOneString(int a, string b) => TimeSolve(TimeBuild("f(x: int, y: string) = x + y.Length"), a, b);

	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest);
}

[TestFixture]
class TestCompileVariables : TestVariables {
	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest).Compile();
}