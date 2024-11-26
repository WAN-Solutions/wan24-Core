using System.Numerics;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Read
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Read one signed byte
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
        public static sbyte ReadOneSByte(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(sbyte), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(sbyte)];
#endif
            stream.ReadExactly(bufferSpan);
            return (sbyte)bufferSpan[0];
        }

        /// <summary>
        /// Read one signed byte
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<sbyte> ReadOneSByteAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(sbyte), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(sbyte)] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return (sbyte)bufferMem.Span[0];
        }

        /// <summary>
        /// Read one unsigned byte
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
        public static byte ReadOneByte(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(byte), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(byte)];
#endif
            stream.ReadExactly(bufferSpan);
            return bufferSpan[0];
        }

        /// <summary>
        /// Read one unsigned byte
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<byte> ReadOneByteAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(byte), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(byte)] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return bufferMem.Span[0];
        }

        /// <summary>
        /// Read a signed short
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static short ReadShort(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(short), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(short)];
#endif
            stream.ReadExactly(bufferSpan);
            return bufferSpan.ToShort();
        }

        /// <summary>
        /// Read a signed short
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<short> ReadShortAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(short), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(short)] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return bufferMem.Span.ToShort();
        }

        /// <summary>
        /// Read an unsigned short
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static ushort ReadUShort(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(ushort), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(ushort)];
#endif
            stream.ReadExactly(bufferSpan);
            return bufferSpan.ToUShort();
        }

        /// <summary>
        /// Read an unsigned short
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<ushort> ReadUShortAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(ushort), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(ushort)] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return bufferMem.Span.ToUShort();
        }

        /// <summary>
        /// Read a signed integer
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static int ReadInteger(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(int), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(int)];
#endif
            stream.ReadExactly(bufferSpan);
            return bufferSpan.ToInt();
        }

        /// <summary>
        /// Read a signed integer
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<int> ReadIntegerAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(int), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(int)] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return bufferMem.Span.ToInt();
        }

        /// <summary>
        /// Read an unsigned integer
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static uint ReadUInteger(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(uint), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(uint)];
#endif
            stream.ReadExactly(bufferSpan);
            return bufferSpan.ToUInt();
        }

        /// <summary>
        /// Read an unsigned integer
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<uint> ReadUIntegerAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(uint), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(uint)] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return bufferMem.Span.ToUInt();
        }

        /// <summary>
        /// Read a signed long
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static long ReadLong(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(long), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(long)];
#endif
            stream.ReadExactly(bufferSpan);
            return bufferSpan.ToLong();
        }

        /// <summary>
        /// Read a signed long
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<long> ReadLongAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(long), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(long)] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return bufferMem.Span.ToLong();
        }

        /// <summary>
        /// Read an unsigned long
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static ulong ReadULong(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(ulong), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(ulong)];
#endif
            stream.ReadExactly(bufferSpan);
            return bufferSpan.ToULong();
        }

        /// <summary>
        /// Read an unsigned long
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<ulong> ReadULongAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(ulong), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(ulong)] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return bufferMem.Span.ToULong();
        }

        /// <summary>
        /// Read a signed Int128
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static Int128 ReadInt128(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(ulong) << 1, clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(ulong) << 1];
#endif
            stream.ReadExactly(bufferSpan);
            return bufferSpan.ToInt128();
        }

        /// <summary>
        /// Read a signed Int128
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<Int128> ReadInt128Async(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(ulong) << 1, clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..(sizeof(ulong) << 1)] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return bufferMem.Span.ToInt128();
        }

        /// <summary>
        /// Read an unsigned UInt128
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static UInt128 ReadUInt128(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(ulong) << 1, clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(ulong) << 1];
#endif
            stream.ReadExactly(bufferSpan);
            return bufferSpan.ToUInt128();
        }

        /// <summary>
        /// Read an unsigned UInt128
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<UInt128> ReadUInt128Async(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(ulong) << 1, clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..(sizeof(ulong) << 1)] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return bufferMem.Span.ToUInt128();
        }

        /// <summary>
        /// Read a half
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static Half ReadHalf(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: 2, clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[2];
#endif
            stream.ReadExactly(bufferSpan);
            return bufferSpan.ToHalf();
        }

        /// <summary>
        /// Read a half
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<Half> ReadHalfAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: 2, clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..2] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return bufferMem.Span.ToHalf();
        }

        /// <summary>
        /// Read a float
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static float ReadFloat(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(float), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(float)];
#endif
            stream.ReadExactly(bufferSpan);
            return bufferSpan.ToFloat();
        }

        /// <summary>
        /// Read a float
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<float> ReadFloatAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(float), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(float)] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return bufferMem.Span.ToFloat();
        }

        /// <summary>
        /// Read a double
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static double ReadDouble(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(double), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(double)];
#endif
            stream.ReadExactly(bufferSpan);
            return bufferSpan.ToDouble();
        }

        /// <summary>
        /// Read a double
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<double> ReadDoubleAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(double), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(double)] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return bufferMem.Span.ToDouble();
        }

        /// <summary>
        /// Read a decimal
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Value</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static decimal ReadDecimal(this Stream stream, int version)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(decimal), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(decimal)];
#endif
            stream.ReadExactly(bufferSpan);
            return bufferSpan.ToDecimal();
        }

        /// <summary>
        /// Read a decimal
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<decimal> ReadDecimalAsync(this Stream stream, int version, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(decimal), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(decimal)] : buffer2!.Value.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return bufferMem.Span.ToDecimal();
        }

        /// <summary>
        /// Read a big integer
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer (limits the maximum red data structure length)</param>
        /// <returns>Value</returns>
        public static BigInteger ReadBigInteger(this Stream stream, in int version, in Memory<byte> buffer)
        {
            stream.ReadDataWithLengthInfo(version, buffer.Span);
            return new(buffer.Span);
        }

        /// <summary>
        /// Read a big integer
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="buffer">Buffer (limits the maximum red data structure length)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public static async Task<BigInteger> ReadBigIntegerAsync(this Stream stream, int version, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await stream.ReadDataWithLengthInfoAsync(version, buffer, cancellationToken).DynamicContext();
            return new(buffer.Span);
        }
    }
}
