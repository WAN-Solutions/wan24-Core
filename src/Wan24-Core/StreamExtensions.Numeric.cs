namespace wan24.Core
{
    // Numeric
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Write a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <returns>Numeric type</returns>
        public static NumericTypes WriteNumeric<T>(this Stream stream, in T value) where T : struct, IConvertible
        {
            NumericTypes type = value.GetFittingType();
            if (type == NumericTypes.None) throw new ArgumentException("Not a number", nameof(value));
            stream.Write((byte)type);
            if (type.HasValueFlags()) return type;
            switch (type)
            {
                case NumericTypes.Byte: stream.Write(value.CastType<sbyte>()); break;
                case NumericTypes.Byte | NumericTypes.Unsigned: stream.Write(value.CastType<byte>()); break;
                case NumericTypes.Short: stream.Write(value.CastType<short>()); break;
                case NumericTypes.Short | NumericTypes.Unsigned: stream.Write(value.CastType<ushort>()); break;
                case NumericTypes.Integer: stream.Write(value.CastType<int>()); break;
                case NumericTypes.Integer | NumericTypes.Unsigned: stream.Write(value.CastType<uint>()); break;
                case NumericTypes.Long: stream.Write(value.CastType<long>()); break;
                case NumericTypes.Long | NumericTypes.Unsigned: stream.Write(value.CastType<ulong>()); break;
                case NumericTypes.Half: stream.Write(value.CastType<Half>()); break;
                case NumericTypes.Float: stream.Write(value.CastType<float>()); break;
                case NumericTypes.Double: stream.Write(value.CastType<double>()); break;
                case NumericTypes.Decimal: stream.Write(value.CastType<decimal>()); break;
                default: throw new InvalidProgramException($"Failed to find method for {type}");
            }
            return type;
        }

        /// <summary>
        /// Write a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <returns>Numeric type</returns>
        public static NumericTypes WriteNumericNullable<T>(this Stream stream, in T? value) where T : struct, IConvertible
        {
            if (!value.HasValue)
            {
                stream.Write((byte)NumericTypes.None);
                return NumericTypes.None;
            }
            else
            {
                return stream.WriteNumeric(value.Value);
            }
        }

        /// <summary>
        /// Write a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Numeric type</returns>
        public static async Task<NumericTypes> WriteNumericAsync<T>(
            this Stream stream,
            T value,
            Memory<byte>? buffer = null,
            CancellationToken cancellationToken = default
            )
            where T : struct, IConvertible
        {
            NumericTypes type = value.GetFittingType();
            if (type == NumericTypes.None) throw new ArgumentException("Not a number", nameof(value));
            await stream.WriteAsync((byte)type, buffer, cancellationToken).DynamicContext();
            if (type.HasValueFlags()) return type;
            switch (type)
            {
                case NumericTypes.Byte: await stream.WriteAsync(value.CastType<sbyte>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Byte | NumericTypes.Unsigned: await stream.WriteAsync(value.CastType<byte>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Short: await stream.WriteAsync(value.CastType<short>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Short | NumericTypes.Unsigned: await stream.WriteAsync(value.CastType<ushort>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Integer: await stream.WriteAsync(value.CastType<int>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Integer | NumericTypes.Unsigned: await stream.WriteAsync(value.CastType<uint>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Long: await stream.WriteAsync(value.CastType<long>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Long | NumericTypes.Unsigned: await stream.WriteAsync(value.CastType<ulong>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Half: await stream.WriteAsync(value.CastType<Half>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Float: await stream.WriteAsync(value.CastType<float>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Double: await stream.WriteAsync(value.CastType<double>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Decimal: await stream.WriteAsync(value.CastType<decimal>(), buffer, cancellationToken).DynamicContext(); break;
                default: throw new InvalidProgramException($"Failed to find method for {type}");
            }
            return type;
        }

        /// <summary>
        /// Write a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Numeric type</returns>
        public static async Task<NumericTypes> WriteNumericNullableAsync<T>(
            this Stream stream,
            T? value,
            Memory<byte>? buffer = null,
            CancellationToken cancellationToken = default
            )
            where T : struct, IConvertible
        {
            if (!value.HasValue)
            {
                await stream.WriteAsync((byte)NumericTypes.None, buffer, cancellationToken).DynamicContext();
                return NumericTypes.None;
            }
            else
            {
                return await stream.WriteNumericAsync(value.Value, buffer, cancellationToken).DynamicContext();
            }
        }

        /// <summary>
        /// Read a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="stream">Stream</param>
        /// <returns>Value</returns>
        public static T ReadNumeric<T>(this Stream stream) where T : struct, IConvertible
        {
            NumericTypes type = (NumericTypes)stream.ReadByte();
            if (type.HasValueFlags()) return type.GetValue().CastType<T>();
            return type switch
            {
                NumericTypes.Byte => stream.ReadByte().CastType<T>(),
                NumericTypes.Byte | NumericTypes.Unsigned => stream.ReadByte().CastType<T>(),
                NumericTypes.Short => stream.ReadShort().CastType<T>(),
                NumericTypes.Short | NumericTypes.Unsigned => stream.ReadUShort().CastType<T>(),
                NumericTypes.Integer => stream.ReadInteger().CastType<T>(),
                NumericTypes.Integer | NumericTypes.Unsigned => stream.ReadUInteger().CastType<T>(),
                NumericTypes.Long => stream.ReadLong().CastType<T>(),
                NumericTypes.Long | NumericTypes.Unsigned => stream.ReadULong().CastType<T>(),
                NumericTypes.Half => stream.ReadHalf().CastType<T>(),
                NumericTypes.Float => stream.ReadFloat().CastType<T>(),
                NumericTypes.Double => stream.ReadDouble().CastType<T>(),
                NumericTypes.Decimal => stream.ReadDecimal().CastType<T>(),
                _ => throw new InvalidDataException($"Unsupported numeric type #{(byte)type}")
            };
        }

        /// <summary>
        /// Read a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="stream">Stream</param>
        /// <returns>Value</returns>
        public static T? ReadNumericNullable<T>(this Stream stream) where T : struct, IConvertible
        {
            NumericTypes type = (NumericTypes)stream.ReadByte();
            if (type == NumericTypes.None) return default;
            if (type.HasValueFlags()) return type.GetValue().CastType<T>();
            return type switch
            {
                NumericTypes.Byte => stream.ReadByte().CastType<T>(),
                NumericTypes.Byte | NumericTypes.Unsigned => stream.ReadByte().CastType<T>(),
                NumericTypes.Short => stream.ReadShort().CastType<T>(),
                NumericTypes.Short | NumericTypes.Unsigned => stream.ReadUShort().CastType<T>(),
                NumericTypes.Integer => stream.ReadInteger().CastType<T>(),
                NumericTypes.Integer | NumericTypes.Unsigned => stream.ReadUInteger().CastType<T>(),
                NumericTypes.Long => stream.ReadLong().CastType<T>(),
                NumericTypes.Long | NumericTypes.Unsigned => stream.ReadULong().CastType<T>(),
                NumericTypes.Half => stream.ReadHalf().CastType<T>(),
                NumericTypes.Float => stream.ReadFloat().CastType<T>(),
                NumericTypes.Double => stream.ReadDouble().CastType<T>(),
                NumericTypes.Decimal => stream.ReadDecimal().CastType<T>(),
                _ => throw new InvalidDataException($"Unsupported numeric type #{(byte)type}")
            };
        }

        /// <summary>
        /// Read a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<T> ReadNumericAsync<T>(this Stream stream, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
            where T : struct, IConvertible
        {
            NumericTypes type = (NumericTypes)await stream.ReadOneByteAsync(buffer, cancellationToken).DynamicContext();
            if (type.HasValueFlags()) return type.GetValue().CastType<T>();
            return type switch
            {
                NumericTypes.Byte => (await stream.ReadOneByteAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Byte | NumericTypes.Unsigned => (await stream.ReadOneByteAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Short => (await stream.ReadShortAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Short | NumericTypes.Unsigned => (await stream.ReadUShortAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Integer => (await stream.ReadIntegerAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Integer | NumericTypes.Unsigned => (await stream.ReadUIntegerAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Long => (await stream.ReadLongAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Long | NumericTypes.Unsigned => (await stream.ReadULongAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Half => (await stream.ReadHalfAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Float => (await stream.ReadFloatAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Double => (await stream.ReadDoubleAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Decimal => (await stream.ReadDecimalAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                _ => throw new InvalidDataException($"Unsupported numeric type #{(byte)type}")
            };
        }

        /// <summary>
        /// Read a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<T?> ReadNumericNullableAsync<T>(this Stream stream, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
            where T : struct, IConvertible
        {
            NumericTypes type = (NumericTypes)await stream.ReadOneByteAsync(buffer, cancellationToken).DynamicContext();
            if (type == NumericTypes.None) return default;
            if (type.HasValueFlags()) return type.GetValue().CastType<T>();
            return type switch
            {
                NumericTypes.Byte => (await stream.ReadOneByteAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Byte | NumericTypes.Unsigned => (await stream.ReadOneByteAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Short => (await stream.ReadShortAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Short | NumericTypes.Unsigned => (await stream.ReadUShortAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Integer => (await stream.ReadIntegerAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Integer | NumericTypes.Unsigned => (await stream.ReadUIntegerAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Long => (await stream.ReadLongAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Long | NumericTypes.Unsigned => (await stream.ReadULongAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Half => (await stream.ReadHalfAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Float => (await stream.ReadFloatAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Double => (await stream.ReadDoubleAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Decimal => (await stream.ReadDecimalAsync(buffer, cancellationToken).DynamicContext()).CastType<T>(),
                _ => throw new InvalidDataException($"Unsupported numeric type #{(byte)type}")
            };
        }
    }
}
