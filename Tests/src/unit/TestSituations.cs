using System;
using System.Collections.Generic;
using System.Numerics;
using Formulas;
using NUnit.Framework;

[TestFixture]
class TestSituations : FormulaTester {
	[TestCase(ExpectedResult=12)]
	public object Precedence1() => TimeSolve(TimeBuild("3 * 2^2"));

	[TestCase(542.2)]
	public void Precedence2(double expected) => Assert.AreEqual(expected, TimeSolve<double>(TimeBuild("(4 * 3|2 + 7|5 + 5 / (2 + 3)^2 + 2)")), Precision);

	[TestCase(ExpectedResult=7)]
	public object Precedence3() => TimeSolve(TimeBuild("f(b: int) = (4 - 2)b + 1"), 3);

	[TestCase(ExpectedResult=10)]
	public object Precedence4() => TimeSolve(TimeBuild("(3 + 2)(3 - 1)"));

	[Test]
	public void Precedence5() => Assert.AreEqual(-0.23077, TimeSolve<double>(TimeBuild($"2 * ((3 - 4) + (5 / (6 + 7))) - 1 + 8 % 3")), Precision);

	[Test]
	public void Precedence6() => Assert.AreEqual(4, TimeSolve<int>(TimeBuild("f(x: int, y: int) = 2 * (x + y) - x^3"), 2, 4), Precision);

	[TestCase(ExpectedResult=-1)]
	public object SingleValueGroup() => TimeSolve(TimeBuild("2 - (3)"));

	[TestCase(ExpectedResult=4)]
	public object MultipleSingleValueGroups() => TimeSolve(TimeBuild("f(c: int) = ((3)(c)) - (c)"), 2);

	[TestCase(ExpectedResult=4)]
	public object MultipleSingleValueMagnitudes() => TimeSolve(TimeBuild("f(c: double) = |(|3|)(|c|)| - |c|"), 2.0);

	[TestCase(ExpectedResult=1)]
	public object NegatedGroup1() => TimeSolve(TimeBuild("-(2 - (3))"));

	[TestCase(ExpectedResult=4)]
	public object NegatedGroup2() => TimeSolve(TimeBuild("-(2 - 2(3))"));

	[TestCase(ExpectedResult=-10)]
	public object NegatedMagnitude() => TimeSolve(TimeBuild("-|2 - (3)4|"));

	[TestCase(ExpectedResult=75)]
	public object AdjacentMultiplication1() => TimeSolve(TimeBuild("f(x: double) = 0.75x"), 100.0);

	[TestCase(ExpectedResult=130)]
	public object AdjacentMultiplication2() => TimeSolve(TimeBuild("f(z: float, b: float, a: float) = 3a1.1b2 - z"), 2f, 4f, 5f);

	[Test]
	public void AdjacentMultiplication3() => Assert.AreEqual(new Vector3(4, -8, 12), TimeSolve(TimeBuild("f(a: vec3, b: float) = 2a4b"), new Vector3(1, -2, 3), 0.5f));

	[Test]
	public void VectorMagnitude() => Assert.AreEqual(new Vector3(1, 2, 3).Length(), TimeSolve(TimeBuild("f(x: vec3) = |x|"), new Vector3(1, 2, 3)));

	[TestCase(2.2, 2, 1.1)]
	public void IntByDouble(double expected, int x, double y) => Assert.AreEqual(expected, TimeSolve<double>(TimeBuild("f(x: int, y: double) = x * y"), x, y), Precision);

	[TestCase(0.75f)]
	public void NumberByVector(float scalar) => Assert.AreEqual(new Vector3(1, 2, 3) * scalar, TimeSolve(TimeBuild($"f(x: vec3) = {scalar}x"), new Vector3(1, 2, 3)));

	[Test]
	public void CreateVector() => Assert.AreEqual(-Vector3.UnitX, TimeSolve(TimeBuild("f(v: vec3) = lvs(v.X)"), Vector3.UnitX));

	[Test]
	public void VectorMember() => Assert.AreEqual(4, TimeSolve(TimeBuild("f(s: vec3) = (s.X - s.Y)^2"), new Vector3(3, 1, 0)));

	[TestCase(13)]
	public void RepeatedIndex(int value) => Assert.AreEqual(value, TimeSolve(TimeBuild($"f(a: map) = a:b:c:d.e.f"), new Dictionary<string, object>(){
		["b"] = new Dictionary<string, object>{
			["c"] = new Dictionary<string, object>{
				["d"] = new{
					e = new{
						f = value
					}
				}
			}
		}
	}));

	[TestCase("b", 4, ExpectedResult=4)]
	public object IndexedMember(string key, int value) => TimeSolve(TimeBuild($"f(a: map) = a:{key}.value"), new Dictionary<string, object>(){
		[key] = new{
			value = value
		}
	});

	[TestCase(-1)]
	public void TransformIndexedMember(int value) => Assert.AreEqual(value, TimeSolve(TimeBuild($"f(a: map) = cos(a:b.c)"), new Dictionary<string, object>(){
		["b"] = new{
			c = (Number)Math.PI
		}
	}));

	[TestCase(4, ExpectedResult=4)]
	public object AnonymousMember(int value) => TimeSolve(TimeBuild($"a.Length"), new{Length = value});

	[Test]
	public void AnonymousIndexedMemberByVector() => Assert.AreEqual(Vector3.UnitX, TimeSolve(TimeBuild($"f(a: map) = a:b.c * rvs(1)"), new Dictionary<string, object>(){
		["b"] = new{
			c = 1
		}
	}));

	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest);
}

[TestFixture]
class TestCompileSituations : TestSituations {
	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest).Compile();
}