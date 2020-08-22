using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Formulas;
using NUnit.Framework;

[TestFixture]
abstract class FormulaTester {
	private const double Precision = 0.001;

	private List<double>
		buildTimes = new List<double>(),
		solveTimes = new List<double>();

	[OneTimeTearDown]
	public void TearDown() => TestContext.Progress.WriteLine($"{GetType().Name}\n\tBuild: {AvgNoOutliers(buildTimes)}ms\n\tSolve: {AvgNoOutliers(solveTimes)}ms");

	protected abstract IFormula Build(string source, params string[] rest);
	protected void Approximately(Number lhs, Number rhs) => Assert.AreEqual(lhs, rhs, Precision);
	protected void Approximately(Number lhs, Number rhs, string message) => Assert.AreEqual(lhs, rhs, Precision, message);

	protected object TimeSolve(IFormula formula, params object[] inputs) {
		var timer = Stopwatch.StartNew();
		var result = formula.Solve(inputs);
		timer.Stop();
		solveTimes.Add(timer.Elapsed.TotalMilliseconds);

		return result;
	}

	protected IFormula TimeBuild(string source, params string[] rest) {
		var timer = Stopwatch.StartNew();
		var result = Build(source, rest);
		timer.Stop();
		buildTimes.Add(timer.Elapsed.TotalMilliseconds);

		return result;
	}

	private double AvgNoOutliers(IEnumerable<double> list) {
		if(list.Count() == 0)
			return double.NaN;

		list = list.OrderByDescending(e => e);
		var (q1, q3) = (list.ElementAt((int)(list.Count() * 0.75)), list.ElementAt((int)(list.Count() * 0.25)));
		var iqr = q3 - q1;

		list = list.Where(e => (q1 - 1.5 * iqr) <= e && e <= (q3 + 1.5 * iqr));
		if(list.Count() == 0)
			return double.NaN;

		return Math.Round(list.Average(), 5);
	}
}