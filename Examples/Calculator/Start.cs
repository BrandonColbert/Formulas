using Formulas;
using System;
using System.Collections.Generic;
using System.Numerics;

public class Start {
	public static void Main(string[] args) {
		Features.Types.Enable<int>("int");
		Features.Types.Enable<float>("float");
		Features.Types.Enable<double>("double");
		Features.Types.Enable<bool>("bool");
		Features.Types.Enable<string>("string");
		Features.Types.Enable<Number>("number", "num");
		Features.Types.Enable<Vector3>("vector3", "vec3");
		Features.Types.Enable<Quaternion>("quaternion", "quat");
		Features.Types.Enable<Dictionary<string, object>>("dictionary", "map");
		Features.Types.Enable<object[]>("array", "arr");

		var rand = new Random();
		Features.Transforms.Add<Vector3, float>("abs", v => v.Length());
		Features.Transforms.Add<Quaternion, float>("abs", v => v.Length());
		Features.Transforms.Add<double, double>("acos", Math.Acos);
		Features.Transforms.Add<double, double>("asin", Math.Asin);
		Features.Transforms.Add<double, double>("atan", Math.Atan);
		Features.Transforms.Add<float, Vector3>("bvs", v => -Vector3.UnitZ * v);
		Features.Transforms.Add<double, double>("cos", Math.Cos);
		Features.Transforms.Add<float, Vector3>("dvs", v => -Vector3.UnitY * v);
		Features.Transforms.Add<float, Vector3>("fvs", v => Vector3.UnitZ * v);
		Features.Transforms.Add<Quaternion, Quaternion>("inq", Quaternion.Inverse);
		Features.Transforms.Add<double, double>("ln", Math.Log);
		Features.Transforms.Add<double, double>("log", Math.Log10);
		Features.Transforms.Add<float, Vector3>("lvs", v => -Vector3.UnitX * v);
		Features.Transforms.Add<double, double>("nml", v => v / Math.Abs(v));
		Features.Transforms.Add<Vector3, Vector3>("nml", Vector3.Normalize);
		Features.Transforms.Add<Quaternion, Quaternion>("nml", Quaternion.Normalize);
		Features.Transforms.Add<Vector3, Quaternion>("qtn", v => Quaternion.CreateFromYawPitchRoll(v.Y, v.X, v.Z));
		Features.Transforms.Add<double, double>("rnd", v => rand.NextDouble() * v);
		Features.Transforms.Add<float, Vector3>("rvs", v => Vector3.UnitX * v);
		Features.Transforms.Add<double, int>("sgn", Math.Sign);
		Features.Transforms.Add<double, double>("sin", Math.Sin);
		Features.Transforms.Add<double, double>("sqrt", Math.Sqrt);
		Features.Transforms.Add<double, double>("tan", Math.Tan);
		Features.Transforms.Add<float, Vector3>("uvs", v => Vector3.UnitY * v);
		Features.Transforms.Add<Quaternion, Vector3>("vec", v => Vector3.Transform(Vector3.UnitZ, v));

		new Calculator();
	}
}