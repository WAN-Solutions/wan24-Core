using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Hex
    public static partial class BytesExtensions
    {
        /// <summary>
        /// Get the byte array as hex string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Hex string</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToHexString(this ReadOnlySpan<byte> bytes) => Convert.ToHexString(bytes);

        /// <summary>
        /// Get the byte array as hex string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Hex string</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToHexString(this Span<byte> bytes) => Convert.ToHexString(bytes);

        /// <summary>
        /// Get the byte array as hex string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Hex string</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToHexString(this Memory<byte> bytes) => Convert.ToHexString(bytes.Span);

        /// <summary>
        /// Get the byte array as hex string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Hex string</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToHexString(this ReadOnlyMemory<byte> bytes) => Convert.ToHexString(bytes.Span);

        /// <summary>
        /// Get the byte array as hex string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Hex string</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToHexString(this byte[] bytes) => Convert.ToHexString(bytes);

        /// <summary>
        /// Get the bits of a byte as string
        /// </summary>
        /// <param name="b">Byte</param>
        /// <returns>Bits as string</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToBitString(this byte b) => Convert.ToString(b, 2).PadLeft(8, '0');
    }
}
