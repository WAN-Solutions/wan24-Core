namespace wan24.Core
{
    /// <summary>
    /// Serializer numeric types
    /// </summary>
    [Flags]
    public enum SerializerNumericTypes : byte
    {
        /// <summary>
        /// None (not a number <see cref="double"/> value)
        /// </summary>
        [DisplayText("Not a number")]
        None = 0,
        /// <summary>
        /// <see cref="byte"/> value (8 bit)
        /// </summary>
        [DisplayText("Unsigned byte (8 bit)")]
        Byte = 1,
        /// <summary>
        /// <see cref="sbyte"/> value
        /// </summary>
        [DisplayText("Signed byte (8 bit)")]
        SByte = Byte | Signed,
        /// <summary>
        /// <see cref="ushort"/> value (16 bit)
        /// </summary>
        [DisplayText("Unsigned short (16 bit)")]
        UShort = 2,
        /// <summary>
        /// <see cref="short"/> value
        /// </summary>
        [DisplayText("Signed short")]
        Short = UShort | Signed,
        /// <summary>
        /// <see cref="System.Half"/> value
        /// </summary>
        [DisplayText("Half value")]
        Half = UShort | FloatingPoint,
        /// <summary>
        /// <see cref="uint"/> value (32 bit)
        /// </summary>
        [DisplayText("Unsigned integer (32 bit)")]
        UInt = 3,
        /// <summary>
        /// <see cref="int"/> value
        /// </summary>
        [DisplayText("Signed integer")]
        Int = UInt | Signed,
        /// <summary>
        /// <see cref="float"/> value
        /// </summary>
        [DisplayText("Float value")]
        Float = UInt | FloatingPoint,
        /// <summary>
        /// <see cref="ulong"/> value (64 bit)
        /// </summary>
        [DisplayText("Unsigned long integer (64 bit)")]
        ULong = 4,
        /// <summary>
        /// <see cref="long"/> value
        /// </summary>
        [DisplayText("Signed long integer")]
        Long = ULong | Signed,
        /// <summary>
        /// <see cref="double"/> value
        /// </summary>
        [DisplayText("Double value")]
        Double = ULong | FloatingPoint,
        /// <summary>
        /// <see cref="decimal"/> value (128 bit)
        /// </summary>
        [DisplayText("Decimal value (128 bit)")]
        Decimal = 5,
        /// <summary>
        /// <see cref="System.Numerics.BigInteger"/> value
        /// </summary>
        [DisplayText("Big integer value")]
        BigInteger = Decimal | FloatingPoint,
        /// <summary>
        /// Infinity <see cref="double"/> value flag
        /// </summary>
        [DisplayText("Infinity double value")]
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
        [DisplayText("Signed")]
        Signed = 128,
        /// <summary>
        /// All flags
        /// </summary>
        [DisplayText("All flags")]
        FLAGS = Infinity | MinValue | MaxValue | FloatingPoint | Signed
    }
}
