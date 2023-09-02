using System.Runtime;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Bitwise UInt16 extensions
    /// </summary>
#if NO_UNSAFE
    public static class BitwiseUInt16Extensions
#else
    public static unsafe class BitwiseUInt16Extensions
#endif
    {
        /// <summary>
        /// Bit rotation offsets
        /// </summary>
        private static readonly int[] BitRotation = new int[]
        {
            0, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1
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
        static BitwiseUInt16Extensions()
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
        public static ushort RotateLeft(this ushort value, in int bits)
        {
            if (bits < 0 || bits > 16) throw new ArgumentOutOfRangeException(nameof(bits));
            if (bits == 0 || bits == 16 || value == 0 || value == ushort.MaxValue) return value;
#if NO_UNSAFE
            return (ushort)((value << bits) | (value >> BitRotation[bits]));
#else
            return (ushort)((value << bits) | (value >> BitRotationPtr[bits]));
#endif
        }

        /// <summary>
        /// Rotate bits right
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static ushort RotateRight(this ushort value, in int bits)
        {
            if (bits < 0 || bits > 16) throw new ArgumentOutOfRangeException(nameof(bits));
            if (bits == 0 || bits == 16 || value == 0 || value == ushort.MaxValue) return value;
#if NO_UNSAFE
            return (ushort)((value >> bits) | (value << BitRotation[bits]));
#else
            return (ushort)((value >> bits) | (value << BitRotationPtr[bits]));
#endif
        }

        /// <summary>
        /// Logical OR
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ushort Or(this ushort value, in ushort other) => (ushort)(value | other);

        /// <summary>
        /// Logical AND
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ushort And(this ushort value, in ushort other) => (ushort)(value & other);

        /// <summary>
        /// Logical XOR
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="other">Other value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ushort Xor(this ushort value, in ushort other) => (ushort)(value ^ other);

        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ushort ShiftLeft(this ushort value, in int bits) => (ushort)(value << bits);

        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ushort ShiftRight(this ushort value, in int bits) => (ushort)(value >> bits);

        /// <summary>
        /// Has flags?
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Has the flags?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool HasFlags(this ushort value, in ushort flags) => (value & flags) == flags;

        /// <summary>
        /// Add flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ushort AddFlags(this ushort value, in ushort flags) => (ushort)(value | flags);

        /// <summary>
        /// Remove flags
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="flags">Flags</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ushort RemoveFlags(this ushort value, in ushort flags) => (ushort)(value & ~flags);

        /// <summary>
        /// Cast as signed byte
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static sbyte ToSByte(this ushort value) => (sbyte)value;

        /// <summary>
        /// Cast as byte
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte ToByte(this ushort value) => (byte)value;

        /// <summary>
        /// Cast as short
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static short ToShort(this ushort value) => (short)value;

        /// <summary>
        /// Cast as integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int ToInt(this ushort value) => value;

        /// <summary>
        /// Cast as unsigned integer
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static uint ToUInt(this ushort value) => value;

        /// <summary>
        /// Cast as long
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static long ToLong(this ushort value) => value;

        /// <summary>
        /// Cast as unsigned long
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static ulong ToULong(this ushort value) => value;
    }
}
