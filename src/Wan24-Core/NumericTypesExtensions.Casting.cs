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
                    BigInteger v => (sbyte)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Short => value switch
                {
                    short v => v,
                    BigInteger v => (short)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Int => value switch
                {
                    int v => v,
                    BigInteger v => (int)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Long => value switch
                {
                    long v => v,
                    BigInteger v => (long)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Half => value switch
                {
                    Half v => v,
                    BigInteger v => (Half)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Float => value switch
                {
                    float v => v,
                    BigInteger v => (float)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Double => value switch
                {
                    double v => v,
                    BigInteger v => (double)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Decimal => value switch
                {
                    decimal v => v,
                    BigInteger v => (decimal)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Byte => value switch
                {
                    byte v => v,
                    BigInteger v => (byte)v,
                    _ => value.CastType(type)
                },
                NumericTypes.UShort => value switch
                {
                    ushort v => v,
                    BigInteger v => (ushort)v,
                    _ => value.CastType(type)
                },
                NumericTypes.UInt => value switch
                {
                    uint v => v,
                    BigInteger v => (uint)v,
                    _ => value.CastType(type)
                },
                NumericTypes.ULong => value switch
                {
                    ulong v => v,
                    BigInteger v => (ulong)v,
                    _ => value.CastType(type)
                },
                NumericTypes.BigInteger => value switch
                {
                    sbyte v => (BigInteger)v,
                    byte v => (BigInteger)v,
                    short v => (BigInteger)v,
                    ushort v => (BigInteger)v,
                    int v => (BigInteger)v,
                    uint v => (BigInteger)v,
                    long v => (BigInteger)v,
                    ulong v => (BigInteger)v,
                    Half v when Half.IsInteger(v) => (BigInteger)v,
                    float v when float.IsInteger(v) => (BigInteger)v,
                    double v when double.IsInteger(v) => (BigInteger)v,
                    decimal v when decimal.IsInteger(v) => (BigInteger)v,
                    BigInteger v => v,
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
                    BigInteger v => (sbyte)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Short => value switch
                {
                    short v => v,
                    BigInteger v => (short)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Int => value switch
                {
                    int v => v,
                    BigInteger v => (int)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Long => value switch
                {
                    long v => v,
                    BigInteger v => (long)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Half => value switch
                {
                    Half v => v,
                    BigInteger v => (Half)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Float => value switch
                {
                    float v => v,
                    BigInteger v => (float)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Double => value switch
                {
                    double v => v,
                    BigInteger v => (double)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Decimal => value switch
                {
                    decimal v => v,
                    BigInteger v => (decimal)v,
                    _ => value.CastType(type)
                },
                NumericTypes.Byte => value switch
                {
                    byte v => v,
                    BigInteger v => (byte)v,
                    _ => value.CastType(type)
                },
                NumericTypes.UShort => value switch
                {
                    ushort v => v,
                    BigInteger v => (ushort)v,
                    _ => value.CastType(type)
                },
                NumericTypes.UInt => value switch
                {
                    uint v => v,
                    BigInteger v => (uint)v,
                    _ => value.CastType(type)
                },
                NumericTypes.ULong => value switch
                {
                    ulong v => v,
                    BigInteger v => (ulong)v,
                    _ => value.CastType(type)
                },
                NumericTypes.BigInteger => value switch
                {
                    sbyte v => (BigInteger)v,
                    byte v => (BigInteger)v,
                    short v => (BigInteger)v,
                    ushort v => (BigInteger)v,
                    int v => (BigInteger)v,
                    uint v => (BigInteger)v,
                    long v => (BigInteger)v,
                    ulong v => (BigInteger)v,
                    Half v when Half.IsInteger(v) => (BigInteger)v,
                    float v when float.IsInteger(v) => (BigInteger)v,
                    double v when double.IsInteger(v) => (BigInteger)v,
                    decimal v when decimal.IsInteger(v) => (BigInteger)v,
                    BigInteger v => v,
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
                    case sbyte v: return (BigInteger)v;
                    case byte v: return (BigInteger)v;
                    case short v: return (BigInteger)v;
                    case ushort v: return (BigInteger)v;
                    case int v: return (BigInteger)v;
                    case uint v: return (BigInteger)v;
                    case long v: return (BigInteger)v;
                    case ulong v: return (BigInteger)v;
                    case Half v when Half.IsInteger(v): return (BigInteger)v;
                    case float v when float.IsInteger(v): return (BigInteger)v;
                    case double v when double.IsInteger(v): return (BigInteger)v;
                    case decimal v when decimal.IsInteger(v): return (BigInteger)v;
                    case BigInteger v: return v;
                }
            if (typeof(T) == typeof(BigInteger))
            {
                BigInteger v = value.CastType<BigInteger>();
                switch (type.GetTypeGroup())
                {
                    case NumericTypes.SByte: return (sbyte)v;
                    case NumericTypes.Byte: return (byte)v;
                    case NumericTypes.Short: return (short)v;
                    case NumericTypes.UShort: return (ushort)v;
                    case NumericTypes.Int: return (int)v;
                    case NumericTypes.UInt: return (uint)v;
                    case NumericTypes.Long: return (long)v;
                    case NumericTypes.ULong: return (ulong)v;
                    case NumericTypes.Half: return (Half)v;
                    case NumericTypes.Float: return (float)v;
                    case NumericTypes.Double: return (double)v;
                    case NumericTypes.Decimal: return (decimal)v;
                    case NumericTypes.BigInteger: return v;
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
                    case sbyte v: return (BigInteger)v;
                    case byte v: return (BigInteger)v;
                    case short v: return (BigInteger)v;
                    case ushort v: return (BigInteger)v;
                    case int v: return (BigInteger)v;
                    case uint v: return (BigInteger)v;
                    case long v: return (BigInteger)v;
                    case ulong v: return (BigInteger)v;
                    case Half v when Half.IsInteger(v): return (BigInteger)v;
                    case float v when float.IsInteger(v): return (BigInteger)v;
                    case double v when double.IsInteger(v): return (BigInteger)v;
                    case decimal v when decimal.IsInteger(v): return (BigInteger)v;
                    case BigInteger v: return v;
                }
            Type valueType = value.GetType();
            if (valueType == typeof(BigInteger))
            {
                BigInteger v = value.CastType<BigInteger>();
                switch (type.GetTypeGroup())
                {
                    case NumericTypes.SByte: return (sbyte)v;
                    case NumericTypes.Byte: return (byte)v;
                    case NumericTypes.Short: return (short)v;
                    case NumericTypes.UShort: return (ushort)v;
                    case NumericTypes.Int: return (int)v;
                    case NumericTypes.UInt: return (uint)v;
                    case NumericTypes.Long: return (long)v;
                    case NumericTypes.ULong: return (ulong)v;
                    case NumericTypes.Half: return (Half)v;
                    case NumericTypes.Float: return (float)v;
                    case NumericTypes.Double: return (double)v;
                    case NumericTypes.Decimal: return (decimal)v;
                    case NumericTypes.BigInteger: return v;
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
