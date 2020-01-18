using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Formulas;
using NUnit.Framework;

abstract class IFormulaTest {
	//Operations
	[TestCase(3, 1, 2, ExpectedResult=9)] public object Op_Grouping(int a, int b, int c) => TimeSolve(TimeBuild($"{a} * ({b} + {c})"));
	[TestCase(1, 2, ExpectedResult=1)] public object Op_Magnitude(int a, int b) => TimeSolve(TimeBuild($"|{a} - {b}|"));
	[TestCase(2, 3, ExpectedResult=8)] public object Op_Exponentiate(int a, int b) => TimeSolve(TimeBuild($"{a}^{b}"));
	[TestCase(2, 3, ExpectedResult=6)] public object Op_Multiply(int a, int b) => TimeSolve(TimeBuild($"{a} * {b}"));
	[TestCase(6, 2, ExpectedResult=3)] public object Op_Divide(int a, int b) => TimeSolve(TimeBuild($"{a} / {b}"));
	[TestCase(6, 5, ExpectedResult=1)] public object Op_Modulus(int a, int b) => TimeSolve(TimeBuild($"{a} % {b}"));
	[TestCase(1, 1, ExpectedResult=2)] public object Op_Add(int a, int b) => TimeSolve(TimeBuild($"{a} + {b}"));
	[TestCase(2, 1, ExpectedResult=1)] public object Op_Subtract(int a, int b) => TimeSolve(TimeBuild($"{a} - {b}"));
	[TestCase(1, ExpectedResult=-1)] public object Op_Negate(int a) => TimeSolve(TimeBuild("-a"), a);
	[TestCase(1, ExpectedResult=1)] public object Op_IndexAccess(int b) => TimeSolve(TimeBuild("a:b"), new Dictionary<string, object>(){["b"] = b});
	[TestCase("abc", "Length", ExpectedResult=3)] public object Op_MemberAccess(object a, string b) => TimeSolve(TimeBuild($"a.{b}"), a);

	//Input
	[TestCase(1, ExpectedResult=1)] public object No_Input(int a) => TimeSolve(TimeBuild($"{a}"));
	[TestCase(1, ExpectedResult=1)] public object Single_PreInput(int a) => TimeSolve(TimeBuild("x", a));
	[TestCase(1, ExpectedResult=1)] public object Single_PostInput(int a) => TimeSolve(TimeBuild("x"), a);
	[TestCase(1, 2, ExpectedResult=3)] public object Single_PreInput_Single_PostInput(int a, int b) => TimeSolve(TimeBuild("x + y", a), b);
	[TestCase(1, 2, ExpectedResult=3)] public object Multiple_PreInput(int a, int b) => TimeSolve(TimeBuild("x + y", a, b));
	[TestCase(1, 2, ExpectedResult=3)] public object Multiple_PostInput(int a, int b) => TimeSolve(TimeBuild("x + y"), a,b);
	[TestCase(1, ExpectedResult=1)] public object Mapped_Single_PreInput(int a) => TimeSolve(TimeBuild("f(x) = x", a));
	[TestCase(1, ExpectedResult=1)] public object Mapped_Single_PostInput(int a) => TimeSolve(TimeBuild("f(x) = x"), a);
	[TestCase(1, 2, ExpectedResult=3)] public object Mapped_Multiple_PreInput(int a, int b) => TimeSolve(TimeBuild("f(x, y) = x + y", a, b));
	[TestCase(1, 2, ExpectedResult=3)] public object Mapped_Multiple_PostInput(int a, int b) => TimeSolve(TimeBuild("f(x, y) = x + y"), a, b);

	//Functions
	[TestCase(1)] public void Sin(float v) => Assert.AreEqual(Formulizer.Provider.Sin(v), TimeSolve(TimeBuild($"sin({v})")));
	[TestCase(1)] public void Asin(float v) => Assert.AreEqual(Formulizer.Provider.Asin(v), TimeSolve(TimeBuild($"asin({v})")));
	[TestCase(1)] public void Cos(float v) => Assert.AreEqual(Formulizer.Provider.Cos(v), TimeSolve(TimeBuild($"cos({v})")));
	[TestCase(1)] public void Acos(float v) => Assert.AreEqual(Formulizer.Provider.Acos(v), TimeSolve(TimeBuild($"acos({v})")));
	[TestCase(1)] public void Tan(float v) => Assert.AreEqual(Formulizer.Provider.Tan(v), TimeSolve(TimeBuild($"tan({v})")));
	[TestCase(1)] public void Atan(float v) => Assert.AreEqual(Formulizer.Provider.Atan(v), TimeSolve(TimeBuild($"atan({v})")));
	[TestCase(16)] public void Sqrt(float v) => Assert.AreEqual(Formulizer.Provider.Sqrt(v), TimeSolve(TimeBuild($"sqrt({v})")));
	[TestCase(2)] public void Ln(float v) => Assert.AreEqual(Formulizer.Provider.Ln(v), TimeSolve(TimeBuild($"ln({v})")));
	[TestCase(2)] public void Log(float v) => Assert.AreEqual(Formulizer.Provider.Log(v), TimeSolve(TimeBuild($"log({v})")));
	[TestCase(1)] public void Sgn(float v) => Assert.AreEqual(Formulizer.Provider.Sgn(v), TimeSolve(TimeBuild($"sgn({v})")));
	[TestCase(1)] public void Rvs(float v) => Assert.AreEqual(Formulizer.Provider.Rvs(v), TimeSolve(TimeBuild($"rvs({v})")));
	[TestCase(1)] public void Lvs(float v) => Assert.AreEqual(Formulizer.Provider.Lvs(v), TimeSolve(TimeBuild($"lvs({v})")));
	[TestCase(1)] public void Uvs(float v) => Assert.AreEqual(Formulizer.Provider.Uvs(v), TimeSolve(TimeBuild($"uvs({v})")));
	[TestCase(1)] public void Dvs(float v) => Assert.AreEqual(Formulizer.Provider.Dvs(v), TimeSolve(TimeBuild($"dvs({v})")));
	[TestCase(1)] public void Fvs(float v) => Assert.AreEqual(Formulizer.Provider.Fvs(v), TimeSolve(TimeBuild($"fvs({v})")));
	[TestCase(1)] public void Bvs(float v) => Assert.AreEqual(Formulizer.Provider.Bvs(v), TimeSolve(TimeBuild($"bvs({v})")));
	[TestCase(1)] public void Rnd(float v) => Assert.AreNotEqual(Formulizer.Provider.Rnd(v), TimeSolve(TimeBuild($"rnd({v}")));
	[TestCase(1)] public void Numeric_Abs(float v) => Assert.AreEqual(Formulizer.Provider.Abs(v), TimeSolve(TimeBuild($"abs({v})")));
	[TestCase(1)] public void Numeric_Nml(float v) => Assert.AreEqual(Formulizer.Provider.Nml(v), TimeSolve(TimeBuild($"nml({v})")));
	[TestCase(1, 2, 3)] public void Vector_Abs(float x, float y, float z) => Assert.AreEqual(Formulizer.Provider.Abs(new Vector3(x, y, z)), TimeSolve(TimeBuild("abs(v)"), new Vector3(x, y, z)));
	[TestCase(1, 2, 3)] public void Vector_Nml(float x, float y, float z) => Assert.AreEqual(Formulizer.Provider.Nml(new Vector3(x, y, z)), TimeSolve(TimeBuild("nml(v)"), new Vector3(x, y, z)));
	[TestCase(1, 2, 3)] public void Vector_Qtn(float x, float y, float z) => Assert.AreEqual(Formulizer.Provider.Qtn(new Vector3(x, y, z)), TimeSolve(TimeBuild("qtn(v)"), new Vector3(x, y, z)));
	[TestCase(1, 2, 3)] public void Quaternion_Vec(float x, float y, float z) => Assert.AreEqual(Formulizer.Provider.Vec(Quaternion.CreateFromYawPitchRoll(y, x, z)), TimeSolve(TimeBuild("vec(v)"), Quaternion.CreateFromYawPitchRoll(y, x, z)));

	//Stress
	[Test] public void Stress1() => TimeSolve(TimeBuild($"2 * ((3 - 4) + (5 / (6 + 7)) - 1 + 8 % 3"));

	protected abstract IFormula Build(string formula, params object[] inputs);

	List<double> buildTimes = new List<double>(), solveTimes = new List<double>();

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