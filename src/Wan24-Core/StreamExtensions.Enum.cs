namespace wan24.Core
{
    // Enum
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Write an enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        public static void Write<T>(this Stream stream, T value) where T : struct, Enum, IConvertible
            => WriteNumeric(stream, (dynamic)Convert.ChangeType(value, typeof(T).GetEnumUnderlyingType()));

        /// <summary>
        /// Write an enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync<T>(this Stream stream, T value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
            where T : struct, Enum, IConvertible
            => await WriteNumericAsync(stream, (dynamic)Convert.ChangeType(value, typeof(T).GetEnumUnderlyingType()), buffer, cancellationToken).DynamicContext();

        /// <summary>
        /// Write an enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        public static void WriteNullable<T>(this Stream stream, T? value) where T : struct, Enum, IConvertible
        {
            if(value.HasValue)
            {
                stream.Write(value.Value);
            }
            else
            {
                stream.Write((byte)NumericTypes.None);
            }
        }

        /// <summary>
        /// Write an enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteNullableAsync<T>(this Stream stream, T? value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
            where T : struct, Enum, IConvertible
        {
            if (value.HasValue)
            {
                await stream.WriteAsync(value.Value, buffer, cancellationToken).DynamicContext();
            }
            else
            {
                await stream.WriteAsync((byte)NumericTypes.None, buffer, cancellationToken).DynamicContext();
            }
        }

        /// <summary>
        /// Read an enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
        public static T ReadEnum<T>(this Stream stream, in int version) where T : struct, Enum, IConvertible
        {
            Type numericType = typeof(T).GetEnumUnderlyingType();
            if (numericType == typeof(sbyte)) return stream.ReadNumeric<sbyte>(version).CastType<T>();
            if (numericType == typeof(byte)) return stream.ReadNumeric<byte>(version).CastType<T>();
            if (numericType == typeof(short)) return stream.ReadNumeric<short>(version).CastType<T>();
            if (numericType == typeof(ushort)) return stream.ReadNumeric<ushort>(version).CastType<T>();
            if (numericType == typeof(int)) return stream.ReadNumeric<int>(version).CastType<T>();
            if (numericType == typeof(uint)) return stream.ReadNumeric<uint>(version).CastType<T>();
            if (numericType == typeof(long)) return stream.ReadNumeric<long>(version).CastType<T>();
            if (numericType == typeof(ulong)) return stream.ReadNumeric<ulong>(version).CastType<T>();
            throw new InvalidProgramException($"Unsupported numeric enum type {numericType} of enum type {typeof(T)}");
        }

        /// <summary>
        /// Read an enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<T> ReadEnumAsync<T>(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
            where T : struct, Enum, IConvertible
        {
            Type numericType = typeof(T).GetEnumUnderlyingType();
            if (numericType == typeof(sbyte)) return (await stream.ReadNumericAsync<sbyte>(version, buffer, cancellationToken).DynamicContext()).CastType<T>();
            if (numericType == typeof(byte)) return (await stream.ReadNumericAsync<byte>(version, buffer, cancellationToken).DynamicContext()).CastType<T>();
            if (numericType == typeof(short)) return (await stream.ReadNumericAsync<short>(version, buffer, cancellationToken).DynamicContext()).CastType<T>();
            if (numericType == typeof(ushort)) return (await stream.ReadNumericAsync<ushort>(version, buffer, cancellationToken).DynamicContext()).CastType<T>();
            if (numericType == typeof(int)) return (await stream.ReadNumericAsync<int>(version, buffer, cancellationToken).DynamicContext()).CastType<T>();
            if (numericType == typeof(uint)) return (await stream.ReadNumericAsync<uint>(version, buffer, cancellationToken).DynamicContext()).CastType<T>();
            if (numericType == typeof(long)) return (await stream.ReadNumericAsync<long>(version, buffer, cancellationToken).DynamicContext()).CastType<T>();
            if (numericType == typeof(ulong)) return (await stream.ReadNumericAsync<ulong>(version, buffer, cancellationToken).DynamicContext()).CastType<T>();
            throw new InvalidProgramException($"Unsupported numeric enum type {numericType} of enum type {typeof(T)}");
        }

        /// <summary>
        /// Read an enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
        public static T? ReadEnumNullable<T>(this Stream stream, in int version) where T : struct, Enum, IConvertible
        {
            Type numericType = typeof(T).GetEnumUnderlyingType();
            object? res;
            if (numericType == typeof(sbyte)) res = stream.ReadNumericNullable<sbyte>(version);
            else if (numericType == typeof(byte)) res = stream.ReadNumericNullable<byte>(version);
            else if (numericType == typeof(short)) res = stream.ReadNumericNullable<short>(version);
            else if (numericType == typeof(ushort)) res = stream.ReadNumericNullable<ushort>(version);
            else if (numericType == typeof(int)) res = stream.ReadNumericNullable<int>(version);
            else if (numericType == typeof(uint)) res = stream.ReadNumericNullable<uint>(version);
            else if (numericType == typeof(long)) res = stream.ReadNumericNullable<long>(version);
            else if (numericType == typeof(ulong)) res = stream.ReadNumericNullable<ulong>(version);
            else throw new InvalidProgramException($"Unsupported numeric enum type {numericType} of enum type {typeof(T)}");
            return res is null
                ? default(T?)
                : res.CastType<T>();
        }

        /// <summary>
        /// Read an enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<T?> ReadEnumNullableAsync<T>(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
            where T : struct, Enum, IConvertible
        {
            Type numericType = typeof(T).GetEnumUnderlyingType();
            object? res;
            if (numericType == typeof(sbyte)) res = await stream.ReadNumericNullableAsync<sbyte>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(byte)) res = await stream.ReadNumericNullableAsync<byte>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(short)) res = await stream.ReadNumericNullableAsync<short>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(ushort)) res = await stream.ReadNumericNullableAsync<ushort>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(int)) res = await stream.ReadNumericNullableAsync<int>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(uint)) res = await stream.ReadNumericNullableAsync<uint>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(long)) res = await stream.ReadNumericNullableAsync<long>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(ulong)) res = await stream.ReadNumericNullableAsync<ulong>(version, buffer, cancellationToken).DynamicContext();
            else throw new InvalidProgramException($"Unsupported numeric enum type {numericType} of enum type {typeof(T)}");
            return res is null
                ? default(T?)
                : res.CastType<T>();
        }
    }
}
