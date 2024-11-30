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
        public static NumericTypes WriteNumeric<T>(this Stream stream, T value) where T : struct, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
        {
            NumericTypes type = value.GetNumericType();
            if (type == NumericTypes.None) throw new ArgumentException("Not a number", nameof(value));
            stream.Write((byte)type);
            if (type.HasValue()) return type;
            switch (type)
            {
                case NumericTypes.SByte: stream.Write(value.CastNumericValue(type).CastType<sbyte>()); break;
                case NumericTypes.Byte: stream.Write(value.CastNumericValue(type).CastType<byte>()); break;
                case NumericTypes.Short: stream.Write(value.CastNumericValue(type).CastType<short>()); break;
                case NumericTypes.UShort: stream.Write(value.CastNumericValue(type).CastType<ushort>()); break;
                case NumericTypes.Int: stream.Write(value.CastNumericValue(type).CastType<int>()); break;
                case NumericTypes.UInt: stream.Write(value.CastNumericValue(type).CastType<uint>()); break;
                case NumericTypes.Long: stream.Write(value.CastNumericValue(type).CastType<long>()); break;
                case NumericTypes.ULong: stream.Write(value.CastNumericValue(type).CastType<ulong>()); break;
                case NumericTypes.Int128: stream.Write(value.CastNumericValue(type).CastType<Int128>()); break;
                case NumericTypes.UInt128: stream.Write(value.CastNumericValue(type).CastType<UInt128>()); break;
                case NumericTypes.Half: stream.Write(value.CastNumericValue(type).CastType<Half>()); break;
                case NumericTypes.Float: stream.Write(value.CastNumericValue(type).CastType<float>()); break;
                case NumericTypes.Double: stream.Write(value.CastNumericValue(type).CastType<double>()); break;
                case NumericTypes.Decimal: stream.Write(value.CastNumericValue(type).CastType<decimal>()); break;
                case NumericTypes.BigInteger: stream.Write(value.CastNumericValue(type).CastType<BigInteger>()); break;
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
        public static NumericTypes WriteNumericNullable<T>(this Stream stream, T? value) where T : struct, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
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
            where T : struct, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
        {
            NumericTypes type = value.GetNumericType();
            if (type == NumericTypes.None) throw new ArgumentException("Not a number", nameof(value));
            await stream.WriteAsync((byte)type, buffer, cancellationToken).DynamicContext();
            if (type.HasValue()) return type;
            switch (type)
            {
                case NumericTypes.SByte: await stream.WriteAsync(value.CastNumericValue(type).CastType<sbyte>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Byte: await stream.WriteAsync(value.CastNumericValue(type).CastType<byte>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Short: await stream.WriteAsync(value.CastNumericValue(type).CastType<short>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.UShort: await stream.WriteAsync(value.CastNumericValue(type).CastType<ushort>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Int: await stream.WriteAsync(value.CastNumericValue(type).CastType<int>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.UInt: await stream.WriteAsync(value.CastNumericValue(type).CastType<uint>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Long: await stream.WriteAsync(value.CastNumericValue(type).CastType<long>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.ULong: await stream.WriteAsync(value.CastNumericValue(type).CastType<ulong>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Int128: await stream.WriteAsync(value.CastNumericValue(type).CastType<Int128>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.UInt128: await stream.WriteAsync(value.CastNumericValue(type).CastType<UInt128>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Half: await stream.WriteAsync(value.CastNumericValue(type).CastType<Half>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Float: await stream.WriteAsync(value.CastNumericValue(type).CastType<float>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Double: await stream.WriteAsync(value.CastNumericValue(type).CastType<double>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.Decimal: await stream.WriteAsync(value.CastNumericValue(type).CastType<decimal>(), buffer, cancellationToken).DynamicContext(); break;
                case NumericTypes.BigInteger: await stream.WriteAsync(value.CastNumericValue(type).CastType<BigInteger>(), buffer, cancellationToken).DynamicContext(); break;
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
            where T : struct, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
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
        public static T ReadNumeric<T>(this Stream stream, in int version, in Memory<byte>? buffer = null)
            where T : struct, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
        {
            NumericTypes requestedType = typeof(T).GetNumericType();
            if (requestedType == NumericTypes.None) throw new ArgumentException($"Unsupported type {typeof(T)}", nameof(T));
            NumericTypes type = (NumericTypes)stream.ReadOneByte(version);
            if (type.HasValue()) return type.GetValue().CastNumericValue(requestedType).CastType<T>();
            return type switch
            {
                NumericTypes.SByte => stream.ReadByte().CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Byte => stream.ReadByte().CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Short => stream.ReadShort(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.UShort => stream.ReadUShort(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Int => stream.ReadInteger(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.UInt => stream.ReadUInteger(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Long => stream.ReadLong(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.ULong => stream.ReadULong(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Int128 => stream.ReadInt128(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.UInt128 => stream.ReadUInt128(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Half => stream.ReadHalf(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Float => stream.ReadFloat(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Double => stream.ReadDouble(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Decimal => stream.ReadDecimal(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.BigInteger => stream.ReadBigInteger(version, buffer ?? throw new ArgumentNullException(nameof(buffer))).CastNumericValue(requestedType)
                    .CastType<T>(),
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
        public static T? ReadNumericNullable<T>(this Stream stream, in int version, in Memory<byte>? buffer = null)
            where T : struct, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
        {
            NumericTypes requestedType = typeof(T).GetNumericType();
            if (requestedType == NumericTypes.None) throw new ArgumentException($"Unsupported type {typeof(T)}", nameof(T));
            NumericTypes type = (NumericTypes)stream.ReadOneByte(version);
            if (type == NumericTypes.None) return new Nullable<T>();
            if (type.HasValue()) return type.GetValue().CastNumericValue(requestedType).CastType<T>();
            return type switch
            {
                NumericTypes.SByte => stream.ReadByte().CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Byte => stream.ReadByte().CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Short => stream.ReadShort(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.UShort => stream.ReadUShort(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Int => stream.ReadInteger(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.UInt => stream.ReadUInteger(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Long => stream.ReadLong(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.ULong => stream.ReadULong(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Int128 => stream.ReadInt128(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.UInt128 => stream.ReadUInt128(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Half => stream.ReadHalf(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Float => stream.ReadFloat(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Double => stream.ReadDouble(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Decimal => stream.ReadDecimal(version).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.BigInteger => stream.ReadBigInteger(version, buffer ?? throw new ArgumentNullException(nameof(buffer))).CastNumericValue(requestedType)
                    .CastType<T>(),
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
            where T : struct, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
        {
            NumericTypes requestedType = typeof(T).GetNumericType();
            if (requestedType == NumericTypes.None) throw new ArgumentException($"Unsupported type {typeof(T)}", nameof(T));
            NumericTypes type = (NumericTypes)await stream.ReadOneByteAsync(version, buffer, cancellationToken).DynamicContext();
            if (type.HasValue()) return type.GetValue().CastNumericValue(requestedType).CastType<T>();
            return type switch
            {
                NumericTypes.SByte => (await stream.ReadOneSByteAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Byte => (await stream.ReadOneByteAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Short => (await stream.ReadShortAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.UShort => (await stream.ReadUShortAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Int => (await stream.ReadIntegerAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.UInt => (await stream.ReadUIntegerAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Long => (await stream.ReadLongAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.ULong => (await stream.ReadULongAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Int128 => (await stream.ReadInt128Async(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.UInt128 => (await stream.ReadUInt128Async(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Half => (await stream.ReadHalfAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Float => (await stream.ReadFloatAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Double => (await stream.ReadDoubleAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Decimal => (await stream.ReadDecimalAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.BigInteger => (await stream.ReadBigIntegerAsync(version, buffer ?? throw new ArgumentNullException(nameof(buffer)), cancellationToken)
                    .DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
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
            where T : struct, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
        {
            NumericTypes requestedType = typeof(T).GetNumericType();
            if (requestedType == NumericTypes.None) throw new ArgumentException($"Unsupported type {typeof(T)}", nameof(T));
            NumericTypes type = (NumericTypes)await stream.ReadOneByteAsync(version, buffer, cancellationToken).DynamicContext();
            if (type == NumericTypes.None) return new Nullable<T>();
            if (type.HasValue()) return type.GetValue().CastNumericValue(requestedType).CastType<T>();
            return type switch
            {
                NumericTypes.SByte => (await stream.ReadOneSByteAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Byte => (await stream.ReadOneByteAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Short => (await stream.ReadShortAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.UShort => (await stream.ReadUShortAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Int => (await stream.ReadIntegerAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.UInt => (await stream.ReadUIntegerAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Long => (await stream.ReadLongAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.ULong => (await stream.ReadULongAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Int128 => (await stream.ReadInt128Async(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.UInt128 => (await stream.ReadUInt128Async(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Half => (await stream.ReadHalfAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Float => (await stream.ReadFloatAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Double => (await stream.ReadDoubleAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.Decimal => (await stream.ReadDecimalAsync(version, buffer, cancellationToken).DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                NumericTypes.BigInteger => (await stream.ReadBigIntegerAsync(version, buffer ?? throw new ArgumentNullException(nameof(buffer)), cancellationToken)
                    .DynamicContext()).CastNumericValue(requestedType).CastType<T>(),
                _ => throw new InvalidDataException($"Unsupported numeric type #{(byte)type}")
            };
        }
    }
}
