using System.Numerics;

namespace wan24.Core
{
    // Casting
    public static partial class NumericTypesExtensions
    {
        /// <summary>
        /// Cast a number to the numeric type
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="value">Numeric value</param>
        /// <param name="type">Type</param>
        /// <returns>Casted numeric type</returns>
        /// <exception cref="ArgumentException">Unsupported numeric type</exception>
        public static object CastNumericValue<T>(this T value, in NumericTypes type) where T : struct, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
            => type.GetTypeGroup() switch
            {
                NumericTypes.SByte => value switch
                {
                    sbyte v => v,
                    Int128 v => (sbyte)v,
                    UInt128 v => (sbyte)v,
                    Half v => (sbyte)v,
                    BigInteger v => (sbyte)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Short => value switch
                {
                    short v => v,
                    Int128 v => (short)v,
                    UInt128 v => (short)v,
                    Half v => (short)v,
                    BigInteger v => (short)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Int => value switch
                {
                    int v => v,
                    Int128 v => (int)v,
                    UInt128 v => (int)v,
                    Half v => (int)v,
                    BigInteger v => (int)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Long => value switch
                {
                    long v => v,
                    Int128 v => (long)v,
                    UInt128 v => (long)v,
                    Half v => (long)v,
                    BigInteger v => (long)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Int128 => value switch
                {
                    Int128 v => v,
                    UInt128 v => (Int128)v,
                    Half v => (Int128)v,
                    BigInteger v => (Int128)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Half => value switch
                {
                    Half v => v,
                    Int128 v => (Half)v,
                    UInt128 v => (Half)v,
                    BigInteger v => (Half)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Float => value switch
                {
                    float v => v,
                    Int128 v => (float)v,
                    UInt128 v => (float)v,
                    Half v => (float)v,
                    BigInteger v => (float)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Double => value switch
                {
                    double v => v,
                    Int128 v => (double)v,
                    UInt128 v => (double)v,
                    Half v => (double)v,
                    BigInteger v => (double)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Decimal => value switch
                {
                    decimal v => v,
                    Int128 v => (decimal)v,
                    UInt128 v => (decimal)v,
                    Half v => (decimal)v,
                    BigInteger v => (decimal)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Byte => value switch
                {
                    byte v => v,
                    Int128 v => (byte)v,
                    UInt128 v => (byte)v,
                    Half v => (byte)v,
                    BigInteger v => (byte)v,
                    _ => value.CastType(type)
                },
                NumericTypes.UShort => value switch
                {
                    ushort v => v,
                    Int128 v => (ushort)v,
                    UInt128 v => (ushort)v,
                    Half v => (ushort)v,
                    BigInteger v => (ushort)v,
                    _ => value.CastType(type)
                },
                NumericTypes.UInt => value switch
                {
                    uint v => v,
                    Int128 v => (uint)v,
                    UInt128 v => (uint)v,
                    Half v => (uint)v,
                    BigInteger v => (uint)v,
                    _ => value.CastType(type)
                },
                NumericTypes.ULong => value switch
                {
                    ulong v => v,
                    Int128 v => (ulong)v,
                    UInt128 v => (ulong)v,
                    Half v => (ulong)v,
                    BigInteger v => (ulong)v,
                    _ => value.CastType(type)
                },
                NumericTypes.UInt128 => value switch
                {
                    UInt128 v => v,
                    Int128 v => (UInt128)v,
                    Half v => (UInt128)v,
                    BigInteger v => (UInt128)v,
                    _ => value.CastType(type)
                },
                NumericTypes.BigInteger => value switch
                {
                    BigInteger v => v,
                    sbyte v => (BigInteger)v,
                    byte v => (BigInteger)v,
                    short v => (BigInteger)v,
                    ushort v => (BigInteger)v,
                    int v => (BigInteger)v,
                    uint v => (BigInteger)v,
                    long v => (BigInteger)v,
                    Int128 v => (BigInteger)v,
                    ulong v => (BigInteger)v,
                    UInt128 v => (BigInteger)v,
                    Half v when Half.IsInteger(v) => (BigInteger)v,
                    float v when float.IsInteger(v) => (BigInteger)v,
                    double v when double.IsInteger(v) => (BigInteger)v,
                    decimal v when decimal.IsInteger(v) => (BigInteger)v,
                    _ => value.CastType(type)
                },
                _ => throw new ArgumentException($"Unsupported type #{(byte)type}")
            };

        /// <summary>
        /// Cast a number to the numeric type
        /// </summary>
        /// <param name="value">Numeric value</param>
        /// <param name="type">Type</param>
        /// <returns>Casted numeric type</returns>
        /// <exception cref="ArgumentException">Unsupported numeric type</exception>
        public static object CastNumericValue(this object value, in NumericTypes type)
            => type.GetTypeGroup() switch
            {
                NumericTypes.SByte => value switch
                {
                    sbyte v => v,
                    Int128 v => (sbyte)v,
                    UInt128 v => (sbyte)v,
                    Half v => (sbyte)v,
                    BigInteger v => (sbyte)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Short => value switch
                {
                    short v => v,
                    Int128 v => (short)v,
                    UInt128 v => (short)v,
                    Half v => (short)v,
                    BigInteger v => (short)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Int => value switch
                {
                    int v => v,
                    Int128 v => (int)v,
                    UInt128 v => (int)v,
                    Half v => (int)v,
                    BigInteger v => (int)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Long => value switch
                {
                    long v => v,
                    Int128 v => (long)v,
                    UInt128 v => (long)v,
                    Half v => (long)v,
                    BigInteger v => (long)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Int128 => value switch
                {
                    Int128 v => v,
                    UInt128 v => (Int128)v,
                    Half v => (Int128)v,
                    BigInteger v => (Int128)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Half => value switch
                {
                    Half v => v,
                    Int128 v => (Half)v,
                    UInt128 v => (Half)v,
                    BigInteger v => (Half)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Float => value switch
                {
                    float v => v,
                    Int128 v => (float)v,
                    UInt128 v => (float)v,
                    Half v => (float)v,
                    BigInteger v => (float)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Double => value switch
                {
                    double v => v,
                    Int128 v => (double)v,
                    UInt128 v => (double)v,
                    Half v => (double)v,
                    BigInteger v => (double)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Decimal => value switch
                {
                    decimal v => v,
                    Int128 v => (decimal)v,
                    UInt128 v => (decimal)v,
                    Half v => (decimal)v,
                    BigInteger v => (decimal)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Byte => value switch
                {
                    byte v => v,
                    Int128 v => (byte)v,
                    UInt128 v => (byte)v,
                    Half v => (byte)v,
                    BigInteger v => (byte)v,
                    _ => value.CastType(type)
                },
                NumericTypes.UShort => value switch
                {
                    ushort v => v,
                    Int128 v => (ushort)v,
                    UInt128 v => (ushort)v,
                    Half v => (ushort)v,
                    BigInteger v => (ushort)v,
                    _ => value.CastType(type)
                },
                NumericTypes.UInt => value switch
                {
                    uint v => v,
                    Int128 v => (uint)v,
                    UInt128 v => (uint)v,
                    Half v => (uint)v,
                    BigInteger v => (uint)v,
                    _ => value.CastType(type)
                },
                NumericTypes.ULong => value switch
                {
                    ulong v => v,
                    Int128 v => (ulong)v,
                    UInt128 v => (ulong)v,
                    Half v => (ulong)v,
                    BigInteger v => (ulong)v,
                    _ => value.CastType(type)
                },
                NumericTypes.UInt128 => value switch
                {
                    UInt128 v => v,
                    Int128 v => (UInt128)v,
                    Half v => (UInt128)v,
                    BigInteger v => (UInt128)v,
                    _ => value.CastType(type)
                },
                NumericTypes.BigInteger => value switch
                {
                    BigInteger v => v,
                    sbyte v => (BigInteger)v,
                    byte v => (BigInteger)v,
                    short v => (BigInteger)v,
                    ushort v => (BigInteger)v,
                    int v => (BigInteger)v,
                    uint v => (BigInteger)v,
                    long v => (BigInteger)v,
                    Int128 v => (BigInteger)v,
                    ulong v => (BigInteger)v,
                    UInt128 v => (BigInteger)v,
                    Half v when Half.IsInteger(v) => (BigInteger)v,
                    float v when float.IsInteger(v) => (BigInteger)v,
                    double v when double.IsInteger(v) => (BigInteger)v,
                    decimal v when decimal.IsInteger(v) => (BigInteger)v,
                    _ => value.CastType(type)
                },
                _ => throw new ArgumentException($"Unsupported type #{(byte)type}")
            };

        /// <summary>
        /// Cast to a numeric type
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="type">Target numeric type</param>
        /// <returns>Casted value</returns>
        public static object CastType<T>(this T value, in NumericTypes type) where T : struct, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
        {
            if (type == NumericTypes.BigInteger)
                switch (value)
                {
                    case BigInteger v: return v;
                    case sbyte v: return (BigInteger)v;
                    case byte v: return (BigInteger)v;
                    case short v: return (BigInteger)v;
                    case ushort v: return (BigInteger)v;
                    case int v: return (BigInteger)v;
                    case uint v: return (BigInteger)v;
                    case long v: return (BigInteger)v;
                    case ulong v: return (BigInteger)v;
                    case Int128 v: return (BigInteger)v;
                    case UInt128 v: return (BigInteger)v;
                    case Half v when Half.IsInteger(v): return (BigInteger)v;
                    case float v when float.IsInteger(v): return (BigInteger)v;
                    case double v when double.IsInteger(v): return (BigInteger)v;
                    case decimal v when decimal.IsInteger(v): return (BigInteger)v;
                }
            if (typeof(T) == typeof(BigInteger))
            {
                BigInteger v = value.CastType<BigInteger>();
                switch (type.GetTypeGroup())
                {
                    case NumericTypes.BigInteger: return v;
                    case NumericTypes.SByte: return (sbyte)v;
                    case NumericTypes.Byte: return (byte)v;
                    case NumericTypes.Short: return (short)v;
                    case NumericTypes.UShort: return (ushort)v;
                    case NumericTypes.Int: return (int)v;
                    case NumericTypes.UInt: return (uint)v;
                    case NumericTypes.Long: return (long)v;
                    case NumericTypes.ULong: return (ulong)v;
                    case NumericTypes.Int128: return (Int128)v;
                    case NumericTypes.UInt128: return (UInt128)v;
                    case NumericTypes.Half: return (Half)v;
                    case NumericTypes.Float: return (float)v;
                    case NumericTypes.Double: return (double)v;
                    case NumericTypes.Decimal: return (decimal)v;
                }
            }
            Type clrType = type.GetClrType();
            return value.GetType() == clrType
                ? value
                : Convert.ChangeType(value, clrType);
        }

        /// <summary>
        /// Cast to a numeric type
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="type">Target numeric type</param>
        /// <returns>Casted value</returns>
        /// <exception cref="ArgumentException">Value is not a number</exception>
        public static object CastType(this object value, in NumericTypes type)
        {
            if (type == NumericTypes.BigInteger)
                switch (value)
                {
                    case BigInteger v: return v;
                    case sbyte v: return (BigInteger)v;
                    case byte v: return (BigInteger)v;
                    case short v: return (BigInteger)v;
                    case ushort v: return (BigInteger)v;
                    case int v: return (BigInteger)v;
                    case uint v: return (BigInteger)v;
                    case long v: return (BigInteger)v;
                    case ulong v: return (BigInteger)v;
                    case Int128 v: return (BigInteger)v;
                    case UInt128 v: return (BigInteger)v;
                    case Half v when Half.IsInteger(v): return (BigInteger)v;
                    case float v when float.IsInteger(v): return (BigInteger)v;
                    case double v when double.IsInteger(v): return (BigInteger)v;
                    case decimal v when decimal.IsInteger(v): return (BigInteger)v;
                }
            Type valueType = value.GetType();
            if (valueType == typeof(BigInteger))
            {
                BigInteger v = value.CastType<BigInteger>();
                switch (type.GetTypeGroup())
                {
                    case NumericTypes.BigInteger: return v;
                    case NumericTypes.SByte: return (sbyte)v;
                    case NumericTypes.Byte: return (byte)v;
                    case NumericTypes.Short: return (short)v;
                    case NumericTypes.UShort: return (ushort)v;
                    case NumericTypes.Int: return (int)v;
                    case NumericTypes.UInt: return (uint)v;
                    case NumericTypes.Long: return (long)v;
                    case NumericTypes.ULong: return (ulong)v;
                    case NumericTypes.Int128: return (Int128)v;
                    case NumericTypes.UInt128: return (UInt128)v;
                    case NumericTypes.Half: return (Half)v;
                    case NumericTypes.Float: return (float)v;
                    case NumericTypes.Double: return (double)v;
                    case NumericTypes.Decimal: return (decimal)v;
                }
            }
            if (!valueType.IsValueType || !typeof(IConvertible).IsAssignableFrom(valueType))
                throw new ArgumentException("Not a number", nameof(value));
            Type clrType = type.GetClrType();
            if (clrType == typeof(void)) throw new ArgumentException("Not a number", nameof(value));
            return valueType == clrType
                ? value
                : Convert.ChangeType(value, clrType);
        }
    }
}
