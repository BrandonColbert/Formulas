using System;
using System.Collections.Generic;
using System.Numerics;
using Formulas;
using NUnit.Framework;

[SetUpFixture]
class Setup {
	[OneTimeSetUp]
	public void Start() {
		Features.Types.Enable(typeof(int), "int");
		Features.Types.Enable(typeof(float), "float");
		Features.Types.Enable(typeof(double), "double");
		Features.Types.Enable(typeof(bool), "bool");
		Features.Types.Enable(typeof(string), "string");
		Features.Types.Enable(typeof(Number), "number", "num");
		Features.Types.Enable(typeof(Vector3), "vector3", "vec3");
		Features.Types.Enable(typeof(Quaternion), "quaternion", "quat");
		Features.Types.Enable(typeof(Dictionary<string, object>), "dictionary", "map");
		Features.Types.Enable(typeof(object[]), "array", "arr");

		var rand = new Random();
		Features.Transforms.Add("abs", new Func<Vector3, Number>(v => v.Length()));
		Features.Transforms.Add("abs", new Func<Quaternion, Number>(v => v.Length()));
		Features.Transforms.Add("acos", new Func<Number, Number>(v => Math.Acos(v)));
		Features.Transforms.Add("asin", new Func<Number, Number>(v => Math.Asin(v)));
		Features.Transforms.Add("atan", new Func<Number, Number>(v => Math.Atan(v)));
		Features.Transforms.Add("bvs", new Func<Number, Vector3>(v => -Vector3.UnitZ * v));
		Features.Transforms.Add("cos", new Func<Number, Number>(v => Math.Cos(v)));
		Features.Transforms.Add("dvs", new Func<Number, Vector3>(v => -Vector3.UnitY * v));
		Features.Transforms.Add("fvs", new Func<Number, Vector3>(v => Vector3.UnitZ * v));
		Features.Transforms.Add("inq", new Func<Quaternion, Quaternion>(v => Quaternion.Inverse(v)));
		Features.Transforms.Add("ln", new Func<Number, Number>(v => Math.Log(v)));
		Features.Transforms.Add("log", new Func<Number, Number>(v => Math.Log10(v)));
		Features.Transforms.Add("lvs", new Func<Number, Vector3>(v => -Vector3.UnitX * v));
		Features.Transforms.Add("nml", new Func<Number, Number>(v => v / Math.Abs(v)));
		Features.Transforms.Add("nml", new Func<Vector3, Vector3>(v => Vector3.Normalize(v)));
		Features.Transforms.Add("nml", new Func<Quaternion, Quaternion>(v => Quaternion.Normalize(v)));
		Features.Transforms.Add("qtn", new Func<Vector3, Quaternion>(v => Quaternion.CreateFromYawPitchRoll(v.Y, v.X, v.Z)));
		Features.Transforms.Add("rnd", new Func<Number, Number>(v => rand.NextDouble() * v));
		Features.Transforms.Add("rvs", new Func<Number, Vector3>(v => Vector3.UnitX * v));
		Features.Transforms.Add("sgn", new Func<Number, Number>(v => Math.Sign(v)));
		Features.Transforms.Add("sin", new Func<Number, Number>(v => Math.Sin(v)));
		Features.Transforms.Add("sqrt", new Func<Number, Number>(v => Math.Sqrt(v)));
		Features.Transforms.Add("tan", new Func<Number, Number>(v => Math.Tan(v)));
		Features.Transforms.Add("uvs", new Func<Number, Vector3>(v => Vector3.UnitY * v));
		Features.Transforms.Add("vec", new Func<Quaternion, Vector3>(v => Vector3.Transform(Vector3.UnitZ, v)));
	}
}