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
        /// <param name="options">Options</param>
        public static void Write(this Stream stream, in ReadOnlySpan<char> str, in StringWritingOptions? options = null)
            => stream.Write(str, Encoding.UTF8, options);

        /// <summary>
        /// Write a string (UTF-8) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="options">Options</param>
        public static void Write(this Stream stream, in string? str, in StringWritingOptions? options = null)
            => stream.Write(str, Encoding.UTF8, options);

        /// <summary>
        /// Write a string (UTF-8) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(
            this Stream stream,
            ReadOnlyMemory<char> str,
            StringWritingOptions? options = null,
            CancellationToken cancellationToken = default
            )
            => await stream.WriteAsync(str, Encoding.UTF8, options, cancellationToken).DynamicContext();

        /// <summary>
        /// Write a string (UTF-8) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, string? str, StringWritingOptions? options = null, CancellationToken cancellationToken = default)
            => await stream.WriteAsync(str, Encoding.UTF8, options, cancellationToken).DynamicContext();

        /// <summary>
        /// Write a string (Unicode) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="options">Options</param>
        public static void Write16(this Stream stream, in ReadOnlySpan<char> str, in StringWritingOptions? options = null)
            => stream.Write(str, Encoding.Unicode, options);

        /// <summary>
        /// Write a string (Unicode) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="options">Options</param>
        public static void Write16(this Stream stream, in string? str, in StringWritingOptions? options = null)
            => stream.Write(str, Encoding.Unicode, options);

        /// <summary>
        /// Write a string (Unicode) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task Write16Async(
            this Stream stream,
            ReadOnlyMemory<char> str,
            StringWritingOptions? options = null,
            CancellationToken cancellationToken = default
            )
            => await stream.WriteAsync(str, Encoding.Unicode, options, cancellationToken).DynamicContext();

        /// <summary>
        /// Write a string (Unicode) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task Write16Async(this Stream stream, string? str, StringWritingOptions? options = null, CancellationToken cancellationToken = default)
            => await stream.WriteAsync(str, Encoding.Unicode, options, cancellationToken).DynamicContext();

        /// <summary>
        /// Write a string (UTF-32) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="options">Options</param>
        public static void Write32(this Stream stream, in ReadOnlySpan<char> str, in StringWritingOptions? options = null)
            => stream.Write(str, Encoding.UTF32, options);

        /// <summary>
        /// Write a string (UTF-32) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="options">Options</param>
        public static void Write32(this Stream stream, in string? str, in StringWritingOptions? options = null)
            => stream.Write(str, Encoding.UTF32, options);

        /// <summary>
        /// Write a string (UTF-32) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task Write32Async(
            this Stream stream,
            ReadOnlyMemory<char> str,
            StringWritingOptions? options = null,
            CancellationToken cancellationToken = default
            )
            => await stream.WriteAsync(str, Encoding.UTF32, options, cancellationToken).DynamicContext();

        /// <summary>
        /// Write a string (UTF-32) with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task Write32Async(this Stream stream, string? str, StringWritingOptions? options = null, CancellationToken cancellationToken = default)
            => await stream.WriteAsync(str, Encoding.UTF32, options, cancellationToken).DynamicContext();

        /// <summary>
        /// Write a string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="options">Options</param>
        public static void Write(this Stream stream, in ReadOnlySpan<char> str, in Encoding encoding, in StringWritingOptions? options = null)
        {
            using RentedMemory<byte>? buffer = (options?.Buffer.HasValue ?? false) ? null : new(len: encoding.GetMaxByteCount(str.Length), clean: false);
            Span<byte> bufferSpan = (options?.Buffer.HasValue ?? false) ? options.Buffer.Value.Span : buffer!.Value.Memory.Span;
            int len = encoding.GetBytes(str, bufferSpan);
            stream.WriteWithLengthInfo(bufferSpan[..len]);
        }

        /// <summary>
        /// Write a string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="options">Options</param>
        public static void Write(this Stream stream, in string? str, in Encoding encoding, in StringWritingOptions? options = null)
        {
            if (str is null)
            {
                stream.Write((byte)NumericTypes.None);
                return;
            }
            stream.Write(str.AsSpan(), encoding, options);
        }

        /// <summary>
        /// Write a string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(
            this Stream stream,
            ReadOnlyMemory<char> str,
            Encoding encoding,
            StringWritingOptions? options = null,
            CancellationToken cancellationToken = default
            )
        {
            using RentedMemory<byte>? buffer2 = (options?.Buffer.HasValue ?? false) ? null : new(len: encoding.GetMaxByteCount(str.Length), clean: false);
            Memory<byte> bufferMem = (options?.Buffer.HasValue ?? false) ? options.Buffer.Value[..encoding.GetMaxByteCount(str.Length)] : buffer2!.Value;
            int len = encoding.GetBytes(str.Span, bufferMem.Span);
            await stream.WriteWithLengthInfoAsync(bufferMem[..len], cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(
            this Stream stream,
            string? str,
            Encoding encoding,
            StringWritingOptions? options = null,
            CancellationToken cancellationToken = default
            )
        {
            if (str is null)
            {
                await stream.WriteAsync((byte)NumericTypes.None, options?.Buffer, cancellationToken).DynamicContext();
                return;
            }
            await stream.WriteAsync(str.AsMemory(), encoding, options, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Read an UTF-8 encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <returns>Number of characters written to the buffer</returns>
        public static int ReadString(this Stream stream, in int version, in StringReadingOptions options)
            => stream.ReadString(version, Encoding.UTF8, options);

        /// <summary>
        /// Read an UTF-8 encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <returns>Number of characters written to the buffer or <c>-1</c>, if <see langword="null"/></returns>
        public static int ReadStringNullable(this Stream stream, in int version, in StringReadingOptions options)
            => stream.ReadStringNullable(version, Encoding.UTF8, options);

        /// <summary>
        /// Read an UTF-8 encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of characters written to the buffer</returns>
        public static async Task<int> ReadStringAsync(this Stream stream, int version, StringReadingOptions options, CancellationToken cancellationToken = default)
            => await stream.ReadStringAsync(version, Encoding.UTF8, options, cancellationToken).DynamicContext();

        /// <summary>
        /// Read an UTF-8 encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of characters written to the buffer or <c>-1</c>, if <see langword="null"/></returns>
        public static async Task<int> ReadStringNullableAsync(
            this Stream stream,
            int version,
            StringReadingOptions options,
            CancellationToken cancellationToken = default
            )
            => await stream.ReadStringNullableAsync(version, Encoding.UTF8, options, cancellationToken).DynamicContext();

        /// <summary>
        /// Read an Unicode encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <returns>Number of characters written to the buffer</returns>
        public static int ReadString16(this Stream stream, in int version, in StringReadingOptions options)
            => stream.ReadString(version, Encoding.Unicode, options);

        /// <summary>
        /// Read an Unicode encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <returns>Number of characters written to the buffer or <c>-1</c>, if <see langword="null"/></returns>
        public static int ReadString16Nullable(this Stream stream, in int version, in StringReadingOptions options)
            => stream.ReadStringNullable(version, Encoding.Unicode, options);

        /// <summary>
        /// Read an Unicode encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of characters written to the buffer</returns>
        public static async Task<int> ReadString16Async(this Stream stream, int version, StringReadingOptions options, CancellationToken cancellationToken = default)
            => await stream.ReadStringAsync(version, Encoding.Unicode, options, cancellationToken).DynamicContext();

        /// <summary>
        /// Read an Unicode encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of characters written to the buffer or <c>-1</c>, if <see langword="null"/></returns>
        public static async Task<int> ReadString16NullableAsync(
            this Stream stream,
            int version,
            StringReadingOptions options,
            CancellationToken cancellationToken = default
            )
            => await stream.ReadStringNullableAsync(version, Encoding.Unicode, options, cancellationToken).DynamicContext();

        /// <summary>
        /// Read an UTF-32 encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <returns>Number of characters written to the buffer</returns>
        public static int ReadString32(this Stream stream, in int version, in StringReadingOptions options)
            => stream.ReadString(version, Encoding.UTF32, options);

        /// <summary>
        /// Read an UTF-32 encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <returns>Number of characters written to the buffer or <c>-1</c>, if <see langword="null"/></returns>
        public static int ReadString32Nullable(this Stream stream, in int version, in StringReadingOptions options)
            => stream.ReadStringNullable(version, Encoding.UTF32, options);

        /// <summary>
        /// Read an UTF-32 encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of characters written to the buffer</returns>
        public static async Task<int> ReadString32Async(this Stream stream, int version, StringReadingOptions options, CancellationToken cancellationToken = default)
            => await stream.ReadStringAsync(version, Encoding.UTF32, options, cancellationToken).DynamicContext();

        /// <summary>
        /// Read an UTF-32 encoded string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of characters written to the buffer or <c>-1</c>, if <see langword="null"/></returns>
        public static async Task<int> ReadString32NullableAsync(
            this Stream stream,
            int version,
            StringReadingOptions options,
            CancellationToken cancellationToken = default
            )
            => await stream.ReadStringNullableAsync(version, Encoding.UTF32, options, cancellationToken).DynamicContext();

        /// <summary>
        /// Read a string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="options">Options</param>
        /// <returns>Number of characters written to the string buffer</returns>
        public static int ReadString(this Stream stream, in int version, in Encoding encoding, in StringReadingOptions options)
        {
            int len = encoding.GetMaxByteCount(options.StringBuffer!.Value.Length);
            using RentedMemory<byte>? buffer = options.Buffer.HasValue ? null : new(len, clean: false);
            Span<byte> bufferSpan = options.Buffer.HasValue ? options.Buffer.Value.Span : buffer!.Value.Memory.Span;
            int red = stream.ReadDataWithLengthInfo(version, bufferSpan);
            if (encoding.GetMaxCharCount(red) < options.MinLength)
                throw new InvalidDataException($"Red {encoding.GetMaxCharCount(red)} characters (min. {options.MinLength} expected)");
            Decoder decoder = encoding.GetDecoder();
            decoder.Convert(bufferSpan[..red], options.StringBuffer.Value.Span, flush: true, out int bytesUsed, out int res, out bool completed);
            if (!completed || bytesUsed != red) throw new InvalidDataException($"{encoding.EncodingName} decoder used {bytesUsed} of {red} bytes");
            return res;
        }

        /// <summary>
        /// Read a string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="options">Options</param>
        /// <returns>Number of characters written to the string buffer or <c>-1</c>, if <see langword="null"/></returns>
        public static int ReadStringNullable(this Stream stream, in int version, in Encoding encoding, in StringReadingOptions options)
        {
            int len = encoding.GetMaxByteCount(options.StringBuffer!.Value.Length);
            using RentedMemory<byte>? buffer = options.Buffer.HasValue ? null : new(len, clean: false);
            Span<byte> bufferSpan = options.Buffer.HasValue ? options.Buffer.Value.Span : buffer!.Value.Memory.Span;
            int red = stream.ReadDataNullableWithLengthInfo(version, bufferSpan);
            if (red < 0) return -1;
            if (encoding.GetMaxCharCount(red) < options.MinLength)
                throw new InvalidDataException($"Red {encoding.GetMaxCharCount(red)} characters (min. {options.MinLength} expected)");
            Decoder decoder = encoding.GetDecoder();
            decoder.Convert(bufferSpan[..red], options.StringBuffer.Value.Span, flush: true, out int bytesUsed, out int res, out bool completed);
            if (!completed || bytesUsed != red) throw new InvalidDataException($"{encoding.EncodingName} decoder used {bytesUsed} of {red} bytes");
            return res;
        }

        /// <summary>
        /// Read a string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of characters written to the string buffer</returns>
        public static async Task<int> ReadStringAsync(
            this Stream stream,
            int version,
            Encoding encoding,
            StringReadingOptions options,
            CancellationToken cancellationToken = default
            )
        {
            int len = encoding.GetMaxByteCount(options.StringBuffer!.Value.Length);
            using RentedMemory<byte>? buffer = options.Buffer.HasValue ? null : new(len, clean: false);
            Memory<byte> bufferMem = options.Buffer.HasValue ? options.Buffer.Value : buffer!.Value.Memory;
            int red = await stream.ReadDataWithLengthInfoAsync(version, bufferMem, cancellationToken);
            if (encoding.GetMaxCharCount(red) < options.MinLength)
                throw new InvalidDataException($"Red {encoding.GetMaxCharCount(red)} characters (min. {options.MinLength} expected)");
            Decoder decoder = encoding.GetDecoder();
            decoder.Convert(bufferMem.Span[..red], options.StringBuffer.Value.Span, flush: true, out int bytesUsed, out int res, out bool completed);
            if (!completed || bytesUsed != red) throw new InvalidDataException($"{encoding.EncodingName} decoder used {bytesUsed} of {red} bytes");
            return res;
        }

        /// <summary>
        /// Read a string with length information
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of characters written to the string buffer or <c>-1</c>, if <see langword="null"/></returns>
        public static async Task<int> ReadStringNullableAsync(
            this Stream stream,
            int version,
            Encoding encoding,
            StringReadingOptions options,
            CancellationToken cancellationToken = default
            )
        {
            int len = encoding.GetMaxByteCount(options.StringBuffer!.Value.Length);
            using RentedMemory<byte>? buffer = options.Buffer.HasValue ? null : new(len, clean: false);
            Memory<byte> bufferMem = options.Buffer.HasValue ? options.Buffer.Value : buffer!.Value.Memory;
            int red = await stream.ReadDataWithLengthInfoAsync(version, bufferMem, cancellationToken);
            if (red < 0) return -1;
            if (encoding.GetMaxCharCount(red) < options.MinLength)
                throw new InvalidDataException($"Red {encoding.GetMaxCharCount(red)} characters (min. {options.MinLength} expected)");
            Decoder decoder = encoding.GetDecoder();
            decoder.Convert(bufferMem.Span[..red], options.StringBuffer.Value.Span, flush: true, out int bytesUsed, out int res, out bool completed);
            if (!completed || bytesUsed != red) throw new InvalidDataException($"{encoding.EncodingName} decoder used {bytesUsed} of {red} bytes");
            return res;
        }
    }
}
