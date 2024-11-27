using System.Numerics;

namespace wan24.Core
{
    // Casting
    public static partial class NumericTypesExtensions
    {
        /// <summary>
        /// Cast a number to the numeric type
        /// </summary>
        /// <param name="value">Numeric value</param>
        /// <param name="type">Type</param>
        /// <returns>Casted numeric type</returns>
        /// <exception cref="ArgumentException">Unsupported numeric type</exception>
        public static object CastNumericValue(this object value, in NumericTypes type)
        {
            NumericTypes typeGroup = type.GetTypeGroup();
            if (typeGroup == NumericTypes.Int128)
                switch (value)
                {
                    case Int128 v: return v;
                    case sbyte v: return (Int128)v;
                    case byte v: return (Int128)v;
                    case short v: return (Int128)v;
                    case ushort v: return (Int128)v;
                    case int v: return (Int128)v;
                    case uint v: return (Int128)v;
                    case long v: return (Int128)v;
                    case ulong v: return (Int128)v;
                    case UInt128 v: return (Int128)v;
                    case Half v when Half.IsInteger(v): return (Int128)v;
                    case float v when float.IsInteger(v): return (Int128)v;
                    case double v when double.IsInteger(v): return (Int128)v;
                    case decimal v when decimal.IsInteger(v): return (Int128)v;
                    case BigInteger v: return (Int128)v;
                }
            if (typeGroup == NumericTypes.UInt128)
                switch (value)
                {
                    case UInt128 v: return v;
                    case sbyte v: return (UInt128)v;
                    case byte v: return (UInt128)v;
                    case short v: return (UInt128)v;
                    case ushort v: return (UInt128)v;
                    case int v: return (UInt128)v;
                    case uint v: return (UInt128)v;
                    case long v: return (UInt128)v;
                    case ulong v: return (UInt128)v;
                    case Int128 v: return (UInt128)v;
                    case Half v when Half.IsInteger(v): return (UInt128)v;
                    case float v when float.IsInteger(v): return (UInt128)v;
                    case double v when double.IsInteger(v): return (UInt128)v;
                    case decimal v when decimal.IsInteger(v): return (UInt128)v;
                    case BigInteger v: return (UInt128)v;
                }
            if (typeGroup == NumericTypes.BigInteger)
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
            if (typeGroup == NumericTypes.Half)
                switch (value)
                {
                    case Half v: return v;
                    case sbyte v: return (Half)v;
                    case byte v: return (Half)v;
                    case short v: return (Half)v;
                    case ushort v: return (Half)v;
                    case int v: return (Half)v;
                    case uint v: return (Half)v;
                    case long v: return (Half)v;
                    case ulong v: return (Half)v;
                    case Int128 v: return (Half)v;
                    case UInt128 v: return (Half)v;
                    case float v: return (Half)v;
                    case double v: return (Half)v;
                    case decimal v: return (Half)v;
                    case BigInteger v: return (Half)v;
                }
            Type valueType = value.GetType();
            if (valueType == typeof(Int128))
            {
                Int128 v = value.CastType<Int128>();
                switch (typeGroup)
                {
                    case NumericTypes.Int128: return v;
                    case NumericTypes.SByte: return (sbyte)v;
                    case NumericTypes.Byte: return (byte)v;
                    case NumericTypes.Short: return (short)v;
                    case NumericTypes.UShort: return (ushort)v;
                    case NumericTypes.Int: return (int)v;
                    case NumericTypes.UInt: return (uint)v;
                    case NumericTypes.Long: return (long)v;
                    case NumericTypes.ULong: return (ulong)v;
                    case NumericTypes.UInt128: return (UInt128)v;
                    case NumericTypes.Half: return (Half)v;
                    case NumericTypes.Float: return (float)v;
                    case NumericTypes.Double: return (double)v;
                    case NumericTypes.Decimal: return (decimal)v;
                    case NumericTypes.BigInteger: return (Int128)v;
                }
            }
            if (valueType == typeof(UInt128))
            {
                UInt128 v = value.CastType<UInt128>();
                switch (typeGroup)
                {
                    case NumericTypes.UInt128: return v;
                    case NumericTypes.SByte: return (sbyte)v;
                    case NumericTypes.Byte: return (byte)v;
                    case NumericTypes.Short: return (short)v;
                    case NumericTypes.UShort: return (ushort)v;
                    case NumericTypes.Int: return (int)v;
                    case NumericTypes.UInt: return (uint)v;
                    case NumericTypes.Long: return (long)v;
                    case NumericTypes.ULong: return (ulong)v;
                    case NumericTypes.Int128: return (Int128)v;
                    case NumericTypes.Half: return (Half)v;
                    case NumericTypes.Float: return (float)v;
                    case NumericTypes.Double: return (double)v;
                    case NumericTypes.Decimal: return (decimal)v;
                    case NumericTypes.BigInteger: return (Int128)v;
                }
            }
            if (valueType == typeof(BigInteger))
            {
                BigInteger v = value.CastType<BigInteger>();
                switch (typeGroup)
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
            if (valueType == typeof(Half))
            {
                Half v = value.CastType<Half>();
                switch (typeGroup)
                {
                    case NumericTypes.Half: return v;
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
                    case NumericTypes.Float: return (float)v;
                    case NumericTypes.Double: return (double)v;
                    case NumericTypes.Decimal: return (decimal)v;
                    case NumericTypes.BigInteger: return (BigInteger)v;
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
