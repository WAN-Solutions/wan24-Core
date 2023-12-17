using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Bitwise extensions
    /// </summary>
#if NO_UNSAFE
    public static partial class BitwiseExtensions
#else
    public static unsafe partial class BitwiseExtensions
#endif
    {
        /// <summary>
        /// Bit rotation offsets
        /// </summary>
        private static readonly int[] BitRotationUInt16 =
        [
            0, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1
        ];
        /// <summary>
        /// Bit rotation offsets
        /// </summary>
        private static readonly int[] BitRotationUInt64 =
        [
            0, 63, 62, 61, 60, 59, 58, 57, 56, 55, 54, 53, 52, 51, 50, 49, 48, 47, 46, 45, 44, 43, 42, 41, 40, 39, 38, 37, 36, 35, 34, 33, 32,
            31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1
        ];
        /// <summary>
        /// Bit rotation offsets
        /// </summary>
        private static readonly int[] BitRotationUInt8 =
        [
            0, 7, 6, 5, 4, 3, 2, 1
        ];
#if !NO_UNSAFE
        /// <summary>
        /// Bit rotation pointer
        /// </summary>
        private static readonly int* BitRotationUInt16Ptr;
        /// <summary>
        /// Bit rotation pin
        /// </summary>
        private static readonly GCHandle BitRotationUInt16Pin;
        /// <summary>
        /// Bit rotation pointer
        /// </summary>
        private static readonly int* BitRotationUInt64Ptr;
        /// <summary>
        /// Bit rotation pin
        /// </summary>
        private static readonly GCHandle BitRotationUInt64Pin;
        /// <summary>
        /// Bit rotation pointer
        /// </summary>
        private static readonly int* BitRotationUInt8Ptr;
        /// <summary>
        /// Bit rotation pin
        /// </summary>
        private static readonly GCHandle BitRotationUInt8Pin;

        /// <summary>
        /// Constructor
        /// </summary>
        static BitwiseExtensions()
        {
            BitRotationUInt16Pin = GCHandle.Alloc(BitRotationUInt16, GCHandleType.Pinned);
            BitRotationUInt16Ptr = (int*)BitRotationUInt16Pin.AddrOfPinnedObject();
            BitRotationUInt64Pin = GCHandle.Alloc(BitRotationUInt64, GCHandleType.Pinned);
            BitRotationUInt64Ptr = (int*)BitRotationUInt64Pin.AddrOfPinnedObject();
            BitRotationUInt8Pin = GCHandle.Alloc(BitRotationUInt8, GCHandleType.Pinned);
            BitRotationUInt8Ptr = (int*)BitRotationUInt8Pin.AddrOfPinnedObject();
        }
#endif
    }
}
