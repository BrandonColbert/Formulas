using System;
using System.Numerics;
using Formulas;

public class FormulaProvider : IFormulaProvider<Number, Vector3, Quaternion> {
	Random rand = new Random();
	public Number Abs(Number v) => Math.Abs(v);
	public Number Abs(Vector3 v) => v.Length();
	public Number Abs(Quaternion v) => v.Length();
	public Number Acos(Number v) => Math.Acos(v);
	public Number Asin(Number v) => Math.Asin(v);
	public Number Atan(Number v) => Math.Atan(v);
	public Vector3 Bvs(Number v) => -Vector3.UnitZ * v;
	public Number Cos(Number v) => Math.Cos(v);
	public Vector3 Dvs(Number v) => -Vector3.UnitY * v;
	public Vector3 Fvs(Number v) => Vector3.UnitZ * v;
	public Quaternion Inq(Quaternion v) => Quaternion.Inverse(v);
	public Number Ln(Number v) => Math.Log(v);
	public Number Log(Number v) => Math.Log10(v);
	public Vector3 Lvs(Number v) => -Vector3.UnitX * v;
	public Number Nml(Number v) => v / Math.Abs(v);
	public Vector3 Nml(Vector3 v) => Vector3.Normalize(v);
	public Quaternion Nml(Quaternion v) => Quaternion.Normalize(v);
	public Number Pow(Number lhs, Number rhs) => Math.Pow(lhs, rhs);
	public Quaternion Qtn(Vector3 v) => Quaternion.CreateFromYawPitchRoll(v.Y, v.X, v.Z);
	public Number Rnd(Number v) => rand.NextDouble() * v;
	public Vector3 Rvs(Number v) => Vector3.UnitX * v;
	public Number Sgn(Number v) => Math.Sign(v);
	public Number Sin(Number v) => Math.Sin(v);
	public Number Sqrt(Number v) => Math.Sqrt(v);
	public Number Tan(Number v) => Math.Tan(v);
	public Vector3 Uvs(Number v) => Vector3.UnitY * v;
	public Vector3 Vec(Quaternion v) => Vector3.Transform(Vector3.UnitZ, v);
	public bool ToNumber(object v, out Number n) => Number.From(v, out n);
	public bool TryParse(string v, out Number n) => Number.TryParse(v, out n);
}