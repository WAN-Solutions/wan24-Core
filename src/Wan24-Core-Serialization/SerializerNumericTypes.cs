namespace wan24.Core
{
    /// <summary>
    /// Serializer numeric types
    /// </summary>
    [Flags]
    public enum SerializerNumericTypes : byte
    {
        /// <summary>
        /// None (invalid!)
        /// </summary>
        [DisplayText("None")]
        None = 0,
        /// <summary>
        /// <see cref="byte"/> value (8 bit)
        /// </summary>
        [DisplayText("Unsigned byte (8 bit)")]
        Byte = 1,
        /// <summary>
        /// <see cref="sbyte"/> value (8 bit)
        /// </summary>
        [DisplayText("Signed byte (8 bit)")]
        SByte = Byte | Signed,
        /// <summary>
        /// <see cref="ushort"/> value (16 bit)
        /// </summary>
        [DisplayText("Unsigned short (16 bit)")]
        UShort = 2,
        /// <summary>
        /// <see cref="short"/> value (16 bit)
        /// </summary>
        [DisplayText("Signed short")]
        Short = UShort | Signed,
        /// <summary>
        /// <see cref="System.Half"/> value (16 bit)
        /// </summary>
        [DisplayText("Half value")]
        Half = Short | FloatingPoint,
        /// <summary>
        /// <see cref="uint"/> value (32 bit)
        /// </summary>
        [DisplayText("Unsigned integer (32 bit)")]
        UInt = 3,
        /// <summary>
        /// <see cref="int"/> value (32 bit)
        /// </summary>
        [DisplayText("Signed integer")]
        Int = UInt | Signed,
        /// <summary>
        /// <see cref="float"/> value (32 bit)
        /// </summary>
        [DisplayText("Float value")]
        Float = Int | FloatingPoint,
        /// <summary>
        /// <see cref="ulong"/> value (64 bit)
        /// </summary>
        [DisplayText("Unsigned long integer (64 bit)")]
        ULong = 4,
        /// <summary>
        /// <see cref="long"/> value (64 bit)
        /// </summary>
        [DisplayText("Signed long integer")]
        Long = ULong | Signed,
        /// <summary>
        /// <see cref="double"/> value (64 bit)
        /// </summary>
        [DisplayText("Double value")]
        Double = Long | FloatingPoint,
        /// <summary>
        /// <see cref="decimal"/> value (128 bit)
        /// </summary>
        [DisplayText("Decimal value (128 bit)")]
        Decimal = 5 | FloatingPoint,
        /// <summary>
        /// <see cref="System.Numerics.BigInteger"/> value (variable length)
        /// </summary>
        [DisplayText("Big integer value")]
        BigInteger = 6,
        /// <summary>
        /// Zero (not a valid number type!)
        /// </summary>
        [DisplayText("Zero")]
        Zero = 7,
        /// <summary>
        /// Infinity value flag (for floating point values except <see cref="decimal"/>)
        /// </summary>
        [DisplayText("Infinity value")]
        Infinity = 8,
        /// <summary>
        /// Minimum value flag
        /// </summary>
        [DisplayText("Minimum value")]
        MinValue = 16,
        /// <summary>
        /// Maximum value flag
        /// </summary>
        [DisplayText("Maximum value")]
        MaxValue = 32,
        /// <summary>
        /// Floating point value flag
        /// </summary>
        [DisplayText("Floating point value")]
        FloatingPoint = 64,
        /// <summary>
        /// Signed flag
        /// </summary>
        [DisplayText("Signed value")]
        Signed = 128,
        /// <summary>
        /// Not a number flag (for floating point values except <see cref="decimal"/>)
        /// </summary>
        [DisplayText("Not a number")]
        NaN = MinValue | MaxValue,
        /// <summary>
        /// All flags
        /// </summary>
        [DisplayText("All flags")]
        FLAGS = Infinity | MinValue | MaxValue | FloatingPoint | Signed,
        /// <summary>
        /// Special numeric flags
        /// </summary>
        [DisplayText("Special numeric flags")]
        SPECIAL_FLAGS = MinValue | MaxValue | Infinity | Signed
    }
}
