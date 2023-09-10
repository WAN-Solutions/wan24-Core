using System.Runtime;

namespace wan24.Core
{
    // UInt64
#if NO_UNSAFE
    public static partial class BitwiseExtensions
#else
    public static unsafe partial class BitwiseExtensions
#endif
    {
        /// <summary>
        /// Rotate bits left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static ulong RotateLeft(this ulong value, in int bits)
        {
            if (bits < 0 || bits > 64) throw new ArgumentOutOfRangeException(nameof(bits));
            if (bits == 0 || bits == 64 || value == 0 || value == ulong.MaxValue) return value;
#if NO_UNSAFE
            return (value << bits) | (value >> BitRotation[bits]);
#else
            return (value << bits) | (value >> BitRotationUInt64Ptr[bits]);
#endif
        }

        /// <summary>
        /// Rotate bits right
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static ulong RotateRight(this ulong value, in int bits)
        {
            if (bits < 0 || bits > 64) throw new ArgumentOutOfRangeException(nameof(bits));
            if (bits == 0 || bits == 64 || value == 0 || value == ulong.MaxValue) return value;
#if NO_UNSAFE
            return (value >> bits) | (value << BitRotation[bits]);
#else
            return (value >> bits) | (value << BitRotationUInt64Ptr[bits]);
#endif
        }

        /// <summary>
        /// Logical OR
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        public static ulong Or(this ulong value, in ulong other) => value | other;

        /// <summary>
        /// Logical AND
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        public static ulong And(this ulong value, in ulong other) => value & other;

        /// <summary>
        /// Logical XOR
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong Xor(this ulong value, in ulong other) => value ^ other;

        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong ShiftLeft(this ulong value, in int bits) => value << bits;

        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong ShiftRight(this ulong value, in int bits) => value >> bits;

        /// <summary>
        /// Has flags?
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Has the flags?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool HasFlags(this ulong value, in ulong flags) => (value & flags) == flags;

        /// <summary>
        /// Add flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong AddFlags(this ulong value, in ulong flags) => value | flags;

        /// <summary>
        /// Remove flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong RemoveFlags(this ulong value, in ulong flags) => value & ~flags;

        /// <summary>
        /// Cast as signed byte
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static sbyte ToSByte(this ulong value) => (sbyte)value;

        /// <summary>
        /// Cast as byte
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte ToByte(this ulong value) => (byte)value;

        /// <summary>
        /// Cast as short
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static short ToShort(this ulong value) => (short)value;

        /// <summary>
        /// Cast as unsigned short
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ushort ToUShort(this ulong value) => (ushort)value;

        /// <summary>
        /// Cast as integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int ToInt(this ulong value) => (int)value;

        /// <summary>
        /// Cast as unsigned integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static uint ToUInt(this ulong value) => (uint)value;

        /// <summary>
        /// Cast as long
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static long ToLong(this ulong value) => (long)value;
    }
}
