namespace wan24.Core
{
    /// <summary>
    /// Bitwise UInt8 extensions
    /// </summary>
    public static class BitwiseUInt8Extensions
    {
        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static byte ShiftLeft(this byte value, int bits) => (byte)(value << bits);

        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static byte ShiftRight(this byte value, int bits) => (byte)(value >> bits);

        /// <summary>
        /// Has flags?
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Has the flags?</returns>
        public static bool HasFlags(this byte value, byte flags) => (value & flags) == flags;

        /// <summary>
        /// Add flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        public static byte AddFlags(this byte value, byte flags) => (byte)(value | flags);

        /// <summary>
        /// Remove flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        public static byte RemoveFlags(this byte value, byte flags) => (byte)(value & ~flags);

        /// <summary>
        /// Cast as signed byte
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static sbyte ToSByte(this byte value) => (sbyte)value;

        /// <summary>
        /// Cast as short
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static short ToShort(this byte value) => value;

        /// <summary>
        /// Cast as unsigned short
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static ushort ToUShort(this byte value) => value;

        /// <summary>
        /// Cast as integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static int ToInt(this byte value) => value;

        /// <summary>
        /// Cast as unsigned integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static uint ToUInt(this byte value) => value;

        /// <summary>
        /// Cast as long
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static long ToLong(this byte value) => value;

        /// <summary>
        /// Cast as unsigned long
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public static ulong ToULong(this byte value) => value;
    }
}
