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
        /// <returns>Type and flags (<see cref="SerializerNumericTypes.MinValue"/> and <see cref="SerializerNumericTypes.MaxValue"/>, <see cref="SerializerNumericTypes.Signed"/> in case the 
        /// type is <see cref="SerializerNumericTypes.BigInteger"/> or <see cref="SerializerNumericTypes.Infinity"/>)</returns>
        public static (SerializerNumericTypes Type, SerializerNumericTypes Flags) GetTypeAnFlags(this SerializerNumericTypes types)
        {
            SerializerNumericTypes typeOnly = types & ~(SerializerNumericTypes.MinValue | SerializerNumericTypes.MaxValue),
                flags = types & (SerializerNumericTypes.MinValue | SerializerNumericTypes.MaxValue);
            return typeOnly switch
            {
                SerializerNumericTypes.None => (typeOnly, SerializerNumericTypes.None),
                SerializerNumericTypes.Infinity => (typeOnly, SerializerNumericTypes.None),
                SerializerNumericTypes.Infinity | SerializerNumericTypes.Signed => (typeOnly, SerializerNumericTypes.Signed),
                SerializerNumericTypes.Byte or
                    SerializerNumericTypes.SByte or
                    SerializerNumericTypes.UShort or
                    SerializerNumericTypes.Short or
                    SerializerNumericTypes.Half or
                    SerializerNumericTypes.UInt or
                    SerializerNumericTypes.Int or
                    SerializerNumericTypes.Float or
                    SerializerNumericTypes.ULong or
                    SerializerNumericTypes.Long or
                    SerializerNumericTypes.Double or
                    SerializerNumericTypes.Decimal or
                    SerializerNumericTypes.BigInteger => (typeOnly, flags),
                SerializerNumericTypes.BigInteger | SerializerNumericTypes.Signed => (typeOnly, flags | SerializerNumericTypes.Signed),
                _ => throw new ArgumentException("Invalid numeric type", nameof(types)),
            };
        }

        /// <summary>
        /// Get the CLR type from a type (see <see cref="GetTypeAnFlags(SerializerNumericTypes)"/>)
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>CLR type</returns>
        public static Type GetClrType(this SerializerNumericTypes type)
            => type switch
            {
                SerializerNumericTypes.None or
                    SerializerNumericTypes.Infinity or
                    SerializerNumericTypes.Double => typeof(double),
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
                SerializerNumericTypes.Decimal => typeof(decimal),
                SerializerNumericTypes.BigInteger => typeof(BigInteger),
                _ => throw new ArgumentException("Invalid numeric type", nameof(type))
            };

        /// <summary>
        /// Get the numeric value of a serializer numeric type and its flags
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="flags">Flags</param>
        /// <returns>Numeric value</returns>
        public static object GetNumericValue(this SerializerNumericTypes type, in SerializerNumericTypes flags)
            => type switch
            {
                SerializerNumericTypes.None => double.NaN,
                SerializerNumericTypes.Infinity => flags switch
                {
                    SerializerNumericTypes.Signed => double.NegativeInfinity,
                    SerializerNumericTypes.None => double.PositiveInfinity,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.Double => flags switch
                {
                    SerializerNumericTypes.MinValue => double.MinValue,
                    SerializerNumericTypes.MaxValue => double.MaxValue,
                    SerializerNumericTypes.None => 0d,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
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
                    SerializerNumericTypes.MinValue => Half.MinValue,
                    SerializerNumericTypes.MaxValue => Half.MaxValue,
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
                    SerializerNumericTypes.MinValue => float.MinValue,
                    SerializerNumericTypes.MaxValue => float.MaxValue,
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
                SerializerNumericTypes.Decimal => flags switch
                {
                    SerializerNumericTypes.MinValue => decimal.MinValue,
                    SerializerNumericTypes.MaxValue => decimal.MaxValue,
                    SerializerNumericTypes.None => (decimal)0,
                    _ => throw new ArgumentException("Invalid flags", nameof(flags))
                },
                SerializerNumericTypes.BigInteger => BigInteger.Zero,
                _ => throw new ArgumentException("Invalid numeric type", nameof(type))
            };

        /// <summary>
        /// Get the serializer numeric type for a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="number">Number</param>
        /// <returns><see cref="SerializerNumericTypes"/></returns>
        public static SerializerNumericTypes GetSerializerNumericTypes<T>(this T number) where T : struct, IConvertible
        {
            switch (number)
            {
                case double num:
                    if (double.IsNaN(num)) return SerializerNumericTypes.None;
                    if (num == double.MinValue) return SerializerNumericTypes.Double | SerializerNumericTypes.MinValue;
                    if (num == double.MaxValue) return SerializerNumericTypes.Double | SerializerNumericTypes.MaxValue;
                    if (double.IsFinite(num)) return SerializerNumericTypes.Double;
                    if (double.IsNegativeInfinity(num)) return SerializerNumericTypes.Infinity;
                    if (double.IsPositiveInfinity(num)) return SerializerNumericTypes.Infinity | SerializerNumericTypes.Signed;
                    throw new InvalidProgramException($"Failed to get serializer numeric type of {typeof(T)} value \"{number}\"");
                case byte num:
                    if (num == byte.MinValue) return SerializerNumericTypes.Byte | SerializerNumericTypes.MinValue;
                    if (num == byte.MaxValue) return SerializerNumericTypes.Byte | SerializerNumericTypes.MaxValue;
                    return SerializerNumericTypes.Byte;
                case sbyte num:
                    if (num == sbyte.MinValue) return SerializerNumericTypes.SByte | SerializerNumericTypes.MinValue;
                    if (num == sbyte.MaxValue) return SerializerNumericTypes.SByte | SerializerNumericTypes.MaxValue;
                    return SerializerNumericTypes.SByte;
                    //TODO Add more cases
            }
        }
    }
}
