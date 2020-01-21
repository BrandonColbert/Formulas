using System;
using System.Numerics;

namespace Formulas {
	/// <summary>Default provider for all formula methods</summary>
	public class DefaultFormulaProvider : IFormulaProvider {
		Random rand = new Random();

		/// <summary>sin</summary>
		public virtual Number Sin(Number value) => Math.Sin(value);

		/// <summary>asin</summary>
		public virtual Number Asin(Number value) => Math.Asin(value);

		/// <summary>cos</summary>
		public virtual Number Cos(Number value) => Math.Cos(value);

		/// <summary>acos</summary>
		public virtual Number Acos(Number value) => Math.Acos(value);

		/// <summary>tan</summary>
		public virtual Number Tan(Number value) => Math.Tan(value);

		/// <summary>atan</summary>
		public virtual Number Atan(Number value) => Math.Atan(value);

		/// <summary>sqrt</summary>
		public virtual Number Sqrt(Number value) => Math.Sqrt(value);

		/// <summary>ln</summary>
		public virtual Number Ln(Number value) => Math.Log(value);

		/// <summary>log</summary>
		public virtual Number Log(Number value) => Math.Log10(value);

		/// <summary>sgn</summary>
		public virtual Number Sgn(Number value) => Math.Sign(value);

		/// <summary>rvs</summary>
		public virtual object Rvs(Number value) => Vector3.UnitX * value;

		/// <summary>lvs</summary>
		public virtual object Lvs(Number value) => -Vector3.UnitX * value;

		/// <summary>uvs</summary>
		public virtual object Uvs(Number value) => Vector3.UnitY * value;

		/// <summary>dvs</summary>
		public virtual object Dvs(Number value) => -Vector3.UnitY * value;

		/// <summary>fvs</summary>
		public virtual object Fvs(Number value) => Vector3.UnitZ * value;

		/// <summary>bvs</summary>
		public virtual object Bvs(Number value) => -Vector3.UnitZ * value;

		/// <summary>rnd</summary>
		public virtual object Rnd(Number value) => rand.NextDouble() * value;

		/// <summary>abs</summary>
		public virtual object Abs(object value) {
			switch(value) {
				case Vector2 v: return v.Length();
				case Vector3 v: return v.Length();
				case Vector4 v: return v.Length();
				default: return Number.TryParse(value, out var n) ? Math.Abs(n) : throw new NotImplementedException();
			}
		}

		/// <summary>nml</summary>
		public virtual object Nml(object value) {
			switch(value) {
				case Vector2 v: return Vector2.Normalize(v);
				case Vector3 v: return Vector3.Normalize(v);
				case Vector4 v: return Vector4.Normalize(v);
				default: return Number.TryParse(value, out var n) ? n / Math.Abs(n) : throw new NotImplementedException();
			}
		}

		/// <summary>qtn</summary>
		public virtual object Qtn(object value) {
			switch(value) {
				case Vector2 v: return Quaternion.CreateFromYawPitchRoll(v.Y, v.X, 0f);
				case Vector3 v: return Quaternion.CreateFromYawPitchRoll(v.Y, v.X, v.Z);
				default: throw new NotImplementedException($"Not implemented for '{value?.GetType()}'");
			}
		}

		/// <summary>vec</summary>
		public virtual object Vec(object value) {
			switch(value) {
				case Quaternion v: return Vector3.Transform(Vector3.UnitZ, v);
				default: throw new NotImplementedException($"Not implemented for '{value?.GetType()}'");
			}
		}
	}
}