using System.Numerics;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="NumericTypes"/> extensions
    /// </summary>
    public static class NumericTypesExtensions
    {
        /// <summary>
        /// Minimum numeric value that fits into a <see cref="NumericTypes"/> value
        /// </summary>
        public const int MIN_VALUE = -1;
        /// <summary>
        /// Maximum numeric value that fits into a <see cref="NumericTypes"/> value
        /// </summary>
        public const int MAX_VALUE = 198;

        /// <summary>
        /// Get the default value of a numeric type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Default value</returns>
        /// <exception cref="ArgumentException">Unsupported numeric type</exception>
        public static object GetDefault(this NumericTypes type)
            => type.GetTypeGroup() switch
            {
                NumericTypes.SByte => default(sbyte),
                NumericTypes.Short => default(short),
                NumericTypes.Int => default(int),
                NumericTypes.Long => default(long),
                NumericTypes.Half => Half.Zero,
                NumericTypes.Float => default(float),
                NumericTypes.Double => default(double),
                NumericTypes.Decimal => decimal.Zero,
                NumericTypes.Byte => default(byte),
                NumericTypes.UShort => default(ushort),
                NumericTypes.UInt => default(uint),
                NumericTypes.ULong => default(ulong),
                NumericTypes.BigInteger => BigInteger.Zero,
                _ => throw new ArgumentException($"Unsupported type #{(byte)type}")
            };

        /// <summary>
        /// Get the CLR type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>CLR type or <see cref="void"/></returns>
        public static Type GetClrType(this NumericTypes type)
            => type.GetTypeGroup() switch
            {
                NumericTypes.SByte => typeof(sbyte),
                NumericTypes.Short => typeof(short),
                NumericTypes.Int => typeof(int),
                NumericTypes.Long => typeof(long),
                NumericTypes.Half => typeof(Half),
                NumericTypes.Float => typeof(float),
                NumericTypes.Double => typeof(double),
                NumericTypes.Decimal => typeof(decimal),
                NumericTypes.Byte => typeof(byte),
                NumericTypes.UShort => typeof(ushort),
                NumericTypes.UInt => typeof(uint),
                NumericTypes.ULong => typeof(ulong),
                NumericTypes.BigInteger => typeof(BigInteger),
                _ => typeof(void)
            };

        /// <summary>
        /// Cast a number to the numeric type
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="value">Numeric value</param>
        /// <param name="type">Type</param>
        /// <returns>Casted numeric type</returns>
        /// <exception cref="ArgumentException">Unsupported numeric type</exception>
        public static object CastNumericValue<T>(this T value, in NumericTypes type) where T : struct, IConvertible
            => type.GetTypeGroup() switch
            {
                NumericTypes.SByte => value is sbyte ? value : value.CastType<sbyte>(),
                NumericTypes.Short => value is short ? value : value.CastType<short>(),
                NumericTypes.Int => value is int ? value : value.CastType<int>(),
                NumericTypes.Long => value is long ? value : value.CastType<long>(),
                NumericTypes.Half => value is Half ? value : value.CastType<Half>(),
                NumericTypes.Float => value is float ? value : value.CastType<float>(),
                NumericTypes.Double => value is double ? value : value.CastType<double>(),
                NumericTypes.Decimal => value is decimal ? value : value.CastType<decimal>(),
                NumericTypes.Byte => value is byte ? value : value.CastType<byte>(),
                NumericTypes.UShort => value is ushort ? value : value.CastType<ushort>(),
                NumericTypes.UInt => value is uint ? value : value.CastType<uint>(),
                NumericTypes.ULong => value is ulong ? value : value.CastType<ulong>(),
                NumericTypes.BigInteger => value is BigInteger ? value : value.CastType<BigInteger>(),
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
                NumericTypes.SByte => value is sbyte ? value : value.CastType<sbyte>(),
                NumericTypes.Short => value is short ? value : value.CastType<short>(),
                NumericTypes.Int => value is int ? value : value.CastType<int>(),
                NumericTypes.Long => value is long ? value : value.CastType<long>(),
                NumericTypes.Half => value is Half ? value : value.CastType<Half>(),
                NumericTypes.Float => value is float ? value : value.CastType<float>(),
                NumericTypes.Double => value is double ? value : value.CastType<double>(),
                NumericTypes.Decimal => value is decimal ? value : value.CastType<decimal>(),
                NumericTypes.Byte => value is byte ? value : value.CastType<byte>(),
                NumericTypes.UShort => value is ushort ? value : value.CastType<ushort>(),
                NumericTypes.UInt => value is uint ? value : value.CastType<uint>(),
                NumericTypes.ULong => value is ulong ? value : value.CastType<ulong>(),
                NumericTypes.BigInteger => value is BigInteger ? value : value.CastType<BigInteger>(),
                _ => throw new ArgumentException($"Unsupported type #{(byte)type}")
            };

        /// <summary>
        /// Get the numeric type of a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Numeric type</returns>
        public static NumericTypes GetNumericType<T>(this T value) where T : struct, IConvertible
            => value switch
            {
                sbyte v when IsValueNumericType(v) => GetValueNumericType(v),
                sbyte v when v == sbyte.MinValue => NumericTypes.SByteMin,
                sbyte => NumericTypes.SByte,
                short v when IsValueNumericType(v) => GetValueNumericType(v),
                short v when v == short.MinValue => NumericTypes.ShortMin,
                short v when v == short.MaxValue => NumericTypes.ShortMax,
                short => NumericTypes.Short,
                int v when IsValueNumericType(v) => GetValueNumericType(v),
                int v when v == int.MinValue => NumericTypes.IntMin,
                int v when v == int.MaxValue => NumericTypes.IntMax,
                int => NumericTypes.Int,
                long v when v >= MIN_VALUE && v <= MAX_VALUE => GetValueNumericType((int)v),
                long v when v == long.MinValue => NumericTypes.LongMin,
                long v when v == long.MaxValue => NumericTypes.LongMax,
                long => NumericTypes.Long,
                Half v when Half.IsInteger(v) && IsValueNumericType((int)v) => GetValueNumericType((int)v),
                Half v when v == Half.E => NumericTypes.HalfE,
                Half v when v == Half.Epsilon => NumericTypes.HalfEpsilon,
                Half v when v == Half.MaxValue => NumericTypes.HalfMax,
                Half v when v == Half.MinValue => NumericTypes.HalfMin,
                Half v when v == Half.MultiplicativeIdentity => NumericTypes.HalfMultiplicativeIdentity,
                Half v when Half.IsNaN(v) => NumericTypes.HalfNaN,
                Half v when v == Half.NegativeInfinity => NumericTypes.HalfNegativeInfinity,
                Half v when v == Half.NegativeZero => NumericTypes.HalfNegativeZero,
                Half v when v == Half.Pi => NumericTypes.HalfPi,
                Half v when v == Half.PositiveInfinity => NumericTypes.HalfPositiveInfinity,
                Half v when v == Half.Tau => NumericTypes.HalfTau,
                Half v when v == Half.Zero => NumericTypes.Zero,
                Half => NumericTypes.Half,
                float v when float.IsInteger(v) && v >= MIN_VALUE && v <= MAX_VALUE => GetValueNumericType((int)v),
                float v when v == float.E => NumericTypes.FloatE,
                float v when v == float.Epsilon => NumericTypes.FloatEpsilon,
                float v when v == float.MaxValue => NumericTypes.FloatMax,
                float v when v == float.MinValue => NumericTypes.FloatMin,
                float v when float.IsNaN(v) => NumericTypes.FloatNaN,
                float v when v == float.NegativeInfinity => NumericTypes.FloatNegativeInfinity,
                float v when v == float.Pi => NumericTypes.FloatPi,
                float v when v == float.PositiveInfinity => NumericTypes.FloatPositiveInfinity,
                float v when v == float.Tau => NumericTypes.FloatTau,
                float => NumericTypes.Float,
                double v when double.IsInteger(v) && v >= MIN_VALUE && v <= MAX_VALUE => GetValueNumericType((int)v),
                double v when v == double.E => NumericTypes.DoubleE,
                double v when v == double.Epsilon => NumericTypes.DoubleEpsilon,
                double v when v == double.MaxValue => NumericTypes.DoubleMax,
                double v when v == double.MinValue => NumericTypes.DoubleMin,
                double v when double.IsNaN(v) => NumericTypes.DoubleNaN,
                double v when v == double.NegativeInfinity => NumericTypes.DoubleNegativeInfinity,
                double v when v == double.Pi => NumericTypes.DoublePi,
                double v when v == double.PositiveInfinity => NumericTypes.DoublePositiveInfinity,
                double v when v == double.Tau => NumericTypes.DoubleTau,
                double => NumericTypes.Double,
                decimal v when decimal.IsInteger(v) && v >= MIN_VALUE && v <= MAX_VALUE => GetValueNumericType((int)v),
                decimal v when v == decimal.MinValue => NumericTypes.DecimalMin,
                decimal v when v == decimal.MaxValue => NumericTypes.DecimalMax,
                decimal => NumericTypes.Decimal,
                byte v when IsValueNumericType(v) => GetValueNumericType(v),
                byte v when v == byte.MaxValue => NumericTypes.ByteMax,
                byte => NumericTypes.Byte,
                ushort v when IsValueNumericType(v) => GetValueNumericType(v),
                ushort v when v == ushort.MaxValue => NumericTypes.UShortMax,
                ushort => NumericTypes.UShort,
                uint v when v <= MAX_VALUE => GetValueNumericType((int)v),
                uint v when v == uint.MaxValue => NumericTypes.UIntMax,
                uint => NumericTypes.UInt,
                ulong v when v <= MAX_VALUE => GetValueNumericType((int)v),
                ulong v when v == ulong.MaxValue => NumericTypes.ULongMax,
                ulong => NumericTypes.ULong,
                BigInteger v when v >= MIN_VALUE && v <= MAX_VALUE && IsValueNumericType((int)v) => GetValueNumericType((int)v),
                BigInteger => NumericTypes.BigInteger,
                _ => NumericTypes.None
            };

        /// <summary>
        /// Get the numeric type of a number
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Numeric type (may contain value flags)</returns>
        public static NumericTypes GetNumericType(this object value)
            => value switch
            {
                sbyte v when IsValueNumericType(v) => GetValueNumericType(v),
                sbyte v when v == sbyte.MinValue => NumericTypes.SByteMin,
                sbyte => NumericTypes.SByte,
                short v when IsValueNumericType(v) => GetValueNumericType(v),
                short v when v == short.MinValue => NumericTypes.ShortMin,
                short v when v == short.MaxValue => NumericTypes.ShortMax,
                short => NumericTypes.Short,
                int v when IsValueNumericType(v) => GetValueNumericType(v),
                int v when v == int.MinValue => NumericTypes.IntMin,
                int v when v == int.MaxValue => NumericTypes.IntMax,
                int => NumericTypes.Int,
                long v when v >= MIN_VALUE && v <= MAX_VALUE => GetValueNumericType((int)v),
                long v when v == long.MinValue => NumericTypes.LongMin,
                long v when v == long.MaxValue => NumericTypes.LongMax,
                long => NumericTypes.Long,
                Half v when Half.IsInteger(v) && IsValueNumericType((int)v) => GetValueNumericType((int)v),
                Half v when v == Half.E => NumericTypes.HalfE,
                Half v when v == Half.Epsilon => NumericTypes.HalfEpsilon,
                Half v when v == Half.MaxValue => NumericTypes.HalfMax,
                Half v when v == Half.MinValue => NumericTypes.HalfMin,
                Half v when v == Half.MultiplicativeIdentity => NumericTypes.HalfMultiplicativeIdentity,
                Half v when Half.IsNaN(v) => NumericTypes.HalfNaN,
                Half v when v == Half.NegativeInfinity => NumericTypes.HalfNegativeInfinity,
                Half v when v == Half.NegativeZero => NumericTypes.HalfNegativeZero,
                Half v when v == Half.Pi => NumericTypes.HalfPi,
                Half v when v == Half.PositiveInfinity => NumericTypes.HalfPositiveInfinity,
                Half v when v == Half.Tau => NumericTypes.HalfTau,
                Half v when v == Half.Zero => NumericTypes.Zero,
                Half => NumericTypes.Half,
                float v when float.IsInteger(v) && v >= MIN_VALUE && v <= MAX_VALUE => GetValueNumericType((int)v),
                float v when v == float.E => NumericTypes.FloatE,
                float v when v == float.Epsilon => NumericTypes.FloatEpsilon,
                float v when v == float.MaxValue => NumericTypes.FloatMax,
                float v when v == float.MinValue => NumericTypes.FloatMin,
                float v when float.IsNaN(v) => NumericTypes.FloatNaN,
                float v when v == float.NegativeInfinity => NumericTypes.FloatNegativeInfinity,
                float v when v == float.Pi => NumericTypes.FloatPi,
                float v when v == float.PositiveInfinity => NumericTypes.FloatPositiveInfinity,
                float v when v == float.Tau => NumericTypes.FloatTau,
                float => NumericTypes.Float,
                double v when double.IsInteger(v) && v >= MIN_VALUE && v <= MAX_VALUE => GetValueNumericType((int)v),
                double v when v == double.E => NumericTypes.DoubleE,
                double v when v == double.Epsilon => NumericTypes.DoubleEpsilon,
                double v when v == double.MaxValue => NumericTypes.DoubleMax,
                double v when v == double.MinValue => NumericTypes.DoubleMin,
                double v when double.IsNaN(v) => NumericTypes.DoubleNaN,
                double v when v == double.NegativeInfinity => NumericTypes.DoubleNegativeInfinity,
                double v when v == double.Pi => NumericTypes.DoublePi,
                double v when v == double.PositiveInfinity => NumericTypes.DoublePositiveInfinity,
                double v when v == double.Tau => NumericTypes.DoubleTau,
                double => NumericTypes.Double,
                decimal v when decimal.IsInteger(v) && v >= MIN_VALUE && v <= MAX_VALUE => GetValueNumericType((int)v),
                decimal v when v == decimal.MinValue => NumericTypes.DecimalMin,
                decimal v when v == decimal.MaxValue => NumericTypes.DecimalMax,
                decimal => NumericTypes.Decimal,
                byte v when IsValueNumericType(v) => GetValueNumericType(v),
                byte v when v == byte.MaxValue => NumericTypes.ByteMax,
                byte => NumericTypes.Byte,
                ushort v when IsValueNumericType(v) => GetValueNumericType(v),
                ushort v when v == ushort.MaxValue => NumericTypes.UShortMax,
                ushort => NumericTypes.UShort,
                uint v when v <= MAX_VALUE => GetValueNumericType((int)v),
                uint v when v == uint.MaxValue => NumericTypes.UIntMax,
                uint => NumericTypes.UInt,
                ulong v when v <= MAX_VALUE => GetValueNumericType((int)v),
                ulong v when v == ulong.MaxValue => NumericTypes.ULongMax,
                ulong => NumericTypes.ULong,
                BigInteger v when v >= MIN_VALUE && v <= MAX_VALUE && IsValueNumericType((int)v) => GetValueNumericType((int)v),
                BigInteger => NumericTypes.BigInteger,
                _ => NumericTypes.None
            };

        /// <summary>
        /// Determine if the type group is unsigned
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If unsigned</returns>
        public static bool IsUnsigned(this NumericTypes type)
            => type.GetTypeGroup() switch
            {
                NumericTypes.Byte
                    or NumericTypes.UShort
                    or NumericTypes.UInt
                    or NumericTypes.ULong
                    => true,
                _ => false
            };

        /// <summary>
        /// Determine if the type group is a floating point
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If is a floating point</returns>
        public static bool IsFloatingPoint(this NumericTypes type)
            => type.GetTypeGroup() switch
            {
                NumericTypes.Half
                    or NumericTypes.Float
                    or NumericTypes.Double
                    => true,
                _ => false
            };

        /// <summary>
        /// Determine if the type group is a decimal
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If is a decimal</returns>
        public static bool IsDecimal(this NumericTypes type) => type.GetTypeGroup() == NumericTypes.Decimal;

        /// <summary>
        /// Determine if the type group is a decimal
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If is a decimal</returns>
        public static bool IsBigInteger(this NumericTypes type) => type.GetTypeGroup() == NumericTypes.BigInteger;

        /// <summary>
        /// Get the numeric type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Numeric type</returns>
        public static NumericTypes GetNumericType(this Type type)
        {
            if (type == typeof(sbyte)) return NumericTypes.SByte;
            if (type == typeof(short)) return NumericTypes.Short;
            if (type == typeof(int)) return NumericTypes.Int;
            if (type == typeof(long)) return NumericTypes.Long;
            if (type == typeof(Half)) return NumericTypes.Half;
            if (type == typeof(float)) return NumericTypes.Float;
            if (type == typeof(double)) return NumericTypes.Double;
            if (type == typeof(decimal)) return NumericTypes.Decimal;
            if (type == typeof(byte)) return NumericTypes.Byte;
            if (type == typeof(ushort)) return NumericTypes.UShort;
            if (type == typeof(uint)) return NumericTypes.UInt;
            if (type == typeof(ulong)) return NumericTypes.ULong;
            if (type == typeof(BigInteger)) return NumericTypes.BigInteger;
            return NumericTypes.None;
        }

        /// <summary>
        /// Get the best fitting numeric type
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Numeric type</returns>
        public static NumericTypes GetFittingType<T>(this T value) where T : struct, IConvertible
        {
            NumericTypes res = value.GetNumericType();
            if (res == NumericTypes.None || res.HasValue() || res.IsDecimal()) return res;
            if (res.IsFloatingPoint())
            {
                double v = value.CastType<double>();
                if (v < float.MinValue || v > float.MaxValue) return NumericTypes.Double;
                if (v < (double)Half.MinValue || v > (double)Half.MaxValue) return NumericTypes.Float;
                return NumericTypes.Half;
            }
            bool hugeValue = false;
            if (res.IsBigInteger())
            {
                BigInteger v = value.CastType<BigInteger>();
                if (v < long.MinValue || v > ulong.MaxValue) return NumericTypes.BigInteger;
                if (v > long.MaxValue) hugeValue = true;
            }
            if (!res.IsUnsigned() && !hugeValue)
            {
                long v = value.CastType<long>();
                if (v < 0)
                {
                    if (v < int.MinValue) return NumericTypes.Long;
                    if (v < short.MinValue) return NumericTypes.Int;
                    if (v < sbyte.MinValue) return NumericTypes.Short;
                    return NumericTypes.SByte;
                }
            }
            {
                ulong v = value.CastType<ulong>();
                if (v > long.MaxValue) return NumericTypes.ULong;
                if (v > uint.MaxValue) return NumericTypes.Long;
                if (v > int.MaxValue) return NumericTypes.UInt;
                if (v > ushort.MaxValue) return NumericTypes.Int;
                if (v > (uint)short.MaxValue) return NumericTypes.UShort;
                if (v > byte.MaxValue) return NumericTypes.Short;
                if (v > (byte)sbyte.MaxValue) return NumericTypes.Byte;
                return NumericTypes.SByte;
            }
        }

        /// <summary>
        /// Cast to a numeric type
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="type">Target numeric type</param>
        /// <returns>Casted value</returns>
        public static object CastType<T>(this T value, in NumericTypes type) where T : struct, IConvertible
        {
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
            Type valueType = value.GetType();
            if (!valueType.IsValueType || !typeof(IConvertible).IsAssignableFrom(valueType))
                throw new ArgumentException("Not a number", nameof(value));
            Type clrType = type.GetClrType();
            if (clrType == typeof(void)) throw new ArgumentException("Not a number", nameof(value));
            return valueType == clrType
                ? value
                : Convert.ChangeType(value, clrType);
        }

        /// <summary>
        /// Get the data structure length of a numeric type in bytes
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Data structure length in bytes (excluding the numeric type enumeration value length of one byte)</returns>
        /// <exception cref="ArgumentException">Unsupported numeric type (like <see cref="NumericTypes.BigInteger"/>, which has a variable data structure length)</exception>
        public static int GetDataStructureLength(this NumericTypes type)
            => type == NumericTypes.None || type.HasValue()
                ? default
                : type.GetTypeGroup() switch
                {
                    NumericTypes.Byte or NumericTypes.SByte => sizeof(byte),
                    NumericTypes.Short or NumericTypes.UShort or NumericTypes.Half => sizeof(short),
                    NumericTypes.Int or NumericTypes.UInt or NumericTypes.Float => sizeof(int),
                    NumericTypes.Long or NumericTypes.ULong or NumericTypes.Double => sizeof(long),
                    NumericTypes.Decimal => sizeof(decimal),
                    _ => throw new ArgumentException($"Unsupported numeric type #{type}", nameof(type))
                };

        /// <summary>
        /// Get the numeric type group of a numeric type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Numeric type group</returns>
        public static NumericTypes GetTypeGroup(this NumericTypes type)
            => (type & ~NumericTypes.FLAGS) switch
            {
                NumericTypes.Zero
                    or NumericTypes.One
                    or NumericTypes.MinusOne
                    => NumericTypes.Int,
                NumericTypes.SByte
                    or NumericTypes.SByteMin
                    => NumericTypes.SByte,
                NumericTypes.Byte
                    or NumericTypes.ByteMax
                    => NumericTypes.Byte,
                NumericTypes.Short
                    or NumericTypes.ShortMin
                    or NumericTypes.ShortMax
                    => NumericTypes.Short,
                NumericTypes.UShort
                    or NumericTypes.UShortMax
                    => NumericTypes.UShort,
                NumericTypes.Int
                    or NumericTypes.IntMin
                    or NumericTypes.IntMax
                    => NumericTypes.Int,
                NumericTypes.UInt
                    or NumericTypes.UIntMax
                    => NumericTypes.UInt,
                NumericTypes.Long
                    or NumericTypes.LongMin
                    or NumericTypes.LongMax
                    => NumericTypes.Long,
                NumericTypes.ULong
                    or NumericTypes.ULongMax
                    => NumericTypes.ULong,
                NumericTypes.Half
                    or NumericTypes.HalfE
                    or NumericTypes.HalfEpsilon
                    or NumericTypes.HalfMax
                    or NumericTypes.HalfMin
                    or NumericTypes.HalfMultiplicativeIdentity
                    or NumericTypes.HalfNaN
                    or NumericTypes.HalfNegativeInfinity
                    or NumericTypes.HalfNegativeZero
                    or NumericTypes.HalfPi
                    or NumericTypes.HalfPositiveInfinity
                    or NumericTypes.HalfTau
                    => NumericTypes.Half,
                NumericTypes.Float
                    or NumericTypes.FloatE
                    or NumericTypes.FloatEpsilon
                    or NumericTypes.FloatMax
                    or NumericTypes.FloatMin
                    or NumericTypes.FloatNaN
                    or NumericTypes.FloatNegativeInfinity
                    or NumericTypes.FloatPi
                    or NumericTypes.FloatPositiveInfinity
                    or NumericTypes.FloatTau
                    => NumericTypes.Float,
                NumericTypes.Double
                    or NumericTypes.DoubleE
                    or NumericTypes.DoubleEpsilon
                    or NumericTypes.DoubleMax
                    or NumericTypes.DoubleMin
                    or NumericTypes.DoubleNaN
                    or NumericTypes.DoubleNegativeInfinity
                    or NumericTypes.DoubleNegativeZero
                    or NumericTypes.DoublePi
                    or NumericTypes.DoublePositiveInfinity
                    or NumericTypes.DoubleTau
                    => NumericTypes.Double,
                NumericTypes.Decimal
                    or NumericTypes.DecimalMax
                    or NumericTypes.DecimalMin
                    => NumericTypes.ULong,
                NumericTypes.BigInteger => NumericTypes.BigInteger,
                _ => (type & NumericTypes.Number70To198) == NumericTypes.Number70To198 || (type & ~NumericTypes.FLAGS) >= NumericTypes.Number2
                    ? NumericTypes.Int
                    : NumericTypes.None
            };

        /// <summary>
        /// Determine if the type resolves to the final numeric value
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If the type resolves to the final numeric value</returns>
        public static bool HasValue(this NumericTypes type)
            => (type & ~NumericTypes.FLAGS) switch
            {
                NumericTypes.Zero
                    or NumericTypes.One
                    or NumericTypes.MinusOne
                    or NumericTypes.SByteMin
                    or NumericTypes.ByteMax
                    or NumericTypes.IntMax
                    or NumericTypes.IntMin
                    or NumericTypes.UIntMax
                    or NumericTypes.LongMax
                    or NumericTypes.LongMin
                    or NumericTypes.ULongMax
                    or NumericTypes.HalfE
                    or NumericTypes.HalfEpsilon
                    or NumericTypes.HalfMax
                    or NumericTypes.HalfMin
                    or NumericTypes.HalfMultiplicativeIdentity
                    or NumericTypes.HalfNaN
                    or NumericTypes.HalfNegativeInfinity
                    or NumericTypes.HalfNegativeZero
                    or NumericTypes.HalfPi
                    or NumericTypes.HalfPositiveInfinity
                    or NumericTypes.FloatE
                    or NumericTypes.FloatEpsilon
                    or NumericTypes.FloatMax
                    or NumericTypes.FloatMin
                    or NumericTypes.FloatNaN
                    or NumericTypes.FloatNegativeInfinity
                    or NumericTypes.FloatPi
                    or NumericTypes.FloatPositiveInfinity
                    or NumericTypes.FloatTau
                    or NumericTypes.DoubleE
                    or NumericTypes.DoubleEpsilon
                    or NumericTypes.DoubleMax
                    or NumericTypes.DoubleMin
                    or NumericTypes.DoubleNaN
                    or NumericTypes.DoubleNegativeInfinity
                    or NumericTypes.DoubleNegativeZero
                    or NumericTypes.DoublePi
                    or NumericTypes.DoublePositiveInfinity
                    or NumericTypes.DoubleTau
                    or NumericTypes.DecimalMax
                    or NumericTypes.DecimalMin
                    => true,
                _ => (type & NumericTypes.Number70To198) == NumericTypes.Number70To198 || (type & ~NumericTypes.FLAGS) >= NumericTypes.Number2
            };

        /// <summary>
        /// Get the final numeric value of a type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Numeric value</returns>
        /// <exception cref="ArgumentException">Type can't be resolved to a final numeric value</exception>
        public static object GetValue(this NumericTypes type)
            => type switch
            {
                NumericTypes.Zero => 0,
                NumericTypes.One => 1,
                NumericTypes.MinusOne => -1,
                NumericTypes.SByteMin => sbyte.MinValue,
                NumericTypes.ByteMax => byte.MaxValue,
                NumericTypes.ShortMax => short.MaxValue,
                NumericTypes.ShortMin => short.MinValue,
                NumericTypes.UShortMax => ushort.MaxValue,
                NumericTypes.IntMax => int.MaxValue,
                NumericTypes.IntMin => int.MinValue,
                NumericTypes.UIntMax => uint.MaxValue,
                NumericTypes.LongMax => long.MaxValue,
                NumericTypes.LongMin => long.MinValue,
                NumericTypes.ULongMax => ulong.MaxValue,
                NumericTypes.HalfE => Half.E,
                NumericTypes.HalfEpsilon => Half.Epsilon,
                NumericTypes.HalfMax => Half.MaxValue,
                NumericTypes.HalfMin => Half.MinValue,
                NumericTypes.HalfMultiplicativeIdentity => Half.MultiplicativeIdentity,
                NumericTypes.HalfNaN => Half.NaN,
                NumericTypes.HalfNegativeInfinity => Half.NegativeInfinity,
                NumericTypes.HalfNegativeZero => Half.NegativeZero,
                NumericTypes.HalfPi => Half.Pi,
                NumericTypes.HalfPositiveInfinity => Half.PositiveInfinity,
                NumericTypes.HalfTau => Half.Tau,
                NumericTypes.FloatE => float.E,
                NumericTypes.FloatEpsilon => float.Epsilon,
                NumericTypes.FloatMax => float.MaxValue,
                NumericTypes.FloatMin => float.MinValue,
                NumericTypes.FloatNaN => float.NaN,
                NumericTypes.FloatNegativeInfinity => float.NegativeInfinity,
                NumericTypes.FloatPi => float.Pi,
                NumericTypes.FloatPositiveInfinity => float.PositiveInfinity,
                NumericTypes.FloatTau => float.Tau,
                NumericTypes.DoubleE => double.E,
                NumericTypes.DoubleEpsilon => double.Epsilon,
                NumericTypes.DoubleMax => double.MaxValue,
                NumericTypes.DoubleMin => double.MinValue,
                NumericTypes.DoubleNaN => double.NaN,
                NumericTypes.DoubleNegativeInfinity => double.NegativeInfinity,
                NumericTypes.DoubleNegativeZero => double.NegativeZero,
                NumericTypes.DoublePi => double.Pi,
                NumericTypes.DoublePositiveInfinity => double.PositiveInfinity,
                NumericTypes.DoubleTau => double.Tau,
                NumericTypes.DecimalMax => decimal.MaxValue,
                NumericTypes.DecimalMin => decimal.MinValue,
                _ => (type & NumericTypes.Number70To198) == NumericTypes.Number70To198
                    ? (int)(type & ~NumericTypes.Number70To198) + 67
                    : type >= NumericTypes.Number2
                        ? (int)type - (int)NumericTypes.Number2 + 2
                        : throw new ArgumentException($"Numeric type {type} has no final numeric value which can be resolved", nameof(type))
            };

        /// <summary>
        /// Determine if the value is a <see cref="NumericTypes"/> value (<see cref="MIN_VALUE"/> to <see cref="MAX_VALUE"/>)
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>If the value is a <see cref="NumericTypes"/> value</returns>
        public static bool IsValueNumericType(this int value) => value >= MIN_VALUE && value <= MAX_VALUE;

        /// <summary>
        /// Get the values <see cref="NumericTypes"/> value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns><see cref="NumericTypes"/> value</returns>
        /// <exception cref="ArgumentOutOfRangeException">Value isn't in the range of <see cref="MIN_VALUE"/> to <see cref="MAX_VALUE"/></exception>
        public static NumericTypes GetValueNumericType(this int value)
        {
            if (!value.IsValueNumericType()) throw new ArgumentOutOfRangeException(nameof(value));
            return value switch
            {
                -1 => NumericTypes.MinusOne,
                0 => NumericTypes.Zero,
                1 => NumericTypes.One,
                _ when value < 70 => (NumericTypes)(value - 2 + (int)NumericTypes.Number2),
                _ => (NumericTypes)(value - 70) | NumericTypes.Number70To198
            };
        }
    }
}
