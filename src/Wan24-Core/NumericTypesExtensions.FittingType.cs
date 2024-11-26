using System.Numerics;

namespace wan24.Core
{
    // Fitting type
    public static partial class NumericTypesExtensions
    {
        /// <summary>
        /// Maximum supported non-integer <see cref="Half"/> value
        /// </summary>
        public static readonly Half MaxNonIntHalf = (Half)Math.Pow(2, 10) - Half.Tau;
        /// <summary>
        /// Maximum supported non-integer <see cref="float"/> value
        /// </summary>
        public static readonly float MaxNonIntFloat = (float)Math.Pow(2, 23) - float.Tau;
        /// <summary>
        /// Maximum supported non-integer <see cref="double"/> value
        /// </summary>
        public static readonly double MaxNonIntDouble = Math.Pow(2, 52) - double.Tau;
        /// <summary>
        /// Minimum supported non-integer <see cref="decimal"/> value
        /// </summary>
        public static readonly decimal MinNonIntDecimal = decimal.MinValue + decimal.One / new decimal(lo: 1, mid: 0, hi: 0, isNegative: false, scale: 28);
        /// <summary>
        /// Maximum supported non-integer <see cref="decimal"/> value
        /// </summary>
        public static readonly decimal MaxNonIntDecimal = decimal.MaxValue - decimal.One / new decimal(lo: 1, mid: 0, hi: 0, isNegative: false, scale: 28);

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
                    or Int128
                    or UInt128
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
                if (v > 0 && v < ushort.MaxValue) return NumericTypes.UShort;
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
                    if (v > 0 && v < ulong.MaxValue) return NumericTypes.ULong;
                }
                else
                {
                    if (v >= (double)(Half.Zero - MaxNonIntHalf) && v <= (double)MaxNonIntHalf)
                    {
                        if (v == (double)Half.MinValue) return NumericTypes.HalfMin;
                        if (v == (double)Half.MaxValue) return NumericTypes.HalfMax;
                        return NumericTypes.Half;
                    }
                    if (v >= 0 - MaxNonIntFloat && v <= MaxNonIntFloat)
                    {
                        if (v == float.MinValue) return NumericTypes.FloatMin;
                        if (v == float.MaxValue) return NumericTypes.FloatMax;
                        return NumericTypes.Float;
                    }
                }
                return NumericTypes.Double;
            }
            if (type == typeof(decimal))
            {
                decimal v = (decimal)value;
                if (v == decimal.MaxValue) return NumericTypes.DecimalMax;
                if (v == decimal.MinValue) return NumericTypes.DecimalMin;
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
                    if (v > 0 && v < uint.MaxValue) return NumericTypes.UInt;
                }
                else
                {
                    if (v >= (float)(Half.Zero - MaxNonIntHalf) && v <= (float)MaxNonIntHalf)
                    {
                        if (v == (double)Half.MinValue) return NumericTypes.HalfMin;
                        if (v == (double)Half.MaxValue) return NumericTypes.HalfMax;
                        return NumericTypes.Half;
                    }
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
                int len = v.GetByteCount();
                if (v > int.MinValue && v < int.MaxValue && IsValueNumericType((int)v)) return GetValueNumericType((int)v);
                if (v == sbyte.MinValue) return NumericTypes.SByteMin;
                if (v < 0 && v > sbyte.MinValue) return NumericTypes.SByte;
                if (v == byte.MaxValue) return NumericTypes.ByteMax;
                if (v > 0 && v < byte.MaxValue) return NumericTypes.Byte;
                if (len >= sizeof(short))
                {
                    if (v == short.MinValue) return NumericTypes.ShortMin;
                    if (v == short.MaxValue) return NumericTypes.ShortMax;
                    if (v > short.MinValue && v < short.MaxValue) return NumericTypes.Short;
                    if (v == ushort.MaxValue) return NumericTypes.UShortMax;
                    if (v > 0 && v < ushort.MaxValue) return NumericTypes.UShort;
                    if (len >= sizeof(int))
                    {
                        if (v == int.MinValue) return NumericTypes.IntMin;
                        if (v == int.MaxValue) return NumericTypes.IntMax;
                        if (v > int.MinValue && v < int.MaxValue) return NumericTypes.Int;
                        if (v == uint.MaxValue) return NumericTypes.UIntMax;
                        if (v > 0 && v < uint.MaxValue) return NumericTypes.UInt;
                        if (len >= sizeof(long))
                        {
                            if (v == long.MinValue) return NumericTypes.LongMin;
                            if (v == long.MaxValue) return NumericTypes.LongMax;
                            if (v > long.MinValue && v < long.MaxValue) return NumericTypes.Long;
                            if (v == ulong.MaxValue) return NumericTypes.ULongMax;
                            if (v > 0 && v < ulong.MaxValue) return NumericTypes.ULong;
                            if (len >= sizeof(ulong) << 1)
                            {
                                if (v == Int128.MinValue) return NumericTypes.Int128Min;
                                if (v == Int128.MaxValue) return NumericTypes.Int128Max;
                                if (v > Int128.MinValue && v < Int128.MaxValue) return NumericTypes.Int128;
                                if (v == UInt128.MaxValue) return NumericTypes.UInt128Max;
                                if (v > 0 && v < UInt128.MaxValue) return NumericTypes.UInt128;
                            }
                        }
                    }
                }
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
            if (type == typeof(Int128))
            {
                Int128 v = (Int128)value;
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
                if (v == uint.MaxValue) return NumericTypes.UShortMax;
                if (v == long.MinValue) return NumericTypes.LongMin;
                if (v == long.MaxValue) return NumericTypes.LongMax;
                if (v > long.MinValue && v < long.MaxValue) return NumericTypes.Long;
                if (v == ulong.MaxValue) return NumericTypes.ULongMax;
                if (v > 0 && v < ulong.MaxValue) return NumericTypes.ULong;
                if (v == Int128.MinValue) return NumericTypes.Int128Min;
                if (v == Int128.MaxValue) return NumericTypes.Int128Max;
                return NumericTypes.Int128;
            }
            if (type == typeof(UInt128))
            {
                UInt128 v = (UInt128)value;
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
                if (v < ulong.MaxValue) return NumericTypes.ULong;
                if (v == (UInt128)Int128.MaxValue) return NumericTypes.Int128Max;
                if (v < (UInt128)Int128.MaxValue) return NumericTypes.Int128;
                if (v == UInt128.MaxValue) return NumericTypes.UInt128Max;
                return NumericTypes.UInt128;
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
