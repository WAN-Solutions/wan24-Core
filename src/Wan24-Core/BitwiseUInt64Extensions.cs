using System.Runtime;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Bitwise UInt64 extensions
    /// </summary>
#if NO_UNSAFE
    public static class BitwiseUInt64Extensions
#else
    public static unsafe class BitwiseUInt64Extensions
#endif
    {
        /// <summary>
        /// Bit rotation offsets
        /// </summary>
        private static readonly int[] BitRotation = new int[]
        {
            0, 63, 62, 61, 60, 59, 58, 57, 56, 55, 54, 53, 52, 51, 50, 49, 48, 47, 46, 45, 44, 43, 42, 41, 40, 39, 38, 37, 36, 35, 34, 33, 32, 
            31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1
        };
#if !NO_UNSAFE
        /// <summary>
        /// Bit rotation pointer
        /// </summary>
        private static readonly int* BitRotationPtr;
        /// <summary>
        /// Bit rotation pin
        /// </summary>
        private static readonly GCHandle BitRotationPin;

        /// <summary>
        /// Constructor
        /// </summary>
        static BitwiseUInt64Extensions()
        {
            BitRotationPin = GCHandle.Alloc(BitRotation, GCHandleType.Pinned);
            BitRotationPtr = (int*)BitRotationPin.AddrOfPinnedObject();
        }
#endif

        /// <summary>
        /// Rotate bits left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static ulong RotateLeft(this ulong value, int bits)
        {
            if (bits < 0 || bits > 64) throw new ArgumentOutOfRangeException(nameof(bits));
            if (bits == 0 || bits == 64 || value == 0 || value == ulong.MaxValue) return value;
#if NO_UNSAFE
            return (value << bits) | (value >> BitRotation[bits]);
#else
            return (value << bits) | (value >> BitRotationPtr[bits]);
#endif
        }

        /// <summary>
        /// Rotate bits right
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static ulong RotateRight(this ulong value, int bits)
        {
            if (bits < 0 || bits > 64) throw new ArgumentOutOfRangeException(nameof(bits));
            if (bits == 0 || bits == 64 || value == 0 || value == ulong.MaxValue) return value;
#if NO_UNSAFE
            return (value >> bits) | (value << BitRotation[bits]);
#else
            return (value >> bits) | (value << BitRotationPtr[bits]);
#endif
        }

        /// <summary>
        /// Logical OR
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        public static ulong Or(this ulong value, ulong other) => value | other;

        /// <summary>
        /// Logical AND
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        public static ulong And(this ulong value, ulong other) => value & other;

        /// <summary>
        /// Logical XOR
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong Xor(this ulong value, ulong other) => value ^ other;

        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong ShiftLeft(this ulong value, int bits) => value << bits;

        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong ShiftRight(this ulong value, int bits) => value >> bits;

        /// <summary>
        /// Has flags?
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Has the flags?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool HasFlags(this ulong value, ulong flags) => (value & flags) == flags;

        /// <summary>
        /// Add flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong AddFlags(this ulong value, ulong flags) => value | flags;

        /// <summary>
        /// Remove flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong RemoveFlags(this ulong value, ulong flags) => value & ~flags;

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
