using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace wan24.Core
{
    /// <summary>
    /// Bytes extensions
    /// </summary>
    public static class BytesExtensions
    {
        /// <summary>
        /// Convert the endian to be little endian for serializing, and big endian, if the system uses it
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Converted bytes</returns>
        public static byte[] ConvertEndian(this ReadOnlySpan<byte> bytes)
        {
            byte[] res = bytes.ToArray();
            if (!BitConverter.IsLittleEndian) Array.Reverse(res);
            return res;
        }

        /// <summary>
        /// Convert the endian to be little endian for serializing, and big endian, if the system uses it
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Converted bytes</returns>
        public static byte[] ConvertEndian(this ReadOnlyMemory<byte> bytes) => bytes.Span.ConvertEndian();

        /// <summary>
        /// Convert the endian to be little endian for serializing, and big endian, if the system uses it
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Converted bytes</returns>
        public static Span<byte> ConvertEndian(this Span<byte> bytes)
        {
            if (!BitConverter.IsLittleEndian) bytes.Reverse();
            return bytes;
        }

        /// <summary>
        /// Convert the endian to be little endian for serializing, and big endian, if the system uses it
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Converted bytes</returns>
        public static Memory<byte> ConvertEndian(this Memory<byte> bytes)
        {
            bytes.Span.ConvertEndian();
            return bytes;
        }

        /// <summary>
        /// Convert the endian to be little endian for serializing, and big endian, if the system uses it
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Converted bytes</returns>
        public static byte[] ConvertEndian(this byte[] bytes)
        {
            bytes.AsSpan().ConvertEndian();
            return bytes;
        }

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        public static bool SlowCompare(this ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            int diff = a.Length ^ b.Length;
            for (int i = Math.Min(a.Length, b.Length) - 1; i >= 0; diff |= a[i] ^ b[i], i--) ;
            return diff == 0;
        }

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        public static bool SlowCompare(this Span<byte> a, ReadOnlySpan<byte> b) => SlowCompare((ReadOnlySpan<byte>)a, b);

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        public static bool SlowCompare(this byte[] a, ReadOnlySpan<byte> b) => SlowCompare((ReadOnlySpan<byte>)a, b);

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        public static bool SlowCompare(this ReadOnlyMemory<byte> a, ReadOnlySpan<byte> b) => SlowCompare(a.Span, b);

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        public static bool SlowCompare(this Memory<byte> a, ReadOnlySpan<byte> b) => SlowCompare((ReadOnlySpan<byte>)a.Span, b);

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        public static bool SlowCompare(this ReadOnlyMemory<byte> a, ReadOnlyMemory<byte> b) => SlowCompare(a.Span, b.Span);

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        public static bool SlowCompare(this Memory<byte> a, Memory<byte> b) => SlowCompare((ReadOnlySpan<byte>)a.Span, b.Span);

        /// <summary>
        /// Get an Int16
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static short ToShort(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadInt16LittleEndian(bits);

        /// <summary>
        /// Get an Int16
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static short ToShort(this Span<byte> bits) => ToShort((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get an UInt16
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static ushort ToUShort(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadUInt16LittleEndian(bits);

        /// <summary>
        /// Get an UInt16
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static ushort ToUShort(this Span<byte> bits) => ToUShort((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get an Int32
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static int ToInt(this ReadOnlySpan<byte> bits)  => BinaryPrimitives.ReadInt32LittleEndian(bits);

        /// <summary>
        /// Get an Int32
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static int ToInt(this Span<byte> bits) => ToInt((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get an UInt32
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static uint ToUInt(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadUInt32LittleEndian(bits);

        /// <summary>
        /// Get an UInt32
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static uint ToUInt(this Span<byte> bits) => ToUInt((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get an Int64
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static long ToLong(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadInt64LittleEndian(bits);

        /// <summary>
        /// Get an Int64
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static long ToLong(this Span<byte> bits) => ToLong((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get an UInt64
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static ulong ToULong(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadUInt64LittleEndian(bits);

        /// <summary>
        /// Get an UInt64
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static ulong ToULong(this Span<byte> bits) => ToULong((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get a float
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static float ToFloat(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadSingleLittleEndian(bits);

        /// <summary>
        /// Get a float
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static float ToFloat(this Span<byte> bits) => ToFloat((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get a double
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static double ToDouble(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadDoubleLittleEndian(bits);

        /// <summary>
        /// Get a double
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static double ToDouble(this Span<byte> bits) => ToDouble((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get a decimal
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static decimal ToDecimal(this ReadOnlySpan<byte> bits)
        {
            if (bits.Length < sizeof(int) << 2) throw new ArgumentOutOfRangeException(nameof(bits));
            using RentedArray<int> intBits = new(len: 4, clean: false);
            for (int i = 0; i < 4; intBits[i] = bits.Slice(i << 2, sizeof(int)).ToInt(), i++) ;
            return new decimal(intBits.Span);
        }

        /// <summary>
        /// Get a decimal
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        public static decimal ToDecimal(this Span<byte> bits) => ToDecimal((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        public static string ToUtf8String(this ReadOnlySpan<byte> bytes, bool ignoreUsed = false)
        {
            using RentedArray<char> chars = new(bytes.Length, clean: false);
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true)
                .GetDecoder()
                .Convert(bytes, chars, flush: true, out int used, out int count, out bool completed);
            if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-8 decoding failed (completed: {completed}, {used}/{bytes.Length})");
            return new string(chars, 0, count);
        }

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="buffer">Output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of used bytes from the output buffer</returns>
        public static int ToUtf8String(this ReadOnlySpan<byte> bytes, Span<char> buffer, bool ignoreUsed = false)
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
        public static string ToUtf8String(this Span<byte> bytes) => ToUtf8String((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static string ToUtf8String(this ReadOnlyMemory<byte> bytes) => bytes.Span.ToUtf8String();

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static string ToUtf8String(this Memory<byte> bytes) => ToUtf8String((ReadOnlySpan<byte>)bytes.Span);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static string ToUtf8String(this byte[] bytes) => bytes.AsSpan().ToUtf8String();

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        public static char[] ToUtf8Chars(this ReadOnlySpan<byte> bytes, bool ignoreUsed = false)
        {
            using RentedArray<char> chars = new(bytes.Length, clean: false);
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true)
                .GetDecoder()
                .Convert(bytes, chars, flush: true, out int used, out int count, out bool completed);
            if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-8 decoding failed (completed: {completed}, {used}/{bytes.Length})");
            return chars.Span[..count].ToArray();
        }

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static char[] ToUtf8Chars(this Span<byte> bytes) => ToUtf8Chars((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static char[] ToUtf8Chars(this ReadOnlyMemory<byte> bytes) => bytes.Span.ToUtf8Chars();

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static char[] ToUtf8Chars(this Memory<byte> bytes) => ToUtf8Chars((ReadOnlySpan<byte>)bytes.Span);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static char[] ToUtf8Chars(this byte[] bytes) => bytes.AsSpan().ToUtf8Chars();

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of used bytes from the output buffer</returns>
        /// <returns>String</returns>
        public static string ToUtf16String(this ReadOnlySpan<byte> bytes, bool ignoreUsed = false)
        {
            using RentedArray<char> chars = new(bytes.Length, clean: false);
            new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true)
                .GetDecoder()
                .Convert(bytes, chars, flush: true, out int used, out int count, out bool completed);
            if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-16 decoding failed (completed: {completed}, {used}/{bytes.Length})");
            return new string(chars, 0, count);
        }

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <param name="buffer">Output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of used bytes from the output buffer</returns>
        public static int ToUtf16String(this ReadOnlySpan<byte> bytes, Span<char> buffer, bool ignoreUsed = false)
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
        public static string ToUtf16String(this Span<byte> bytes) => ToUtf16String((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        public static string ToUtf16String(this ReadOnlyMemory<byte> bytes) => bytes.Span.ToUtf16String();

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        public static string ToUtf16String(this Memory<byte> bytes) => ToUtf16String((ReadOnlySpan<byte>)bytes.Span);

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        public static string ToUtf16String(this byte[] bytes) => bytes.AsSpan().ToUtf16String();

        /// <summary>
        /// Get an UTF-16 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        public static char[] ToUtf16Chars(this ReadOnlySpan<byte> bytes, bool ignoreUsed = false)
        {
            using RentedArray<char> chars = new(bytes.Length, clean: false);
            new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true)
                .GetDecoder()
                .Convert(bytes, chars, flush: true, out int used, out int count, out bool completed);
            if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-16 decoding failed (completed: {completed}, {used}/{bytes.Length})");
            return chars.Span[..count].ToArray();
        }

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static char[] ToUtf16Chars(this Span<byte> bytes) => ToUtf16Chars((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static char[] ToUtf16Chars(this ReadOnlyMemory<byte> bytes) => bytes.Span.ToUtf16Chars();

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static char[] ToUtf16Chars(this Memory<byte> bytes) => ToUtf16Chars((ReadOnlySpan<byte>)bytes.Span);

        /// <summary>
        /// Get an UTF-8 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static char[] ToUtf16Chars(this byte[] bytes) => bytes.AsSpan().ToUtf16Chars();

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        public static string ToUtf32String(this ReadOnlySpan<byte> bytes, bool ignoreUsed = false)
        {
            using RentedArray<char> chars = new(bytes.Length, clean: false);
            new UTF32Encoding(bigEndian: false, byteOrderMark: false, throwOnInvalidCharacters: true)
                .GetDecoder()
                .Convert(bytes, chars, flush: true, out int used, out int count, out bool completed);
            if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-32 decoding failed (completed: {completed}, {used}/{bytes.Length})");
            return new string(chars, 0, count);
        }

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <param name="buffer">Output buffer</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>Number of used bytes from the output buffer</returns>
        public static int ToUtf32String(this ReadOnlySpan<byte> bytes, Span<char> buffer, bool ignoreUsed = false)
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
        public static string ToUtf32String(this Span<byte> bytes) => ToUtf32String((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        public static string ToUtf32String(this ReadOnlyMemory<byte> bytes) => bytes.Span.ToUtf32String();

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        public static string ToUtf32String(this Memory<byte> bytes) => ToUtf32String((ReadOnlySpan<byte>)bytes.Span);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <returns>String</returns>
        public static string ToUtf32String(this byte[] bytes) => bytes.AsSpan().ToUtf32String();

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes (little endian)</param>
        /// <param name="ignoreUsed">Ignore the number of used bytes?</param>
        /// <returns>String</returns>
        public static char[] ToUtf32Chars(this ReadOnlySpan<byte> bytes, bool ignoreUsed = false)
        {
            using RentedArray<char> chars = new(bytes.Length, clean: false);
            new UTF32Encoding(bigEndian: false, byteOrderMark: false, throwOnInvalidCharacters: true)
                .GetDecoder()
                .Convert(bytes, chars, flush: true, out int used, out int count, out bool completed);
            if (!completed || (!ignoreUsed && used != bytes.Length)) throw new InvalidDataException($"UTF-32 decoding failed (completed: {completed}, {used}/{bytes.Length})");
            return chars.Span[..count].ToArray();
        }

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static char[] ToUtf32Chars(this Span<byte> bytes) => ToUtf32Chars((ReadOnlySpan<byte>)bytes);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static char[] ToUtf32Chars(this ReadOnlyMemory<byte> bytes) => bytes.Span.ToUtf32Chars();

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static char[] ToUtf32Chars(this Memory<byte> bytes) => ToUtf32Chars((ReadOnlySpan<byte>)bytes.Span);

        /// <summary>
        /// Get an UTF-32 string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>String</returns>
        public static char[] ToUtf32Chars(this byte[] bytes) => bytes.AsSpan().ToUtf32Chars();

        /// <summary>
        /// Clear the array
        /// </summary>
        /// <param name="bytes">Bytes</param>
        public static void Clear(this byte[] bytes)
        {
            if (bytes.Length == 0) return;
            RandomNumberGenerator.Fill(bytes);
            Array.Clear(bytes);
        }
    }
}
