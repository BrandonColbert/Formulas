using System;
using System.Collections.Generic;
using System.Numerics;
using Formulas;
using NUnit.Framework;

[TestFixture]
class TestTransforms : FormulaTester {
	[TestCase(0)]
	[TestCase(1)]
	[TestCase((float)Math.PI)]
	[TestCase(5f * (float)Math.PI)]
	public void Sin(float v) => Assert.AreEqual((Number)Features.Transforms.Apply("sin", v), (Number)TimeSolve(TimeBuild($"sin({v})")), Precision);

	[TestCase(0)]
	[TestCase(1)]
	[TestCase((float)Math.PI)]
	[TestCase(5f * (float)Math.PI)]
	public void Asin(float v) => Assert.AreEqual((Number)Features.Transforms.Apply("asin", v), (Number)TimeSolve(TimeBuild($"asin({v})")), Precision);

	[TestCase(0)]
	[TestCase(1)]
	[TestCase((float)Math.PI)]
	[TestCase(5f * (float)Math.PI)]
	public void Cos(float v) => Assert.AreEqual((Number)Features.Transforms.Apply("cos", v), (Number)TimeSolve(TimeBuild($"cos({v})")), Precision);

	[TestCase(0)]
	[TestCase(1)]
	[TestCase((float)Math.PI)]
	[TestCase(5f * (float)Math.PI)]
	public void Acos(float v) => Assert.AreEqual((Number)Features.Transforms.Apply("acos", v), (Number)TimeSolve(TimeBuild($"acos({v})")), Precision);

	[TestCase(0)]
	[TestCase(1)]
	[TestCase((float)Math.PI)]
	[TestCase(5f * (float)Math.PI)]
	public void Tan(float v) => Assert.AreEqual((Number)Features.Transforms.Apply("tan", v), (Number)TimeSolve(TimeBuild($"tan({v})")), Precision);

	[TestCase(0)]
	[TestCase(1)]
	[TestCase((float)Math.PI)]
	[TestCase(5f * (float)Math.PI)]
	public void Atan(float v) => Assert.AreEqual((Number)Features.Transforms.Apply("atan", v), (Number)TimeSolve(TimeBuild($"atan({v})")), Precision);

	[TestCase(0)]
	[TestCase(2)]
	[TestCase(16)]
	[TestCase(121)]
	public void Sqrt(float v) => Assert.AreEqual((Number)Features.Transforms.Apply("sqrt", v), (Number)TimeSolve(TimeBuild($"sqrt({v})")), Precision);

	[TestCase(0)]
	[TestCase(1)]
	[TestCase((float)Math.E)]
	[TestCase(14)]
	public void Ln(float v) => Assert.AreEqual((Number)Features.Transforms.Apply("ln", v), (Number)TimeSolve(TimeBuild($"ln({v})")), Precision);

	[TestCase(0)]
	[TestCase(1)]
	[TestCase((float)Math.E)]
	[TestCase(14)]
	public void Log(float v) => Assert.AreEqual((Number)Features.Transforms.Apply("log", v), (Number)TimeSolve(TimeBuild($"log({v})")), Precision);

	[TestCase(-18)]
	[TestCase(-1)]
	[TestCase(1)]
	[TestCase(18)]
	public void Sgn(float v) => Assert.AreEqual((Number)Features.Transforms.Apply("sgn", v), (Number)TimeSolve(TimeBuild($"sgn({v})")), Precision);

	[TestCase(-19)]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(3)]
	[TestCase(1024)]
	public void Rvs(float v) => Assert.AreEqual(Features.Transforms.Apply("rvs", v), TimeSolve(TimeBuild($"rvs({v})")));

	[TestCase(-19)]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(3)]
	[TestCase(1024)]
	public void Lvs(float v) => Assert.AreEqual(Features.Transforms.Apply("lvs", v), TimeSolve(TimeBuild($"lvs({v})")));

	[TestCase(-19)]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(3)]
	[TestCase(1024)]
	public void Uvs(float v) => Assert.AreEqual(Features.Transforms.Apply("uvs", v), TimeSolve(TimeBuild($"uvs({v})")));

	[TestCase(-19)]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(3)]
	[TestCase(1024)]
	public void Dvs(float v) => Assert.AreEqual(Features.Transforms.Apply("dvs", v), TimeSolve(TimeBuild($"dvs({v})")));

	[TestCase(-19)]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(3)]
	[TestCase(1024)]
	public void Fvs(float v) => Assert.AreEqual(Features.Transforms.Apply("fvs", v), TimeSolve(TimeBuild($"fvs({v})")));

	[TestCase(-19)]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(3)]
	[TestCase(1024)]
	public void Bvs(float v) => Assert.AreEqual(Features.Transforms.Apply("bvs", v), TimeSolve(TimeBuild($"bvs({v})")));

	[TestCase(-19)]
	[TestCase(1)]
	[TestCase(3)]
	[TestCase(1024)]
	public void Rnd(float v) {
		var count = 100;
		var generated = new HashSet<float>();
		var formula = TimeBuild($"rnd({v}");

		for(var i = 0; i < count; i++)
			generated.Add((Number)TimeSolve(formula));


		if(generated.Count == 1)
			Assert.Fail($"'rnd({v})' transform does not generate random numbers");
		else if(generated.Count < 0.25 * count)
			Assert.Fail($"'rnd({v})' transform is highly inconsistent with only {generated.Count}/{count} numbers being unique");
		else if(generated.Count < 0.5 * count)
			TestContext.Error.WriteLine($"'rnd({v})' transform is not consistent as {generated.Count}/{count} numbers were unique");
	}

	[TestCase(-(float)Math.PI)]
	[TestCase(-1)]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase((float)Math.PI)]
	public void NumericAbs(float v) => Assert.AreEqual((Number)Features.Transforms.Apply("abs", v), (Number)TimeSolve(TimeBuild($"abs({v})")), Precision);

	[TestCase(-(float)Math.PI)]
	[TestCase(-1)]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase((float)Math.PI)]
	public void NumericNml(float v) => Assert.AreEqual((Number)Features.Transforms.Apply("nml", v), (Number)TimeSolve(TimeBuild($"nml({v})")), Precision);

	[TestCase(1, 2, 3)]
	[TestCase(19, -32, 5)]
	[TestCase(0, 1, 0)]
	[TestCase(-5, -3, -6)]
	public void VectorAbs(float x, float y, float z) => Assert.AreEqual(Features.Transforms.Apply("abs", new Vector3(x, y, z)), TimeSolve(TimeBuild("f(v: vec3) = abs(v)"), new Vector3(x, y, z)));

	[TestCase(1, 2, 3)]
	[TestCase(19, -32, 5)]
	[TestCase(0, 1, 0)]
	[TestCase(-5, -3, -6)]
	public void VectorNml(float x, float y, float z) => Assert.AreEqual(Features.Transforms.Apply("nml", new Vector3(x, y, z)), TimeSolve(TimeBuild("f(v: vec3) = nml(v)"), new Vector3(x, y, z)));

	[TestCase(1, 2, 3)]
	[TestCase(19, -32, 5)]
	[TestCase(0, 1, 0)]
	[TestCase(-5, -3, -6)]
	public void VectorQtn(float x, float y, float z) => Assert.AreEqual(Features.Transforms.Apply("qtn", new Vector3(x, y, z)), TimeSolve(TimeBuild("f(v: vec3) = qtn(v)"), new Vector3(x, y, z)));

	[TestCase(1, 2, 3)]
	[TestCase(19, -32, 5)]
	[TestCase(0, 1, 0)]
	[TestCase(-5, -3, -6)]
	public void QuaternionVec(float x, float y, float z) => Assert.AreEqual(Features.Transforms.Apply("vec", Quaternion.CreateFromYawPitchRoll(y, x, z)), TimeSolve(TimeBuild("f(v: quat) = vec(v)"), Quaternion.CreateFromYawPitchRoll(y, x, z)));

	[TestCase(1, 2, 3)]
	[TestCase(19, -32, 5)]
	[TestCase(0, 1, 0)]
	[TestCase(-5, -3, -6)]
	public void QuaternionAbs(float x, float y, float z) => Assert.AreEqual(Features.Transforms.Apply("abs", Quaternion.CreateFromYawPitchRoll(y, x, z)), TimeSolve(TimeBuild("f(v: quat) = abs(v)"), Quaternion.CreateFromYawPitchRoll(y, x, z)));

	[TestCase(1, 2, 3)]
	[TestCase(19, -32, 5)]
	[TestCase(0, 1, 0)]
	[TestCase(-5, -3, -6)]
	public void QuaternionNml(float x, float y, float z) => Assert.AreEqual(Features.Transforms.Apply("nml", Quaternion.CreateFromYawPitchRoll(y, x, z)), TimeSolve(TimeBuild("f(v: quat) = nml(v)"), Quaternion.CreateFromYawPitchRoll(y, x, z)));

	[TestCase(1, 2, 3)]
	[TestCase(19, -32, 5)]
	[TestCase(0, 1, 0)]
	[TestCase(-5, -3, -6)]
	public void QuaternionInq(float x, float y, float z) => Assert.AreEqual(Features.Transforms.Apply("inq", Quaternion.CreateFromYawPitchRoll(y, x, z)), TimeSolve(TimeBuild("f(v: quat) = inq(v)"), Quaternion.CreateFromYawPitchRoll(y, x, z)));

	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest);
}

[TestFixture]
class TestCompileTransforms : TestTransforms {
	protected override IFormula Build(string source, params string[] rest) => new Formula(source, rest).Compile();
}