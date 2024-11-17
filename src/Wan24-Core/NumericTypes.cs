namespace wan24.Core
{
    /// <summary>
    /// Numeric types
    /// </summary>
    [Flags]
    public enum NumericTypes : byte
    {
        /// <summary>
        /// None
        /// </summary>
        [DisplayText("None")]
        None = 0,
        /// <summary>
        /// Byte
        /// </summary>
        [DisplayText("Byte")]
        Byte = 1,
        /// <summary>
        /// Short
        /// </summary>
        [DisplayText("Short")]
        Short = 2,
        /// <summary>
        /// Integer
        /// </summary>
        [DisplayText("Integer")]
        Integer = 3,
        /// <summary>
        /// Long
        /// </summary>
        [DisplayText("Long")]
        Long = 4,
        /// <summary>
        /// Half
        /// </summary>
        [DisplayText("Half")]
        Half = 5,
        /// <summary>
        /// Float
        /// </summary>
        [DisplayText("Float")]
        Float = 6,
        /// <summary>
        /// Double
        /// </summary>
        [DisplayText("Double")]
        Double = 7,
        /// <summary>
        /// Decimal
        /// </summary>
        [DisplayText("Decimal")]
        Decimal = 8,
        /// <summary>
        /// Zero value
        /// </summary>
        [DisplayText("Zero")]
        Zero = 16,
        /// <summary>
        /// Minimum value
        /// </summary>
        [DisplayText("Minimum value")]
        Min = 32,
        /// <summary>
        /// Maximum value
        /// </summary>
        [DisplayText("Maximum value")]
        Max = 64,
        /// <summary>
        /// Unsigned flag
        /// </summary>
        [DisplayText("Unsigned")]
        Unsigned = 128,
        /// <summary>
        /// Value flags
        /// </summary>
        [DisplayText("Value flags")]
        VALUE_FLAGS = Zero | Min | Max,
        /// <summary>
        /// All flags
        /// </summary>
        [DisplayText("All flags")]
        FLAGS = VALUE_FLAGS | Unsigned,
    }
}
