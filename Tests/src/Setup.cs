using System;
using System.Numerics;
using Formulas;
using NUnit.Framework;

[SetUpFixture]
class Setup {
	[OneTimeSetUp]
	public void Start() {
		Features.EnableType(typeof(int), "int");
		Features.EnableType(typeof(float), "float");
		Features.EnableType(typeof(double), "double");
		Features.EnableType(typeof(bool), "bool");
		Features.EnableType(typeof(string), "string");
		Features.EnableType(typeof(Number), "number", "num");
		Features.EnableType(typeof(Vector3), "vector3", "vec3");
		Features.EnableType(typeof(Quaternion), "quaternion", "quat");
		Features.EnableType(typeof(System.Collections.IDictionary), "dictionary", "map");

		var rand = new Random();
		Features.AddFunction("abs", new Func<Vector3, Number>(v => v.Length()));
		Features.AddFunction("abs", new Func<Quaternion, Number>(v => v.Length()));
		Features.AddFunction("acos", new Func<Number, Number>(v => Math.Acos(v)));
		Features.AddFunction("asin", new Func<Number, Number>(v => Math.Asin(v)));
		Features.AddFunction("atan", new Func<Number, Number>(v => Math.Atan(v)));
		Features.AddFunction("bvs", new Func<Number, Vector3>(v => -Vector3.UnitZ * v));
		Features.AddFunction("cos", new Func<Number, Number>(v => Math.Cos(v)));
		Features.AddFunction("dvs", new Func<Number, Vector3>(v => -Vector3.UnitY * v));
		Features.AddFunction("fvs", new Func<Number, Vector3>(v => Vector3.UnitZ * v));
		Features.AddFunction("inq", new Func<Quaternion, Quaternion>(v => Quaternion.Inverse(v)));
		Features.AddFunction("ln", new Func<Number, Number>(v => Math.Log(v)));
		Features.AddFunction("log", new Func<Number, Number>(v => Math.Log10(v)));
		Features.AddFunction("lvs", new Func<Number, Vector3>(v => -Vector3.UnitX * v));
		Features.AddFunction("nml", new Func<Number, Number>(v => v / Math.Abs(v)));
		Features.AddFunction("nml", new Func<Vector3, Vector3>(v => Vector3.Normalize(v)));
		Features.AddFunction("nml", new Func<Quaternion, Quaternion>(v => Quaternion.Normalize(v)));
		Features.AddFunction("qtn", new Func<Vector3, Quaternion>(v => Quaternion.CreateFromYawPitchRoll(v.Y, v.X, v.Z)));
		Features.AddFunction("rnd", new Func<Number, Number>(v => rand.NextDouble() * v));
		Features.AddFunction("rvs", new Func<Number, Vector3>(v => Vector3.UnitX * v));
		Features.AddFunction("sgn", new Func<Number, Number>(v => Math.Sign(v)));
		Features.AddFunction("sin", new Func<Number, Number>(v => Math.Sin(v)));
		Features.AddFunction("sqrt", new Func<Number, Number>(v => Math.Sqrt(v)));
		Features.AddFunction("tan", new Func<Number, Number>(v => Math.Tan(v)));
		Features.AddFunction("uvs", new Func<Number, Vector3>(v => Vector3.UnitY * v));
		Features.AddFunction("vec", new Func<Quaternion, Vector3>(v => Vector3.Transform(Vector3.UnitZ, v)));
	}
}