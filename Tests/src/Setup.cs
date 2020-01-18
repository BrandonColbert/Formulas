using Formulas;
using NUnit.Framework;

[SetUpFixture]
class Setup {
	[OneTimeTearDown]
	public void Cleanup() => NativeFormula.Cleanup();
}