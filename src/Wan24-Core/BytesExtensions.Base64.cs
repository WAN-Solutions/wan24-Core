using System.Buffers;
using System.Buffers.Text;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace wan24.Core
{
    // Base64
    public static partial class BytesExtensions
    {
        /// <summary>
        /// Get the max. base64 encoded string length in characters
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Number of characters</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBase64StringLength(this byte[] bytes) => Base64.GetMaxEncodedToUtf8Length(bytes.Length);

        /// <summary>
        /// Get the max. base64 encoded string length in characters
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Number of characters</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBase64StringLength(this Span<byte> bytes) => Base64.GetMaxEncodedToUtf8Length(bytes.Length);

        /// <summary>
        /// Get the max. base64 encoded string length in characters
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Number of characters</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBase64StringLength(this ReadOnlySpan<byte> bytes) => Base64.GetMaxEncodedToUtf8Length(bytes.Length);

        /// <summary>
        /// Get the max. base64 encoded string length in characters
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Number of characters</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBase64StringLength(this Memory<byte> bytes) => Base64.GetMaxEncodedToUtf8Length(bytes.Length);

        /// <summary>
        /// Get the max. base64 encoded string length in characters
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Number of characters</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBase64StringLength(this ReadOnlyMemory<byte> bytes) => Base64.GetMaxEncodedToUtf8Length(bytes.Length);

        /// <summary>
        /// Get a base64 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>base64 string</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string GetBase64String(this byte[] bytes)
        {
            using RentedArrayRefStruct<byte> buffer2 = new(len: Base64.GetMaxEncodedToUtf8Length(bytes.Length), clean: false);
            return Encoding.UTF8.GetString(buffer2.Array, 0, GetBase64(bytes, buffer2.Span));
        }

        /// <summary>
        /// Get base64 characters
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>base64 characters</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] GetBase64Chars(this byte[] bytes)
        {
            using RentedArrayRefStruct<byte> buffer2 = new(len: Base64.GetMaxEncodedToUtf8Length(bytes.Length), clean: false);
            return Encoding.UTF8.GetChars(buffer2.Array, 0, GetBase64(bytes, buffer2.Span));
        }

        /// <summary>
        /// Get base64 UTF-8 bytes
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>base64 UTF-8 bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBase64Bytes(this byte[] bytes)
        {
            using RentedMemoryRef<byte> buffer2 = new(len: Base64.GetMaxEncodedToUtf8Length(bytes.Length), clean: false);
            Span<byte> bufferSpan = buffer2.Span;
            return bufferSpan[..GetBase64(bytes, bufferSpan)].ToArray();
        }

        /// <summary>
        /// Get base64 characters
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of characters written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBase64(this byte[] bytes, in Span<char> buffer)
        {
            using RentedMemoryRef<byte> buffer2 = new(len: Base64.GetMaxEncodedToUtf8Length(bytes.Length), clean: false);
            Span<byte> bufferSpan = buffer2.Span;
            return Encoding.UTF8.GetChars(bufferSpan[..GetBase64(bytes, bufferSpan)], buffer);
        }

        /// <summary>
        /// Get base64 UTF-8 bytes
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBase64(this byte[] bytes, in Span<byte> buffer)
        {
            Base64.EncodeToUtf8(bytes, buffer, out int _, out int written);
            return written;
        }

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="bytes">base64 UTF-8 bytes</param>
        /// <returns>Decoded bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] DecodeBase64(this byte[] bytes) => DecodeBase64((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="bytes">base64 UTF-8 bytes</param>
        /// <returns>Decoded bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] DecodeBase64(this Span<byte> bytes) => DecodeBase64((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="bytes">base64 UTF-8 bytes</param>
        /// <returns>Decoded bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] DecodeBase64(this ReadOnlySpan<byte> bytes)
        {
            using RentedMemoryRef<byte> buffer = new(len: Base64.GetMaxDecodedFromUtf8Length(bytes.Length), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            return bufferSpan[..DecodeBase64(bytes, bufferSpan)].ToArray();
        }

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="bytes">base64 UTF-8 bytes</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of decoded bytes written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int DecodeBase64(this byte[] bytes, byte[] buffer) => DecodeBase64((ReadOnlySpan<byte>)bytes, buffer);

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="bytes">base64 UTF-8 bytes</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of decoded bytes written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int DecodeBase64(this Span<byte> bytes, Span<byte> buffer) => DecodeBase64((ReadOnlySpan<byte>)bytes, buffer);

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="bytes">base64 UTF-8 bytes</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of decoded bytes written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int DecodeBase64(this ReadOnlySpan<byte> bytes, Span<byte> buffer)
        {
            OperationStatus status = Base64.DecodeFromUtf8(bytes, buffer, out _, out int written);
            return status switch
            {
                OperationStatus.Done => written,
                OperationStatus.InvalidData => throw new InvalidDataException(),
                OperationStatus.DestinationTooSmall => throw new ArgumentOutOfRangeException(nameof(buffer)),
                _ => throw new ArgumentException($"Invalid base64 UTF-8 bytes: {status}", nameof(bytes)),
            };
        }
    }
}
