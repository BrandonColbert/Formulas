using System;
using System.Numerics;
using Formulas;
using NUnit.Framework;

[TestFixture]
class TestMath : FormulaTester {
	#region Fundamental

	[TestCase(-1, -1)]
	[TestCase(0, 0)]
	[TestCase(1, 1)]
	public void None(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = x"), x), Precision);

	[TestCase(-1, 1)]
	[TestCase(0, 0)]
	[TestCase(1, -1)]
	public void Negate(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = -x"), x), Precision);

	[TestCase(2, 1, 1)]
	[TestCase(3, 1, 2)]
	[TestCase(1, -1, 2)]
	[TestCase(-1, 1, -2)]
	[TestCase(-2, -1, -1)]
	public void Add(double expected, double x, double y) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double, y: double) = x + y"), x, y), Precision);

	[TestCase(0, 1, 1)]
	[TestCase(-1, 1, 2)]
	[TestCase(-3, -1, 2)]
	[TestCase(3, 1, -2)]
	[TestCase(0, -1, -1)]
	public void Subtract(double expected, double x, double y) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double, y: double) = x - y"), x, y), Precision);

	[TestCase(1, 1, 1)]
	[TestCase(2, 1, 2)]
	[TestCase(-2, -1, 2)]
	[TestCase(-2, 1, -2)]
	[TestCase(1, -1, -1)]
	public void Multiply(double expected, double x, double y) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double, y: double) = xy"), x, y), Precision);

	[TestCase(1, 1, 1)]
	[TestCase(0.5, 1, 2)]
	[TestCase(-0.5, -1, 2)]
	[TestCase(-0.5, 1, -2)]
	[TestCase(1, -1, -1)]
	public void Divide(double expected, double x, double y) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double, y: double) = x/y"), x, y), Precision);

	#endregion

	#region Relations

	[TestCase(1, -1)]
	[TestCase(0, 0)]
	[TestCase(1, 1)]
	public void Abs(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = |x|"), x), Precision);

	[TestCase(7, 3, 2, 1)]
	[TestCase(-5, -3, 2, 1)]
	[TestCase(-5, 3, -2, 1)]
	[TestCase(5, 3, 2, -1)]
	public void Linear(double expected, double m, double x, double b) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(m: double, x: double, b: double) = mx + b"), m, x, b), Precision);

	[TestCase(4, 2)]
	[TestCase(9, 3)]
	[TestCase(16, 4)]
	public void Square(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = x^2"), x), Precision);

	[TestCase(8, 2)]
	[TestCase(27, 3)]
	[TestCase(64, 4)]
	public void Cube(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = x^3"), x), Precision);

	[TestCase(2, 4)]
	[TestCase(3, 9)]
	[TestCase(4, 16)]
	public void Sqrt(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = sqrt(x)"), x), Precision);

	[TestCase(0.5, 2)]
	[TestCase(2, 0.5)]
	[TestCase(1, 1)]
	public void Reciprocal(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = 1/x"), x), Precision);

	[TestCase(0, 1)]
	[TestCase(1, Math.E)]
	public void Log(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = ln(x)"), x), Precision);

	[TestCase(4, 2)]
	[TestCase(8, 3)]
	[TestCase(16, 4)]
	[TestCase(32, 5)]
	public void Exponentiate(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = 2^x"), x), Precision);

	[TestCase(1, 1.01)]
	[TestCase(1, 1.25)]
	[TestCase(1, 1.5)]
	[TestCase(1, 1.75)]
	[TestCase(1, 1.99)]
	public void Floor(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = x - x % 1"), x), Precision);

	[TestCase(2, 1.01)]
	[TestCase(2, 1.25)]
	[TestCase(2, 1.5)]
	[TestCase(2, 1.75)]
	[TestCase(2, 1.99)]
	public void Ceil(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = x + 1 - x % 1"), x), Precision);

	[TestCase(1, 1.01)]
	[TestCase(1, 1.25)]
	[TestCase(2, 1.5)]
	[TestCase(2, 1.75)]
	[TestCase(2, 1.99)]
	public void Round(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = x + 0.5 - (x + 0.5) % 1"), x), Precision);

	#endregion

	#region Algebra

	[TestCase(-1, 1, 3, 2)]
	public void PositiveQuadratic(double expected, double a, double b, double c) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(a: double, b: double, c: double) = (-b + sqrt(b^2 - 4ac)) / (2a)"), a, b, c), Precision);

	[TestCase(-2, 1, 3, 2)]
	public void NegativeQuadratic(double expected, double a, double b, double c) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(a: double, b: double, c: double) = (-b - sqrt(b^2 - 4ac)) / (2a)"), a, b, c), Precision);

	[TestCase(1, 1, 2)]
	[TestCase(1, 2, 1)]
	[TestCase(-1, -1, 1)]
	[TestCase(-1, 1, -1)]
	[TestCase(0, 0, 1)]
	[TestCase(0, 1, 0)]
	[TestCase(-2, -2, -1)]
	[TestCase(-2, -1, -2)]
	public void Min(double expected, double a, double b) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(a: double, b: double) = (a + b - |a - b|)/2"), a, b), Precision);

	[TestCase(2, 1, 2)]
	[TestCase(2, 2, 1)]
	[TestCase(1, -1, 1)]
	[TestCase(1, 1, -1)]
	[TestCase(1, 0, 1)]
	[TestCase(1, 1, 0)]
	[TestCase(-1, -2, -1)]
	[TestCase(-1, -1, -2)]
	public void Max(double expected, double a, double b) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(a: double, b: double) = (a + b + |a - b|)/2"), a, b), Precision);

	[TestCase(1, 0, 1, 2)]
	[TestCase(1.5, 1.5, 1, 2)]
	[TestCase(2, 3, 1, 2)]
	[TestCase(1, 2, -1, 1)]
	[TestCase(-1, -2, -1, 1)]
	public void Clamp(double expected, double x, double l, double u) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild(
		"m = (x + l + |x - l|)/2",
		"f(x: double, l: double, u: double) = (m + u - |m - u|)/2"
	), x, l, u), Precision);

	#endregion

	#region Geometry

	[TestCase(4, 2)]
	[TestCase(9, 3)]
	[TestCase(16, 4)]
	public void AreaSquare(double expected, double s) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(s: double) = s^2"), s), Precision);

	[TestCase(6, 2, 3)]
	[TestCase(6, 3, 2)]
	[TestCase(20, 4, 5)]
	public void AreaRectangle(double expected, double l, double w) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(l: double, w: double) = lw"), l, w), Precision);

	[TestCase(1, 1, 2)]
	[TestCase(2, 2, 2)]
	[TestCase(6, 3, 4)]
	public void AreaTriangle(double expected, double b, double h) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(b: double, h: double) = bh/2"), b, h), Precision);

	[TestCase(4 * Math.PI, 2)]
	[TestCase(9 * Math.PI, 3)]
	public void AreaCircle(double expected, double r) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild($"f(r: double) = {Math.PI}r^2"), r), Precision);

	[TestCase(2 * Math.PI, 1, 2)]
	[TestCase(4 * Math.PI, 2, 2)]
	[TestCase(6 * Math.PI, 3, 2)]
	public void AreaEllipse(double expected, double a, double b) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild($"f(a: double, b: double) = {Math.PI}ab"), a, b), Precision);

	[TestCase(4.5, 1, 2, 3)]
	public void AreaTrapezoid(double expected, double a, double b, double h) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(a: double, b: double, h: double) = (a + b)h/2"), a, b, h), Precision);

	[TestCase(36 * Math.PI, 3)]
	public void SurfaceSphere(double expected, double r) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild($"f(r: double) = 4({Math.PI})r^2"), r), Precision);

	[TestCase(24, 2)]
	public void SurfaceCube(double expected, double s) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(s: double) = 6s^2"), s), Precision);

	[TestCase(28 * Math.PI, 2, 5)]
	public void SurfaceCylinder(double expected, double r, double h) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild($"f(r: double, h: double) = 2({Math.PI})rh + 2({Math.PI})r^2"), r, h), Precision);

	[TestCase(20, 5)]
	public void PerimeterSquare(double expected, double s) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(s: double) = 4s"), s), Precision);

	[TestCase(10, 3, 2)]
	public void PerimeterRectangle(double expected, double l, double w) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(l: double, w: double) = 2l + 2w"), l, w), Precision);

	[TestCase(6, 1, 2, 3)]
	public void PerimeterTriangle(double expected, double a, double b, double c) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(a: double, b: double, c: double) = abc"), a, b, c), Precision);

	[TestCase(4 * Math.PI, 2)]
	public void CircumferenceCircle(double expected, double r) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild($"f(r: double) = 2({Math.PI})r"), r), Precision);

	[TestCase(19.86917686, 2, 4)]
	public void CircumferenceEllipse(double expected, double a, double b) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild($"f(a: double, b: double) = {Math.PI} * sqrt(2(a^2 + b^2))"), a, b), Precision);

	[TestCase(27, 3)]
	public void VolumeCube(double expected, double s) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(s: double) = s^3"), s), Precision);

	[TestCase(6, 1, 2, 3)]
	public void VolumeRectangularPrism(double expected, double l, double w, double h) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(l: double, w: double, h: double) = lwh"), l, w, h), Precision);

	[TestCase(4, 2, 3)]
	public void VolumeSquarePyramid(double expected, double b, double h) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(b: double, h: double) = b^2(h/3)"), b, h), Precision);

	[TestCase(80 * Math.PI, 4, 5)]
	public void VolumeCylinder(double expected, double r, double h) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild($"f(r: double, h: double) = {Math.PI}r^2 h"), r, h), Precision);

	[TestCase(8 * Math.PI, 2, 6)]
	public void VolumeCone(double expected, double r, double h) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild($"f(r: double, h: double) = {Math.PI}r^2 h/3"), r, h), Precision);

	[TestCase(36 * Math.PI, 3)]
	public void VolumeSphere(double expected, double r) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild($"f(r: double) = 4/3 * {Math.PI}r^3"), r), Precision);

	#endregion

	#region Trigonometry

	[TestCase(1, Math.PI / 2)]
	public void Sin(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = sin(x)"), x), Precision);

	[TestCase(Math.PI / 2, 1)]
	public void Asin(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = asin(x)"), x), Precision);

	[TestCase(1.188395106, 1)]
	public void Csc(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = 1/sin(x)"), x), Precision);

	[TestCase(1, 0)]
	public void Cos(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = cos(x)"), x), Precision);

	[TestCase(0, 1)]
	public void Acos(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = acos(x)"), x), Precision);

	[TestCase(1.850815718, 1)]
	public void Sec(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = 1/cos(x)"), x), Precision);

	[TestCase(1.557407725, 1)]
	public void Tan(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = tan(x)"), x), Precision);

	[TestCase(Math.PI / 4, 1)]
	public void Atan(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = atan(x)"), x), Precision);

	[TestCase(0.642092616, 1)]
	public void Cot(double expected, double x) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double) = 1/tan(x)"), x), Precision);

	#endregion

	#region Physics

	[TestCase(7, 1, 2, 3)]
	public void Velocity(double expected, double v, double a, double t) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(v: double, a: double, t: double) = v + at"), v, a, t), Precision);

	[TestCase(33, 1, 2, 3, 4)]
	public void Position(double expected, double x, double v, double a, double t) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(x: double, v: double, a: double, t: double) = x + vt + 0.5at^2"), x, v, a, t), Precision);

	[TestCase(20, 2, 10)]
	public void Weight(double expected, double m, double g) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(m: double, g: double) = mg"), m, g), Precision);

	[TestCase(8, 4, 2)]
	public void CentripitalAcceleration(double expected, double v, double r) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(v: double, r: double) = v^2 / r"), v, r), Precision);

	[TestCase(14, 2, 7)]
	public void Momentum(double expected, double m, double v) => Assert.AreEqual(expected, (Number)TimeSolve(TimeBuild("f(m: double, v: double) = mv"), m, v), Precision);

	[Test]
	public void Gravity() => Assert.AreEqual(0.42918455f * (new Vector3(1, 1.4f, 2) - new Vector3(-0.5f, 1.6f, 2.2f)), TimeSolve(TimeBuild("f(g: float, s: vec3, t: vec3) = g / ((s.X - t.X)^2 + (s.Y - t.Y)^2 + (s.Z - t.Z)^2) * (s - t)"), 1f, new Vector3(1, 1.4f, 2), new Vector3(-0.5f, 1.6f, 2.2f)));

	#endregion

	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest);
}

[TestFixture]
class TestCompileMath : TestMath {
	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest).Compile();
}