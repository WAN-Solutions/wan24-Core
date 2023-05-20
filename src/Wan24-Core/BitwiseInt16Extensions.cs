namespace wan24.Core
{
    /// <summary>
    /// Bitwise Int16 extensions
    /// </summary>
    public static class BitwiseInt16Extensions
    {
        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static short ShiftLeft(this short value, int bits) => (short)(value << bits);

        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static short ShiftRight(this short value, int bits) => (short)(value >> bits);

        /// <summary>
        /// Has flags?
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Has the flags?</returns>
        public static bool HasFlags(this short value, short flags) => (value & flags) == flags;

        /// <summary>
        /// Add flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        public static short AddFlags(this short value, short flags) => (short)(value | flags);

        /// <summary>
        /// Remove flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        public static short RemoveFlags(this short value, short flags) => (short)(value & ~flags);

        /// <summary>
        /// Cast as byte
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static byte ToByte(this short value) => (byte)value;

        /// <summary>
        /// Cast as signed byte
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static sbyte ToSByte(this short value) => (sbyte)value;

        /// <summary>
        /// Cast as unsigned short
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static ushort ToUShort(this short value) => (ushort)value;

        /// <summary>
        /// Cast as integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static int ToInt(this short value) => value;

        /// <summary>
        /// Cast as unsigned integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static uint ToUInt(this short value) => (uint)value;

        /// <summary>
        /// Cast as long
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static long ToLong(this short value) => value;

        /// <summary>
        /// Cast as unsigned long
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static ulong ToULong(this short value) => (ulong)value;
    }
}
