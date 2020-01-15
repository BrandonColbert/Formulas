namespace Formulas {
	/// <summary>Provides standard definitions for all formula functions</summary>
	public interface IFormulaProvider {
		object Sin(object v);
		object Asin(object v);
		object Cos(object v);
		object Acos(object v);
		object Tan(object v);
		object Atan(object v);
		object Sqrt(object v);
		object Ln(object v);
		object Log(object v);
		object Sgn(object v);
		object Rvs(object v);
		object Lvs(object v);
		object Uvs(object v);
		object Dvs(object v);
		object Fvs(object v);
		object Bvs(object v);
		object Rnd(object v);
		object Abs(object v);
		object Nml(object v);
		object Qtn(object v);
		object Vec(object v);
	}
}