#pragma warning disable 1591

using System;

namespace Formulas {
	/// <summary>Any int, long, float, or double</summary>
	public struct Number : IComparable, IComparable<Number>, IEquatable<Number>, IComparable<int>, IEquatable<int>, IComparable<long>, IEquatable<long>, IComparable<float>, IEquatable<float>, IComparable<double>, IEquatable<double> {
		float value;

		public override bool Equals(object obj) => value.Equals(obj);
		public override int GetHashCode() => value.GetHashCode();
		public override string ToString() => value.ToString();

		//Comparison and equality
		public int CompareTo(object obj) => value.CompareTo(obj);
		public int CompareTo(int other) => value.CompareTo(other);
		public bool Equals(int other) => value.Equals(other);
		public int CompareTo(long other) => value.CompareTo(other);
		public bool Equals(long other) => value.Equals(other);
		public int CompareTo(float other) => value.CompareTo(other);
		public bool Equals(float other) => value.Equals(other);
		public int CompareTo(double other) => value.CompareTo(other);
		public bool Equals(double other) => value.Equals(other);
		public int CompareTo(Number other) => value.CompareTo(other.value);
		public bool Equals(Number other) => value.Equals(other.value);

		//Conversion from
		public static implicit operator Number(int value) => new Number{value = value};
		public static implicit operator Number(long value) => new Number{value = value};
		public static implicit operator Number(float value) => new Number{value = value};
		public static implicit operator Number(double value) => new Number{value = (float)value};

		//Conversions into
		public static explicit operator int(Number number) => (int)number.value;
		public static explicit operator long(Number number) => (long)number.value;
		public static implicit operator float(Number number) => number.value;
		public static implicit operator double(Number number) => number.value;

		//From object
		public static bool From(object value, out Number number) {
			switch(value) {
				case sbyte v: number.value = v; return true;
				case byte v: number.value = v; return true;
				case short v: number.value = v; return true;
				case ushort v: number.value = v; return true;
				case int v: number.value = v; return true;
				case uint v: number.value = v; return true;
				case long v: number.value = v; return true;
				case ulong v: number.value = v; return true;
				case float v: number.value = v; return true;
				case decimal v: number.value = (float)v; return true;
				case double v: number.value = (float)v; return true;
				case Number v: number.value = v; return true;
				default: number = float.NaN; return false;
			}
		}

		public static bool TryParse(string value, out Number number) {
			if(float.TryParse(value, out var result)) {
				number = result;
				return true;
			}

			number = float.NaN;
			return false;
		}

		//Operators
		public static Number operator +(Number n) => n.value;
		public static Number operator -(Number n) => -n.value;
		public static Number operator ++(Number n) => ++n.value;
		public static Number operator --(Number n) => --n.value;
		public static Number operator +(Number lhs, Number rhs) => lhs.value + rhs.value;
		public static Number operator -(Number lhs, Number rhs) => lhs.value - rhs.value;
		public static Number operator *(Number lhs, Number rhs) => lhs.value * rhs.value;
		public static Number operator /(Number lhs, Number rhs) => lhs.value / rhs.value;
		public static Number operator %(Number lhs, Number rhs) => lhs.value % rhs.value;
		public static bool operator ==(Number lhs, Number rhs) => lhs.value == rhs.value;
		public static bool operator !=(Number lhs, Number rhs) => lhs.value != rhs.value;
		public static bool operator <(Number lhs, Number rhs) => lhs.value < rhs.value;
		public static bool operator >(Number lhs, Number rhs) => lhs.value > rhs.value;
		public static bool operator >=(Number lhs, Number rhs) => lhs.value >= rhs.value;
		public static bool operator <=(Number lhs, Number rhs) => lhs.value <= rhs.value;
	}
}