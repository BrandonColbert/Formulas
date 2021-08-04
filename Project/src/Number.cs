#pragma warning disable 1591

using System;
using System.Collections.Generic;

namespace Formulas {
	/// <summary>Any int, long, float, or double</summary>
	public partial struct Number {
		private double value;
		public override bool Equals(object obj) => value.Equals(obj);
		public override int GetHashCode() => value.GetHashCode();
		public override string ToString() => value.ToString();

		/// <summary>Tries to convert an object to a number</summary>
		/// <param name="value">Original object</param>
		/// <param name="number">Resulting number</param>
		/// <returns>Whether the object could be converted to a number</returns>
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
				case decimal v: number.value = (double)v; return true;
				case double v: number.value = (double)v; return true;
				case Number v: number.value = v; return true;
				default: number = double.NaN; return false;
			}
		}

		/// <summary>Tries to convert a string to its numeric equivalent</summary>
		/// <param name="value">Numeric string</param>
		/// <param name="number">Resulting number</param>
		/// <returns>Whether conversion was possible</returns>
		public static bool TryParse(string value, out Number number) {
			if(double.TryParse(value, out var result)) {
				number = result;
				return true;
			}

			number = double.NaN;
			return false;
		}

		/// <param name="type">Type to check</param>
		/// <returns>Whether the type is a number</returns>
		public static bool Is(Type type) => numericTypes.Contains(type);

		/// <typeparam name="T">Type to check</typeparam>
		/// <returns>Whether the type is a number</returns>
		public static bool Is<T>() => Is(typeof(T));

		/// <param name="type">Type to check</param>
		/// <returns>Whether the type is a numeric primitive</returns>
		public static bool IsNumericPrimitive(Type type) => Is(type) && type != typeof(Number);

		/// <typeparam name="T">Type to check</typeparam>
		/// <returns>Whether the type is a numeric primitve</returns>
		public static bool IsNumericPrimitive<T>() => IsNumericPrimitive(typeof(T));

		private static HashSet<Type> numericTypes = new HashSet<Type>(){
			typeof(sbyte),
			typeof(byte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(float),
			typeof(decimal),
			typeof(double),
			typeof(Number)
		};
	}

	public partial struct Number : IComparable {
		public int CompareTo(object obj) => value.CompareTo(obj);
	}

	public partial struct Number : IComparable<Number>, IEquatable<Number> {
		public int CompareTo(Number other) => value.CompareTo(other.value);
		public bool Equals(Number other) => value.Equals(other.value);
	}

	public partial struct Number : IComparable<int>, IEquatable<int> {
		public int CompareTo(int other) => value.CompareTo(other);
		public bool Equals(int other) => value.Equals(other);
		public static implicit operator Number(int value) => new Number{value = value};
		public static explicit operator int(Number number) => (int)number.value;
	}

	public partial struct Number : IComparable<long>, IEquatable<long> {
		public int CompareTo(long other) => value.CompareTo(other);
		public bool Equals(long other) => value.Equals(other);
		public static implicit operator Number(long value) => new Number{value = value};
		public static explicit operator long(Number number) => (long)number.value;
	}

	public partial struct Number : IComparable<float>, IEquatable<float> {
		public int CompareTo(float other) => value.CompareTo(other);
		public bool Equals(float other) => value.Equals(other);
		public static implicit operator Number(float value) => new Number{value = value};
		public static implicit operator float(Number number) => (float)number.value;
	}

	public partial struct Number : IComparable<double>, IEquatable<double> {
		public int CompareTo(double other) => value.CompareTo(other);
		public bool Equals(double other) => value.Equals(other);
		public static implicit operator Number(double value) => new Number{value = (float)value};
		public static implicit operator double(Number number) => number.value;
	}

	public partial struct Number : IConvertible {
		public TypeCode GetTypeCode() => value.GetTypeCode();
		public object ToType(Type conversionType, IFormatProvider provider) => Convert.ChangeType(value, conversionType, provider);
		public bool ToBoolean(IFormatProvider provider) => Convert.ToBoolean(value, provider);
		public byte ToByte(IFormatProvider provider) => Convert.ToByte(value, provider);
		public char ToChar(IFormatProvider provider) => Convert.ToChar(value, provider);
		public DateTime ToDateTime(IFormatProvider provider) => Convert.ToDateTime(value, provider);
		public decimal ToDecimal(IFormatProvider provider) => Convert.ToDecimal(value, provider);
		public double ToDouble(IFormatProvider provider) => Convert.ToDouble(value, provider);
		public short ToInt16(IFormatProvider provider) => Convert.ToInt16(value, provider);
		public int ToInt32(IFormatProvider provider) => Convert.ToInt32(value, provider);
		public long ToInt64(IFormatProvider provider) => Convert.ToInt64(value, provider);
		public sbyte ToSByte(IFormatProvider provider) => Convert.ToSByte(value, provider);
		public float ToSingle(IFormatProvider provider) => Convert.ToSingle(value, provider);
		public string ToString(IFormatProvider provider) => Convert.ToString(value, provider);
		public ushort ToUInt16(IFormatProvider provider) => Convert.ToUInt16(value, provider);
		public uint ToUInt32(IFormatProvider provider) => Convert.ToUInt32(value, provider);
		public ulong ToUInt64(IFormatProvider provider) => Convert.ToUInt64(value, provider);
	}

	public partial struct Number {
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