﻿using System.Buffers.Text;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="char"/> array extensions
    /// </summary>
    public static class CharsExtensions
    {
        /// <summary>
        /// Clear the array
        /// </summary>
        /// <param name="arr">Array</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void Clear(this char[] arr)
        {
            if (arr.Length == 0) return;
#if !NO_UNSAFE
            if (arr.Length > Settings.StackAllocBorder)
            {
#endif
                using RentedArrayRefStruct<byte> random = new(arr.Length, clean: false);
                RandomNumberGenerator.Fill(random.Span);
#if !NO_UNSAFE
                unsafe
                {
                    fixed (byte* r = random.Span)
                    fixed (char* a = arr)
#endif
                        unchecked
                        {
#if NO_UNSAFE
                            for (int i = 0, len = arr.Length; i < len; arr[i] = (char)random[i], i++) ;
#else
                            for (int i = 0, len = arr.Length; i < len; a[i] = (char)r[i], a[i] = '\0', i++) ;
#endif
                        }
#if !NO_UNSAFE
                }
#else
                Array.Clear(arr);
#endif
#if !NO_UNSAFE
            }
            else
            {
                Span<byte> random = stackalloc byte[arr.Length];
                RandomNumberGenerator.Fill(random);
                unsafe
                {
                    fixed (byte* r = random)
                    fixed (char* a = arr)
                        unchecked
                        {
                            for (int i = 0, len = arr.Length; i < len; a[i] = (char)r[i], a[i] = '\0', i++) ;
                        }
                }
            }
#endif
        }

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="str">base64 UTF-8 string</param>
        /// <returns>Decoded bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] DecodeBase64(this char[] str) => DecodeBase64((ReadOnlySpan<char>)str);

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="str">base64 UTF-8 string</param>
        /// <returns>Decoded bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] DecodeBase64(this Span<char> str) => DecodeBase64((ReadOnlySpan<char>)str);

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="str">base64 UTF-8 string</param>
        /// <returns>Decoded bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte[] DecodeBase64(this ReadOnlySpan<char> str)
        {
            using RentedArrayRefStruct<byte> buffer = new(len: Base64.GetMaxDecodedFromUtf8Length(str.Length), clean: false);
            return buffer.Span[..DecodeBase64(str, buffer.Span)].ToArray();
        }

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="str">base64 UTF-8 string</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of decoded characters written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int DecodeBase64(this char[] str, byte[] buffer) => DecodeBase64((ReadOnlySpan<char>)str, buffer);

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="str">base64 UTF-8 string</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of decoded characters written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int DecodeBase64(this Span<char> str, Span<byte> buffer) => DecodeBase64((ReadOnlySpan<char>)str, buffer);

        /// <summary>
        /// Decode base64
        /// </summary>
        /// <param name="str">base64 UTF-8 string</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of decoded characters written to <c>buffer</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int DecodeBase64(this ReadOnlySpan<char> str, Span<byte> buffer)
        {
            using RentedArrayRefStruct<byte> bytes = new(len: buffer.Length, clean: false);
            return bytes.Span[..Encoding.UTF8.GetBytes(str, bytes.Span)].DecodeBase64(buffer);
        }
    }
}
