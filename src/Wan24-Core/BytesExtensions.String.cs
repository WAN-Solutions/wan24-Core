using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace wan24.Core
{
    // String
    public static partial class BytesExtensions
    {
        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static string ToUtf8String(this ReadOnlySpan<byte> bytes, in bool ignoreUsed = false)
        {
#if !NO_UNSAFE
            if (bytes.Length > Settings.StackAllocBorder)
            {
#endif
                using RentedArrayRefStruct<char> chars = new(bytes.Length, clean: false);
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true)
                    .GetDecoder()
                    .Convert(bytes, chars.Span, flush: true, out int used, out int count, out bool completed);
                if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-8 decoding failed (completed: {completed}, {used}/{bytes.Length})");
                return new string(chars.Span[..count]);
#if !NO_UNSAFE
            }
            else
            {
                Span<char> chars = stackalloc char[bytes.Length];
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true)
                    .GetDecoder()
                    .Convert(bytes, chars, flush: true, out int used, out int count, out bool completed);
                if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-8 decoding failed (completed: {completed}, {used}/{bytes.Length})");
                return new string(chars[..count]);
            }
#endif
        }

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="buffer">Output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of used bytes from the output buffer</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int ToUtf8String(this ReadOnlySpan<byte> bytes, in Span<char> buffer, in bool ignoreUsed = false)
        {
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true)
                .GetDecoder()
                .Convert(bytes, buffer, flush: true, out int used, out int count, out bool completed);
            if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-8 decoding failed (completed: {completed}, {used}/{bytes.Length})");
            return count;
        }

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string ToUtf8String(this Span<byte> bytes) => ToUtf8String((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string ToUtf8String(this ReadOnlyMemory<byte> bytes) => bytes.Span.ToUtf8String();

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string ToUtf8String(this Memory<byte> bytes) => ToUtf8String((ReadOnlySpan<byte>)bytes.Span);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string ToUtf8String(this byte[] bytes) => bytes.AsSpan().ToUtf8String();

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] ToUtf8Chars(this ReadOnlySpan<byte> bytes, in bool ignoreUsed = false)
        {
#if !NO_UNSAFE
            if (bytes.Length > Settings.StackAllocBorder)
            {
#endif
                using RentedArrayRefStruct<char> chars = new(bytes.Length, clean: false);
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true)
                    .GetDecoder()
                    .Convert(bytes, chars.Span, flush: true, out int used, out int count, out bool completed);
                if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-8 decoding failed (completed: {completed}, {used}/{bytes.Length})");
                return chars.Span[..count].ToArray();
#if !NO_UNSAFE
            }
            else
            {
                Span<char> chars = stackalloc char[bytes.Length];
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true)
                    .GetDecoder()
                    .Convert(bytes, chars, flush: true, out int used, out int count, out bool completed);
                if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-8 decoding failed (completed: {completed}, {used}/{bytes.Length})");
                return chars[..count].ToArray();
            }
#endif
        }

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] ToUtf8Chars(this Span<byte> bytes) => ToUtf8Chars((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] ToUtf8Chars(this ReadOnlyMemory<byte> bytes) => bytes.Span.ToUtf8Chars();

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] ToUtf8Chars(this Memory<byte> bytes) => ToUtf8Chars((ReadOnlySpan<byte>)bytes.Span);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] ToUtf8Chars(this byte[] bytes) => bytes.AsSpan().ToUtf8Chars();

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of used bytes from the output buffer</returns>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static string ToUtf16String(this ReadOnlySpan<byte> bytes, in bool ignoreUsed = false)
        {
#if !NO_UNSAFE
            if (bytes.Length > Settings.StackAllocBorder)
            {
#endif
                using RentedArrayRefStruct<char> chars = new(bytes.Length, clean: false);
                new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true)
                    .GetDecoder()
                    .Convert(bytes, chars.Span, flush: true, out int used, out int count, out bool completed);
                if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-16 decoding failed (completed: {completed}, {used}/{bytes.Length})");
                return new string(chars.Span[..count]);
#if !NO_UNSAFE
            }
            else
            {
                Span<char> chars = stackalloc char[bytes.Length];
                new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true)
                    .GetDecoder()
                    .Convert(bytes, chars, flush: true, out int used, out int count, out bool completed);
                if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-16 decoding failed (completed: {completed}, {used}/{bytes.Length})");
                return new string(chars[..count]);
            }
#endif
        }

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <param name="buffer">Output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of used bytes from the output buffer</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int ToUtf16String(this ReadOnlySpan<byte> bytes, in Span<char> buffer, in bool ignoreUsed = false)
        {
            new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true)
                .GetDecoder()
                .Convert(bytes, buffer, flush: true, out int used, out int count, out bool completed);
            if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-16 decoding failed (completed: {completed}, {used}/{bytes.Length})");
            return count;
        }

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string ToUtf16String(this Span<byte> bytes) => ToUtf16String((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string ToUtf16String(this ReadOnlyMemory<byte> bytes) => bytes.Span.ToUtf16String();

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string ToUtf16String(this Memory<byte> bytes) => ToUtf16String((ReadOnlySpan<byte>)bytes.Span);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string ToUtf16String(this byte[] bytes) => bytes.AsSpan().ToUtf16String();

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static char[] ToUtf16Chars(this ReadOnlySpan<byte> bytes, in bool ignoreUsed = false)
        {
#if !NO_UNSAFE
            if (bytes.Length > Settings.StackAllocBorder)
            {
#endif
                using RentedArrayRefStruct<char> chars = new(bytes.Length, clean: false);
                new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true)
                    .GetDecoder()
                    .Convert(bytes, chars.Span, flush: true, out int used, out int count, out bool completed);
                if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-16 decoding failed (completed: {completed}, {used}/{bytes.Length})");
                return chars.Span[..count].ToArray();
#if !NO_UNSAFE
            }
            else
            {
                Span<char> chars = stackalloc char[bytes.Length];
                new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true)
                    .GetDecoder()
                    .Convert(bytes, chars, flush: true, out int used, out int count, out bool completed);
                if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-16 decoding failed (completed: {completed}, {used}/{bytes.Length})");
                return chars[..count].ToArray();
            }
#endif
        }

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] ToUtf16Chars(this Span<byte> bytes) => ToUtf16Chars((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] ToUtf16Chars(this ReadOnlyMemory<byte> bytes) => bytes.Span.ToUtf16Chars();

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] ToUtf16Chars(this Memory<byte> bytes) => ToUtf16Chars((ReadOnlySpan<byte>)bytes.Span);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] ToUtf16Chars(this byte[] bytes) => bytes.AsSpan().ToUtf16Chars();

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static string ToUtf32String(this ReadOnlySpan<byte> bytes, in bool ignoreUsed = false)
        {
#if !NO_UNSAFE
            if (bytes.Length > Settings.StackAllocBorder)
            {
#endif
                using RentedArrayRefStruct<char> chars = new(bytes.Length, clean: false);
                new UTF32Encoding(bigEndian: false, byteOrderMark: false, throwOnInvalidCharacters: true)
                    .GetDecoder()
                    .Convert(bytes, chars.Span, flush: true, out int used, out int count, out bool completed);
                if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-32 decoding failed (completed: {completed}, {used}/{bytes.Length})");
                return new string(chars.Span[..count]);
#if !NO_UNSAFE
            }
            else
            {
                Span<char> chars = stackalloc char[bytes.Length];
                new UTF32Encoding(bigEndian: false, byteOrderMark: false, throwOnInvalidCharacters: true)
                    .GetDecoder()
                    .Convert(bytes, chars, flush: true, out int used, out int count, out bool completed);
                if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-32 decoding failed (completed: {completed}, {used}/{bytes.Length})");
                return new string(chars[..count]);
            }
#endif
        }

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <param name="buffer">Output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of used bytes from the output buffer</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int ToUtf32String(this ReadOnlySpan<byte> bytes, in Span<char> buffer, in bool ignoreUsed = false)
        {
            new UTF32Encoding(bigEndian: false, byteOrderMark: false, throwOnInvalidCharacters: true)
                .GetDecoder()
                .Convert(bytes, buffer, flush: true, out int used, out int count, out bool completed);
            if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-32 decoding failed (completed: {completed}, {used}/{bytes.Length})");
            return count;
        }

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string ToUtf32String(this Span<byte> bytes) => ToUtf32String((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string ToUtf32String(this ReadOnlyMemory<byte> bytes) => bytes.Span.ToUtf32String();

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string ToUtf32String(this Memory<byte> bytes) => ToUtf32String((ReadOnlySpan<byte>)bytes.Span);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string ToUtf32String(this byte[] bytes) => bytes.AsSpan().ToUtf32String();

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static char[] ToUtf32Chars(this ReadOnlySpan<byte> bytes, in bool ignoreUsed = false)
        {
#if !NO_UNSAFE
            if (bytes.Length > Settings.StackAllocBorder)
            {
#endif
                using RentedArrayRefStruct<char> chars = new(bytes.Length, clean: false);
                new UTF32Encoding(bigEndian: false, byteOrderMark: false, throwOnInvalidCharacters: true)
                    .GetDecoder()
                    .Convert(bytes, chars.Span, flush: true, out int used, out int count, out bool completed);
                if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-32 decoding failed (completed: {completed}, {used}/{bytes.Length})");
                return chars.Span[..count].ToArray();
#if !NO_UNSAFE
            }
            else
            {
                Span<char> chars = stackalloc char[bytes.Length];
                new UTF32Encoding(bigEndian: false, byteOrderMark: false, throwOnInvalidCharacters: true)
                    .GetDecoder()
                    .Convert(bytes, chars, flush: true, out int used, out int count, out bool completed);
                if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-32 decoding failed (completed: {completed}, {used}/{bytes.Length})");
                return chars[..count].ToArray();
            }
#endif
        }

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] ToUtf32Chars(this Span<byte> bytes) => ToUtf32Chars((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] ToUtf32Chars(this ReadOnlyMemory<byte> bytes) => bytes.Span.ToUtf32Chars();

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] ToUtf32Chars(this Memory<byte> bytes) => ToUtf32Chars((ReadOnlySpan<byte>)bytes.Span);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] ToUtf32Chars(this byte[] bytes) => bytes.AsSpan().ToUtf32Chars();
    }
}
