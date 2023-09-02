using System.Runtime;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Bitwise UInt8 extensions
    /// </summary>
#if NO_UNSAFE
    public static class BitwiseUInt8Extensions
#else
    public static unsafe class BitwiseUInt8Extensions
#endif
    {
        /// <summary>
        /// Bit rotation offsets
        /// </summary>
        private static readonly int[] BitRotation = new int[]
        {
            0, 7, 6, 5, 4, 3, 2, 1
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
        static BitwiseUInt8Extensions()
        {
            BitRotationPin = GCHandle.Alloc(BitRotation, GCHandleType.Pinned);
            BitRotationPtr = (int*)BitRotationPin.AddrOfPinnedObject();
        }
#endif

        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte ShiftLeft(this byte value, in int bits) => (byte)(value << bits);

        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte ShiftRight(this byte value, in int bits) => (byte)(value >> bits);

        /// <summary>
        /// Rotate bits left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static byte RotateLeft(this byte value, in int bits)
        {
            if (bits < 0 || bits > 8) throw new ArgumentOutOfRangeException(nameof(bits));
            if (bits == 0 || bits == 8 || value == 0 || value == byte.MaxValue) return value;
#if NO_UNSAFE
            return (byte)((value << bits) | (value >> BitRotation[bits]));
#else
            return (byte)((value << bits) | (value >> BitRotationPtr[bits]));
#endif
        }

        /// <summary>
        /// Rotate bits right
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static byte RotateRight(this byte value, in int bits)
        {
            if (bits < 0 || bits > 8) throw new ArgumentOutOfRangeException(nameof(bits));
            if (bits == 0 || bits == 8 || value == 0 || value == byte.MaxValue) return value;
#if NO_UNSAFE
            return (byte)((value >> bits) | (value << BitRotation[bits]));
#else
            return (byte)((value >> bits) | (value << BitRotationPtr[bits]));
#endif
        }

        /// <summary>
        /// Logical OR
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte Or(this byte value, in byte other) => (byte)(value | other);

        /// <summary>
        /// Logical AND
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte And(this byte value, in byte other) => (byte)(value & other);

        /// <summary>
        /// Logical XOR
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte Xor(this byte value, in byte other) => (byte)(value ^ other);

        /// <summary>
        /// Has flags?
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Has the flags?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool HasFlags(this byte value, in byte flags) => (value & flags) == flags;

        /// <summary>
        /// Add flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte AddFlags(this byte value, in byte flags) => (byte)(value | flags);

        /// <summary>
        /// Remove flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte RemoveFlags(this byte value, in byte flags) => (byte)(value & ~flags);

        /// <summary>
        /// Cast as signed byte
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static sbyte ToSByte(this byte value) => (sbyte)value;

        /// <summary>
        /// Cast as short
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static short ToShort(this byte value) => value;

        /// <summary>
        /// Cast as unsigned short
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ushort ToUShort(this byte value) => value;

        /// <summary>
        /// Cast as integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int ToInt(this byte value) => value;

        /// <summary>
        /// Cast as unsigned integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static uint ToUInt(this byte value) => value;

        /// <summary>
        /// Cast as long
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static long ToLong(this byte value) => value;

        /// <summary>
        /// Cast as unsigned long
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong ToULong(this byte value) => value;
    }
}
