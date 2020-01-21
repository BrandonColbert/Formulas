#pragma warning disable 1591
using System;

namespace Formulas {
	/// <summary>Any int, long, float, or double</summary>
	public struct Number : IComparable, IComparable<int>, IEquatable<int>, IComparable<long>, IEquatable<long>, IComparable<float>, IEquatable<float>, IComparable<double>, IEquatable<double> {
		double value;

		public override string ToString() => value.ToString();

		//Comparison and equality
		public int CompareTo(int other) => value.CompareTo(other);
		public bool Equals(int other) => value.Equals(other);
		public int CompareTo(long other) => value.CompareTo(other);
		public bool Equals(long other) => value.Equals(other);
		public int CompareTo(float other) => value.CompareTo(other);
		public bool Equals(float other) => value.Equals(other);
		public int CompareTo(double other) => value.CompareTo(other);
		public bool Equals(double other) => value.Equals(other);
		public int CompareTo(object obj) => value.CompareTo(obj);

		//Conversion from
		public static implicit operator Number(int value) => new Number{value = value};
		public static implicit operator Number(long value) => new Number{value = value};
		public static implicit operator Number(float value) => new Number{value = value};
		public static implicit operator Number(double value) => new Number{value = value};
		
		//Conversions into
		public static implicit operator int(Number number) => (int)number.value;
		public static implicit operator long(Number number) => (long)number.value;
		public static implicit operator float(Number number) => (float)number.value;
		public static implicit operator double(Number number) => number.value;

		//From object
		public static bool TryParse(object value, out Number number) {
			switch(value) {
				case sbyte v: number = v; return true;
				case byte v: number = v; return true;
				case short v: number = v; return true;
				case ushort v: number = v; return true;
				case int v: number = v; return true;
				case uint v: number = v; return true;
				case long v: number = v; return true;
				case ulong v: number = v; return true;
				case float v: number = v; return true;
				case decimal v: number = (double)v; return true;
				case double v: number = v; return true;
				case Number v: number = v; return true;
				default: number = double.NaN; return false;
			}
		}
	}
}