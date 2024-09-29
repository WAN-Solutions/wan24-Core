using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Checksum extensions
    /// </summary>
    public static class ChecksumExtensions
    {
        /// <summary>
        /// Create a checksum
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="len">Checksum length in bytes (must be greater than zero, a power of two and not larger than <c>256</c>)</param>
        /// <returns>Checksum</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] CreateChecksum(this byte[] data, in int len = ChecksumTransform.HASH_LENGTH) => CreateChecksum((ReadOnlySpan<byte>)data, len);

        /// <summary>
        /// Create a checksum
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="len">Checksum length in bytes (must be greater than zero, a power of two and not larger than <c>256</c>)</param>
        /// <returns>Checksum</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] CreateChecksum(this in Memory<byte> data, in int len = ChecksumTransform.HASH_LENGTH) => CreateChecksum((ReadOnlySpan<byte>)data.Span, len);

        /// <summary>
        /// Create a checksum
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="len">Checksum length in bytes (must be greater than zero, a power of two and not larger than <c>256</c>)</param>
        /// <returns>Checksum</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] CreateChecksum(this in ReadOnlyMemory<byte> data, in int len = ChecksumTransform.HASH_LENGTH) => CreateChecksum(data.Span, len);

        /// <summary>
        /// Create a checksum
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="len">Checksum length in bytes (must be greater than zero, a power of two and not larger than <c>256</c>)</param>
        /// <returns>Checksum</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] CreateChecksum(this in Span<byte> data, in int len = ChecksumTransform.HASH_LENGTH) => CreateChecksum((ReadOnlySpan<byte>)data, len);

        /// <summary>
        /// Create a checksum
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="len">Checksum length in bytes (must be greater than zero, a power of two and not larger than <c>256</c>)</param>
        /// <returns>Checksum</returns>
        public static byte[] CreateChecksum(this in ReadOnlySpan<byte> data, in int len = ChecksumTransform.HASH_LENGTH)
        {
            if (len <= 0 || len > 1 << 8 || (len & (len - 1)) != 0)
                throw new ArgumentOutOfRangeException(nameof(len), "Checksum length must be non-zero, a power of two and not larger than 256");
            byte[] res = new byte[len];
            if (data.Length != 0) UpdateChecksum(data, res);
            return res;
        }

        /// <summary>
        /// Update a checksum
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="checksum">Checksum (length must be greater than zero, a power of two and not larger than <c>256</c>)</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UpdateChecksum(this byte[] data, in Span<byte> checksum) => UpdateChecksum((ReadOnlySpan<byte>)data, checksum);

        /// <summary>
        /// Update a checksum
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="checksum">Checksum (length must be greater than zero, a power of two and not larger than <c>256</c>)</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UpdateChecksum(this in Memory<byte> data, in Span<byte> checksum) => UpdateChecksum((ReadOnlySpan<byte>)data.Span, checksum);

        /// <summary>
        /// Update a checksum
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="checksum">Checksum (length must be greater than zero, a power of two and not larger than <c>256</c>)</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UpdateChecksum(this in ReadOnlyMemory<byte> data, in Span<byte> checksum) => UpdateChecksum(data.Span, checksum);

        /// <summary>
        /// Update a checksum
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="checksum">Checksum (length must be greater than zero, a power of two and not larger than <c>256</c>)</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UpdateChecksum(this in Span<byte> data, in Span<byte> checksum) => UpdateChecksum((ReadOnlySpan<byte>)data, checksum);

        /// <summary>
        /// Update a checksum
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="checksum">Checksum (length must be greater than zero, a power of two and not larger than <c>256</c>)</param>
        public static void UpdateChecksum(this in ReadOnlySpan<byte> data, in Span<byte> checksum)
        {
            int len = checksum.Length,
                mask = len - 1;
            if (len <= 0 || len > 1 << 8 || (len & mask) != 0)
                throw new ArgumentOutOfRangeException(nameof(checksum), "Checksum length must be greater than zero, a power of two and not larger than 256");
            if (data.Length == 0) return;
#if NO_UNSAFE
            for (int i = 0, dataLen = data.Length; i < dataLen; checksum[checksum[data[i] & mask] & mask] ^= data[i], i++) ;
#else
            unsafe
            {
                fixed (byte* dataPtr = data)
                fixed (byte* checksumPtr = checksum)
                    for (int i = 0, dataLen = data.Length; i < dataLen; checksumPtr[dataPtr[i] & mask] ^= dataPtr[i], i++) ;
            }
#endif
        }
    }
}
