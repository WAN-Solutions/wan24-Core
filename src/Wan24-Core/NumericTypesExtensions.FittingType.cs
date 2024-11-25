using System.Numerics;

namespace wan24.Core
{
    // Fitting type
    public static partial class NumericTypesExtensions
    {
        /// <summary>
        /// Get the numeric type of a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Numeric type</returns>
        public static NumericTypes GetNumericType<T>(this T value) where T : struct, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
            => value switch
            {
                // Sorted by ~ most usage out there
                int
                    or double
                    or decimal
                    or float
                    or long
                    or byte
                    or short
                    or BigInteger
                    or sbyte
                    or ushort
                    or uint
                    or ulong
                    or Half
                    => GetNumericType((object)value),
                _ => NumericTypes.None
            };

        /// <summary>
        /// Get the numeric type of a number
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Numeric type (may contain value flags)</returns>
        public static NumericTypes GetNumericType(this object value)
        {
            Type type = value.GetType();
            // Sorted by ~ most usage out there
            if (type == typeof(int))
            {
                int v = (int)value;
                if (IsValueNumericType(v)) return GetValueNumericType(v);
                if (v == sbyte.MinValue) return NumericTypes.SByteMin;
                if (v < 0 && v > sbyte.MinValue) return NumericTypes.SByte;
                if (v == byte.MaxValue) return NumericTypes.ByteMax;
                if (v > 0 && v < byte.MaxValue) return NumericTypes.Byte;
                if (v == short.MinValue) return NumericTypes.ShortMin;
                if (v == short.MaxValue) return NumericTypes.ShortMax;
                if (v > short.MinValue && v < short.MaxValue) return NumericTypes.Short;
                if (v == ushort.MaxValue) return NumericTypes.UShortMax;
                if (v > 0 && v < ushort.MaxValue) return NumericTypes.UInt;
                if (v == int.MinValue) return NumericTypes.IntMin;
                if (v == int.MaxValue) return NumericTypes.IntMax;
                return NumericTypes.Int;
            }
            if (type == typeof(double))
            {
                double v = (double)value;
                if (double.IsNaN(v)) return NumericTypes.DoubleNaN;
                if (v == double.E) return NumericTypes.DoubleE;
                if (v == double.Epsilon) return NumericTypes.DoubleEpsilon;
                if (v == double.MaxValue) return NumericTypes.DoubleMax;
                if (v == double.MinValue) return NumericTypes.DoubleMin;
                if (v == double.NegativeInfinity) return NumericTypes.DoubleNegativeInfinity;
                if (v == double.Pi) return NumericTypes.DoublePi;
                if (v == double.PositiveInfinity) return NumericTypes.DoublePositiveInfinity;
                if (v == double.Tau) return NumericTypes.DoubleTau;
                if (double.IsInteger(v))
                {
                    if (v > int.MinValue && v < int.MaxValue && IsValueNumericType((int)v)) return GetValueNumericType((int)v);
                    if (v == sbyte.MinValue) return NumericTypes.SByteMin;
                    if (v > sbyte.MinValue && v < sbyte.MaxValue) return NumericTypes.SByte;
                    if (v == byte.MaxValue) return NumericTypes.ByteMax;
                    if (v > 0 && v < byte.MaxValue) return NumericTypes.Byte;
                    if (v == short.MinValue) return NumericTypes.ShortMin;
                    if (v == short.MaxValue) return NumericTypes.ShortMax;
                    if (v > short.MinValue && v < short.MaxValue) return NumericTypes.Short;
                    if (v == ushort.MaxValue) return NumericTypes.UShortMax;
                    if (v > 0 && v < ushort.MaxValue) return NumericTypes.UShort;
                    if (v == int.MinValue) return NumericTypes.IntMin;
                    if (v == int.MaxValue) return NumericTypes.IntMax;
                    if (v > int.MinValue && v < int.MaxValue) return NumericTypes.Int;
                    if (v == uint.MaxValue) return NumericTypes.UIntMax;
                    if (v > 0 && v < uint.MaxValue) return NumericTypes.UInt;
                    if (v == long.MinValue) return NumericTypes.LongMin;
                    if (v == long.MaxValue) return NumericTypes.LongMax;
                    if (v > long.MinValue && v < long.MaxValue) return NumericTypes.Long;
                    if (v == ulong.MaxValue) return NumericTypes.ULongMax;
                    if (v > 0 && v < ulong.MaxValue) return NumericTypes.ULong;
                    return NumericTypes.BigInteger;
                }
                return NumericTypes.Double;
            }
            if (type == typeof(decimal))
            {
                decimal v = (decimal)value;
                if (v == decimal.MaxValue) return NumericTypes.DoubleMax;
                if (v == decimal.MinValue) return NumericTypes.DoubleMin;
                if (decimal.IsInteger(v))
                {
                    if (v > int.MinValue && v < int.MaxValue && IsValueNumericType((int)v)) return GetValueNumericType((int)v);
                    if (v == sbyte.MinValue) return NumericTypes.SByteMin;
                    if (v > sbyte.MinValue && v < sbyte.MaxValue) return NumericTypes.SByte;
                    if (v == byte.MaxValue) return NumericTypes.ByteMax;
                    if (v > 0 && v < byte.MaxValue) return NumericTypes.Byte;
                    if (v == short.MinValue) return NumericTypes.ShortMin;
                    if (v == short.MaxValue) return NumericTypes.ShortMax;
                    if (v > short.MinValue && v < short.MaxValue) return NumericTypes.Short;
                    if (v == ushort.MaxValue) return NumericTypes.UShortMax;
                    if (v > 0 && v < ushort.MaxValue) return NumericTypes.UShort;
                    if (v == int.MinValue) return NumericTypes.IntMin;
                    if (v == int.MaxValue) return NumericTypes.IntMax;
                    if (v > int.MinValue && v < int.MaxValue) return NumericTypes.Int;
                    if (v == uint.MaxValue) return NumericTypes.UIntMax;
                    if (v > 0 && v < uint.MaxValue) return NumericTypes.UInt;
                    if (v == long.MinValue) return NumericTypes.LongMin;
                    if (v == long.MaxValue) return NumericTypes.LongMax;
                    if (v > long.MinValue && v < long.MaxValue) return NumericTypes.Long;
                    if (v == ulong.MaxValue) return NumericTypes.ULongMax;
                    if (v > 0 && v < ulong.MaxValue) return NumericTypes.ULong;
                    return NumericTypes.BigInteger;
                }
                return NumericTypes.Decimal;
            }
            if (type == typeof(float))
            {
                float v = (float)value;
                if (float.IsNaN(v)) return NumericTypes.FloatNaN;
                if (v == float.E) return NumericTypes.FloatE;
                if (v == float.Epsilon) return NumericTypes.FloatEpsilon;
                if (v == float.MaxValue) return NumericTypes.FloatMax;
                if (v == float.MinValue) return NumericTypes.FloatMin;
                if (v == float.NegativeInfinity) return NumericTypes.FloatNegativeInfinity;
                if (v == float.Pi) return NumericTypes.FloatPi;
                if (v == float.PositiveInfinity) return NumericTypes.FloatPositiveInfinity;
                if (v == float.Tau) return NumericTypes.FloatTau;
                if (float.IsInteger(v))
                {
                    if (v > int.MinValue && v > int.MaxValue && IsValueNumericType((int)v)) return GetValueNumericType((int)v);
                    if (v == sbyte.MinValue) return NumericTypes.SByteMin;
                    if (v > sbyte.MinValue && v < sbyte.MaxValue) return NumericTypes.SByte;
                    if (v == byte.MaxValue) return NumericTypes.ByteMax;
                    if (v > 0 && v < byte.MaxValue) return NumericTypes.Byte;
                    if (v == short.MinValue) return NumericTypes.ShortMin;
                    if (v == short.MaxValue) return NumericTypes.ShortMax;
                    if (v > short.MinValue && v < short.MaxValue) return NumericTypes.Short;
                    if (v == ushort.MaxValue) return NumericTypes.UShortMax;
                    if (v > 0 && v < ushort.MaxValue) return NumericTypes.UShort;
                    if (v == int.MinValue) return NumericTypes.IntMin;
                    if (v == int.MaxValue) return NumericTypes.IntMax;
                    if (v > int.MinValue && v < int.MaxValue) return NumericTypes.Int;
                    if (v == uint.MaxValue) return NumericTypes.UIntMax;
                    if (v > 0 && v < uint.MaxValue) return NumericTypes.UInt;
                }
                return NumericTypes.Float;
            }
            if (type == typeof(long))
            {
                long v = (long)value;
                if (v > int.MinValue && v < int.MaxValue && IsValueNumericType((int)v)) return GetValueNumericType((int)v);
                if (v == sbyte.MinValue) return NumericTypes.SByteMin;
                if (v < 0 && v > sbyte.MinValue) return NumericTypes.SByte;
                if (v == byte.MaxValue) return NumericTypes.ByteMax;
                if (v > 0 && v < byte.MaxValue) return NumericTypes.Byte;
                if (v == short.MinValue) return NumericTypes.ShortMin;
                if (v == short.MaxValue) return NumericTypes.ShortMax;
                if (v > short.MinValue && v < short.MaxValue) return NumericTypes.Short;
                if (v == ushort.MaxValue) return NumericTypes.UShortMax;
                if (v > 0 && v < ushort.MaxValue) return NumericTypes.UShort;
                if (v == int.MinValue) return NumericTypes.IntMin;
                if (v == int.MaxValue) return NumericTypes.IntMax;
                if (v > int.MinValue && v < int.MaxValue) return NumericTypes.Int;
                if (v == uint.MaxValue) return NumericTypes.UIntMax;
                if (v > 0 && v < uint.MaxValue) return NumericTypes.UInt;
                if (v == long.MinValue) return NumericTypes.LongMin;
                if (v == long.MaxValue) return NumericTypes.LongMax;
                return NumericTypes.Long;
            }
            if (type == typeof(byte))
            {
                byte v = (byte)value;
                if (IsValueNumericType(v)) return GetValueNumericType(v);
                if (v == byte.MaxValue) return NumericTypes.ByteMax;
                return NumericTypes.Byte;
            }
            if (type == typeof(short))
            {
                short v = (short)value;
                if (IsValueNumericType(v)) return GetValueNumericType(v);
                if (v == sbyte.MinValue) return NumericTypes.SByteMin;
                if (v < 0 && v > sbyte.MinValue) return NumericTypes.SByte;
                if (v == byte.MaxValue) return NumericTypes.ByteMax;
                if (v > 0 && v <= byte.MaxValue) return NumericTypes.Byte;
                if (v == short.MinValue) return NumericTypes.ShortMin;
                if (v == short.MaxValue) return NumericTypes.ShortMax;
                return NumericTypes.Short;
            }
            if (type == typeof(BigInteger))
            {
                BigInteger v = (BigInteger)value;
                if (v > int.MinValue && v < int.MaxValue && IsValueNumericType((int)v)) return GetValueNumericType((int)v);
                if (v == sbyte.MinValue) return NumericTypes.SByteMin;
                if (v < 0 && v > sbyte.MinValue) return NumericTypes.SByte;
                if (v == byte.MaxValue) return NumericTypes.ByteMax;
                if (v > 0 && v < byte.MaxValue) return NumericTypes.Byte;
                if (v == short.MinValue) return NumericTypes.ShortMin;
                if (v == short.MaxValue) return NumericTypes.ShortMax;
                if (v > short.MinValue && v < short.MaxValue) return NumericTypes.Short;
                if (v == ushort.MaxValue) return NumericTypes.UShortMax;
                if (v > 0 && v < ushort.MaxValue) return NumericTypes.UShort;
                if (v == int.MinValue) return NumericTypes.IntMin;
                if (v == int.MaxValue) return NumericTypes.IntMax;
                if (v > int.MinValue && v < int.MaxValue) return NumericTypes.Int;
                if (v == uint.MaxValue) return NumericTypes.UIntMax;
                if (v > 0 && v < uint.MaxValue) return NumericTypes.UInt;
                if (v == long.MinValue) return NumericTypes.LongMin;
                if (v == long.MaxValue) return NumericTypes.LongMax;
                if (v > long.MinValue && v < long.MaxValue) return NumericTypes.Long;
                if (v == ulong.MaxValue) return NumericTypes.ULongMax;
                if (v > 0 && v < ulong.MaxValue) return NumericTypes.ULong;
                return NumericTypes.BigInteger;
            }
            if (type == typeof(sbyte))
            {
                sbyte v = (sbyte)value;
                if (IsValueNumericType(v)) return GetValueNumericType(v);
                if (v == sbyte.MinValue) return NumericTypes.SByteMin;
                return NumericTypes.SByte;
            }
            if (type == typeof(ushort))
            {
                ushort v = (ushort)value;
                if (IsValueNumericType(v)) return GetValueNumericType(v);
                if (v == byte.MaxValue) return NumericTypes.ByteMax;
                if (v < byte.MaxValue) return NumericTypes.Byte;
                if (v == short.MaxValue) return NumericTypes.ShortMax;
                if (v < short.MaxValue) return NumericTypes.Short;
                if (v == ushort.MaxValue) return NumericTypes.UShortMax;
                return NumericTypes.UShort;
            }
            if (type == typeof(uint))
            {
                uint v = (uint)value;
                if (v < int.MaxValue && IsValueNumericType((int)v)) return GetValueNumericType((int)v);
                if (v == byte.MaxValue) return NumericTypes.ByteMax;
                if (v < byte.MaxValue) return NumericTypes.Byte;
                if (v == short.MaxValue) return NumericTypes.ShortMax;
                if (v < short.MaxValue) return NumericTypes.Short;
                if (v == ushort.MaxValue) return NumericTypes.UShortMax;
                if (v < ushort.MaxValue) return NumericTypes.UShort;
                if (v == int.MaxValue) return NumericTypes.IntMax;
                if (v < int.MaxValue) return NumericTypes.Int;
                if (v == uint.MaxValue) return NumericTypes.UIntMax;
                return NumericTypes.UInt;
            }
            if (type == typeof(ulong))
            {
                ulong v = (ulong)value;
                if (v < int.MaxValue && IsValueNumericType((int)v)) return GetValueNumericType((int)v);
                if (v == byte.MaxValue) return NumericTypes.ByteMax;
                if (v < byte.MaxValue) return NumericTypes.Byte;
                if (v == (ulong)short.MaxValue) return NumericTypes.ShortMax;
                if (v < (ulong)short.MaxValue) return NumericTypes.Short;
                if (v == ushort.MaxValue) return NumericTypes.UShortMax;
                if (v < ushort.MaxValue) return NumericTypes.UShort;
                if (v == int.MaxValue) return NumericTypes.IntMax;
                if (v < int.MaxValue) return NumericTypes.Int;
                if (v == uint.MaxValue) return NumericTypes.UIntMax;
                if (v < uint.MaxValue) return NumericTypes.UInt;
                if (v == long.MaxValue) return NumericTypes.LongMax;
                if (v < long.MaxValue) return NumericTypes.Long;
                if (v == ulong.MaxValue) return NumericTypes.ULongMax;
                return NumericTypes.ULong;
            }
            if (type == typeof(Half))
            {
                Half v = (Half)value;
                if (Half.IsNaN(v)) return NumericTypes.HalfNaN;
                if (v == Half.E) return NumericTypes.HalfE;
                if (v == Half.Epsilon) return NumericTypes.HalfEpsilon;
                if (v == Half.MaxValue) return NumericTypes.HalfMax;
                if (v == Half.MinValue) return NumericTypes.HalfMin;
                if (v == Half.NegativeInfinity) return NumericTypes.HalfNegativeInfinity;
                if (v == Half.NegativeZero) return NumericTypes.HalfNegativeZero;
                if (v == Half.Pi) return NumericTypes.HalfPi;
                if (v == Half.PositiveInfinity) return NumericTypes.HalfPositiveInfinity;
                if (v == Half.Tau) return NumericTypes.HalfTau;
                if (Half.IsInteger(v))
                {
                    if (IsValueNumericType((int)v)) return GetValueNumericType((int)v);
                    if (v == sbyte.MinValue) return NumericTypes.SByteMin;
                    if (v > sbyte.MinValue && v < sbyte.MaxValue) return NumericTypes.SByte;
                    if (v == byte.MaxValue) return NumericTypes.ByteMax;
                    if (v > Half.Zero && v < byte.MaxValue) return NumericTypes.Byte;
                    if (v == (Half)short.MaxValue) return NumericTypes.ShortMax;
                    if (v > (Half)short.MinValue && v < (Half)short.MaxValue) return NumericTypes.Short;
                    if (v > Half.Zero && v < (Half)ushort.MaxValue) return NumericTypes.UShort;
                }
                return NumericTypes.Half;
            }
            return NumericTypes.None;
        }
    }
}
