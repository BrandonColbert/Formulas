using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Formulas;
using NUnit.Framework;

[TestFixture]
class TestFormula {
	//Operations
	[TestCase(3, 1, 2, ExpectedResult=9)] public object Op_Grouping(int a, int b, int c) => TimeSolve(TimeBuild($"{a} * ({b} + {c})"));
	[TestCase(1, 2, ExpectedResult=1)] public object Op_Magnitude(int a, int b) => TimeSolve(TimeBuild($"|{a} - {b}|"));
	[TestCase(2, 3, ExpectedResult=8)] public object Op_Exponentiate(int a, int b) => TimeSolve(TimeBuild($"{a}^{b}"));
	[TestCase(2, 3, ExpectedResult=6)] public object Op_Multiply(int a, int b) => TimeSolve(TimeBuild($"{a} * {b}"));
	[TestCase(6, 2, ExpectedResult=3)] public object Op_Divide(int a, int b) => TimeSolve(TimeBuild($"{a} / {b}"));
	[TestCase(6, 5, ExpectedResult=1)] public object Op_Modulus(int a, int b) => TimeSolve(TimeBuild($"{a} % {b}"));
	[TestCase(1, 1, ExpectedResult=2)] public object Op_Add(int a, int b) => TimeSolve(TimeBuild($"{a} + {b}"));
	[TestCase(2, 1, ExpectedResult=1)] public object Op_Subtract(int a, int b) => TimeSolve(TimeBuild($"{a} - {b}"));
	[TestCase(1, ExpectedResult=-1)] public object Op_Negate(int a) => TimeSolve(TimeBuild("f(a: int) = -a"), a);
	[TestCase("b", 1, ExpectedResult=1)] public object Op_IndexAccess(string key, int value) => TimeSolve(TimeBuild($"f(a: map) = a:{key}"), new Dictionary<string, object>(){[key] = value});
	[TestCase("abc", "Length", ExpectedResult=3)] public object Op_MemberAccess(string a, string b) => TimeSolve(TimeBuild($"f(a: string) = a.{b}"), a);

	//Input
	[TestCase(1, ExpectedResult=1)] public object No_Input(int a) => TimeSolve(TimeBuild($"{a}"));
	[TestCase(1, ExpectedResult=1)] public object Single_PreInput(int a) => TimeSolve(TimeBuild("f(x: int) = x", a));
	[TestCase(1, ExpectedResult=1)] public object Single_PostInput(int a) => TimeSolve(TimeBuild("f(x: int) = x"), a);
	[TestCase(1, 2, ExpectedResult=3)] public object Single_PreAndPostInput(int a, int b) => TimeSolve(TimeBuild("f(x: int, y: int) = x + y", a), b);
	[TestCase(1, 2, ExpectedResult=3)] public object Multiple_PreInput(int a, int b) => TimeSolve(TimeBuild("f(x: int, y: int) = x + y", a, b));
	[TestCase(1, 2, ExpectedResult=3)] public object Multiple_PostInput(int a, int b) => TimeSolve(TimeBuild("f(x: int, y: int) = x + y"), a, b);
	[TestCase(1, ExpectedResult=1)] public object Mapped_Single_PreInput(int a) => TimeSolve(TimeBuild("f(x: int) = x", a));
	[TestCase(1, ExpectedResult=1)] public object Mapped_Single_PostInput(int a) => TimeSolve(TimeBuild("f(x: int) = x"), a);
	[TestCase(1, 2, ExpectedResult=3)] public object Mapped_Multiple_PreInput(int a, int b) => TimeSolve(TimeBuild("f(x: int, y: int) = x + y", a, b));
	[TestCase(1, 2, ExpectedResult=3)] public object Mapped_Multiple_PostInput(int a, int b) => TimeSolve(TimeBuild("f(x: int, y: int) = x + y"), a, b);

	//Functions
	[TestCase(1)] public void Sin(float v) => Assert.AreEqual(Features.Function("sin", v), TimeSolve(TimeBuild($"sin({v})")));
	[TestCase(1)] public void Asin(float v) => Assert.AreEqual(Features.Function("asin", v), TimeSolve(TimeBuild($"asin({v})")));
	[TestCase(1)] public void Cos(float v) => Assert.AreEqual(Features.Function("cos", v), TimeSolve(TimeBuild($"cos({v})")));
	[TestCase(1)] public void Acos(float v) => Assert.AreEqual(Features.Function("acos", v), TimeSolve(TimeBuild($"acos({v})")));
	[TestCase(1)] public void Tan(float v) => Assert.AreEqual(Features.Function("tan", v), TimeSolve(TimeBuild($"tan({v})")));
	[TestCase(1)] public void Atan(float v) => Assert.AreEqual(Features.Function("atan", v), TimeSolve(TimeBuild($"atan({v})")));
	[TestCase(16)] public void Sqrt(float v) => Assert.AreEqual(Features.Function("sqrt", v), TimeSolve(TimeBuild($"sqrt({v})")));
	[TestCase(2)] public void Ln(float v) => Assert.AreEqual(Features.Function("ln", v), TimeSolve(TimeBuild($"ln({v})")));
	[TestCase(2)] public void Log(float v) => Assert.AreEqual(Features.Function("log", v), TimeSolve(TimeBuild($"log({v})")));
	[TestCase(1)] public void Sgn(float v) => Assert.AreEqual(Features.Function("sgn", v), TimeSolve(TimeBuild($"sgn({v})")));
	[TestCase(1)] public void Rvs(float v) => Assert.AreEqual(Features.Function("rvs", v), TimeSolve(TimeBuild($"rvs({v})")));
	[TestCase(1)] public void Lvs(float v) => Assert.AreEqual(Features.Function("lvs", v), TimeSolve(TimeBuild($"lvs({v})")));
	[TestCase(1)] public void Uvs(float v) => Assert.AreEqual(Features.Function("uvs", v), TimeSolve(TimeBuild($"uvs({v})")));
	[TestCase(1)] public void Dvs(float v) => Assert.AreEqual(Features.Function("dvs", v), TimeSolve(TimeBuild($"dvs({v})")));
	[TestCase(1)] public void Fvs(float v) => Assert.AreEqual(Features.Function("fvs", v), TimeSolve(TimeBuild($"fvs({v})")));
	[TestCase(1)] public void Bvs(float v) => Assert.AreEqual(Features.Function("bvs", v), TimeSolve(TimeBuild($"bvs({v})")));
	[TestCase(1)] public void Rnd(float v) {
		for(var i = 0; i < 100; i++)
			if((object)Features.Function("rnd", v) != TimeSolve(TimeBuild($"rnd({v}")))
				return;

		Assert.Fail("Provider RND not random");
	}
	[TestCase(1)] public void Numeric_Abs(float v) => Assert.AreEqual(Features.Function("abs", v), TimeSolve(TimeBuild($"abs({v})")));
	[TestCase(1)] public void Numeric_Nml(float v) => Assert.AreEqual(Features.Function("nml", v), TimeSolve(TimeBuild($"nml({v})")));
	[TestCase(1, 2, 3)] public void Vector_Abs(float x, float y, float z) => Assert.AreEqual(Features.Function("abs", new Vector3(x, y, z)), TimeSolve(TimeBuild("f(v: vec3) = abs(v)"), new Vector3(x, y, z)));
	[TestCase(1, 2, 3)] public void Vector_Nml(float x, float y, float z) => Assert.AreEqual(Features.Function("nml", new Vector3(x, y, z)), TimeSolve(TimeBuild("f(v: vec3) = nml(v)"), new Vector3(x, y, z)));
	[TestCase(1, 2, 3)] public void Vector_Qtn(float x, float y, float z) => Assert.AreEqual(Features.Function("qtn", new Vector3(x, y, z)), TimeSolve(TimeBuild("f(v: vec3) = qtn(v)"), new Vector3(x, y, z)));
	[TestCase(1, 2, 3)] public void Quaternion_Vec(float x, float y, float z) => Assert.AreEqual(Features.Function("vec", Quaternion.CreateFromYawPitchRoll(y, x, z)), TimeSolve(TimeBuild("f(v: quat) = vec(v)"), Quaternion.CreateFromYawPitchRoll(y, x, z)));
	[TestCase(1, 2, 3)] public void Quaternion_Abs(float x, float y, float z) => Assert.AreEqual(Features.Function("abs", Quaternion.CreateFromYawPitchRoll(y, x, z)), TimeSolve(TimeBuild("f(v: quat) = abs(v)"), Quaternion.CreateFromYawPitchRoll(y, x, z)));
	[TestCase(1, 2, 3)] public void Quaternion_Nml(float x, float y, float z) => Assert.AreEqual(Features.Function("nml", Quaternion.CreateFromYawPitchRoll(y, x, z)), TimeSolve(TimeBuild("f(v: quat) = nml(v)"), Quaternion.CreateFromYawPitchRoll(y, x, z)));
	[TestCase(1, 2, 3)] public void Quaternion_Inq(float x, float y, float z) => Assert.AreEqual(Features.Function("inq", Quaternion.CreateFromYawPitchRoll(y, x, z)), TimeSolve(TimeBuild("f(v: quat) = inq(v)"), Quaternion.CreateFromYawPitchRoll(y, x, z)));

	//Stress
	[Test] public void Stress1() => Approximately(542, (Number)TimeSolve(TimeBuild("(4 * 3|2 + 7|5 + 3 / (2 + 4)^5 + 2)")));
	[TestCase(ExpectedResult=12)] public object Stress2() => TimeSolve(TimeBuild("3 * 2^2"));
	[TestCase(ExpectedResult=-1)] public object Stress3() => TimeSolve(TimeBuild("2 - (3)"));
	[TestCase(ExpectedResult=1)] public object Stress4() => TimeSolve(TimeBuild("-(2 - (3))"));
	[TestCase(ExpectedResult=4)] public object Stress5() => TimeSolve(TimeBuild("-(2 - 2(3))"));
	[TestCase(ExpectedResult=-10)] public object Stress6() => TimeSolve(TimeBuild("-|2 - (3)4|"));
	[TestCase(ExpectedResult=7)] public object Stress7() => TimeSolve(TimeBuild("f(b: int) = (4 - 2)b + 1"), 3);
	[Test] public void Stress8() => Approximately(130, (Number)TimeSolve(TimeBuild("f(z: float, b: float, a: float) = 3a1.1b2 - z", 2f, 4f), 5f));
	[Test] public void Stress9() => Assert.AreEqual(new Vector3(4, -8, 12), TimeSolve(TimeBuild("f(a: vec3, b: float) = 2a4b", new Vector3(1, -2, 3)), 0.5f));
	[Test] public void Stress10() => Assert.AreEqual(0.42918455f * (new Vector3(1, 1.4f, 2) - new Vector3(-0.5f, 1.6f, 2.2f)), TimeSolve(TimeBuild("f(g: float, s: vec3, t: vec3) = g / ((s.X - t.X)^2 + (s.Y - t.Y)^2 + (s.Z - t.Z)^2) * (s - t)"), 1f, new Vector3(1, 1.4f, 2), new Vector3(-0.5f, 1.6f, 2.2f)));
	[TestCase(ExpectedResult=10)] public object Stress11() => TimeSolve(TimeBuild("(3 + 2)(3 - 1)"));
	[TestCase(ExpectedResult=4)] public object Stress12() => TimeSolve(TimeBuild("f(c: int) = ((3)(c)) - (c)"), 2);
	[TestCase(ExpectedResult=4)] public object Stress13() => TimeSolve(TimeBuild("f(c: num) = |(|3|)(|c|)| - |c|"), (Number)2);
	[Test] public void Stress14() => Assert.AreEqual(new Vector3(1, 2, 3).Length(), TimeSolve(TimeBuild("f(x: vec3) = |x|"), new Vector3(1, 2, 3)));
	[TestCase] public void Stress15() => Approximately(-0.23077, (Number)TimeSolve(TimeBuild($"2 * ((3 - 4) + (5 / (6 + 7))) - 1 + 8 % 3")));

	//Edge
	[TestCase(ExpectedResult=75)] public object Edge1() => TimeSolve(TimeBuild("f(x: double) = 0.75x"), 100.0);
	[Test] public void Edge2() => Approximately(1.1, (Number)TimeSolve(TimeBuild("1 * 1.1")));
	[TestCase("b", 4, ExpectedResult=4)] public object Edge3(string key, int value) => TimeSolve(TimeBuild($"f(a: map) = a:{key}.value"), new Dictionary<string, object>(){[key] = new {value = value}});
	[TestCase(13)] public void Edge4(int value) => Assert.AreEqual(value, TimeSolve(TimeBuild($"f(a: map) = a:b:c:d.e.f"), new Dictionary<string, object>(){["b"] = new Dictionary<string, object>{["c"] = new Dictionary<string, object>{["d"] = new {e = new {f = value}}}}}));
	[TestCase(-1)] public void Edge5(int value) => Assert.AreEqual(value, TimeSolve(TimeBuild($"f(a: map) = cos(a:b.c)"), new Dictionary<string, object>(){["b"] = new {c = (Number)Math.PI}}));
	[Test] public void Edge6() => Assert.AreEqual(4, TimeSolve(TimeBuild("f(s: vec3) = (s.X - s.Y)^2"), new Vector3(3, 1, 0)));
	[Test] public void Edge7() => Assert.AreEqual(-Vector3.UnitX, TimeSolve(TimeBuild("f(v: vec3) = lvs(v.X)"), Vector3.UnitX));

	//Hidden member from an indexed value used in an operation
	// [Test] public void Edge8() => Assert.AreEqual(Vector3.UnitX, TimeSolve(TimeBuild($"f(a: map) = a:b.c * rvs(1)"), new Dictionary<string, object>(){["b"] = new {c = (Number)1}}));

	protected virtual IFormula Build(string formula, params object[] inputs) => new Formula(formula, inputs);

	List<double> buildTimes = new List<double>(), solveTimes = new List<double>();

	void Approximately(Number lhs, Number rhs) => Assert.AreEqual(lhs, rhs, 0.001);

	object TimeSolve(IFormula formula, params object[] inputs) {
		var timer = Stopwatch.StartNew();
		var result = formula.Solve(inputs);
		timer.Stop();
		solveTimes.Add(timer.Elapsed.TotalMilliseconds);

		return result;
	}

	IFormula TimeBuild(string formula, params object[] inputs) {
		var timer = Stopwatch.StartNew();
		var result = Build(formula, inputs);
		timer.Stop();
		buildTimes.Add(timer.Elapsed.TotalMilliseconds);

		return result;
	}

	double AvgNoOutliers(IEnumerable<double> list) {
		if(list.Count() == 0)
			return double.NaN;

		list = list.OrderByDescending(e => e);
		var (q1, q3) = (list.ElementAt((int)(list.Count() * 0.75)), list.ElementAt((int)(list.Count() * 0.25)));
		var iqr = q3 - q1;

		list = list.Where(e => (q1 - 1.5 * iqr) <= e && e <= (q3 + 1.5 * iqr));
		if(list.Count() == 0)
			return double.NaN;

		return Math.Round(list.Average(), 5);
	}

	[OneTimeTearDown] public void TearDown() => Console.Error.WriteLine($"{GetType().Name}\n\tBuild: {AvgNoOutliers(buildTimes)}ms\n\tSolve: {AvgNoOutliers(solveTimes)}ms");
}