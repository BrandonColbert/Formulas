using System.Collections.Generic;
using Formulas;
using NUnit.Framework;

[TestFixture]
class TestOperations : FormulaTester {
	[TestCase(1, 1, ExpectedResult=2)]
	[TestCase(15, 13, ExpectedResult=28)]
	[TestCase(48, 2048, ExpectedResult=2096)]
	[TestCase(3.14f, 3.14f, ExpectedResult=6.28f)]
	public double Add(float a, float b) => TimeSolve<float>(TimeBuild($"{a} + {b}"));

	[TestCase(2, 1, ExpectedResult=1)]
	[TestCase(1, 2, ExpectedResult=-1)]
	[TestCase(3, 3, ExpectedResult=0)]
	public float Subtract(float a, float b) => TimeSolve<float>(TimeBuild($"{a} - {b}"));

	[TestCase(2, 3, ExpectedResult=6)]
	[TestCase(12, 5, ExpectedResult=60)]
	public float Multiply(float a, float b) => TimeSolve<float>(TimeBuild($"{a} * {b}"));
	
	[TestCase(6, 2, ExpectedResult=3)]
	[TestCase(15, 3, ExpectedResult=5)]
	[TestCase(1, 4, ExpectedResult=0.25)]
	public float Divide(float a, float b) => TimeSolve<float>(TimeBuild($"{a} / {b}"));
	
	[TestCase(6, 5, ExpectedResult=1)]
	[TestCase(4, 5, ExpectedResult=4)]
	[TestCase(19, 7, ExpectedResult=5)]
	public float Modulus(float a, float b) => TimeSolve<float>(TimeBuild($"{a} % {b}"));

	[TestCase(2, 3, ExpectedResult=8)]
	[TestCase(3, 3, ExpectedResult=27)]
	[TestCase(182, 2, ExpectedResult=33124)]
	public float Exponentiate(float a, float b) => TimeSolve<float>(TimeBuild($"{a}^{b}"));
	
	[TestCase(1, ExpectedResult=-1)]
	[TestCase(2.5f, ExpectedResult=-2.5f)]
	[TestCase(88, ExpectedResult=-88)]
	[TestCase(0, ExpectedResult=0)]
	public float Negate(float a) => TimeSolve<float>(TimeBuild($"-{a}"));

	[TestCase("abc", "Length", ExpectedResult=3)]
	[TestCase("c-3po", "Length", ExpectedResult=5)]
	public object MemberAccess(string a, string b) => TimeSolve(TimeBuild($"f(a: string) = a.{b}"), a);

	[TestCase("b", ExpectedResult=-22)]
	[TestCase("cat", ExpectedResult="meow")]
	[TestCase("w00f", ExpectedResult="dog")]
	public object IndexAccess(string key) => TimeSolve(TimeBuild($"f(a: map) = a:{key}"), new Dictionary<string, object>(){
		["b"] = -22,
		["cat"] = "meow",
		["w00f"] = "dog"
	});
	
	[TestCase(0, ExpectedResult="Peter")]
	[TestCase(1, ExpectedResult="Sally")]
	[TestCase(2, ExpectedResult="Bob")]
	public object ArrayAccess(int key) => TimeSolve(TimeBuild($"f(a: array) = a:{key}"), new[]{"Peter", "Sally", "Bob"} as object);

	[TestCase(3, 1, 2, ExpectedResult=9)]
	[TestCase(2, 4, 1, ExpectedResult=10)]
	[TestCase(0.5f, 3, 3, ExpectedResult=3f)]
	public object Grouping(float a, float b, float c) => TimeSolve(TimeBuild($"{a} * ({b} + {c})"));

	[TestCase(1, 2, ExpectedResult=1)]
	[TestCase(2, 1, ExpectedResult=1)]
	[TestCase(3, 3, ExpectedResult=0)]
	public object Magnitude(float a, float b) => TimeSolve(TimeBuild($"|{a} - {b}|"));

	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest);
}

[TestFixture]
class TestCompileOperations : TestOperations {
	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest).Compile();
}