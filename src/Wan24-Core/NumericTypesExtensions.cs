using System.Numerics;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="NumericTypes"/> extensions
    /// </summary>
    public static partial class NumericTypesExtensions
    {
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
                    => NumericTypes.Decimal,
                NumericTypes.BigInteger => NumericTypes.BigInteger,
                _ => (type & NumericTypes.Number71To199) == NumericTypes.Number71To199 || (type & ~NumericTypes.FLAGS) >= NumericTypes.Number2
                    ? NumericTypes.Int
                    : NumericTypes.None
            };
    }
}
