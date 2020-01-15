using System;
using System.Numerics;

namespace Formulas {
	public class DefaultFormulaProvider : IFormulaProvider {
		Random rand = new Random();

		public object Sin(object value) {
			switch(value) {
				case float v: return Math.Sin(v);
				default: throw new NotImplementedException();
			}
		}

		public object Asin(object value) {
			switch(value) {
				case float v: return Math.Asin(v);
				default: throw new NotImplementedException();
			}
		}

		public object Cos(object value) {
			switch(value) {
				case float v: return Math.Cos(v);
				default: throw new NotImplementedException();
			}
		}

		public object Acos(object value) {
			switch(value) {
				case float v: return Math.Acos(v);
				default: throw new NotImplementedException();
			}
		}

		public object Tan(object value) {
			switch(value) {
				case float v: return Math.Tan(v);
				default: throw new NotImplementedException();
			}
		}

		public object Atan(object value) {
			switch(value) {
				case float v: return Math.Atan(v);
				default: throw new NotImplementedException();
			}
		}

		public object Sqrt(object value) {
			switch(value) {
				case float v: return Math.Sqrt(v);
				default: throw new NotImplementedException();
			}
		}

		public object Ln(object value) {
			switch(value) {
				case float v: return Math.Log(v);
				default: throw new NotImplementedException();
			}
		}

		public object Log(object value) {
			switch(value) {
				case float v: return Math.Log10(v);
				default: throw new NotImplementedException();
			}
		}

		public object Sgn(object value) {
			switch(value) {
				case float v: return Math.Sign(v);
				default: throw new NotImplementedException();
			}
		}

		public object Rvs(object value) {
			switch(value) {
				case float v: return Vector3.UnitX * v;
				default: throw new NotImplementedException();
			}
		}

		public object Lvs(object value) {
			switch(value) {
				case float v: return -Vector3.UnitX * v;
				default: throw new NotImplementedException();
			}
		}

		public object Uvs(object value) {
			switch(value) {
				case float v: return Vector3.UnitY * v;
				default: throw new NotImplementedException();
			}
		}

		public object Dvs(object value) {
			switch(value) {
				case float v: return -Vector3.UnitY * v;
				default: throw new NotImplementedException();
			}
		}

		public object Fvs(object value) {
			switch(value) {
				case float v: return Vector3.UnitZ * v;
				default: throw new NotImplementedException();
			}
		}

		public object Bvs(object value) {
			switch(value) {
				case float v: return -Vector3.UnitZ * v;
				default: throw new NotImplementedException();
			}
		}

		public object Rnd(object value) {
			switch(value) {
				case float v: return rand.NextDouble() * v;
				default: throw new NotImplementedException();
			}
		}

		public object Abs(object value) {
			switch(value) {
				case float v: return  Math.Abs(v);
				case Vector2 v: return v.Length();
				case Vector3 v: return v.Length();
				case Vector4 v: return v.Length();
				default: throw new NotImplementedException();
			}
		}

		public object Nml(object value) {
			switch(value) {
				case float v: return  v / Math.Abs(v);
				case Vector2 v: return Vector2.Normalize(v);
				case Vector3 v: return Vector3.Normalize(v);
				case Vector4 v: return Vector4.Normalize(v);
				default: throw new NotImplementedException();
			}
		}

		public object Qtn(object value) {
			switch(value) {
				case Vector2 v: return new Quaternion(new Vector3(v.X, v.Y, 0f), 1f);
				case Vector3 v: return new Quaternion(v, 1f);
				case Vector4 v: return new Quaternion(new Vector3(v.X, v.Y, v.Z), 1f);
				default: throw new NotImplementedException();
			}
		}

		public object Vec(object value) {
			switch(value) {
				case Quaternion v: return new Vector3(v.X, v.Y, v.Z);
				default: throw new NotImplementedException();
			}
		}
	}
}