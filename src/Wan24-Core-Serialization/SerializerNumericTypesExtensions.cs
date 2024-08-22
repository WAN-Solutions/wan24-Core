using System.Numerics;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="SerializerNumericTypes"/> extensions
    /// </summary>
    public static class SerializerNumericTypesExtensions
    {
        /// <summary>
        /// Get the type and flags from <see cref="SerializerNumericTypes"/>
        /// </summary>
        /// <param name="types">Types</param>
        /// <returns>Type and flags (see <see cref="SerializerNumericTypes.SPECIAL_FLAGS"/>)</returns>
        public static (SerializerNumericTypes Type, SerializerNumericTypes Flags) SeparateTypeAndFlags(this SerializerNumericTypes types)
            => (types & ~SerializerNumericTypes.SPECIAL_FLAGS, types & SerializerNumericTypes.SPECIAL_FLAGS);

        /// <summary>
        /// Get the CLR type from a type
        /// </summary>
        /// <param name="type">Type (may contain <see cref="SerializerNumericTypes.SPECIAL_FLAGS"/>)</param>
        /// <returns>CLR type</returns>
        /// <exception cref="ArgumentException">Invalid numeric type</exception>
        public static Type ToClrType(this SerializerNumericTypes type)
            => (type & ~SerializerNumericTypes.SPECIAL_FLAGS) switch
            {
                SerializerNumericTypes.Byte => typeof(byte),
                SerializerNumericTypes.SByte => typeof(sbyte),
                SerializerNumericTypes.UShort => typeof(ushort),
                SerializerNumericTypes.Short => typeof(short),
                SerializerNumericTypes.Half => typeof(Half),
                SerializerNumericTypes.UInt => typeof(uint),
                SerializerNumericTypes.Int => typeof(int),
                SerializerNumericTypes.Float => typeof(float),
                SerializerNumericTypes.ULong => typeof(ulong),
                SerializerNumericTypes.Long => typeof(long),
                SerializerNumericTypes.Double => typeof(double),
                SerializerNumericTypes.Decimal => typeof(decimal),
                SerializerNumericTypes.BigInteger => typeof(BigInteger),
                _ => throw new ArgumentException($"Invalid numeric type {type}", nameof(type))
            };

        /// <summary>
        /// Get the numeric value of a serializer numeric type and its flags
        /// </summary>
        /// <param name="type">Type (may contain <see cref="SerializerNumericTypes.SPECIAL_FLAGS"/>)</param>
        /// <returns>Numeric value</returns>
        /// <exception cref="ArgumentException">Invalid numeric type or invalid flags</exception>
        public static object ToNumericValue(this SerializerNumericTypes type)
        {
            (SerializerNumericTypes types, SerializerNumericTypes flags) = type.SeparateTypeAndFlags();
            return ToNumericValue(types, flags);
        }

        /// <summary>
        /// Get the numeric value of a serializer numeric type and its flags
        /// </summary>
        /// <param name="type">Type (without flags)</param>
        /// <param name="flags">Flags (see <see cref="SerializerNumericTypes.SPECIAL_FLAGS"/>)</param>
        /// <returns>Numeric value</returns>
        /// <exception cref="ArgumentException">Invalid numeric type or invalid flags</exception>
        public static object ToNumericValue(this SerializerNumericTypes type, in SerializerNumericTypes flags)
            => type switch
            {
                SerializerNumericTypes.Byte => flags switch
                {
                    SerializerNumericTypes.MinValue => byte.MinValue,
                    SerializerNumericTypes.MaxValue => byte.MaxValue,
                    SerializerNumericTypes.None => (byte)0,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.SByte => flags switch
                {
                    SerializerNumericTypes.MinValue => sbyte.MinValue,
                    SerializerNumericTypes.MaxValue => sbyte.MaxValue,
                    SerializerNumericTypes.None => (sbyte)0,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.UShort => flags switch
                {
                    SerializerNumericTypes.MinValue => ushort.MinValue,
                    SerializerNumericTypes.MaxValue => ushort.MaxValue,
                    SerializerNumericTypes.None => (ushort)0,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.Short => flags switch
                {
                    SerializerNumericTypes.MinValue => short.MinValue,
                    SerializerNumericTypes.MaxValue => short.MaxValue,
                    SerializerNumericTypes.None => (short)0,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.Half => flags switch
                {
                    SerializerNumericTypes.NaN => Half.NaN,
                    SerializerNumericTypes.MinValue => Half.MinValue,
                    SerializerNumericTypes.MaxValue => Half.MaxValue,
                    SerializerNumericTypes.Infinity => Half.PositiveInfinity,
                    SerializerNumericTypes.Infinity | SerializerNumericTypes.Signed => Half.NegativeInfinity,
                    SerializerNumericTypes.None => (Half)0,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.UInt => flags switch
                {
                    SerializerNumericTypes.MinValue => uint.MinValue,
                    SerializerNumericTypes.MaxValue => uint.MaxValue,
                    SerializerNumericTypes.None => 0U,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.Int => flags switch
                {
                    SerializerNumericTypes.MinValue => int.MinValue,
                    SerializerNumericTypes.MaxValue => int.MaxValue,
                    SerializerNumericTypes.None => 0,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.Float => flags switch
                {
                    SerializerNumericTypes.NaN => float.NaN,
                    SerializerNumericTypes.MinValue => float.MinValue,
                    SerializerNumericTypes.MaxValue => float.MaxValue,
                    SerializerNumericTypes.Infinity => float.PositiveInfinity,
                    SerializerNumericTypes.Infinity | SerializerNumericTypes.Signed => float.NegativeInfinity,
                    SerializerNumericTypes.None => 0f,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.ULong => flags switch
                {
                    SerializerNumericTypes.MinValue => ulong.MinValue,
                    SerializerNumericTypes.MaxValue => ulong.MaxValue,
                    SerializerNumericTypes.None => 0UL,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.Long => flags switch
                {
                    SerializerNumericTypes.MinValue => long.MinValue,
                    SerializerNumericTypes.MaxValue => long.MaxValue,
                    SerializerNumericTypes.None => 0L,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.Double => flags switch
                {
                    SerializerNumericTypes.NaN => double.NaN,
                    SerializerNumericTypes.MinValue => double.MinValue,
                    SerializerNumericTypes.MaxValue => double.MaxValue,
                    SerializerNumericTypes.Infinity => double.PositiveInfinity,
                    SerializerNumericTypes.Infinity | SerializerNumericTypes.Signed => double.NegativeInfinity,
                    SerializerNumericTypes.None => 0d,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.Decimal => flags switch
                {
                    SerializerNumericTypes.MinValue => decimal.MinValue,
                    SerializerNumericTypes.MaxValue => decimal.MaxValue,
                    SerializerNumericTypes.None => decimal.Zero,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.BigInteger => BigInteger.Zero,
                _ => throw new ArgumentException($"Invalid numeric type {type}", nameof(type))
            };

        /// <summary>
        /// Get the serializer numeric type for a number
        /// </summary>
        /// <param name="type">Number</param>
        /// <returns><see cref="SerializerNumericTypes"/> including <see cref="SerializerNumericTypes.SPECIAL_FLAGS"/></returns>
        /// <exception cref="ArgumentException">Not a supported numeric value</exception>
        public static SerializerNumericTypes ToSerializerNumericTypes(this object type)
        {
            switch (type)
            {
                case byte num:
                    if (num == byte.MinValue) return SerializerNumericTypes.Byte | SerializerNumericTypes.MinValue;
                    if (num == byte.MaxValue) return SerializerNumericTypes.Byte | SerializerNumericTypes.MaxValue;
                    return SerializerNumericTypes.Byte;
                case sbyte num:
                    if (num == sbyte.MinValue) return SerializerNumericTypes.SByte | SerializerNumericTypes.MinValue;
                    if (num == sbyte.MaxValue) return SerializerNumericTypes.SByte | SerializerNumericTypes.MaxValue;
                    return SerializerNumericTypes.SByte;
                case ushort num:
                    if (num == ushort.MinValue) return SerializerNumericTypes.UShort | SerializerNumericTypes.MinValue;
                    if (num == ushort.MaxValue) return SerializerNumericTypes.UShort | SerializerNumericTypes.MaxValue;
                    return SerializerNumericTypes.UShort;
                case short num:
                    if (num == short.MinValue) return SerializerNumericTypes.Short | SerializerNumericTypes.MinValue;
                    if (num == short.MaxValue) return SerializerNumericTypes.Short | SerializerNumericTypes.MaxValue;
                    return SerializerNumericTypes.Short;
                case Half num:
                    if (Half.IsNaN(num)) return SerializerNumericTypes.Half | SerializerNumericTypes.NaN;
                    if (num == Half.MinValue) return SerializerNumericTypes.Half | SerializerNumericTypes.MinValue;
                    if (num == Half.MaxValue) return SerializerNumericTypes.Half | SerializerNumericTypes.MaxValue;
                    if (Half.IsPositiveInfinity(num)) return SerializerNumericTypes.Half | SerializerNumericTypes.Infinity;
                    if (Half.IsNegativeInfinity(num)) return SerializerNumericTypes.Half | SerializerNumericTypes.Infinity | SerializerNumericTypes.Signed;
                    return SerializerNumericTypes.Half;
                case uint num:
                    if (num == uint.MinValue) return SerializerNumericTypes.UInt | SerializerNumericTypes.MinValue;
                    if (num == uint.MaxValue) return SerializerNumericTypes.UInt | SerializerNumericTypes.MaxValue;
                    return SerializerNumericTypes.UInt;
                case int num:
                    if (num == int.MinValue) return SerializerNumericTypes.Int | SerializerNumericTypes.MinValue;
                    if (num == int.MaxValue) return SerializerNumericTypes.Int | SerializerNumericTypes.MaxValue;
                    return SerializerNumericTypes.Int;
                case float num:
                    if (float.IsNaN(num)) return SerializerNumericTypes.Float | SerializerNumericTypes.NaN;
                    if (num == float.MinValue) return SerializerNumericTypes.Float | SerializerNumericTypes.MinValue;
                    if (num == float.MaxValue) return SerializerNumericTypes.Float | SerializerNumericTypes.MaxValue;
                    if (float.IsPositiveInfinity(num)) return SerializerNumericTypes.Float | SerializerNumericTypes.Infinity;
                    if (float.IsNegativeInfinity(num)) return SerializerNumericTypes.Float | SerializerNumericTypes.Infinity | SerializerNumericTypes.Signed;
                    return SerializerNumericTypes.Float;
                case ulong num:
                    if (num == ulong.MinValue) return SerializerNumericTypes.ULong | SerializerNumericTypes.MinValue;
                    if (num == ulong.MaxValue) return SerializerNumericTypes.ULong | SerializerNumericTypes.MaxValue;
                    return SerializerNumericTypes.ULong;
                case long num:
                    if (num == long.MinValue) return SerializerNumericTypes.Long | SerializerNumericTypes.MinValue;
                    if (num == long.MaxValue) return SerializerNumericTypes.Long | SerializerNumericTypes.MaxValue;
                    return SerializerNumericTypes.Long;
                case double num:
                    if (double.IsNaN(num)) return SerializerNumericTypes.Double | SerializerNumericTypes.NaN;
                    if (num == double.MinValue) return SerializerNumericTypes.Double | SerializerNumericTypes.MinValue;
                    if (num == double.MaxValue) return SerializerNumericTypes.Double | SerializerNumericTypes.MaxValue;
                    if (double.IsPositiveInfinity(num)) return SerializerNumericTypes.Double | SerializerNumericTypes.Infinity;
                    if (double.IsNegativeInfinity(num)) return SerializerNumericTypes.Double | SerializerNumericTypes.Infinity | SerializerNumericTypes.Signed;
                    return SerializerNumericTypes.Double;
                case decimal num:
                    if (num == decimal.MinValue) return SerializerNumericTypes.Decimal | SerializerNumericTypes.MinValue;
                    if (num == decimal.MaxValue) return SerializerNumericTypes.Decimal | SerializerNumericTypes.MaxValue;
                    return SerializerNumericTypes.Decimal;
                case BigInteger:
                    return SerializerNumericTypes.BigInteger;
                default:
                    throw new ArgumentException("Not a supported numeric value", nameof(type));
            }
        }

        /// <summary>
        /// Get the serializer numeric type for a type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns><see cref="SerializerNumericTypes"/> including <see cref="SerializerNumericTypes.SPECIAL_FLAGS"/></returns>
        public static SerializerNumericTypes ToSerializerNumericTypes(this Type type)
        {
            if (type == typeof(byte)) return SerializerNumericTypes.Byte;
            else if (type == typeof(sbyte)) return SerializerNumericTypes.SByte;
            else if (type == typeof(ushort)) return SerializerNumericTypes.UShort;
            else if (type == typeof(short)) return SerializerNumericTypes.Short;
            else if (type == typeof(Half)) return SerializerNumericTypes.Half;
            else if (type == typeof(uint)) return SerializerNumericTypes.UInt;
            else if (type == typeof(int)) return SerializerNumericTypes.Int;
            else if (type == typeof(float)) return SerializerNumericTypes.Float;
            else if (type == typeof(ulong)) return SerializerNumericTypes.ULong;
            else if (type == typeof(long)) return SerializerNumericTypes.Long;
            else if (type == typeof(double)) return SerializerNumericTypes.Double;
            else if (type == typeof(decimal)) return SerializerNumericTypes.Decimal;
            else if (type == typeof(BigInteger)) return SerializerNumericTypes.BigInteger;
            else return SerializerNumericTypes.None;
        }

        /// <summary>
        /// Get the serializer numeric type for a number
        /// </summary>
        /// <param name="number">Number</param>
        /// <param name="result"><see cref="SerializerNumericTypes"/> including <see cref="SerializerNumericTypes.SPECIAL_FLAGS"/></param>
        /// <returns>If succeed</returns>
        public static bool TryGetSerializerNumericTypes(this object number, out SerializerNumericTypes result)
        {
            switch (number)
            {
                case byte num:
                    if (num == byte.MinValue) result = SerializerNumericTypes.Byte | SerializerNumericTypes.MinValue;
                    else if (num == byte.MaxValue) result = SerializerNumericTypes.Byte | SerializerNumericTypes.MaxValue;
                    else result = SerializerNumericTypes.Byte;
                    return true;
                case sbyte num:
                    if (num == sbyte.MinValue) result = SerializerNumericTypes.SByte | SerializerNumericTypes.MinValue;
                    else if (num == sbyte.MaxValue) result = SerializerNumericTypes.SByte | SerializerNumericTypes.MaxValue;
                    else result = SerializerNumericTypes.SByte;
                    return true;
                case ushort num:
                    if (num == ushort.MinValue) result = SerializerNumericTypes.UShort | SerializerNumericTypes.MinValue;
                    else if (num == ushort.MaxValue) result = SerializerNumericTypes.UShort | SerializerNumericTypes.MaxValue;
                    else result = SerializerNumericTypes.UShort;
                    return true;
                case short num:
                    if (num == short.MinValue) result = SerializerNumericTypes.Short | SerializerNumericTypes.MinValue;
                    else if (num == short.MaxValue) result = SerializerNumericTypes.Short | SerializerNumericTypes.MaxValue;
                    else result = SerializerNumericTypes.Short;
                    return true;
                case Half num:
                    if (Half.IsNaN(num)) result = SerializerNumericTypes.Half | SerializerNumericTypes.NaN;
                    else if (num == Half.MinValue) result = SerializerNumericTypes.Half | SerializerNumericTypes.MinValue;
                    else if (num == Half.MaxValue) result = SerializerNumericTypes.Half | SerializerNumericTypes.MaxValue;
                    else if (Half.IsPositiveInfinity(num)) result = SerializerNumericTypes.Half | SerializerNumericTypes.Infinity;
                    else if (Half.IsNegativeInfinity(num)) result = SerializerNumericTypes.Half | SerializerNumericTypes.Infinity | SerializerNumericTypes.Signed;
                    else result = SerializerNumericTypes.Half;
                    return true;
                case uint num:
                    if (num == uint.MinValue) result = SerializerNumericTypes.UInt | SerializerNumericTypes.MinValue;
                    else if (num == uint.MaxValue) result = SerializerNumericTypes.UInt | SerializerNumericTypes.MaxValue;
                    else result = SerializerNumericTypes.UInt;
                    return true;
                case int num:
                    if (num == int.MinValue) result = SerializerNumericTypes.Int | SerializerNumericTypes.MinValue;
                    else if (num == int.MaxValue) result = SerializerNumericTypes.Int | SerializerNumericTypes.MaxValue;
                    else result = SerializerNumericTypes.Int;
                    return true;
                case float num:
                    if (float.IsNaN(num)) result = SerializerNumericTypes.Float | SerializerNumericTypes.NaN;
                    else if (num == float.MinValue) result = SerializerNumericTypes.Float | SerializerNumericTypes.MinValue;
                    else if (num == float.MaxValue) result = SerializerNumericTypes.Float | SerializerNumericTypes.MaxValue;
                    else if (float.IsPositiveInfinity(num)) result = SerializerNumericTypes.Float | SerializerNumericTypes.Infinity;
                    else if (float.IsNegativeInfinity(num)) result = SerializerNumericTypes.Float | SerializerNumericTypes.Infinity | SerializerNumericTypes.Signed;
                    else result = SerializerNumericTypes.Float;
                    return true;
                case ulong num:
                    if (num == ulong.MinValue) result = SerializerNumericTypes.ULong | SerializerNumericTypes.MinValue;
                    else if (num == ulong.MaxValue) result = SerializerNumericTypes.ULong | SerializerNumericTypes.MaxValue;
                    else result = SerializerNumericTypes.ULong;
                    return true;
                case long num:
                    if (num == long.MinValue) result = SerializerNumericTypes.Long | SerializerNumericTypes.MinValue;
                    else if (num == long.MaxValue) result = SerializerNumericTypes.Long | SerializerNumericTypes.MaxValue;
                    else result = SerializerNumericTypes.Long;
                    return true;
                case double num:
                    if (double.IsNaN(num)) result = SerializerNumericTypes.Double | SerializerNumericTypes.NaN;
                    else if (num == double.MinValue) result = SerializerNumericTypes.Double | SerializerNumericTypes.MinValue;
                    else if (num == double.MaxValue) result = SerializerNumericTypes.Double | SerializerNumericTypes.MaxValue;
                    else if (double.IsPositiveInfinity(num)) result = SerializerNumericTypes.Double | SerializerNumericTypes.Infinity;
                    else if (double.IsNegativeInfinity(num)) result = SerializerNumericTypes.Double | SerializerNumericTypes.Infinity | SerializerNumericTypes.Signed;
                    else result = SerializerNumericTypes.Double;
                    return true;
                case decimal num:
                    if (num == decimal.MinValue) result = SerializerNumericTypes.Decimal | SerializerNumericTypes.MinValue;
                    else if (num == decimal.MaxValue) result = SerializerNumericTypes.Decimal | SerializerNumericTypes.MaxValue;
                    else result = SerializerNumericTypes.Decimal;
                    return true;
                case BigInteger:
                    result = SerializerNumericTypes.BigInteger;
                    return true;
                default:
                    result = SerializerNumericTypes.None;
                    return false;
            }
        }

        /// <summary>
        /// Get the smallest (less serialized data) matching serializer numeric type for a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="number">Number (special values and zero won't be converted)</param>
        /// <returns>Converted number and its type (including <see cref="SerializerNumericTypes.SPECIAL_FLAGS"/>)</returns>
        /// <exception cref="ArgumentException">Not a supported numeric value</exception>
        public static (object Number, SerializerNumericTypes Type) ToSmallestMatchingSerializerNumericTypes<T>(this T number) where T : struct, IConvertible
        {
            object res = number;
            SerializerNumericTypes type = number.ToSerializerNumericTypes();
            (SerializerNumericTypes typeOnly, SerializerNumericTypes flags) = type.SeparateTypeAndFlags();
            if (flags != SerializerNumericTypes.None) return (res, type);
            // Try convert to an integer
            switch (type)
            {
                case SerializerNumericTypes.BigInteger:
                    {
                        BigInteger intVal = (BigInteger)Convert.ChangeType(number, typeof(BigInteger));
                        if (intVal == BigInteger.Zero) return (res, type);
                        if (intVal < BigInteger.Zero)
                        {
                            if (intVal >= long.MinValue)
                            {
                                // Use 64 bit signed integer
                                res = (long)intVal;
                                type = typeOnly = SerializerNumericTypes.Long;
                            }
                        }
                        else if (intVal <= ulong.MaxValue)
                        {
                            // Use 64 bit unsigned integer
                            res = (ulong)intVal;
                            type = typeOnly = SerializerNumericTypes.ULong;
                        }
                        else
                        {
                            // Use big integer
                            return (res, type);
                        }
                    }
                    break;
                case SerializerNumericTypes.Decimal:
                    {
                        decimal decimalVal = (decimal)Convert.ChangeType(res, typeof(decimal));
                        if (decimalVal == 0) return (res, type);
                        if (decimal.IsInteger(decimalVal) && decimalVal <= ulong.MaxValue && decimalVal >= long.MinValue)
                        {
                            // Use a 64 bit integer
                            if (decimalVal <= long.MaxValue)
                            {
                                // Signed
                                res = (long)decimalVal;
                                type = typeOnly = SerializerNumericTypes.Long;
                            }
                            else
                            {
                                // Unsigned
                                res = (ulong)decimalVal;
                                type = typeOnly = SerializerNumericTypes.ULong;
                            }
                        }
                        else
                        {
                            // Use decimal
                            return (res, type);
                        }
                    }
                    break;
                case SerializerNumericTypes.Half:
                case SerializerNumericTypes.Float:
                case SerializerNumericTypes.Double:
                    {
                        double doubleVal = (double)Convert.ChangeType(res, typeof(double));
                        if (doubleVal == 0d) return (res, type);
                        if (double.IsInteger(doubleVal) && doubleVal <= ulong.MaxValue && doubleVal >= long.MinValue)
                            if (doubleVal <= long.MaxValue)
                            {
                                // Signed
                                res = (long)doubleVal;
                                type = typeOnly = SerializerNumericTypes.Long;
                            }
                            else
                            {
                                // Unsigned
                                res = (ulong)doubleVal;
                                type = typeOnly = SerializerNumericTypes.ULong;
                            }
                    }
                    break;
            }
            // Find the smallest matching numeric type
            if ((typeOnly & SerializerNumericTypes.FloatingPoint) == SerializerNumericTypes.FloatingPoint)
            {
                // Floating point
                double doubleVal = (double)Convert.ChangeType(res, typeof(double));
                if (doubleVal < 0)
                {
                    // Negative value
                    if (doubleVal < float.MinValue) return (doubleVal, SerializerNumericTypes.Double);
                    if (doubleVal.CompareTo(Half.MinValue) < 0) return ((float)doubleVal, SerializerNumericTypes.Float);
                    return ((Half)doubleVal, SerializerNumericTypes.Half);
                }
                // Positive value
                if (doubleVal > float.MaxValue) return (doubleVal, SerializerNumericTypes.Double);
                if (doubleVal.CompareTo(Half.MaxValue) > 0) return ((float)doubleVal, SerializerNumericTypes.Float);
                return ((Half)doubleVal, SerializerNumericTypes.Half);
            }
            long longVal;
            if (typeOnly == SerializerNumericTypes.ULong)
            {
                // 64 bit unsigned integer
                ulong ulongVal = (ulong)Convert.ChangeType(res, typeof(ulong));
                if (ulongVal == 0) return (res, type);
                if (ulongVal > long.MaxValue) return (res, type);
                longVal = (long)ulongVal;
            }
            else
            {
                // 64 bit signed integer
                longVal = (long)Convert.ChangeType(res, typeof(long));
                if (longVal == 0) return (res, type);
            }
            if (longVal < 0)
            {
                // Negative value
                if (longVal < int.MaxValue) return (longVal, SerializerNumericTypes.Long);
                if (longVal < short.MaxValue) return ((int)longVal, SerializerNumericTypes.Int);
                if (longVal < sbyte.MaxValue) return ((short)longVal, SerializerNumericTypes.Short);
                return ((sbyte)longVal, SerializerNumericTypes.SByte);
            }
            // Positive value
            if (longVal > uint.MaxValue) return (longVal, SerializerNumericTypes.Long);
            if (longVal > ushort.MaxValue) return ((uint)longVal, SerializerNumericTypes.UInt);
            if (longVal > byte.MaxValue) return ((ushort)longVal, SerializerNumericTypes.UShort);
            return ((byte)longVal, SerializerNumericTypes.Byte);
        }

        /// <summary>
        /// Get the <see cref="SerializerTypeInformation"/>
        /// </summary>
        /// <param name="type"><see cref="SerializerNumericTypes"/></param>
        /// <returns><see cref="SerializerTypeInformation"/></returns>
        public static SerializerTypeInformation ToSerializerTypeInformation(this SerializerNumericTypes type)
            => (type & ~SerializerNumericTypes.SPECIAL_FLAGS) switch
            {
                SerializerNumericTypes.Byte => SerializerTypeInformation.Byte,
                SerializerNumericTypes.SByte => SerializerTypeInformation.SByte,
                SerializerNumericTypes.UShort => SerializerTypeInformation.UShort,
                SerializerNumericTypes.Short => SerializerTypeInformation.Short,
                SerializerNumericTypes.Half => SerializerTypeInformation.Half,
                SerializerNumericTypes.UInt => SerializerTypeInformation.UInt,
                SerializerNumericTypes.Int => SerializerTypeInformation.Int,
                SerializerNumericTypes.Float => SerializerTypeInformation.Float,
                SerializerNumericTypes.ULong => SerializerTypeInformation.ULong,
                SerializerNumericTypes.Long => SerializerTypeInformation.Long,
                SerializerNumericTypes.Double => SerializerTypeInformation.Double,
                SerializerNumericTypes.Decimal => SerializerTypeInformation.Decimal,
                SerializerNumericTypes.BigInteger => SerializerTypeInformation.BigInteger,
                _ => SerializerTypeInformation.Null
            };
    }
}
