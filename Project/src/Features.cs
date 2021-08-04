using System;

namespace Formulas {
	/// <summary>Allows functions to be added and types to be enabled for formulas</summary>
	public static class Features {
		/// <summary>Types registry</summary>
		public static Types Types { get; } = new Types();

		/// <summary>Transform function registry</summary>
		public static Transforms Transforms { get; } = new Transforms{
			[MagnitudeNode.TransformName] = new Transforms.Function<Number, Number>(x => Math.Abs(x))
		};
	}
}