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
            => type switch
            {
                NumericTypes.Byte => default(sbyte),
                NumericTypes.Short => default(short),
                NumericTypes.Integer => default(int),
                NumericTypes.Long => default(long),
                NumericTypes.Half => default(Half),
                NumericTypes.Float => default(float),
                NumericTypes.Double => default(double),
                NumericTypes.Decimal => default(decimal),
                NumericTypes.Byte | NumericTypes.Unsigned => default(byte),
                NumericTypes.Short | NumericTypes.Unsigned => default(ushort),
                NumericTypes.Integer | NumericTypes.Unsigned => default(uint),
                NumericTypes.Long | NumericTypes.Unsigned => default(ulong),
                _ => throw new ArgumentException($"Unsupported type #{(byte)type}")
            };

        /// <summary>
        /// Get the CLR type
        /// </summary>
        /// <param name="type">Type (must not contain value flags)</param>
        /// <returns>CLR type or <see cref="void"/></returns>
        public static Type GetClrType(this NumericTypes type)
            => type switch
            {
                NumericTypes.Byte => typeof(sbyte),
                NumericTypes.Short => typeof(short),
                NumericTypes.Integer => typeof(int),
                NumericTypes.Long => typeof(long),
                NumericTypes.Half => typeof(Half),
                NumericTypes.Float => typeof(float),
                NumericTypes.Double => typeof(double),
                NumericTypes.Decimal => typeof(decimal),
                NumericTypes.Byte | NumericTypes.Unsigned => typeof(byte),
                NumericTypes.Short | NumericTypes.Unsigned => typeof(ushort),
                NumericTypes.Integer | NumericTypes.Unsigned => typeof(uint),
                NumericTypes.Long | NumericTypes.Unsigned => typeof(ulong),
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
            => type switch
            {
                NumericTypes.Byte => value is sbyte ? value : value.CastType<sbyte>(),
                NumericTypes.Short => value is short ? value : value.CastType<short>(),
                NumericTypes.Integer => value is int ? value : value.CastType<int>(),
                NumericTypes.Long => value is long ? value : value.CastType<long>(),
                NumericTypes.Half => value is Half ? value : value.CastType<Half>(),
                NumericTypes.Float => value is float ? value : value.CastType<float>(),
                NumericTypes.Double => value is double ? value : value.CastType<double>(),
                NumericTypes.Decimal => value is decimal ? value : value.CastType<decimal>(),
                NumericTypes.Byte | NumericTypes.Unsigned => value is byte ? value : value.CastType<byte>(),
                NumericTypes.Short | NumericTypes.Unsigned => value is ushort ? value : value.CastType<ushort>(),
                NumericTypes.Integer | NumericTypes.Unsigned => value is uint ? value : value.CastType<uint>(),
                NumericTypes.Long | NumericTypes.Unsigned => value is ulong ? value : value.CastType<ulong>(),
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
                NumericTypes.Byte => value is sbyte ? value : value.CastType<sbyte>(),
                NumericTypes.Short => value is short ? value : value.CastType<short>(),
                NumericTypes.Integer => value is int ? value : value.CastType<int>(),
                NumericTypes.Long => value is long ? value : value.CastType<long>(),
                NumericTypes.Half => value is Half ? value : value.CastType<Half>(),
                NumericTypes.Float => value is float ? value : value.CastType<float>(),
                NumericTypes.Double => value is double ? value : value.CastType<double>(),
                NumericTypes.Decimal => value is decimal ? value : value.CastType<decimal>(),
                NumericTypes.Byte | NumericTypes.Unsigned => value is byte ? value : value.CastType<byte>(),
                NumericTypes.Short | NumericTypes.Unsigned => value is ushort ? value : value.CastType<ushort>(),
                NumericTypes.Integer | NumericTypes.Unsigned => value is uint ? value : value.CastType<uint>(),
                NumericTypes.Long | NumericTypes.Unsigned => value is ulong ? value : value.CastType<ulong>(),
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
                sbyte v when v == default(sbyte) => NumericTypes.Byte | NumericTypes.Zero,
                sbyte v when v == sbyte.MinValue => NumericTypes.Byte | NumericTypes.Min,
                sbyte v when v == sbyte.MaxValue => NumericTypes.Byte | NumericTypes.Max,
                sbyte => NumericTypes.Byte,
                short v when v == default(short) => NumericTypes.Short | NumericTypes.Zero,
                short v when v == short.MinValue => NumericTypes.Short | NumericTypes.Min,
                short v when v == short.MaxValue => NumericTypes.Short | NumericTypes.Max,
                short => NumericTypes.Short,
                int v when v == default => NumericTypes.Integer | NumericTypes.Zero,
                int v when v == int.MinValue => NumericTypes.Integer | NumericTypes.Min,
                int v when v == int.MaxValue => NumericTypes.Integer | NumericTypes.Max,
                int => NumericTypes.Integer,
                long v when v == default => NumericTypes.Long | NumericTypes.Zero,
                long v when v == long.MinValue => NumericTypes.Long | NumericTypes.Min,
                long v when v == long.MaxValue => NumericTypes.Long | NumericTypes.Max,
                long => NumericTypes.Long,
                Half v when v == default => NumericTypes.Half | NumericTypes.Zero,
                Half v when v == Half.MinValue => NumericTypes.Half | NumericTypes.Min,
                Half v when v == Half.MaxValue => NumericTypes.Half | NumericTypes.Max,
                Half => NumericTypes.Half,
                float v when v == default => NumericTypes.Float | NumericTypes.Zero,
                float v when v == float.MinValue => NumericTypes.Float | NumericTypes.Min,
                float v when v == float.MaxValue => NumericTypes.Float | NumericTypes.Max,
                float => NumericTypes.Float,
                double v when v == default => NumericTypes.Double | NumericTypes.Zero,
                double v when v == double.MinValue => NumericTypes.Double | NumericTypes.Min,
                double v when v == double.MaxValue => NumericTypes.Double | NumericTypes.Max,
                double => NumericTypes.Double,
                decimal v when v == default => NumericTypes.Decimal | NumericTypes.Zero,
                decimal v when v == decimal.MinValue => NumericTypes.Decimal | NumericTypes.Min,
                decimal v when v == decimal.MaxValue => NumericTypes.Decimal | NumericTypes.Max,
                decimal => NumericTypes.Decimal,
                byte v when v == default(byte) => NumericTypes.Byte | NumericTypes.Unsigned | NumericTypes.Zero,
                byte v when v == byte.MinValue => NumericTypes.Byte | NumericTypes.Unsigned | NumericTypes.Min,
                byte v when v == byte.MaxValue => NumericTypes.Byte | NumericTypes.Unsigned | NumericTypes.Max,
                byte => NumericTypes.Byte | NumericTypes.Unsigned,
                ushort v when v == default(ushort) => NumericTypes.Short | NumericTypes.Unsigned | NumericTypes.Zero,
                ushort v when v == ushort.MinValue => NumericTypes.Short | NumericTypes.Unsigned | NumericTypes.Min,
                ushort v when v == ushort.MaxValue => NumericTypes.Short | NumericTypes.Unsigned | NumericTypes.Max,
                ushort => NumericTypes.Short | NumericTypes.Unsigned,
                uint v when v == default => NumericTypes.Integer | NumericTypes.Unsigned | NumericTypes.Zero,
                uint v when v == uint.MinValue => NumericTypes.Integer | NumericTypes.Unsigned | NumericTypes.Min,
                uint v when v == uint.MaxValue => NumericTypes.Integer | NumericTypes.Unsigned | NumericTypes.Max,
                uint => NumericTypes.Integer | NumericTypes.Unsigned,
                ulong v when v == default => NumericTypes.Long | NumericTypes.Unsigned | NumericTypes.Zero,
                ulong v when v == ulong.MinValue => NumericTypes.Long | NumericTypes.Unsigned | NumericTypes.Min,
                ulong v when v == ulong.MaxValue => NumericTypes.Long | NumericTypes.Unsigned | NumericTypes.Max,
                ulong => NumericTypes.Long | NumericTypes.Unsigned,
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
                sbyte v when v == default(sbyte) => NumericTypes.Byte | NumericTypes.Zero,
                sbyte v when v == sbyte.MinValue => NumericTypes.Byte | NumericTypes.Min,
                sbyte v when v == sbyte.MaxValue => NumericTypes.Byte | NumericTypes.Max,
                sbyte => NumericTypes.Byte,
                short v when v == default(short) => NumericTypes.Short | NumericTypes.Zero,
                short v when v == short.MinValue => NumericTypes.Short | NumericTypes.Min,
                short v when v == short.MaxValue => NumericTypes.Short | NumericTypes.Max,
                short => NumericTypes.Short,
                int v when v == default => NumericTypes.Integer | NumericTypes.Zero,
                int v when v == int.MinValue => NumericTypes.Integer | NumericTypes.Min,
                int v when v == int.MaxValue => NumericTypes.Integer | NumericTypes.Max,
                int => NumericTypes.Integer,
                long v when v == default => NumericTypes.Long | NumericTypes.Zero,
                long v when v == long.MinValue => NumericTypes.Long | NumericTypes.Min,
                long v when v == long.MaxValue => NumericTypes.Long | NumericTypes.Max,
                long => NumericTypes.Long,
                Half v when v == default => NumericTypes.Half | NumericTypes.Zero,
                Half v when v == Half.MinValue => NumericTypes.Half | NumericTypes.Min,
                Half v when v == Half.MaxValue => NumericTypes.Half | NumericTypes.Max,
                Half => NumericTypes.Half,
                float v when v == default => NumericTypes.Float | NumericTypes.Zero,
                float v when v == float.MinValue => NumericTypes.Float | NumericTypes.Min,
                float v when v == float.MaxValue => NumericTypes.Float | NumericTypes.Max,
                float => NumericTypes.Float,
                double v when v == default => NumericTypes.Double | NumericTypes.Zero,
                double v when v == double.MinValue => NumericTypes.Double | NumericTypes.Min,
                double v when v == double.MaxValue => NumericTypes.Double | NumericTypes.Max,
                double => NumericTypes.Double,
                decimal v when v == default => NumericTypes.Decimal | NumericTypes.Zero,
                decimal v when v == decimal.MinValue => NumericTypes.Decimal | NumericTypes.Min,
                decimal v when v == decimal.MaxValue => NumericTypes.Decimal | NumericTypes.Max,
                decimal => NumericTypes.Decimal,
                byte v when v == default(byte) => NumericTypes.Byte | NumericTypes.Unsigned | NumericTypes.Zero,
                byte v when v == byte.MinValue => NumericTypes.Byte | NumericTypes.Unsigned | NumericTypes.Min,
                byte v when v == byte.MaxValue => NumericTypes.Byte | NumericTypes.Unsigned | NumericTypes.Max,
                byte => NumericTypes.Byte | NumericTypes.Unsigned,
                ushort v when v == default(ushort) => NumericTypes.Short | NumericTypes.Unsigned | NumericTypes.Zero,
                ushort v when v == ushort.MinValue => NumericTypes.Short | NumericTypes.Unsigned | NumericTypes.Min,
                ushort v when v == ushort.MaxValue => NumericTypes.Short | NumericTypes.Unsigned | NumericTypes.Max,
                ushort => NumericTypes.Short | NumericTypes.Unsigned,
                uint v when v == default => NumericTypes.Integer | NumericTypes.Unsigned | NumericTypes.Zero,
                uint v when v == uint.MinValue => NumericTypes.Integer | NumericTypes.Unsigned | NumericTypes.Min,
                uint v when v == uint.MaxValue => NumericTypes.Integer | NumericTypes.Unsigned | NumericTypes.Max,
                uint => NumericTypes.Integer | NumericTypes.Unsigned,
                ulong v when v == default => NumericTypes.Long | NumericTypes.Unsigned | NumericTypes.Zero,
                ulong v when v == ulong.MinValue => NumericTypes.Long | NumericTypes.Unsigned | NumericTypes.Min,
                ulong v when v == ulong.MaxValue => NumericTypes.Long | NumericTypes.Unsigned | NumericTypes.Max,
                ulong => NumericTypes.Long | NumericTypes.Unsigned,
                _ => NumericTypes.None
            };

        /// <summary>
        /// Determine if the type is unsigned
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If unsigned</returns>
        public static bool IsUnsigned(this NumericTypes type) => (type & NumericTypes.Unsigned) == NumericTypes.Unsigned;

        /// <summary>
        /// Determine if the type is a floating point
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If is a floating point</returns>
        public static bool IsFloatingPoint(this NumericTypes type)
            => (type & NumericTypes.FLAGS) switch
            {
                NumericTypes.Half or NumericTypes.Float or NumericTypes.Double => true,
                _ => false
            };

        /// <summary>
        /// Determine if the type is a decimal point
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If is a decimal</returns>
        public static bool IsDecimal(this NumericTypes type) => (type & NumericTypes.Decimal) == NumericTypes.Decimal;

        /// <summary>
        /// Get the value flags
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Value flags</returns>
        public static NumericTypes GetValueFlags(this NumericTypes type) => type & NumericTypes.VALUE_FLAGS;

        /// <summary>
        /// Get the value flags
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Value flags</returns>
        public static NumericTypes RemoveValueFlags(this NumericTypes type) => type & ~NumericTypes.VALUE_FLAGS;

        /// <summary>
        /// Determine if value flags are contained
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If value flags are contained</returns>
        public static bool HasValueFlags(this NumericTypes type) => (type & NumericTypes.VALUE_FLAGS) != NumericTypes.None;

        /// <summary>
        /// Get the value
        /// </summary>
        /// <param name="type">Type with value flags</param>
        /// <exception cref="ArgumentException">Unsupported numeric type</exception>
        /// <returns>Value</returns>
        public static object GetValue(this NumericTypes type)
            => type.GetValueFlags() switch
            {
                NumericTypes.Zero => type.RemoveValueFlags().GetDefault(),
                NumericTypes.Min => type.RemoveValueFlags() switch
                {
                    NumericTypes.Byte => sbyte.MinValue,
                    NumericTypes.Short => short.MinValue,
                    NumericTypes.Integer => int.MinValue,
                    NumericTypes.Long => long.MinValue,
                    NumericTypes.Half => Half.MinValue,
                    NumericTypes.Float => float.MinValue,
                    NumericTypes.Double => double.MinValue,
                    NumericTypes.Decimal => decimal.MinValue,
                    NumericTypes.Byte | NumericTypes.Unsigned => byte.MinValue,
                    NumericTypes.Short | NumericTypes.Unsigned => ushort.MinValue,
                    NumericTypes.Integer | NumericTypes.Unsigned => uint.MinValue,
                    NumericTypes.Long | NumericTypes.Unsigned => ulong.MinValue,
                    _ => throw new ArgumentException($"Unsupported type #{(byte)type}")
                },
                NumericTypes.Max => type.RemoveValueFlags() switch
                {
                    NumericTypes.Byte => sbyte.MaxValue,
                    NumericTypes.Short => short.MaxValue,
                    NumericTypes.Integer => int.MaxValue,
                    NumericTypes.Long => long.MaxValue,
                    NumericTypes.Half => Half.MaxValue,
                    NumericTypes.Float => float.MaxValue,
                    NumericTypes.Double => double.MaxValue,
                    NumericTypes.Decimal => decimal.MaxValue,
                    NumericTypes.Byte | NumericTypes.Unsigned => byte.MaxValue,
                    NumericTypes.Short | NumericTypes.Unsigned => ushort.MaxValue,
                    NumericTypes.Integer | NumericTypes.Unsigned => uint.MaxValue,
                    NumericTypes.Long | NumericTypes.Unsigned => ulong.MaxValue,
                    _ => throw new ArgumentException($"Unsupported type #{(byte)type}")
                },
                _ => throw new ArgumentException("No value flags", nameof(type))
            };

        /// <summary>
        /// Get the numeric type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Numeric type</returns>
        public static NumericTypes GetNumericType(this Type type)
        {
            if (type == typeof(sbyte)) return NumericTypes.Byte;
            if (type == typeof(short)) return NumericTypes.Short;
            if (type == typeof(int)) return NumericTypes.Integer;
            if (type == typeof(long)) return NumericTypes.Long;
            if (type == typeof(Half)) return NumericTypes.Half;
            if (type == typeof(float)) return NumericTypes.Float;
            if (type == typeof(double)) return NumericTypes.Double;
            if (type == typeof(decimal)) return NumericTypes.Decimal;
            if (type == typeof(byte)) return NumericTypes.Byte | NumericTypes.Unsigned;
            if (type == typeof(ushort)) return NumericTypes.Short | NumericTypes.Unsigned;
            if (type == typeof(uint)) return NumericTypes.Integer | NumericTypes.Unsigned;
            if (type == typeof(ulong)) return NumericTypes.Long | NumericTypes.Unsigned;
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
            if (res == NumericTypes.None || res.HasValueFlags() || res.IsDecimal()) return res;
            if (res.IsFloatingPoint())
            {
                double v = value.CastType<double>();
                if (v < float.MinValue || v > float.MaxValue) return NumericTypes.Double;
                if (v < (float)Half.MinValue || v > (float)Half.MaxValue) return NumericTypes.Float;
                return NumericTypes.Half;
            }
            if (!res.IsUnsigned())
            {
                long v = value.CastType<long>();
                if (v < 0)
                {
                    if (v < int.MinValue) return NumericTypes.Long;
                    if (v < short.MinValue) return NumericTypes.Integer;
                    if (v < sbyte.MinValue) return NumericTypes.Short;
                    return NumericTypes.Byte;
                }
            }
            {
                ulong v = value.CastType<ulong>();
                if (v > long.MaxValue) return NumericTypes.Long | NumericTypes.Unsigned;
                if (v > uint.MaxValue) return NumericTypes.Long;
                if (v > int.MaxValue) return NumericTypes.Integer | NumericTypes.Unsigned;
                if (v > ushort.MaxValue) return NumericTypes.Integer;
                if (v > (uint)short.MaxValue) return NumericTypes.Short | NumericTypes.Unsigned;
                if (v > byte.MaxValue) return NumericTypes.Short;
                if (v > (byte)sbyte.MaxValue) return NumericTypes.Byte | NumericTypes.Unsigned;
                return NumericTypes.Byte;
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
    }
}
