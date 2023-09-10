using System.Runtime;

namespace wan24.Core
{
    // Int8
    public static partial class BitwiseExtensions
    {
        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static sbyte ShiftLeft(this sbyte value, in int bits) => (sbyte)(value << bits);

        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static sbyte ShiftRight(this sbyte value, in int bits) => (sbyte)(value >> bits);

        /// <summary>
        /// Logical OR
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static sbyte Or(this sbyte value, in sbyte other) => (sbyte)(value | other);

        /// <summary>
        /// Logical AND
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static sbyte And(this sbyte value, in sbyte other) => (sbyte)(value & other);

        /// <summary>
        /// Logical XOR
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static sbyte Xor(this sbyte value, in sbyte other) => (sbyte)(value ^ other);

        /// <summary>
        /// Has flags?
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Has the flags?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool HasFlags(this sbyte value, in sbyte flags) => (value & flags) == flags;

        /// <summary>
        /// Add flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static sbyte AddFlags(this sbyte value, in sbyte flags) => (sbyte)(value | flags);

        /// <summary>
        /// Remove flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static sbyte RemoveFlags(this sbyte value, in sbyte flags) => (sbyte)(value & ~flags);

        /// <summary>
        /// Cast as byte
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte ToByte(this sbyte value) => (byte)value;

        /// <summary>
        /// Cast as short
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static short ToShort(this sbyte value) => value;

        /// <summary>
        /// Cast as unsigned short
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ushort ToUShort(this sbyte value) => (ushort)value;

        /// <summary>
        /// Cast as integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int ToInt(this sbyte value) => value;

        /// <summary>
        /// Cast as unsigned integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static uint ToUInt(this sbyte value) => (uint)value;

        /// <summary>
        /// Cast as long
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static long ToLong(this sbyte value) => value;

        /// <summary>
        /// Cast as unsigned long
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong ToULong(this sbyte value) => (ulong)value;
    }
}
