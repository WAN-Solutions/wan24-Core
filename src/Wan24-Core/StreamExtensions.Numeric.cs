using System.Numerics;

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
            if (type.HasValue()) return type;
            switch (type)
            {
                case NumericTypes.SByte: stream.Write(value.CastType<sbyte>()); break;
                case NumericTypes.Byte: stream.Write(value.CastType<byte>()); break;
                case NumericTypes.Short: stream.Write(value.CastType<short>()); break;
                case NumericTypes.UShort: stream.Write(value.CastType<ushort>()); break;
                case NumericTypes.Int: stream.Write(value.CastType<int>()); break;
                case NumericTypes.UInt: stream.Write(value.CastType<uint>()); break;
                case NumericTypes.Long: stream.Write(value.CastType<long>()); break;
                case NumericTypes.ULong: stream.Write(value.CastType<ulong>()); break;
                case NumericTypes.Half: stream.Write(value.CastType<Half>()); break;
                case NumericTypes.Float: stream.Write(value.CastType<float>()); break;
                case NumericTypes.Double: stream.Write(value.CastType<double>()); break;
                case NumericTypes.Decimal: stream.Write(value.CastType<decimal>()); break;
                case NumericTypes.BigInteger: stream.Write(value.CastType<BigInteger>()); break;
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
            if (type.HasValue()) return type;
            switch (type)
            {
                case NumericTypes.SByte: await stream.WriteAsync(value.CastType<sbyte>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Byte: await stream.WriteAsync(value.CastType<byte>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Short: await stream.WriteAsync(value.CastType<short>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.UShort: await stream.WriteAsync(value.CastType<ushort>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Int: await stream.WriteAsync(value.CastType<int>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.UInt: await stream.WriteAsync(value.CastType<uint>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Long: await stream.WriteAsync(value.CastType<long>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.ULong: await stream.WriteAsync(value.CastType<ulong>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Half: await stream.WriteAsync(value.CastType<Half>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Float: await stream.WriteAsync(value.CastType<float>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Double: await stream.WriteAsync(value.CastType<double>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Decimal: await stream.WriteAsync(value.CastType<decimal>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.BigInteger: await stream.WriteAsync(value.CastType<BigInteger>(), buffer, cancellationToken).DynamicContext(); break;
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
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer (required for reading a big integer only)</param>
        /// <returns>Value</returns>
        public static T ReadNumeric<T>(this Stream stream, in int version, in Memory<byte>? buffer = null) where T : struct, IConvertible
        {
            NumericTypes type = (NumericTypes)stream.ReadByte();
            if (type.HasValue()) return type.GetValue().CastType<T>();
            return type switch
            {
                NumericTypes.SByte => stream.ReadByte().CastType<T>(),
                NumericTypes.Byte => stream.ReadByte().CastType<T>(),
                NumericTypes.Short => stream.ReadShort(version).CastType<T>(),
                NumericTypes.UShort => stream.ReadUShort(version).CastType<T>(),
                NumericTypes.Int => stream.ReadInteger(version).CastType<T>(),
                NumericTypes.UInt => stream.ReadUInteger(version).CastType<T>(),
                NumericTypes.Long => stream.ReadLong(version).CastType<T>(),
                NumericTypes.ULong => stream.ReadULong(version).CastType<T>(),
                NumericTypes.Half => stream.ReadHalf(version).CastType<T>(),
                NumericTypes.Float => stream.ReadFloat(version).CastType<T>(),
                NumericTypes.Double => stream.ReadDouble(version).CastType<T>(),
                NumericTypes.Decimal => stream.ReadDecimal(version).CastType<T>(),
                NumericTypes.BigInteger => stream.ReadBigInteger(version, buffer ?? throw new ArgumentNullException(nameof(buffer))).CastType<T>(),
                _ => throw new InvalidDataException($"Unsupported numeric type #{(byte)type}")
            };
        }

        /// <summary>
        /// Read a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer (required for reading a big integer only)</param>
        /// <returns>Value</returns>
        public static T? ReadNumericNullable<T>(this Stream stream, in int version, in Memory<byte>? buffer = null) where T : struct, IConvertible
        {
            NumericTypes type = (NumericTypes)stream.ReadByte();
            if (type == NumericTypes.None) return default;
            if (type.HasValue()) return type.GetValue().CastType<T>();
            return type switch
            {
                NumericTypes.SByte => stream.ReadByte().CastType<T>(),
                NumericTypes.Byte => stream.ReadByte().CastType<T>(),
                NumericTypes.Short => stream.ReadShort(version).CastType<T>(),
                NumericTypes.UShort => stream.ReadUShort(version).CastType<T>(),
                NumericTypes.Int => stream.ReadInteger(version).CastType<T>(),
                NumericTypes.UInt => stream.ReadUInteger(version).CastType<T>(),
                NumericTypes.Long => stream.ReadLong(version).CastType<T>(),
                NumericTypes.ULong => stream.ReadULong(version).CastType<T>(),
                NumericTypes.Half => stream.ReadHalf(version).CastType<T>(),
                NumericTypes.Float => stream.ReadFloat(version).CastType<T>(),
                NumericTypes.Double => stream.ReadDouble(version).CastType<T>(),
                NumericTypes.Decimal => stream.ReadDecimal(version).CastType<T>(),
                NumericTypes.BigInteger => stream.ReadBigInteger(version, buffer ?? throw new ArgumentNullException(nameof(buffer))).CastType<T>(),
                _ => throw new InvalidDataException($"Unsupported numeric type #{(byte)type}")
            };
        }

        /// <summary>
        /// Read a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<T> ReadNumericAsync<T>(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
            where T : struct, IConvertible
        {
            NumericTypes type = (NumericTypes)await stream.ReadOneByteAsync(version, buffer, cancellationToken).DynamicContext();
            if (type.HasValue()) return type.GetValue().CastType<T>();
            return type switch
            {
                NumericTypes.SByte => (await stream.ReadOneSByteAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Byte => (await stream.ReadOneByteAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Short => (await stream.ReadShortAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.UShort => (await stream.ReadUShortAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Int => (await stream.ReadIntegerAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.UInt => (await stream.ReadUIntegerAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Long => (await stream.ReadLongAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.ULong => (await stream.ReadULongAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Half => (await stream.ReadHalfAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Float => (await stream.ReadFloatAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Double => (await stream.ReadDoubleAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Decimal => (await stream.ReadDecimalAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.BigInteger => (await stream.ReadBigIntegerAsync(version, buffer ?? throw new ArgumentNullException(nameof(buffer)), cancellationToken)
                    .DynamicContext()).CastType<T>(),
                _ => throw new InvalidDataException($"Unsupported numeric type #{(byte)type}")
            };
        }

        /// <summary>
        /// Read a number
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<T?> ReadNumericNullableAsync<T>(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
            where T : struct, IConvertible
        {
            NumericTypes type = (NumericTypes)await stream.ReadOneByteAsync(version, buffer, cancellationToken).DynamicContext();
            if (type == NumericTypes.None) return default;
            if (type.HasValue()) return type.GetValue().CastType<T>();
            return type switch
            {
                NumericTypes.SByte => (await stream.ReadOneSByteAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Byte => (await stream.ReadOneByteAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Short => (await stream.ReadShortAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.UShort => (await stream.ReadUShortAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Int => (await stream.ReadIntegerAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.UInt => (await stream.ReadUIntegerAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Long => (await stream.ReadLongAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.ULong => (await stream.ReadULongAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Half => (await stream.ReadHalfAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Float => (await stream.ReadFloatAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Double => (await stream.ReadDoubleAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.Decimal => (await stream.ReadDecimalAsync(version, buffer, cancellationToken).DynamicContext()).CastType<T>(),
                NumericTypes.BigInteger => (await stream.ReadBigIntegerAsync(version, buffer ?? throw new ArgumentNullException(nameof(buffer)), cancellationToken)
                    .DynamicContext()).CastType<T>(),
                _ => throw new InvalidDataException($"Unsupported numeric type #{(byte)type}")
            };
        }
    }
}
