namespace Formulas {
	/// <summary>Provides standard definitions for all formula functions</summary>
	public interface IFormulaProvider {
		/// <summary>sin</summary>
		Number Sin(Number v);

		/// <summary>asin</summary>
		Number Asin(Number v);

		/// <summary>cos</summary>
		Number Cos(Number v);

		/// <summary>acos</summary>
		Number Acos(Number v);

		/// <summary>tan</summary>
		Number Tan(Number v);

		/// <summary>atan</summary>
		Number Atan(Number v);

		/// <summary>sqrt</summary>
		Number Sqrt(Number v);

		/// <summary>ln</summary>
		Number Ln(Number v);

		/// <summary>log</summary>
		Number Log(Number v);

		/// <summary>sgn</summary>
		Number Sgn(Number v);

		/// <summary>rvs</summary>
		object Rvs(Number v);

		/// <summary>lvs</summary>
		object Lvs(Number v);

		/// <summary>uvs</summary>
		object Uvs(Number v);

		/// <summary>dvs</summary>
		object Dvs(Number v);

		/// <summary>fvs</summary>
		object Fvs(Number v);

		/// <summary>bvs</summary>
		object Bvs(Number v);

		/// <summary>rnd</summary>
		object Rnd(Number v);

		/// <summary>abs</summary>
		object Abs(object v);

		/// <summary>nml</summary>
		object Nml(object v);

		/// <summary>qtn</summary>
		object Qtn(object v);

		/// <summary>vec</summary>
		object Vec(object v);
	}
}