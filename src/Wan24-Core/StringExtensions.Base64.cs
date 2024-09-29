using System.Buffers;
using System.Buffers.Text;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace wan24.Core
{
    // Base64
    public static partial class StringExtensions
    {
        /// <summary>
        /// Get the max. number of decoded bytes from a base64 string
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Number of bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBase64BytesLength(this ReadOnlySpan<char> str) => Base64.GetMaxDecodedFromUtf8Length(Encoding.UTF8.GetByteCount(str));

        /// <summary>
        /// Get bytes from a base64 string
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBase64Bytes(this ReadOnlySpan<char> str)
        {
            using RentedMemoryRef<byte> buffer = new(len: Base64.GetMaxDecodedFromUtf8Length(Encoding.UTF8.GetByteCount(str)), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            OperationStatus status = Base64.DecodeFromUtf8InPlace(bufferSpan[..Encoding.UTF8.GetBytes(str, bufferSpan)], out int written);
            return status switch
            {
                OperationStatus.Done => bufferSpan[..written].ToArray(),
                OperationStatus.InvalidData => throw new InvalidDataException(),
                _ => throw new ArgumentException($"Invalid base64 string: {status}", nameof(str)),
            };
        }

        /// <summary>
        /// Get bytes from a base64 string
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBase64Bytes(this string str)
        {
            using RentedMemoryRef<byte> buffer = new(len: Base64.GetMaxDecodedFromUtf8Length(Encoding.UTF8.GetByteCount(str)), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            OperationStatus status = Base64.DecodeFromUtf8InPlace(bufferSpan[..Encoding.UTF8.GetBytes(str, bufferSpan)], out int written);
            return status switch
            {
                OperationStatus.Done => bufferSpan[..written].ToArray(),
                OperationStatus.InvalidData => throw new InvalidDataException(),
                _ => throw new ArgumentException($"Invalid base64 string: {status}", nameof(str)),
            };
        }

        /// <summary>
        /// Get bytes from a base64 string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Output buffer (large enough to fit the UTF-8 decoded <c>str</c> bytes and the base64 decoded <c>str</c> bytes)</param>
        /// <returns>Number of bytes written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBase64Bytes(this ReadOnlySpan<char> str, in Span<byte> buffer)
        {
            OperationStatus status = Base64.DecodeFromUtf8InPlace(buffer[..Encoding.UTF8.GetBytes(str, buffer)], out int written);
            return status switch
            {
                OperationStatus.Done => written,
                OperationStatus.InvalidData => throw new InvalidDataException(),
                OperationStatus.DestinationTooSmall => throw new ArgumentOutOfRangeException(nameof(buffer)),
                _ => throw new ArgumentException($"Invalid base64 string: {status}", nameof(str)),
            };
        }

        /// <summary>
        /// Get bytes from a base64 string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Output buffer (large enough to fit the UTF-8 decoded <c>str</c> bytes and the base64 decoded <c>str</c> bytes)</param>
        /// <returns>Number of bytes written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBase64Bytes(this string str, in Span<byte> buffer)
        {
            OperationStatus status = Base64.DecodeFromUtf8InPlace(buffer[..Encoding.UTF8.GetBytes(str, buffer)], out int written);
            return status switch
            {
                OperationStatus.Done => written,
                OperationStatus.InvalidData => throw new InvalidDataException(),
                OperationStatus.DestinationTooSmall => throw new ArgumentOutOfRangeException(nameof(buffer)),
                _ => throw new ArgumentException($"Invalid base64 string: {status}", nameof(str)),
            };
        }
    }
}
