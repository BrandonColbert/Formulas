using System;
using System.Collections.Generic;
using System.Numerics;
using Formulas;
using NUnit.Framework;

[TestFixture]
class TestFormula : FormulaTester {
	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest);

	//Operations
	[TestCase(3, 1, 2, ExpectedResult=9)] public object OpGrouping(int a, int b, int c) => TimeSolve(TimeBuild($"{a} * ({b} + {c})"));
	[TestCase(1, 2, ExpectedResult=1)] public object OpMagnitude(int a, int b) => TimeSolve(TimeBuild($"|{a} - {b}|"));
	[TestCase(2, 3, ExpectedResult=8)] public object OpExponentiate(int a, int b) => TimeSolve(TimeBuild($"{a}^{b}"));
	[TestCase(2, 3, ExpectedResult=6)] public object OpMultiply(int a, int b) => TimeSolve(TimeBuild($"{a} * {b}"));
	[TestCase(6, 2, ExpectedResult=3)] public object OpDivide(int a, int b) => TimeSolve(TimeBuild($"{a} / {b}"));
	[TestCase(6, 5, ExpectedResult=1)] public object OpModulus(int a, int b) => TimeSolve(TimeBuild($"{a} % {b}"));
	[TestCase(1, 1, ExpectedResult=2)] public object OpAdd(int a, int b) => TimeSolve(TimeBuild($"{a} + {b}"));
	[TestCase(2, 1, ExpectedResult=1)] public object OpSubtract(int a, int b) => TimeSolve(TimeBuild($"{a} - {b}"));
	[TestCase(1, ExpectedResult=-1)] public object OpNegate(int a) => TimeSolve(TimeBuild("f(a: int) = -a"), a);
	[TestCase("b", 1, ExpectedResult=1)] public object OpIndexAccess(string key, int value) => TimeSolve(TimeBuild($"f(a: map) = a:{key}"), new Dictionary<string, object>(){[key] = value});
	[TestCase(1, ExpectedResult="Sally")] public object OpArrayAccess(int key) => TimeSolve(TimeBuild($"f(a: array) = a:{key}"), new[]{"Peter", "Sally", "Bob"} as object);
	[TestCase("abc", "Length", ExpectedResult=3)] public object OpMemberAccess(string a, string b) => TimeSolve(TimeBuild($"f(a: string) = a.{b}"), a);

	//Declaration
	[TestCase(2, 5, ExpectedResult=5)] public object Declare1(int x, int y) => TimeSolve(TimeBuild(
		"z = y - x",
		"f(x: int, y: int) = x + z"
	), x, y);
	[TestCase(1, 2, 3, ExpectedResult=10)] public object Declare2(int a, int b, int c) => TimeSolve(TimeBuild(
		"e = b * c - b",
		"d = a * e",
		"f(a: int, b: int, c: int) = a + b + c + d"
	), a, b, c);
	[TestCase(1, 2, 3, ExpectedResult=24)] public object Declare3(int a, int b, int c) => TimeSolve(TimeBuild(
		"e = abc",
		"v = sqrt|e + c|",
		"g = sgn(e)^(2v)",
		"f(a: int, b: int, c: int) = e * 2(v - a) * a^g"
	), a, b, c);
	[TestCase(1, 2, 3, ExpectedResult=6)] public object Declare3Ignore2(int a, int b, int c) => TimeSolve(TimeBuild(
		"e = abc",
		"v = sqrt|e + b|",
		"g = sgn(e)^(2v)",
		"f(a: int, b: int, c: int) = e"
	), a, b, c);

	//Input
	[TestCase(1, ExpectedResult=1)] public object No_Input(int a) => TimeSolve(TimeBuild($"{a}"));
	[TestCase(1, ExpectedResult=1)] public object Single_PreInput(int a) => TimeSolve(new Mediator(TimeBuild("f(x: int) = x"), a));
	[TestCase(1, ExpectedResult=1)] public object Single_Input(int a) => TimeSolve(TimeBuild("f(x: int) = x"), a);
	[TestCase(1, 2, ExpectedResult=3)] public object Single_PreAndPostInput(int a, int b) => TimeSolve(new Mediator(TimeBuild("f(x: int, y: int) = x + y"), a), b);
	[TestCase(1, 2, ExpectedResult=3)] public object Multiple_PreInput(int a, int b) => TimeSolve(new Mediator(TimeBuild("f(x: int, y: int) = x + y"), a, b));
	[TestCase(1, 2, ExpectedResult=3)] public object Multiple_Input(int a, int b) => TimeSolve(TimeBuild("f(x: int, y: int) = x + y"), a, b);
	[TestCase(1, ExpectedResult=1)] public object Mapped_Single_PreInput(int a) => TimeSolve(new Mediator(TimeBuild("f(x: int) = x"), a));
	[TestCase(1, ExpectedResult=1)] public object Mapped_Single_Input(int a) => TimeSolve(TimeBuild("f(x: int) = x"), a);
	[TestCase(1, 2, ExpectedResult=3)] public object Mapped_Multiple_PreInput(int a, int b) => TimeSolve(new Mediator(TimeBuild("f(x: int, y: int) = x + y"), a, b));
	[TestCase(1, 2, ExpectedResult=3)] public object Mapped_Multiple_Input(int a, int b) => TimeSolve(TimeBuild("f(x: int, y: int) = x + y"), a, b);

	//Features
	[TestCase(1)] public void FeatureSin(float v) => Assert.AreEqual(Features.Function("sin", v), TimeSolve(TimeBuild($"sin({v})")));
	[TestCase(1)] public void FeatureAsin(float v) => Assert.AreEqual(Features.Function("asin", v), TimeSolve(TimeBuild($"asin({v})")));
	[TestCase(1)] public void FeatureCos(float v) => Assert.AreEqual(Features.Function("cos", v), TimeSolve(TimeBuild($"cos({v})")));
	[TestCase(1)] public void FeatureAcos(float v) => Assert.AreEqual(Features.Function("acos", v), TimeSolve(TimeBuild($"acos({v})")));
	[TestCase(1)] public void FeatureTan(float v) => Assert.AreEqual(Features.Function("tan", v), TimeSolve(TimeBuild($"tan({v})")));
	[TestCase(1)] public void FeatureAtan(float v) => Assert.AreEqual(Features.Function("atan", v), TimeSolve(TimeBuild($"atan({v})")));
	[TestCase(16)] public void FeatureSqrt(float v) => Assert.AreEqual(Features.Function("sqrt", v), TimeSolve(TimeBuild($"sqrt({v})")));
	[TestCase(2)] public void FeatureLn(float v) => Assert.AreEqual(Features.Function("ln", v), TimeSolve(TimeBuild($"ln({v})")));
	[TestCase(2)] public void FeatureLog(float v) => Assert.AreEqual(Features.Function("log", v), TimeSolve(TimeBuild($"log({v})")));
	[TestCase(1)] public void FeatureSgn(float v) => Assert.AreEqual(Features.Function("sgn", v), TimeSolve(TimeBuild($"sgn({v})")));
	[TestCase(1)] public void FeatureRvs(float v) => Assert.AreEqual(Features.Function("rvs", v), TimeSolve(TimeBuild($"rvs({v})")));
	[TestCase(1)] public void FeatureLvs(float v) => Assert.AreEqual(Features.Function("lvs", v), TimeSolve(TimeBuild($"lvs({v})")));
	[TestCase(1)] public void FeatureUvs(float v) => Assert.AreEqual(Features.Function("uvs", v), TimeSolve(TimeBuild($"uvs({v})")));
	[TestCase(1)] public void FeatureDvs(float v) => Assert.AreEqual(Features.Function("dvs", v), TimeSolve(TimeBuild($"dvs({v})")));
	[TestCase(1)] public void FeatureFvs(float v) => Assert.AreEqual(Features.Function("fvs", v), TimeSolve(TimeBuild($"fvs({v})")));
	[TestCase(1)] public void FeatureBvs(float v) => Assert.AreEqual(Features.Function("bvs", v), TimeSolve(TimeBuild($"bvs({v})")));
	[TestCase(1)] public void FeatureRnd(float v) {
		for(var i = 0; i < 100; i++)
			if((object)Features.Function("rnd", v) != TimeSolve(TimeBuild($"rnd({v}")))
				return;

		Assert.Fail("Provider RND not random");
	}
	[TestCase(1)] public void FeatureNumericAbs(float v) => Assert.AreEqual(Features.Function("abs", v), TimeSolve(TimeBuild($"abs({v})")));
	[TestCase(1)] public void FeatureNumericNml(float v) => Assert.AreEqual(Features.Function("nml", v), TimeSolve(TimeBuild($"nml({v})")));
	[TestCase(1, 2, 3)] public void FeatureVectorAbs(float x, float y, float z) => Assert.AreEqual(Features.Function("abs", new Vector3(x, y, z)), TimeSolve(TimeBuild("f(v: vec3) = abs(v)"), new Vector3(x, y, z)));
	[TestCase(1, 2, 3)] public void FeatureVectorNml(float x, float y, float z) => Assert.AreEqual(Features.Function("nml", new Vector3(x, y, z)), TimeSolve(TimeBuild("f(v: vec3) = nml(v)"), new Vector3(x, y, z)));
	[TestCase(1, 2, 3)] public void FeatureVectorQtn(float x, float y, float z) => Assert.AreEqual(Features.Function("qtn", new Vector3(x, y, z)), TimeSolve(TimeBuild("f(v: vec3) = qtn(v)"), new Vector3(x, y, z)));
	[TestCase(1, 2, 3)] public void FeatureQuaternionVec(float x, float y, float z) => Assert.AreEqual(Features.Function("vec", Quaternion.CreateFromYawPitchRoll(y, x, z)), TimeSolve(TimeBuild("f(v: quat) = vec(v)"), Quaternion.CreateFromYawPitchRoll(y, x, z)));
	[TestCase(1, 2, 3)] public void FeatureQuaternionAbs(float x, float y, float z) => Assert.AreEqual(Features.Function("abs", Quaternion.CreateFromYawPitchRoll(y, x, z)), TimeSolve(TimeBuild("f(v: quat) = abs(v)"), Quaternion.CreateFromYawPitchRoll(y, x, z)));
	[TestCase(1, 2, 3)] public void FeatureQuaternionNml(float x, float y, float z) => Assert.AreEqual(Features.Function("nml", Quaternion.CreateFromYawPitchRoll(y, x, z)), TimeSolve(TimeBuild("f(v: quat) = nml(v)"), Quaternion.CreateFromYawPitchRoll(y, x, z)));
	[TestCase(1, 2, 3)] public void FeatureQuaternionInq(float x, float y, float z) => Assert.AreEqual(Features.Function("inq", Quaternion.CreateFromYawPitchRoll(y, x, z)), TimeSolve(TimeBuild("f(v: quat) = inq(v)"), Quaternion.CreateFromYawPitchRoll(y, x, z)));

	//Stress
	[Test] public void Stress1() => Approximately(542, (Number)TimeSolve(TimeBuild("(4 * 3|2 + 7|5 + 3 / (2 + 4)^5 + 2)")));
	[TestCase(ExpectedResult=12)] public object Stress2() => TimeSolve(TimeBuild("3 * 2^2"));
	[TestCase(ExpectedResult=-1)] public object Stress3() => TimeSolve(TimeBuild("2 - (3)"));
	[TestCase(ExpectedResult=1)] public object Stress4() => TimeSolve(TimeBuild("-(2 - (3))"));
	[TestCase(ExpectedResult=4)] public object Stress5() => TimeSolve(TimeBuild("-(2 - 2(3))"));
	[TestCase(ExpectedResult=-10)] public object Stress6() => TimeSolve(TimeBuild("-|2 - (3)4|"));
	[TestCase(ExpectedResult=7)] public object Stress7() => TimeSolve(TimeBuild("f(b: int) = (4 - 2)b + 1"), 3);
	[Test] public void Stress8() => Approximately(130, (Number)TimeSolve(TimeBuild("f(z: float, b: float, a: float) = 3a1.1b2 - z"), 2f, 4f, 5f));
	[Test] public void Stress9() => Assert.AreEqual(new Vector3(4, -8, 12), TimeSolve(TimeBuild("f(a: vec3, b: float) = 2a4b"), new Vector3(1, -2, 3), 0.5f));
	[Test] public void Stress10() => Assert.AreEqual(0.42918455f * (new Vector3(1, 1.4f, 2) - new Vector3(-0.5f, 1.6f, 2.2f)), TimeSolve(TimeBuild("f(g: float, s: vec3, t: vec3) = g / ((s.X - t.X)^2 + (s.Y - t.Y)^2 + (s.Z - t.Z)^2) * (s - t)"), 1f, new Vector3(1, 1.4f, 2), new Vector3(-0.5f, 1.6f, 2.2f)));
	[TestCase(ExpectedResult=10)] public object Stress11() => TimeSolve(TimeBuild("(3 + 2)(3 - 1)"));
	[TestCase(ExpectedResult=4)] public object Stress12() => TimeSolve(TimeBuild("f(c: int) = ((3)(c)) - (c)"), 2);
	[TestCase(ExpectedResult=4)] public object Stress13() => TimeSolve(TimeBuild("f(c: num) = |(|3|)(|c|)| - |c|"), (Number)2);
	[Test] public void Stress14() => Assert.AreEqual(new Vector3(1, 2, 3).Length(), TimeSolve(TimeBuild("f(x: vec3) = |x|"), new Vector3(1, 2, 3)));
	[TestCase] public void Stress15() => Approximately(-0.23077, (Number)TimeSolve(TimeBuild($"2 * ((3 - 4) + (5 / (6 + 7))) - 1 + 8 % 3")));
	[TestCase] public void Stress16() => Approximately(4, (Number)TimeSolve(TimeBuild("f(x: int, y: int) = 2 * (x + y) - x^3"), 2, 4));

	//Edge
	[TestCase(ExpectedResult=75)] public object Edge1() => TimeSolve(TimeBuild("f(x: double) = 0.75x"), 100.0);
	[Test] public void Edge2() => Approximately(1.1, (Number)TimeSolve(TimeBuild("1 * 1.1")));
	[TestCase("b", 4, ExpectedResult=4)] public object Edge3(string key, int value) => TimeSolve(TimeBuild($"f(a: map) = a:{key}.value"), new Dictionary<string, object>(){[key] = new{value = value}});
	[TestCase(13)] public void Edge4(int value) => Assert.AreEqual(value, TimeSolve(TimeBuild($"f(a: map) = a:b:c:d.e.f"), new Dictionary<string, object>(){["b"] = new Dictionary<string, object>{["c"] = new Dictionary<string, object>{["d"] = new{e = new{f = value}}}}}));
	[TestCase(-1)] public void Edge5(int value) => Assert.AreEqual(value, TimeSolve(TimeBuild($"f(a: map) = cos(a:b.c)"), new Dictionary<string, object>(){["b"] = new{c = (Number)Math.PI}}));
	[Test] public void Edge6() => Assert.AreEqual(4, TimeSolve(TimeBuild("f(s: vec3) = (s.X - s.Y)^2"), new Vector3(3, 1, 0)));
	[Test] public void Edge7() => Assert.AreEqual(-Vector3.UnitX, TimeSolve(TimeBuild("f(v: vec3) = lvs(v.X)"), Vector3.UnitX));

	[TestCase(0.75f)] public void Edge8(float scalar) => Assert.AreEqual(new Vector3(1, 2, 3) * scalar, TimeSolve(TimeBuild($"f(x: vec3) = {scalar}x"), new Vector3(1, 2, 3)));
	[TestCase(4, ExpectedResult=4)] public object Edge9(int value) => TimeSolve(TimeBuild($"a.Length"), new{Length = value});

	//Hidden member from an indexed value used in an operation
	[Test] public void Edge10() => Assert.AreEqual(Vector3.UnitX, TimeSolve(TimeBuild($"f(a: map) = a:b.c * rvs(1)"), new Dictionary<string, object>(){["b"] = new{c = (Number)1}}));

	//Math
	[TestCase(1, 1)] public void None(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = x"), x));
	[TestCase(-1, 1)] public void Negate(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = -x"), x));
	[TestCase(3, 1, 2)] public void Add(double result, double x, double y) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double, y: double) = x + y"), x, y));
	[TestCase(-1, 1, 2)] public void Subtract(double result, double x, double y) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double, y: double) = x - y"), x, y));
	[TestCase(6, 2, 3)] public void Multiply(double result, double x, double y) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double, y: double) = xy"), x, y));
	[TestCase(0.5, 1, 2)] public void Divide(double result, double x, double y) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double, y: double) = x/y"), x, y));
	[TestCase(1, 1)] public void Abs(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = |x|"), x));
	[TestCase(11, 4, 2, 3)] public void Linear(double result, double m, double x, double b) => Approximately(result, (Number)TimeSolve(TimeBuild("f(m: double, x: double, b: double) = mx + b"), m, x, b));
	[TestCase(9, 3)] public void Square(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = x^2"), x));
	[TestCase(64, 4)] public void Cube(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = x^3"), x));
	[TestCase(3, 9)] public void Sqrt(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = sqrt(x)"), x));
	[TestCase(0.5, 2)] public void Reciprocal(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = 1/x"), x));
	[TestCase(1, Math.E)] public void Log(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = ln(x)"), x));
	[TestCase(20.123648, 3)] public void Exponential(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = 2.72^x"), x));
	[TestCase(2, 2.7)] public void Floor(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = x - x % 1"), x));
	[TestCase(3, 2.05)] public void Ceil(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = x + 1 - x % 1"), x));
	[TestCase(10, 9.5)] public void Round(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = x + 0.5 - (x + 0.5) % 1"), x));
	[TestCase(-1, 1, 3, 2)] public void PositiveQuadratic(double result, double a, double b, double c) => Approximately(result, (Number)TimeSolve(TimeBuild("f(a: double, b: double, c: double) = (-b + sqrt(b^2 - 4ac)) / (2a)"), a, b, c));
	[TestCase(-2, 1, 3, 2)] public void NegativeQuadratic(double result, double a, double b, double c) => Approximately(result, (Number)TimeSolve(TimeBuild("f(a: double, b: double, c: double) = (-b - sqrt(b^2 - 4ac)) / (2a)"), a, b, c));
	[TestCase(3, 3, 18)] public void Min(double result, double a, double b) => Approximately(result, (Number)TimeSolve(TimeBuild("f(a: double, b: double) = (a + b - |a - b|)/2"), a, b));
	[TestCase(-4, -4, -5)] public void Max(double result, double a, double b) => Approximately(result, (Number)TimeSolve(TimeBuild("f(a: double, b: double) = (a + b + |a - b|)/2"), a, b));

	[TestCase(5, 19, -2, 5)]
	[TestCase(-2, -14, -2, 5)]
	[TestCase(3, 3, -2, 5)]
	public void Clamp(double result, double x, double l, double u) => Approximately(result, (Number)TimeSolve(TimeBuild(
		"m = (x + l + |x - l|)/2",
		"f(x: double, l: double, u: double) = (m + u - |m - u|)/2"
	), x, l, u));

	[TestCase(16, 4)] public void AreaSquare(double result, double s) => Approximately(result, (Number)TimeSolve(TimeBuild("f(s: double) = s^2"), s));
	[TestCase(20, 4, 5)] public void AreaRectangle(double result, double l, double w) => Approximately(result, (Number)TimeSolve(TimeBuild("f(l: double, w: double) = lw"), l, w));
	[TestCase(1, 1, 2)] public void AreaTriangle(double result, double b, double h) => Approximately(result, (Number)TimeSolve(TimeBuild("f(b: double, h: double) = bh/2"), b, h));
	[TestCase(12.56637096, 2)] public void AreaCircle(double result, double r) => Approximately(result, (Number)TimeSolve(TimeBuild($"f(r: double) = {Math.PI}r^2"), r));
	[TestCase(6.283185482, 1, 2)] public void AreaEllipse(double result, double a, double b) => Approximately(result, (Number)TimeSolve(TimeBuild($"f(a: double, b: double) = {Math.PI}ab"), a, b));
	[TestCase(4.5, 1, 2, 3)] public void AreaTrapezoid(double result, double a, double b, double h) => Approximately(result, (Number)TimeSolve(TimeBuild("f(a: double, b: double, h: double) = (a + b)h/2"), a, b, h));
	[TestCase(113.0973358, 3)] public void SurfaceSphere(double result, double r) => Approximately(result, (Number)TimeSolve(TimeBuild($"f(r: double) = 4({Math.PI})r^2"), r));
	[TestCase(24, 2)] public void SurfaceCube(double result, double s) => Approximately(result, (Number)TimeSolve(TimeBuild("f(s: double) = 6s^2"), s));
	[TestCase(87.9646, 2, 5)] public void SurfaceCylinder(double result, double r, double h) => Approximately(result, (Number)TimeSolve(TimeBuild($"f(r: double, h: double) = 2({Math.PI})rh + 2({Math.PI})r^2"), r, h));
	[TestCase(20, 5)] public void PerimeterSquare(double result, double s) => Approximately(result, (Number)TimeSolve(TimeBuild("f(s: double) = 4s"), s));
	[TestCase(10, 3, 2)] public void PerimeterRectangle(double result, double l, double w) => Approximately(result, (Number)TimeSolve(TimeBuild("f(l: double, w: double) = 2l + 2w"), l, w));
	[TestCase(6, 1, 2, 3)] public void PerimeterTriangle(double result, double a, double b, double c) => Approximately(result, (Number)TimeSolve(TimeBuild("f(a: double, b: double, c: double) = abc"), a, b, c));
	[TestCase(12.56637096405, 2)] public void CircumferenceCircle(double result, double r) => Approximately(result, (Number)TimeSolve(TimeBuild($"f(r: double) = 2({Math.PI})r"), r));
	[TestCase(19.86917686, 2, 4)] public void CircumferenceEllipse(double result, double a, double b) => Approximately(result, (Number)TimeSolve(TimeBuild($"f(a: double, b: double) = {Math.PI} * sqrt(2(a^2 + b^2))"), a, b));
	[TestCase(27, 3)] public void VolumeCube(double result, double s) => Approximately(result, (Number)TimeSolve(TimeBuild("f(s: double) = s^3"), s));
	[TestCase(6, 1, 2, 3)] public void VolumeRectangularPrism(double result, double l, double w, double h) => Approximately(result, (Number)TimeSolve(TimeBuild("f(l: double, w: double, h: double) = lwh"), l, w, h));
	[TestCase(4, 2, 3)] public void VolumeSquarePyramid(double result, double b, double h) => Approximately(result, (Number)TimeSolve(TimeBuild("f(b: double, h: double) = b^2(h/3)"), b, h));
	[TestCase(251.3274231, 4, 5)] public void VolumeCylinder(double result, double r, double h) => Approximately(result, (Number)TimeSolve(TimeBuild($"f(r: double, h: double) = {Math.PI}r^2 h"), r, h));
	[TestCase(75.39822388, 6, 2)] public void VolumeCone(double result, double r, double h) => Approximately(result, (Number)TimeSolve(TimeBuild($"f(r: double, h: double) = {Math.PI}r^2 h/3"), r, h));
	[TestCase(33.51, 2)] public void VolumeSphere(double result, double r) => Approximately(result, (Number)TimeSolve(TimeBuild($"f(r: double) = 4/3 * {Math.PI}r^3"), r));
	[TestCase(1, Math.PI / 2)] public void Sin(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = sin(x)"), x));
	[TestCase(Math.PI / 2, 1)] public void Asin(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = asin(x)"), x));
	[TestCase(1.188395106, 1)] public void Csc(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = 1/sin(x)"), x));
	[TestCase(1, 0)] public void Cos(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = cos(x)"), x));
	[TestCase(0, 1)] public void Acos(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = acos(x)"), x));
	[TestCase(1.850815718, 1)] public void Sec(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = 1/cos(x)"), x));
	[TestCase(1.557407725, 1)] public void Tan(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = tan(x)"), x));
	[TestCase(Math.PI / 4, 1)] public void Atan(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = atan(x)"), x));
	[TestCase(0.642092616, 1)] public void Cot(double result, double x) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double) = 1/tan(x)"), x));
	[TestCase(7, 1, 2, 3)] public void Velocity(double result, double v, double a, double t) => Approximately(result, (Number)TimeSolve(TimeBuild("f(v: double, a: double, t: double) = v + at"), v, a, t));
	[TestCase(33, 1, 2, 3, 4)] public void Position(double result, double x, double v, double a, double t) => Approximately(result, (Number)TimeSolve(TimeBuild("f(x: double, v: double, a: double, t: double) = x + vt + 0.5at^2"), x, v, a, t));
	[TestCase(20, 2, 10)] public void Weight(double result, double m, double g) => Approximately(result, (Number)TimeSolve(TimeBuild("f(m: double, g: double) = mg"), m, g));
	[TestCase(8, 4, 2)] public void CentripitalAcceleration(double result, double v, double r) => Approximately(result, (Number)TimeSolve(TimeBuild("f(v: double, r: double) = v^2 / r"), v, r));
	[TestCase(14, 2, 7)] public void Momentum(double result, double m, double v) => Approximately(result, (Number)TimeSolve(TimeBuild("f(m: double, v: double) = mv"), m, v));
}