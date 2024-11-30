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
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf8String(this ReadOnlySpan<byte> bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.UTF8, ignoreUsed);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="buffer">Output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of used bytes from the output buffer</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int ToUtf8String(this ReadOnlySpan<byte> bytes, in Span<char> buffer, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.UTF8, buffer, ignoreUsed);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf8String(this Span<byte> bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.UTF8, ignoreUsed);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf8String(this ReadOnlyMemory<byte> bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.UTF8, ignoreUsed);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf8String(this Memory<byte> bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.UTF8, ignoreUsed);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf8String(this byte[] bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.UTF8, ignoreUsed);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] ToUtf8Chars(this ReadOnlySpan<byte> bytes, in bool ignoreUsed = false)
        {
            Encoding encoding = StringEncodings.UTF8;
            int maxLen = encoding.GetMaxCharCount(bytes.Length);
#if !NO_UNSAFE
            if (maxLen * 3 > Settings.StackAllocBorder)
            {
#endif
                using RentedMemoryRef<char> buffer = new(maxLen, clean: false);
                Span<char> bufferSpan = buffer.Span;
                return bufferSpan[..bytes.ToDecodedString(encoding, bufferSpan, ignoreUsed)].ToArray();
#if !NO_UNSAFE
            }
            else
            {
                Span<char> bufferSpan = stackalloc char[maxLen];
                return bufferSpan[..bytes.ToDecodedString(encoding, bufferSpan, ignoreUsed)].ToArray();
            }
#endif
        }

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] ToUtf8Chars(this Span<byte> bytes, in bool ignoreUsed = false) => ToUtf8Chars((ReadOnlySpan<byte>)bytes, ignoreUsed);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] ToUtf8Chars(this ReadOnlyMemory<byte> bytes, in bool ignoreUsed = false) => ToUtf8Chars(bytes.Span, ignoreUsed);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] ToUtf8Chars(this Memory<byte> bytes, in bool ignoreUsed = false) => ToUtf8Chars(bytes.Span, ignoreUsed);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] ToUtf8Chars(this byte[] bytes, in bool ignoreUsed = false) => ToUtf8Chars(bytes.AsSpan(), ignoreUsed);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of used bytes from the output buffer</returns>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf16String(this ReadOnlySpan<byte> bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.Unicode, ignoreUsed);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="buffer">Output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of used bytes from the output buffer</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int ToUtf16String(this ReadOnlySpan<byte> bytes, in Span<char> buffer, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.Unicode, buffer, ignoreUsed);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf16String(this Span<byte> bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.Unicode, ignoreUsed);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf16String(this ReadOnlyMemory<byte> bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.Unicode, ignoreUsed);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf16String(this Memory<byte> bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.Unicode, ignoreUsed);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf16String(this byte[] bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.Unicode, ignoreUsed);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] ToUtf16Chars(this ReadOnlySpan<byte> bytes, in bool ignoreUsed = false)
        {
            Encoding encoding = StringEncodings.Unicode;
            int maxLen = encoding.GetMaxCharCount(bytes.Length);
#if !NO_UNSAFE
            if (maxLen << 1 > Settings.StackAllocBorder)
            {
#endif
                using RentedMemoryRef<char> buffer = new(maxLen, clean: false);
                Span<char> bufferSpan = buffer.Span;
                return bufferSpan[..bytes.ToDecodedString(encoding, bufferSpan, ignoreUsed)].ToArray();
#if !NO_UNSAFE
            }
            else
            {
                Span<char> bufferSpan = stackalloc char[maxLen];
                return bufferSpan[..bytes.ToDecodedString(encoding, bufferSpan, ignoreUsed)].ToArray();
            }
#endif
        }

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] ToUtf16Chars(this Span<byte> bytes, in bool ignoreUsed = false) => ToUtf16Chars((ReadOnlySpan<byte>)bytes, ignoreUsed);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] ToUtf16Chars(this ReadOnlyMemory<byte> bytes, in bool ignoreUsed = false) => ToUtf16Chars(bytes.Span, ignoreUsed);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] ToUtf16Chars(this Memory<byte> bytes, in bool ignoreUsed = false) => ToUtf16Chars(bytes.Span, ignoreUsed);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] ToUtf16Chars(this byte[] bytes, in bool ignoreUsed = false) => ToUtf16Chars(bytes.AsSpan(), ignoreUsed);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf32String(this ReadOnlySpan<byte> bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.UTF32, ignoreUsed);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="buffer">Output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of used bytes from the output buffer</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int ToUtf32String(this ReadOnlySpan<byte> bytes, in Span<char> buffer, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.UTF32, buffer, ignoreUsed);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf32String(this Span<byte> bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.UTF32, ignoreUsed);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf32String(this ReadOnlyMemory<byte> bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.UTF32, ignoreUsed);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf32String(this Memory<byte> bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.UTF32, ignoreUsed);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToUtf32String(this byte[] bytes, in bool ignoreUsed = false)
            => bytes.ToDecodedString(StringEncodings.UTF32, ignoreUsed);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] ToUtf32Chars(this ReadOnlySpan<byte> bytes, in bool ignoreUsed = false)
        {
            Encoding encoding = StringEncodings.UTF32;
            int maxLen = encoding.GetMaxCharCount(bytes.Length);
#if !NO_UNSAFE
            if (maxLen << 2 > Settings.StackAllocBorder)
            {
#endif
                using RentedMemoryRef<char> buffer = new(maxLen, clean: false);
                Span<char> bufferSpan = buffer.Span;
                return bufferSpan[..bytes.ToDecodedString(encoding, bufferSpan, ignoreUsed)].ToArray();
#if !NO_UNSAFE
            }
            else
            {
                Span<char> bufferSpan = stackalloc char[maxLen];
                return bufferSpan[..bytes.ToDecodedString(encoding, bufferSpan, ignoreUsed)].ToArray();
            }
#endif
        }

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] ToUtf32Chars(this Span<byte> bytes, in bool ignoreUsed = false) => ToUtf32Chars((ReadOnlySpan<byte>)bytes, ignoreUsed);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] ToUtf32Chars(this ReadOnlyMemory<byte> bytes, in bool ignoreUsed = false) => ToUtf32Chars(bytes.Span, ignoreUsed);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] ToUtf32Chars(this Memory<byte> bytes, in bool ignoreUsed = false) => ToUtf32Chars(bytes.Span, ignoreUsed);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian without byte order mark)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] ToUtf32Chars(this byte[] bytes, in bool ignoreUsed = false) => ToUtf32Chars(bytes.AsSpan(), ignoreUsed);

        /// <summary>
        /// Get a decoded string
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="characters">Characters output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of characters written to the output buffer</returns>
        public static int ToDecodedString(this ReadOnlySpan<byte> buffer, in Encoding encoding, in Span<char> characters, in bool ignoreUsed = false)
        {
            Decoder decoder = encoding.GetDecoder();
            decoder.Convert(buffer, characters, flush: true, out int used, out int res, out bool completed);
            if (!ignoreUsed && used != buffer.Length) throw new InvalidDataException("Invalid byte sequence");
            if (!completed) throw new InvalidDataException("Incomplete byte sequence");
            return res;
        }

        /// <summary>
        /// Get a decoded string
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="characters">Characters output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of characters written to the output buffer</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int ToDecodedString(this ReadOnlyMemory<byte> buffer, in Encoding encoding, in Span<char> characters, in bool ignoreUsed = false)
            => buffer.Span.ToDecodedString(encoding, characters, ignoreUsed);

        /// <summary>
        /// Get a decoded string
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="characters">Characters output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of characters written to the output buffer</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int ToDecodedString(this Span<byte> buffer, in Encoding encoding, in Span<char> characters, in bool ignoreUsed = false)
            => ((ReadOnlySpan<byte>)buffer).ToDecodedString(encoding, characters, ignoreUsed);

        /// <summary>
        /// Get a decoded string
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="characters">Characters output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of characters written to the output buffer</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int ToDecodedString(this Memory<byte> buffer, in Encoding encoding, in Span<char> characters, in bool ignoreUsed = false)
            => ((ReadOnlyMemory<byte>)buffer).Span.ToDecodedString(encoding, characters, ignoreUsed);

        /// <summary>
        /// Get a decoded string
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="characters">Characters output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of characters written to the output buffer</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int ToDecodedString(this byte[] buffer, in Encoding encoding, in Span<char> characters, in bool ignoreUsed = false)
            => ((ReadOnlySpan<byte>)buffer).ToDecodedString(encoding, characters, ignoreUsed);

        /// <summary>
        /// Get a decoded string
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static string ToDecodedString(this ReadOnlySpan<byte> buffer, in Encoding encoding, in bool ignoreUsed = false)
        {
            int maxLen = encoding.GetMaxCharCount(buffer.Length);
#if !NO_UNSAFE
            if (maxLen << 2 > Settings.StackAllocBorder)
            {
#endif
                using RentedMemoryRef<char> characters = new(maxLen, clean: false);
                Span<char> charactersSpan = characters.Span;
                return new(charactersSpan[..buffer.ToDecodedString(encoding, charactersSpan, ignoreUsed)]);
#if !NO_UNSAFE
            }
            else
            {
                Span<char> charactersSpan = stackalloc char[maxLen];
                int len = buffer.ToDecodedString(encoding, charactersSpan, ignoreUsed);
                try
                {
                    return new(charactersSpan[..len]);
                }
                finally
                {
                    if (Settings.ClearBuffers) charactersSpan[..len].Clear();
                }
            }
#endif
        }

        /// <summary>
        /// Get a decoded string
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToDecodedString(this ReadOnlyMemory<byte> buffer, in Encoding encoding, in bool ignoreUsed = false)
            => buffer.Span.ToDecodedString(encoding, ignoreUsed);

        /// <summary>
        /// Get a decoded string
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToDecodedString(this Span<byte> buffer, in Encoding encoding, in bool ignoreUsed = false)
            => ((ReadOnlySpan<byte>)buffer).ToDecodedString(encoding, ignoreUsed);

        /// <summary>
        /// Get a decoded string
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToDecodedString(this Memory<byte> buffer, in Encoding encoding, in bool ignoreUsed = false)
            => ((ReadOnlyMemory<byte>)buffer).Span.ToDecodedString(encoding, ignoreUsed);

        /// <summary>
        /// Get a decoded string
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToDecodedString(this byte[] buffer, in Encoding encoding, in bool ignoreUsed = false)
            => ((ReadOnlySpan<byte>)buffer).ToDecodedString(encoding, ignoreUsed);
    }
}
