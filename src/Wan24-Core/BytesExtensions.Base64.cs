﻿using System.Buffers;
using System.Buffers.Text;
using System.Runtime;
using System.Text;

namespace wan24.Core
{
    // Base64
    public static partial class BytesExtensions
    {
        /// <summary>
        /// Get a base64 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>base64 string</returns>
        [TargetedPatchingOptOut("Tiny method")]
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
        public static byte[] GetBase64Bytes(this byte[] bytes)
        {
            using RentedArrayRefStruct<byte> buffer2 = new(len: Base64.GetMaxEncodedToUtf8Length(bytes.Length), clean: false);
            return buffer2.Span[..GetBase64(bytes, buffer2.Span)].ToArray();
        }

        /// <summary>
        /// Get base64 characters
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of characters written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int GetBase64(this byte[] bytes, in Span<char> buffer)
        {
            using RentedArrayRefStruct<byte> buffer2 = new(len: Base64.GetMaxEncodedToUtf8Length(bytes.Length), clean: false);
            return Encoding.UTF8.GetChars(buffer2.Span[..GetBase64(bytes, buffer2.Span)], buffer);
        }

        /// <summary>
        /// Get base64 UTF-8 bytes
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
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
        public static byte[] DecodeBase64(this byte[] bytes) => DecodeBase64((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="bytes">base64 UTF-8 bytes</param>
        /// <returns>Decoded bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] DecodeBase64(this Span<byte> bytes) => DecodeBase64((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="bytes">base64 UTF-8 bytes</param>
        /// <returns>Decoded bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte[] DecodeBase64(this ReadOnlySpan<byte> bytes)
        {
            using RentedArrayRefStruct<byte> buffer = new(len: Base64.GetMaxDecodedFromUtf8Length(bytes.Length), clean: false);
            return buffer.Span[..DecodeBase64(bytes, buffer.Span)].ToArray();
        }

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="bytes">base64 UTF-8 bytes</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of decoded bytes written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int DecodeBase64(this byte[] bytes, byte[] buffer) => DecodeBase64((ReadOnlySpan<byte>)bytes, buffer);

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="bytes">base64 UTF-8 bytes</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of decoded bytes written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int DecodeBase64(this Span<byte> bytes, Span<byte> buffer) => DecodeBase64((ReadOnlySpan<byte>)bytes, buffer);

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="bytes">base64 UTF-8 bytes</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of decoded bytes written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
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
