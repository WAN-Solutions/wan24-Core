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
        /// Write an enum (adapter for <see cref="Write{T}(Stream, T)"/> for dynamic generic calls only)
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        public static void WriteEnum<T>(this Stream stream, T value) where T : struct, Enum, IConvertible => stream.Write(value);

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
        /// Write an enum (adapter for <see cref="WriteAsync{T}(Stream, T, Memory{byte}?, CancellationToken)"/> for dynamic generic calls only)
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteEnumAsync<T>(this Stream stream, T value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
            where T : struct, Enum, IConvertible
            => await WriteAsync(stream, value, buffer, cancellationToken).DynamicContext();

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
        /// Write an enum (adapter for <see cref="WriteNullable{T}(Stream, T?)"/> for dynamic generic calls only)
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        public static void WriteEnumNullable<T>(this Stream stream, T? value) where T : struct, Enum, IConvertible => stream.WriteNullable(value);

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
        /// Write an enum (adapter for <see cref="WriteNullableAsync{T}(Stream, T?, Memory{byte}?, CancellationToken)"/> for dynamic generic calls only)
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteEnumNullableAsync<T>(this Stream stream, T? value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
            where T : struct, Enum, IConvertible
            => await stream.WriteNullableAsync(value, buffer, cancellationToken).DynamicContext();

        /// <summary>
        /// Read an enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
        public static T ReadEnum<T>(this Stream stream, in int version) where T : struct, Enum, IConvertible => stream.ReadEnum(version, typeof(T)).CastType<T>();

        /// <summary>
        /// Read an enum
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="type">Enum type</param>
        /// <returns>Value</returns>
        public static object ReadEnum(this Stream stream, in int version, in Type type)
        {
            if (!type.IsEnum) throw new ArgumentException("Enum type expected", nameof(type));
            Type numericType = type.GetEnumUnderlyingType();
            object res;
            if (numericType == typeof(sbyte)) res = stream.ReadNumeric<sbyte>(version);
            else if (numericType == typeof(byte)) res = stream.ReadNumeric<byte>(version);
            else if (numericType == typeof(short)) res = stream.ReadNumeric<short>(version);
            else if (numericType == typeof(ushort)) res = stream.ReadNumeric<ushort>(version);
            else if (numericType == typeof(int)) res = stream.ReadNumeric<int>(version);
            else if (numericType == typeof(uint)) res = stream.ReadNumeric<uint>(version);
            else if (numericType == typeof(long)) res = stream.ReadNumeric<long>(version);
            else if (numericType == typeof(ulong)) res = stream.ReadNumeric<ulong>(version);
            else throw new InvalidProgramException($"Unsupported numeric enum type {numericType} of enum type {type}");
            res = Enum.ToObject(type, res);
            if (!type.GetEnumInfo().IsValidValue(res)) throw new InvalidDataException($"Invalid {type} enumeration value \"{res}\"");
            return res;
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
            => (await stream.ReadEnumAsync(version, typeof(T), buffer, cancellationToken).DynamicContext()).CastType<T>();

        /// <summary>
        /// Read an enum
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="type">Enum type</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<object> ReadEnumAsync(this Stream stream, int version, Type type, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            if (!type.IsEnum) throw new ArgumentException("Enum type expected", nameof(type));
            Type numericType = type.GetEnumUnderlyingType();
            object res;
            if (numericType == typeof(sbyte)) res = await stream.ReadNumericAsync<sbyte>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(byte)) res = await stream.ReadNumericAsync<byte>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(short)) res = await stream.ReadNumericAsync<short>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(ushort)) res = await stream.ReadNumericAsync<ushort>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(int)) res = await stream.ReadNumericAsync<int>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(uint)) res = await stream.ReadNumericAsync<uint>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(long)) res = await stream.ReadNumericAsync<long>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(ulong)) res = await stream.ReadNumericAsync<ulong>(version, buffer, cancellationToken).DynamicContext();
            else throw new InvalidProgramException($"Unsupported numeric enum type {numericType} of enum type {type}");
            res = Enum.ToObject(type, res);
            if (!type.GetEnumInfo().IsValidValue(res)) throw new InvalidDataException($"Invalid {type} enumeration value \"{res}\"");
            return res;
        }

        /// <summary>
        /// Read an enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
        public static T? ReadEnumNullable<T>(this Stream stream, in int version) where T : struct, Enum, IConvertible
            => stream.ReadEnumNullable(version, typeof(T))?.CastType<T>();

        /// <summary>
        /// Read an enum
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="type">Enum type</param>
        /// <returns>Value</returns>
        public static object? ReadEnumNullable(this Stream stream, in int version, in Type type)
        {
            if (!type.IsEnum) throw new ArgumentException("Enum type expected", nameof(type));
            Type numericType = type.GetEnumUnderlyingType();
            object? res;
            if (numericType == typeof(sbyte)) res = stream.ReadNumericNullable<sbyte>(version);
            else if (numericType == typeof(byte)) res = stream.ReadNumericNullable<byte>(version);
            else if (numericType == typeof(short)) res = stream.ReadNumericNullable<short>(version);
            else if (numericType == typeof(ushort)) res = stream.ReadNumericNullable<ushort>(version);
            else if (numericType == typeof(int)) res = stream.ReadNumericNullable<int>(version);
            else if (numericType == typeof(uint)) res = stream.ReadNumericNullable<uint>(version);
            else if (numericType == typeof(long)) res = stream.ReadNumericNullable<long>(version);
            else if (numericType == typeof(ulong)) res = stream.ReadNumericNullable<ulong>(version);
            else throw new InvalidProgramException($"Unsupported numeric enum type {numericType} of enum type {type}");
            if (res is not null)
            {
                res = Enum.ToObject(type, res);
                if (!type.GetEnumInfo().IsValidValue(res)) throw new InvalidDataException($"Invalid {type} enumeration value \"{res}\"");
            }
            return res;
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
            => (await stream.ReadEnumNullableAsync(version, typeof(T), buffer, cancellationToken).DynamicContext())?.CastType<T>();

        /// <summary>
        /// Read an enum
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="type">Enum type</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<object?> ReadEnumNullableAsync(
            this Stream stream, 
            int version, 
            Type type, 
            Memory<byte>? buffer = null, 
            CancellationToken cancellationToken = default
            )
        {
            if (!type.IsEnum) throw new ArgumentException("Enum type expected", nameof(type));
            Type numericType = type.GetEnumUnderlyingType();
            object? res;
            if (numericType == typeof(sbyte)) res = await stream.ReadNumericNullableAsync<sbyte>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(byte)) res = await stream.ReadNumericNullableAsync<byte>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(short)) res = await stream.ReadNumericNullableAsync<short>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(ushort)) res = await stream.ReadNumericNullableAsync<ushort>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(int)) res = await stream.ReadNumericNullableAsync<int>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(uint)) res = await stream.ReadNumericNullableAsync<uint>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(long)) res = await stream.ReadNumericNullableAsync<long>(version, buffer, cancellationToken).DynamicContext();
            else if (numericType == typeof(ulong)) res = await stream.ReadNumericNullableAsync<ulong>(version, buffer, cancellationToken).DynamicContext();
            else throw new InvalidProgramException($"Unsupported numeric enum type {numericType} of enum type {type}");
            if (res is not null)
            {
                res = Enum.ToObject(type, res);
                if (!type.GetEnumInfo().IsValidValue(res)) throw new InvalidDataException($"Invalid {type} enumeration value \"{res}\"");
            }
            return res;
        }
    }
}
