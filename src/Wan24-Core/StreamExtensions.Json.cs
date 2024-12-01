using System.Text;

namespace wan24.Core
{
    // JSON
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Write a value JSON serialized
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        public static void WriteJson<T>(this Stream stream, T value) => stream.Write(JsonHelper.Encode(value));

        /// <summary>
        /// Write a value JSON serialized
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        public static void WriteJsonNullable<T>(this Stream stream, T? value)
        {
            if (value is null)
            {
                stream.Write((byte)NumericTypes.None);
            }
            else
            {
                stream.Write(JsonHelper.Encode(value));
            }
        }
        /// <summary>
        /// Write a value JSON serialized
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteJsonAsync<T>(this Stream stream, T value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
            => await stream.WriteAsync(JsonHelper.Encode(value), buffer, cancellationToken).DynamicContext();

        /// <summary>
        /// Write a value JSON serialized
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteJsonNullableAsync<T>(this Stream stream, T? value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            if (value is null)
            {
                await stream.WriteAsync((byte)NumericTypes.None, buffer, cancellationToken).DynamicContext();
            }
            else
            {
                await stream.WriteAsync(JsonHelper.Encode(value), buffer, cancellationToken).DynamicContext();
            }
        }

        /// <summary>
        /// Read a JSON encoded value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Value</returns>
        public static T ReadJson<T>(this Stream stream, in int version, in Span<byte> buffer)
        {
            int jsonLen = Encoding.UTF8.GetMaxByteCount(buffer.Length);
            using RentedMemoryRef<char> jsonBuffer = new(jsonLen, clean: false);
            Span<char> jsonBufferSpan = jsonBuffer.Span;
            int len = stream.ReadString(version, jsonBufferSpan, minLen: 1);
            return JsonHelper.Decode<T>(new string(jsonBufferSpan[..len])) ?? throw new InvalidDataException($"JSON decodes to NULL");
        }

        /// <summary>
        /// Read a JSON encoded value
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="objType">Object type</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Value</returns>
        public static object ReadJson(this Stream stream, in int version, in Type objType, in Span<byte> buffer)
        {
            int jsonLen = Encoding.UTF8.GetMaxByteCount(buffer.Length);
            using RentedMemoryRef<char> jsonBuffer = new(jsonLen, clean: false);
            Span<char> jsonBufferSpan = jsonBuffer.Span;
            int len = stream.ReadString(version, jsonBufferSpan, minLen: 1);
            return JsonHelper.DecodeObject(objType, new string(jsonBufferSpan[..len])) ?? throw new InvalidDataException($"JSON decodes to NULL");
        }

        /// <summary>
        /// Read a JSON encoded value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<T> ReadJsonAsync<T>(this Stream stream, int version, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int jsonLen = Encoding.UTF8.GetMaxByteCount(buffer.Length);
            using RentedMemory<char> jsonBuffer = new(jsonLen, clean: false);
            Memory<char> jsonBufferMem = jsonBuffer.Memory;
            int len = await stream.ReadStringAsync(version, jsonBufferMem, minLen: 1, cancellationToken).DynamicContext();
            return JsonHelper.Decode<T>(new string(jsonBufferMem.Span[..len])) ?? throw new InvalidDataException("JSON decodes to NULL");
        }

        /// <summary>
        /// Read a JSON encoded value
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="objType">Object type</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<object> ReadJsonAsync(this Stream stream, int version, Type objType, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int jsonLen = Encoding.UTF8.GetMaxByteCount(buffer.Length);
            using RentedMemory<char> jsonBuffer = new(jsonLen, clean: false);
            Memory<char> jsonBufferMem = jsonBuffer.Memory;
            int len = await stream.ReadStringAsync(version, jsonBufferMem, minLen: 1, cancellationToken).DynamicContext();
            return JsonHelper.DecodeObject(objType, new string(jsonBufferMem.Span[..len])) ?? throw new InvalidDataException("JSON decodes to NULL");
        }

        /// <summary>
        /// Read a JSON encoded value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Value</returns>
        public static T? ReadJsonNullable<T>(this Stream stream, in int version, in Span<byte> buffer)
        {
            int jsonLen = Encoding.UTF8.GetMaxByteCount(buffer.Length);
            using RentedMemoryRef<char> jsonBuffer = new(jsonLen, clean: false);
            Span<char> jsonBufferSpan = jsonBuffer.Span;
            int len = stream.ReadStringNullable(version, jsonBufferSpan, minLen: 1);
            return len < 0 ? default(T?) : JsonHelper.Decode<T>(new string(jsonBufferSpan[..len])) ?? throw new InvalidDataException("JSON decodes to NULL");
        }

        /// <summary>
        /// Read a JSON encoded value
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="objType">Object type</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Value</returns>
        public static object? ReadJsonNullable(this Stream stream, in int version, in Type objType, in Span<byte> buffer)
        {
            int jsonLen = Encoding.UTF8.GetMaxByteCount(buffer.Length);
            using RentedMemoryRef<char> jsonBuffer = new(jsonLen, clean: false);
            Span<char> jsonBufferSpan = jsonBuffer.Span;
            int len = stream.ReadStringNullable(version, jsonBufferSpan, minLen: 1);
            return len < 0 ? null : JsonHelper.DecodeObject(objType, new string(jsonBufferSpan[..len])) ?? throw new InvalidDataException("JSON decodes to NULL");
        }

        /// <summary>
        /// Read a JSON encoded value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<T?> ReadJsonNullableAsync<T>(this Stream stream, int version, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int jsonLen = Encoding.UTF8.GetMaxByteCount(buffer.Length);
            using RentedMemory<char> jsonBuffer = new(jsonLen, clean: false);
            Memory<char> jsonBufferMem = jsonBuffer.Memory;
            int len = await stream.ReadStringNullableAsync(version, jsonBufferMem, minLen: 1, cancellationToken).DynamicContext();
            return len < 0 ? default(T?) : JsonHelper.Decode<T>(new string(jsonBufferMem.Span[..len])) ?? throw new InvalidDataException("JSON decodes to NULL");
        }

        /// <summary>
        /// Read a JSON encoded value
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="objType">Object type</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<object?> ReadJsonNullableAsync<T>(
            this Stream stream, 
            int version, 
            Type objType, 
            Memory<byte> buffer, 
            CancellationToken cancellationToken = default
            )
        {
            int jsonLen = Encoding.UTF8.GetMaxByteCount(buffer.Length);
            using RentedMemory<char> jsonBuffer = new(jsonLen, clean: false);
            Memory<char> jsonBufferMem = jsonBuffer.Memory;
            int len = await stream.ReadStringNullableAsync(version, jsonBufferMem, minLen: 1, cancellationToken).DynamicContext();
            return len < 0 ? null : JsonHelper.DecodeObject(objType, new string(jsonBufferMem.Span[..len])) ?? throw new InvalidDataException("JSON decodes to NULL");
        }
    }
}
