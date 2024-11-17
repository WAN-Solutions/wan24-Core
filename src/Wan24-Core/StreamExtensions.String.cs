using System.Text;

namespace wan24.Core
{
    // String
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Write a string (UTF-8) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        public static void Write(this Stream stream, in ReadOnlySpan<char> str)
        {
            using RentedMemoryRef<byte> buffer = new(len: Encoding.UTF8.GetMaxByteCount(str.Length), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            int len = Encoding.UTF8.GetBytes(str, bufferSpan);
            stream.WriteWithLengthInfo(bufferSpan[..len]);
        }

        /// <summary>
        /// Write a string (UTF-8) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        public static void Write(this Stream stream, in ReadOnlyMemory<char>? str)
        {
            if (!str.HasValue)
            {
                stream.WriteNumericNullable<int>(value: default);
                return;
            }
            stream.Write(str.Value.Span);
        }

        /// <summary>
        /// Write a string (UTF-8) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, ReadOnlyMemory<char> str, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: Encoding.UTF8.GetMaxByteCount(str.Length), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..Encoding.UTF8.GetMaxByteCount(str.Length)] : buffer2!.Value;
            int len = Encoding.UTF8.GetBytes(str.Span, bufferMem.Span);
            await stream.WriteWithLengthInfoAsync(bufferMem[..len], cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a string (UTF-8) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, ReadOnlyMemory<char>? str, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            if (!str.HasValue)
            {
                await stream.WriteNumericNullableAsync<int>(value: default, cancellationToken: cancellationToken).DynamicContext();
                return;
            }
            await stream.WriteAsync(str.Value, buffer, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a string (Unicode) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        public static void Write16(this Stream stream, in ReadOnlySpan<char> str)
        {
            using RentedMemoryRef<byte> buffer = new(len: Encoding.Unicode.GetMaxByteCount(str.Length), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            int len = Encoding.Unicode.GetBytes(str, bufferSpan);
            stream.WriteWithLengthInfo(bufferSpan[..len]);
        }

        /// <summary>
        /// Write a string (Unicode) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        public static void Write16(this Stream stream, in ReadOnlyMemory<char>? str)
        {
            if (!str.HasValue)
            {
                stream.WriteNumericNullable<int>(value: default);
                return;
            }
            stream.Write16(str.Value.Span);
        }

        /// <summary>
        /// Write a string (Unicode) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task Write16Async(this Stream stream, ReadOnlyMemory<char> str, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: Encoding.Unicode.GetMaxByteCount(str.Length), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..Encoding.UTF8.GetMaxByteCount(str.Length)] : buffer2!.Value;
            int len = Encoding.Unicode.GetBytes(str.Span, bufferMem.Span);
            await stream.WriteWithLengthInfoAsync(bufferMem[..len], cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a string (Unicode) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task Write16Async(this Stream stream, ReadOnlyMemory<char>? str, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            if (!str.HasValue)
            {
                await stream.WriteNumericNullableAsync<int>(value: default, cancellationToken: cancellationToken).DynamicContext();
                return;
            }
            await stream.Write16Async(str.Value, buffer, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a string (UTF-32) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        public static void Write32(this Stream stream, in ReadOnlySpan<char> str)
        {
            using RentedMemoryRef<byte> buffer = new(len: Encoding.UTF32.GetMaxByteCount(str.Length), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            int len = Encoding.UTF32.GetBytes(str, bufferSpan);
            stream.WriteWithLengthInfo(bufferSpan[..len]);
        }

        /// <summary>
        /// Write a string (UTF-32) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        public static void Write32(this Stream stream, in ReadOnlyMemory<char>? str)
        {
            if (!str.HasValue)
            {
                stream.WriteNumericNullable<int>(value: default);
                return;
            }
            stream.Write32(str.Value.Span);
        }

        /// <summary>
        /// Write a string (UTF-32) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task Write32Async(this Stream stream, ReadOnlyMemory<char> str, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: Encoding.UTF32.GetMaxByteCount(str.Length), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..Encoding.UTF32.GetMaxByteCount(str.Length)] : buffer2!.Value;
            int len = Encoding.UTF32.GetBytes(str.Span, bufferMem.Span);
            await stream.WriteWithLengthInfoAsync(bufferMem[..len], cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a string (UTF-32) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task Write32Async(this Stream stream, ReadOnlyMemory<char>? str, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            if (!str.HasValue)
            {
                await stream.WriteNumericNullableAsync<int>(value: default, cancellationToken: cancellationToken).DynamicContext();
                return;
            }
            await stream.Write32Async(str.Value, buffer, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Read an UTF-8 encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String buffer</param>
        /// <param name="minLen">Minimum length in bytes</param>
        /// <returns>Number of characters written to the buffer</returns>
        public static int ReadString(this Stream stream, in Span<char> str, in int minLen = 0)
        {
            int len = Encoding.UTF8.GetMaxByteCount(str.Length);
            using RentedMemoryRef<byte> buffer = new(len, clean: false);
            Span<byte> bufferSpan = buffer.Span;
            int red = stream.ReadDataWithLengthInfo(bufferSpan);
            if (red < minLen) throw new InvalidDataException($"Red {red} bytes (min. {minLen} expected)");
            Decoder decoder = Encoding.UTF8.GetDecoder();
            decoder.Convert(bufferSpan[..red], str, flush: true, out int bytesUsed, out int res, out bool completed);
            if (!completed || bytesUsed != red) throw new InvalidDataException($"UTF-8 decoder used {bytesUsed} of {red} bytes");
            return res;
        }

        /// <summary>
        /// Read an UTF-8 encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String buffer</param>
        /// <param name="minLen">Minimum length in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of characters written to the buffer</returns>
        public static async Task<int> ReadStringAsync(this Stream stream, Memory<char> str, int minLen = 0, CancellationToken cancellationToken = default)
        {
            int len = Encoding.UTF8.GetMaxByteCount(str.Length);
            using RentedMemory<byte> buffer = new(len, clean: false);
            Memory<byte> bufferMem = buffer.Memory;
            int red = await stream.ReadDataWithLengthInfoAsync(bufferMem, cancellationToken).DynamicContext();
            if (red < minLen) throw new InvalidDataException($"Red {red} bytes (min. {minLen} expected)");
            Decoder decoder = Encoding.UTF8.GetDecoder();
            decoder.Convert(bufferMem.Span[..red], str.Span, flush: true, out int bytesUsed, out int res, out bool completed);
            if (!completed || bytesUsed != red) throw new InvalidDataException($"UTF-8 decoder used {bytesUsed} of {red} bytes");
            return res;
        }

        /// <summary>
        /// Read an Unicode encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String buffer</param>
        /// <param name="minLen">Minimum length in bytes</param>
        /// <returns>Number of characters written to the buffer</returns>
        public static int ReadString16(this Stream stream, in Span<char> str, in int minLen = 0)
        {
            int len = Encoding.Unicode.GetMaxByteCount(str.Length);
            using RentedMemoryRef<byte> buffer = new(len, clean: false);
            Span<byte> bufferSpan = buffer.Span;
            int red = stream.ReadDataWithLengthInfo(bufferSpan);
            if (red < minLen) throw new InvalidDataException($"Red {red} bytes (min. {minLen} expected)");
            Decoder decoder = Encoding.Unicode.GetDecoder();
            decoder.Convert(bufferSpan[..red], str, flush: true, out int bytesUsed, out int res, out bool completed);
            if (!completed || bytesUsed != red) throw new InvalidDataException($"Unicode decoder used {bytesUsed} of {red} bytes");
            return res;
        }

        /// <summary>
        /// Read an Unicode encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String buffer</param>
        /// <param name="minLen">Minimum length in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of characters written to the buffer</returns>
        public static async Task<int> ReadString16Async(this Stream stream, Memory<char> str, int minLen = 0, CancellationToken cancellationToken = default)
        {
            int len = Encoding.Unicode.GetMaxByteCount(str.Length);
            using RentedMemory<byte> buffer = new(len, clean: false);
            Memory<byte> bufferMem = buffer.Memory;
            int red = await stream.ReadDataWithLengthInfoAsync(bufferMem, cancellationToken).DynamicContext();
            if (red < minLen) throw new InvalidDataException($"Red {red} bytes (min. {minLen} expected)");
            Decoder decoder = Encoding.Unicode.GetDecoder();
            decoder.Convert(bufferMem.Span[..red], str.Span, flush: true, out int bytesUsed, out int res, out bool completed);
            if (!completed || bytesUsed != red) throw new InvalidDataException($"Unicode decoder used {bytesUsed} of {red} bytes");
            return res;
        }

        /// <summary>
        /// Read an UTF-32 encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String buffer</param>
        /// <param name="minLen">Minimum length in bytes</param>
        /// <returns>Number of characters written to the buffer</returns>
        public static int ReadString32(this Stream stream, in Span<char> str, in int minLen = 0)
        {
            int len = Encoding.UTF32.GetMaxByteCount(str.Length);
            using RentedMemoryRef<byte> buffer = new(len, clean: false);
            Span<byte> bufferSpan = buffer.Span;
            int red = stream.ReadDataWithLengthInfo(bufferSpan);
            if (red < minLen) throw new InvalidDataException($"Red {red} bytes (min. {minLen} expected)");
            Decoder decoder = Encoding.UTF32.GetDecoder();
            decoder.Convert(bufferSpan[..red], str, flush: true, out int bytesUsed, out int res, out bool completed);
            if (!completed || bytesUsed != red) throw new InvalidDataException($"UTF-32 decoder used {bytesUsed} of {red} bytes");
            return res;
        }

        /// <summary>
        /// Read an UTF-32 encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String buffer</param>
        /// <param name="minLen">Minimum length in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of characters written to the buffer</returns>
        public static async Task<int> ReadString32Async(this Stream stream, Memory<char> str, int minLen = 0, CancellationToken cancellationToken = default)
        {
            int len = Encoding.UTF32.GetMaxByteCount(str.Length);
            using RentedMemory<byte> buffer = new(len, clean: false);
            Memory<byte> bufferMem = buffer.Memory;
            int red = await stream.ReadDataWithLengthInfoAsync(bufferMem, cancellationToken).DynamicContext();
            if (red < minLen) throw new InvalidDataException($"Red {red} bytes (min. {minLen} expected)");
            Decoder decoder = Encoding.UTF32.GetDecoder();
            decoder.Convert(bufferMem.Span[..red], str.Span, flush: true, out int bytesUsed, out int res, out bool completed);
            if (!completed || bytesUsed != red) throw new InvalidDataException($"UTF-32 decoder used {bytesUsed} of {red} bytes");
            return res;
        }
    }
}
