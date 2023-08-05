using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Bitwise Int32 extensions
    /// </summary>
    public static class BitwiseInt32Extensions
    {
        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int ShiftLeft(this int value, int bits) => value << bits;

        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int ShiftRight(this int value, int bits) => value >> bits;

        /// <summary>
        /// Logical OR
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int Or(this int value, int other) => value | other;

        /// <summary>
        /// Logical AND
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int And(this int value, int other) => value & other;

        /// <summary>
        /// Logical XOR
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int Xor(this int value, int other) => value ^ other;

        /// <summary>
        /// Has flags?
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Has the flags?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool HasFlags(this int value, int flags) => (value & flags) == flags;

        /// <summary>
        /// Add flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int AddFlags(this int value, int flags) => value | flags;

        /// <summary>
        /// Remove flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int RemoveFlags(this int value, int flags) => value & ~flags;

        /// <summary>
        /// Cast as signed byte
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static sbyte ToSByte(this int value) => (sbyte)value;

        /// <summary>
        /// Cast as byte
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte ToByte(this int value) => (byte)value;

        /// <summary>
        /// Cast as short
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static short ToShort(this int value) => (short)value;

        /// <summary>
        /// Cast as unsigned short
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ushort ToUShort(this int value) => (ushort)value;

        /// <summary>
        /// Cast as unsigned integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static uint ToUInt(this int value) => (uint)value;

        /// <summary>
        /// Cast as long
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static long ToLong(this int value) => value;

        /// <summary>
        /// Cast as unsigned long
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong ToULong(this int value) => (ulong)value;
    }
}
