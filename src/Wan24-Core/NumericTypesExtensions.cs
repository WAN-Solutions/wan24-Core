namespace wan24.Core
{
    /// <summary>
    /// <see cref="NumericTypes"/> extensions
    /// </summary>
    public static class NumericTypesExtensions
    {
        /// <summary>
        /// Get the default value of a numeric type
        /// </summary>
        /// <param name="type">Type (must not be <see cref="NumericTypes.None"/> or contain value flags)</param>
        /// <returns>Default value</returns>
        /// <exception cref="ArgumentException">Unsupported numeric type</exception>
        public static object GetDefault(this NumericTypes type)
            => type.GetTypeGroup() switch
            {
                NumericTypes.SByte => default(sbyte),
                NumericTypes.Short => default(short),
                NumericTypes.Int => default(int),
                NumericTypes.Long => default(long),
                NumericTypes.Half => default(Half),
                NumericTypes.Float => default(float),
                NumericTypes.Double => default(double),
                NumericTypes.Decimal => default(decimal),
                NumericTypes.Byte => default(byte),
                NumericTypes.UShort => default(ushort),
                NumericTypes.UInt => default(uint),
                NumericTypes.ULong => default(ulong),
                _ => throw new ArgumentException($"Unsupported type #{(byte)type}")
            };

        /// <summary>
        /// Get the CLR type
        /// </summary>
        /// <param name="type">Type (must not contain value flags)</param>
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
                _ => typeof(void)
            };

        /// <summary>
        /// Cast a number to the numeric type
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="value">Numeric value</param>
        /// <param name="type">Type (must not be <see cref="NumericTypes.None"/> or contain value flags)</param>
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
                _ => throw new ArgumentException($"Unsupported type #{(byte)type}")
            };

        /// <summary>
        /// Cast a number to the numeric type
        /// </summary>
        /// <param name="value">Numeric value</param>
        /// <param name="type">Type (must not be <see cref="NumericTypes.None"/> or contain value flags)</param>
        /// <returns>Casted numeric type</returns>
        /// <exception cref="ArgumentException">Unsupported numeric type</exception>
        public static object CastNumericValue(this object value, in NumericTypes type)
            => type switch
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
                _ => throw new ArgumentException($"Unsupported type #{(byte)type}")
            };

        /// <summary>
        /// Get the numeric type of a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Numeric type (may contain value flags)</returns>
        public static NumericTypes GetNumericType<T>(this T value) where T : struct, IConvertible
            => value switch
            {
                sbyte v when v == default(sbyte) => NumericTypes.Zero,
                sbyte v when IsNonZeroValueNumericType(v) => GetValueNumericType(v),
                sbyte v when v == sbyte.MinValue => NumericTypes.SByteMin,
                sbyte v when v == sbyte.MaxValue => NumericTypes.SByteMax,
                sbyte => NumericTypes.SByte,
                short v when v == default(short) => NumericTypes.Zero,
                short v when IsNonZeroValueNumericType(v) => GetValueNumericType(v),
                short v when v == short.MinValue => NumericTypes.ShortMin,
                short v when v == short.MaxValue => NumericTypes.ShortMax,
                short => NumericTypes.Short,
                int v when v == default => NumericTypes.Zero,
                int v when IsNonZeroValueNumericType(v) => GetValueNumericType(v),
                int v when v == int.MinValue => NumericTypes.IntMin,
                int v when v == int.MaxValue => NumericTypes.IntMax,
                int => NumericTypes.Int,
                long v when v == default => NumericTypes.Zero,
                long v when IsNonZeroValueNumericType((int)v) => GetValueNumericType((int)v),
                long v when v == long.MinValue => NumericTypes.LongMin,
                long v when v == long.MaxValue => NumericTypes.LongMax,
                long => NumericTypes.Long,
                Half v when v == default => NumericTypes.Zero,//TODO Is Half.Zero == default?
                Half v when Half.IsInteger(v) && !Half.IsNegative(v) && IsNonZeroValueNumericType((int)v) => GetValueNumericType((int)v),
                Half v when v == Half.E => NumericTypes.HalfE,
                Half v when v == Half.Epsilon => NumericTypes.HalfEpsilon,
                Half v when v == Half.MaxValue => NumericTypes.HalfMax,
                Half v when v == Half.MinValue => NumericTypes.HalfMin,
                Half v when v == Half.MultiplicativeIdentity => NumericTypes.HalfMultiplicativeIdentity,
                Half v when Half.IsNaN(v) => NumericTypes.HalfNaN,
                Half v when v == Half.NegativeInfinity => NumericTypes.HalfNegativeInfinity,
                Half v when v == Half.NegativeOne => NumericTypes.HalfNegativeOne,//TODO Is Half.NegativeOne == -1?
                Half v when v == Half.NegativeZero => NumericTypes.HalfNegativeZero,
                Half v when v == Half.One => NumericTypes.Number1,//TODO Is Half.One == 1?
                Half v when v == Half.Pi => NumericTypes.HalfPi,//TODO Can cast from double.Pi -> Half.Pi?
                Half v when v == Half.PositiveInfinity => NumericTypes.HalfPositiveInfinity,
                Half v when v == Half.Tau => NumericTypes.HalfTau,
                Half v when v == Half.Zero => NumericTypes.Zero,
                Half => NumericTypes.Half,
                float v when v == default => NumericTypes.Zero,
                float v when float.IsInteger(v) && !float.IsNegative(v) && IsNonZeroValueNumericType((int)v) => GetValueNumericType((int)v),
                float v when v == float.E => NumericTypes.FloatE,
                float v when v == float.Epsilon => NumericTypes.FloatEpsilon,
                float v when v == float.MaxValue => NumericTypes.FloatMax,
                float v when v == float.MinValue => NumericTypes.FloatMin,
                float v when float.IsNaN(v) => NumericTypes.FloatNaN,
                float v when v == float.NegativeInfinity => NumericTypes.FloatNegativeInfinity,
                float v when v == float.NegativeZero => NumericTypes.FloatNegativeZero,
                float v when v == float.Pi => NumericTypes.FloatPi,
                float v when v == float.PositiveInfinity => NumericTypes.FloatPositiveInfinity,
                float v when v == float.Tau => NumericTypes.FloatTau,
                float => NumericTypes.Float,
                double v when v == default => NumericTypes.Zero,
                double v when double.IsInteger(v) && !double.IsNegative(v) && IsNonZeroValueNumericType((int)v) => GetValueNumericType((int)v),
                double v when v == double.E => NumericTypes.FloatE,
                double v when v == double.Epsilon => NumericTypes.FloatEpsilon,
                double v when v == double.MaxValue => NumericTypes.FloatMax,
                double v when v == double.MinValue => NumericTypes.FloatMin,
                double v when double.IsNaN(v) => NumericTypes.FloatNaN,
                double v when v == double.NegativeInfinity => NumericTypes.FloatNegativeInfinity,
                double v when v == double.NegativeZero => NumericTypes.FloatNegativeZero,
                double v when v == double.Pi => NumericTypes.FloatPi,
                double v when v == double.PositiveInfinity => NumericTypes.FloatPositiveInfinity,
                double v when v == double.Tau => NumericTypes.FloatTau,
                double => NumericTypes.Double,
                decimal v when v == default => NumericTypes.Decimal | NumericTypes.Zero,
                decimal v when v > 1 && v < 515 => GetValueNumericType((int)v),
                decimal v when v == decimal.MinValue => NumericTypes.DecimalMin,
                decimal v when v == decimal.MaxValue => NumericTypes.DecimalMax,
                decimal v when v == decimal.MinusOne => NumericTypes.DecimalNegativeOne,
                decimal v when v == decimal.One => NumericTypes.DecimalOne,
                decimal => NumericTypes.Decimal,
                byte v when v == default(byte) => NumericTypes.Zero,
                byte v => GetValueNumericType(v),
                ushort v when v == default(ushort) => NumericTypes.Zero,
                ushort v when IsNonZeroValueNumericType(v) => GetValueNumericType(v),
                ushort v when v == ushort.MaxValue => NumericTypes.UShortMax,
                ushort => NumericTypes.UShort,
                uint v when v == default => NumericTypes.Zero,
                uint v when v < 515 => GetValueNumericType((int)v),
                uint v when v == uint.MaxValue => NumericTypes.UIntMax,
                uint => NumericTypes.UInt,
                ulong v when v == default => NumericTypes.Zero,
                ulong v when v < 515 => GetValueNumericType((int)v),
                ulong v when v == ulong.MaxValue => NumericTypes.ULongMax,
                ulong => NumericTypes.ULong,
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
                sbyte v when v == default(sbyte) => NumericTypes.Zero,
                sbyte v when IsNonZeroValueNumericType(v) => GetValueNumericType(v),
                sbyte v when v == sbyte.MinValue => NumericTypes.SByteMin,
                sbyte v when v == sbyte.MaxValue => NumericTypes.SByteMax,
                sbyte => NumericTypes.SByte,
                short v when v == default(short) => NumericTypes.Zero,
                short v when IsNonZeroValueNumericType(v) => GetValueNumericType(v),
                short v when v == short.MinValue => NumericTypes.ShortMin,
                short v when v == short.MaxValue => NumericTypes.ShortMax,
                short => NumericTypes.Short,
                int v when v == default => NumericTypes.Zero,
                int v when IsNonZeroValueNumericType(v) => GetValueNumericType(v),
                int v when v == int.MinValue => NumericTypes.IntMin,
                int v when v == int.MaxValue => NumericTypes.IntMax,
                int => NumericTypes.Int,
                long v when v == default => NumericTypes.Zero,
                long v when IsNonZeroValueNumericType((int)v) => GetValueNumericType((int)v),
                long v when v == long.MinValue => NumericTypes.LongMin,
                long v when v == long.MaxValue => NumericTypes.LongMax,
                long => NumericTypes.Long,
                Half v when v == default => NumericTypes.Zero,//TODO Is Half.Zero == default?
                Half v when Half.IsInteger(v) && !Half.IsNegative(v) && IsNonZeroValueNumericType((int)v) => GetValueNumericType((int)v),
                Half v when v == Half.E => NumericTypes.HalfE,
                Half v when v == Half.Epsilon => NumericTypes.HalfEpsilon,
                Half v when v == Half.MaxValue => NumericTypes.HalfMax,
                Half v when v == Half.MinValue => NumericTypes.HalfMin,
                Half v when v == Half.MultiplicativeIdentity => NumericTypes.HalfMultiplicativeIdentity,
                Half v when Half.IsNaN(v) => NumericTypes.HalfNaN,
                Half v when v == Half.NegativeInfinity => NumericTypes.HalfNegativeInfinity,
                Half v when v == Half.NegativeOne => NumericTypes.HalfNegativeOne,//TODO Is Half.NegativeOne == -1?
                Half v when v == Half.NegativeZero => NumericTypes.HalfNegativeZero,
                Half v when v == Half.One => NumericTypes.Number1,//TODO Is Half.One == 1?
                Half v when v == Half.Pi => NumericTypes.HalfPi,//TODO Can cast from double.Pi -> Half.Pi?
                Half v when v == Half.PositiveInfinity => NumericTypes.HalfPositiveInfinity,
                Half v when v == Half.Tau => NumericTypes.HalfTau,
                Half v when v == Half.Zero => NumericTypes.Zero,
                Half => NumericTypes.Half,
                float v when v == default => NumericTypes.Zero,
                float v when float.IsInteger(v) && !float.IsNegative(v) && IsNonZeroValueNumericType((int)v) => GetValueNumericType((int)v),
                float v when v == float.E => NumericTypes.FloatE,
                float v when v == float.Epsilon => NumericTypes.FloatEpsilon,
                float v when v == float.MaxValue => NumericTypes.FloatMax,
                float v when v == float.MinValue => NumericTypes.FloatMin,
                float v when float.IsNaN(v) => NumericTypes.FloatNaN,
                float v when v == float.NegativeInfinity => NumericTypes.FloatNegativeInfinity,
                float v when v == float.NegativeZero => NumericTypes.FloatNegativeZero,
                float v when v == float.Pi => NumericTypes.FloatPi,
                float v when v == float.PositiveInfinity => NumericTypes.FloatPositiveInfinity,
                float v when v == float.Tau => NumericTypes.FloatTau,
                float => NumericTypes.Float,
                double v when v == default => NumericTypes.Zero,
                double v when double.IsInteger(v) && !double.IsNegative(v) && IsNonZeroValueNumericType((int)v) => GetValueNumericType((int)v),
                double v when v == double.E => NumericTypes.FloatE,
                double v when v == double.Epsilon => NumericTypes.FloatEpsilon,
                double v when v == double.MaxValue => NumericTypes.FloatMax,
                double v when v == double.MinValue => NumericTypes.FloatMin,
                double v when double.IsNaN(v) => NumericTypes.FloatNaN,
                double v when v == double.NegativeInfinity => NumericTypes.FloatNegativeInfinity,
                double v when v == double.NegativeZero => NumericTypes.FloatNegativeZero,
                double v when v == double.Pi => NumericTypes.FloatPi,
                double v when v == double.PositiveInfinity => NumericTypes.FloatPositiveInfinity,
                double v when v == double.Tau => NumericTypes.FloatTau,
                double => NumericTypes.Double,
                decimal v when v == default => NumericTypes.Decimal | NumericTypes.Zero,
                decimal v when v > 1 && v < 515 => GetValueNumericType((int)v),
                decimal v when v == decimal.MinValue => NumericTypes.DecimalMin,
                decimal v when v == decimal.MaxValue => NumericTypes.DecimalMax,
                decimal v when v == decimal.MinusOne => NumericTypes.DecimalNegativeOne,
                decimal v when v == decimal.One => NumericTypes.DecimalOne,
                decimal => NumericTypes.Decimal,
                byte v when v == default(byte) => NumericTypes.Zero,
                byte v => GetValueNumericType(v),
                ushort v when v == default(ushort) => NumericTypes.Zero,
                ushort v when IsNonZeroValueNumericType(v) => GetValueNumericType(v),
                ushort v when v == ushort.MaxValue => NumericTypes.UShortMax,
                ushort => NumericTypes.UShort,
                uint v when v == default => NumericTypes.Zero,
                uint v when v < 515 => GetValueNumericType((int)v),
                uint v when v == uint.MaxValue => NumericTypes.UIntMax,
                uint => NumericTypes.UInt,
                ulong v when v == default => NumericTypes.Zero,
                ulong v when v < 515 => GetValueNumericType((int)v),
                ulong v when v == ulong.MaxValue => NumericTypes.ULongMax,
                ulong => NumericTypes.ULong,
                _ => NumericTypes.None
            };

        /// <summary>
        /// Determine if the type group is unsigned
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If unsigned</returns>
        public static bool IsUnsigned(this NumericTypes type)
            => (type & ~NumericTypes.FLAGS) switch
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
            if (!res.IsUnsigned())
            {
                long v = value.CastType<long>();
                if (v < 0)
                {
                    if (v < int.MinValue) return NumericTypes.Long;
                    if (v < short.MinValue) return NumericTypes.Int;
                    if (v < sbyte.MinValue) return NumericTypes.Short;
                    return NumericTypes.Byte;
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
        /// <exception cref="ArgumentException">Unsupported numeric type</exception>
        public static int GetDataStructureLength(this NumericTypes type)
            => type == NumericTypes.None || (type.HasValue() && type != NumericTypes.DoubleNumber)
                ? default
                : type.GetTypeGroup() switch
                {
                    NumericTypes.Byte or NumericTypes.SByte => sizeof(byte),
                    NumericTypes.Short when type.IsPositiveNumericNonConstantValue() => sizeof(byte),
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
                NumericTypes.SByte
                    or NumericTypes.SByteMin
                    or NumericTypes.SByteMax
                    => NumericTypes.SByte,
                NumericTypes.Byte
                    or NumericTypes.ByteMax
                    => NumericTypes.Byte,
                NumericTypes.Short
                    or NumericTypes.SByteMin
                    or NumericTypes.SByteMax
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
                    or NumericTypes.HalfNegativeOne
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
                    or NumericTypes.FloatNegativeZero
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
                    or NumericTypes.DecimalNegativeOne
                    or NumericTypes.DecimalOne
                    => NumericTypes.ULong,
                _ => type.IsPositiveNumericNonConstantValue()
                    ? NumericTypes.Short
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
                    or NumericTypes.SByteMax
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
                    or NumericTypes.HalfNegativeOne
                    or NumericTypes.HalfNegativeZero
                    or NumericTypes.HalfPi
                    or NumericTypes.HalfPositiveInfinity
                    or NumericTypes.FloatE
                    or NumericTypes.FloatEpsilon
                    or NumericTypes.FloatMax
                    or NumericTypes.FloatMin
                    or NumericTypes.FloatNaN
                    or NumericTypes.FloatNegativeInfinity
                    or NumericTypes.FloatNegativeZero
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
                    or NumericTypes.DecimalNegativeOne
                    or NumericTypes.DecimalOne
                    or NumericTypes.DoubleNumber
                    => true,
                _ => (type & ~NumericTypes.FLAGS) >= NumericTypes.NumberMinus1
            };

        /// <summary>
        /// If the next byte is required to get the value by <see cref="GetValue(NumericTypes, in byte?)"/>
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If the next byte is required</returns>
        public static bool RequiresNextByte(this NumericTypes type) => type == NumericTypes.DoubleNumber;

        /// <summary>
        /// Determine if a type is a positive (non-zero) numeric non-constant value (1..514)
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If a type is a non-zero numeric non-constant value</returns>
        public static bool IsPositiveNumericNonConstantValue(this NumericTypes type)
            => type == NumericTypes.DoubleNumber || type >= NumericTypes.Number1;

        /// <summary>
        /// Get the final numeric value of a type
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="nextByte">The next byte (required, if <c>type</c> equals <see cref="NumericTypes.DoubleNumber"/>)</param>
        /// <returns>Numeric value</returns>
        /// <exception cref="ArgumentException">Type can't be resolved to a final numeric value</exception>
        public static object GetValue(this NumericTypes type, in byte? nextByte = null)
            => type switch
            {
                NumericTypes.Zero => 0,
                NumericTypes.NumberMinus1 => -1,
                NumericTypes.SByteMax => sbyte.MaxValue,
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
                NumericTypes.HalfNegativeOne => Half.NegativeOne,
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
                NumericTypes.FloatNegativeZero => float.NegativeZero,
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
                NumericTypes.DecimalNegativeOne => decimal.MinusOne,
                NumericTypes.DecimalOne => decimal.One,
                NumericTypes.DoubleNumber => nextByte.HasValue
                    ? nextByte.Value + 130
                    : throw new ArgumentNullException(nameof(nextByte)),
                _ => (type & ~NumericTypes.FLAGS) >= NumericTypes.NumberMinus1
                    ? (type & NumericTypes.DoubleNumber) == NumericTypes.DoubleNumber
                        ? ((byte)type - (byte)NumericTypes.Number1 + 1) << 1
                        : (byte)type - (byte)NumericTypes.Number1 + 1
                    : throw new ArgumentException($"Numeric type {type} has no final numeric value which can be resolved", nameof(type))
            };

        /// <summary>
        /// Determine if the value is a non-zero <see cref="NumericTypes"/> value (1..514)
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>If the value is a non-zero <see cref="NumericTypes"/> value</returns>
        public static bool IsNonZeroValueNumericType(this int value) => value > 0 && value < 515;

        /// <summary>
        /// Get the values <see cref="NumericTypes"/> value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns><see cref="NumericTypes"/> value</returns>
        /// <exception cref="ArgumentOutOfRangeException">Value isn't in the range of 1-514</exception>
        public static NumericTypes GetValueNumericType(this int value)
        {
            if (!value.IsNonZeroValueNumericType()) throw new ArgumentOutOfRangeException(nameof(value));
            if (value > 129) return NumericTypes.DoubleNumber;
            return (value & 1) == 1
                ? (NumericTypes)(value + (byte)NumericTypes.Number1 - 1)
                : (NumericTypes)((value >> 1) + (byte)NumericTypes.Number1 - 1) | NumericTypes.DoubleNumber;
        }
    }
}
