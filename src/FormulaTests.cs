using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;

namespace Formulas {
	abstract class IFormulaTests {
		protected abstract IFormula Build(string formula, params object[] inputs);

		//Operations
		[TestCase(ExpectedResult=1)] public object Op_MemberAccess() => Build("a.b").Solve(new {b = 1});
		[TestCase(ExpectedResult=1)] public object Op_IndexAccess() => Build("a:b").Solve(new Dictionary<string, object>(){["b"] = 1});
		[TestCase(ExpectedResult=9)] public object Op_Grouping() => Build("3 * (1 + 2)").Solve();
		[TestCase(ExpectedResult=1)] public object Op_Magnitude() => Build("|1 - 2|").Solve();
		[TestCase(ExpectedResult=8)] public object Op_Exponentiate() => Build("2^3").Solve();
		[TestCase(ExpectedResult=6)] public object Op_Multiply() => Build("2 * 3").Solve();
		[TestCase(ExpectedResult=3)] public object Op_Divide() => Build("6 / 2").Solve();
		[TestCase(ExpectedResult=1)] public object Op_Modulus() => Build("6 % 5").Solve();
		[TestCase(ExpectedResult=2)] public object Op_Add() => Build("1 + 1").Solve();
		[TestCase(ExpectedResult=1)] public object Op_Subtract() => Build("2 - 1").Solve();
		[TestCase(ExpectedResult=-1)] public object Op_Negate() => Build("-1").Solve();

		//Input
		[TestCase(ExpectedResult=1)] public object No_Input() => Build("1").Solve();
		[TestCase(ExpectedResult=1)] public object Single_PreInput() => Build("x", 1).Solve();
		[TestCase(ExpectedResult=1)] public object Single_PostInput() => Build("x").Solve(1);
		[TestCase(ExpectedResult=3)] public object Single_PreInput_Single_PostInput() => Build("x + y", 1).Solve(2);
		[TestCase(ExpectedResult=3)] public object Multiple_PreInput() => Build("x + y", 1, 2).Solve();
		[TestCase(ExpectedResult=3)] public object Multiple_PostInput() => Build("x + y").Solve(1, 2);
		[TestCase(ExpectedResult=1)] public object Mapped_Single_PreInput() => Build("f(x) = x", 1).Solve();
		[TestCase(ExpectedResult=1)] public object Mapped_Single_PostInput() => Build("f(x) = x").Solve(1);
		[TestCase(ExpectedResult=3)] public object Mapped_Multiple_PreInput() => Build("f(x, y) = x + y", 1, 2).Solve();
		[TestCase(ExpectedResult=3)] public object Mapped_Multiple_PostInput() => Build("f(x, y) = x + y").Solve(1, 2);
	
		//Functions
		[TestCase(1)] public void Sin(float v) => Assert.AreEqual(Formulizer.Provider.Sin(v), Build($"sin({v})").Solve());
		[TestCase(1)] public void Asin(float v) => Assert.AreEqual(Formulizer.Provider.Asin(v), Build($"asin({v})").Solve());
		[TestCase(1)] public void Cos(float v) => Assert.AreEqual(Formulizer.Provider.Cos(v), Build($"cos({v})").Solve());
		[TestCase(1)] public void Acos(float v) => Assert.AreEqual(Formulizer.Provider.Acos(v), Build($"acos({v})").Solve());
		[TestCase(1)] public void Tan(float v) => Assert.AreEqual(Formulizer.Provider.Tan(v), Build($"tan({v})").Solve());
		[TestCase(1)] public void Atan(float v) => Assert.AreEqual(Formulizer.Provider.Atan(v), Build($"atan({v})").Solve());
		[TestCase(16)] public void Sqrt(float v) => Assert.AreEqual(Formulizer.Provider.Sqrt(v), Build($"sqrt({v})").Solve());
		[TestCase(2)] public void Ln(float v) => Assert.AreEqual(Formulizer.Provider.Ln(v), Build($"ln({v})").Solve());
		[TestCase(2)] public void Log(float v) => Assert.AreEqual(Formulizer.Provider.Log(v), Build($"log({v})").Solve());
		[TestCase(1)] public void Sgn(float v) => Assert.AreEqual(Formulizer.Provider.Sgn(v), Build($"sgn({v})").Solve());
		[TestCase(1)] public void Rvs(float v) => Assert.AreEqual(Formulizer.Provider.Rvs(v), Build($"rvs({v})").Solve());
		[TestCase(1)] public void Lvs(float v) => Assert.AreEqual(Formulizer.Provider.Lvs(v), Build($"lvs({v})").Solve());
		[TestCase(1)] public void Uvs(float v) => Assert.AreEqual(Formulizer.Provider.Uvs(v), Build($"uvs({v})").Solve());
		[TestCase(1)] public void Dvs(float v) => Assert.AreEqual(Formulizer.Provider.Dvs(v), Build($"dvs({v})").Solve());
		[TestCase(1)] public void Fvs(float v) => Assert.AreEqual(Formulizer.Provider.Fvs(v), Build($"fvs({v})").Solve());
		[TestCase(1)] public void Bvs(float v) => Assert.AreEqual(Formulizer.Provider.Bvs(v), Build($"bvs({v})").Solve());
		[TestCase(1)] public void Rnd(float v) => Assert.AreNotEqual(Formulizer.Provider.Rnd(v), Build($"rnd({v}").Solve());

		[TestCase(1)] public void Numeric_Abs(float v) => Assert.AreEqual(Formulizer.Provider.Abs(v), Build($"abs({v})").Solve());
		[TestCase(1)] public void Numeric_Nml(float v) => Assert.AreEqual(Formulizer.Provider.Nml(v), Build($"nml({v})").Solve());

		[Test] public void Vector_Abs() => Assert.AreEqual(Formulizer.Provider.Abs(new Vector3(1, 2, 3)), Build("abs(v)").Solve(new Vector3(1, 2, 3))); 
		[Test] public void Vector_Nml() => Assert.AreEqual(Formulizer.Provider.Nml(new Vector3(1, 2, 3)), Build("nml(v)").Solve(new Vector3(1, 2, 3))); 
		[Test] public void Vector_Qtn() => Assert.AreEqual(Formulizer.Provider.Qtn(new Vector3(1, 2, 3)), Build("qtn(v)").Solve(new Vector3(1, 2, 3))); 
		[Test] public void Quaternion_Vec() => Assert.AreEqual(Formulizer.Provider.Vec(Quaternion.Identity), Build("vec(v)").Solve(Quaternion.Identity)); 
	}

	[TestFixture]
	class FormulaTests : IFormulaTests {
		protected override IFormula Build(string formula, params object[] inputs) => new Formula(formula, inputs);
	}

	// [TestFixture]
	// class NativeFormulaTests : IFormulaTests {
	// 	protected override IFormula Build(string formula, params object[] inputs) => new Formula(formula, inputs).Compile();
	// }

	[SetUpFixture]
	class Setup {
		[OneTimeTearDown] public void Cleanup() => NativeFormula.Cleanup();
	}
}
