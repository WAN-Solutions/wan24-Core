using System.Numerics;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Numeric
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Write a signed byte
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        public static void Write(this Stream stream, in sbyte value) => stream.WriteByte((byte)value);

        /// <summary>
        /// Write a signed byte
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, sbyte value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(sbyte), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(sbyte)] : buffer2!.Value.Memory;
            bufferMem.Span[0] = (byte)value;
            await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write an unsigned byte
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        public static void Write(this Stream stream, in byte value) => stream.WriteByte(value);

        /// <summary>
        /// Write an unsigned byte
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, byte value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(byte), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(byte)] : buffer2!.Value.Memory;
            bufferMem.Span[0] = value;
            await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a signed short
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void Write(this Stream stream, in short value)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(short), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(short)];
#endif
            value.GetBytes(bufferSpan);
            stream.Write(bufferSpan);
        }

        /// <summary>
        /// Write a signed short
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, short value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(short), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(short)] : buffer2!.Value.Memory;
            value.GetBytes(bufferMem.Span);
            await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write an unsigned short
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void Write(this Stream stream, in ushort value)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(ushort), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(ushort)];
#endif
            value.GetBytes(bufferSpan);
            stream.Write(bufferSpan);
        }

        /// <summary>
        /// Write an unsigned short
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, ushort value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(ushort), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(ushort)] : buffer2!.Value.Memory;
            value.GetBytes(bufferMem.Span);
            await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a signed integer
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void Write(this Stream stream, in int value)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(int), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(int)];
#endif
            value.GetBytes(bufferSpan);
            stream.Write(bufferSpan);
        }

        /// <summary>
        /// Write a signed integer
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, int value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(int), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(int)] : buffer2!.Value.Memory;
            value.GetBytes(bufferMem.Span);
            await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write an unsigned integer
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void Write(this Stream stream, in uint value)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(uint), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(uint)];
#endif
            value.GetBytes(bufferSpan);
            stream.Write(bufferSpan);
        }

        /// <summary>
        /// Write an unsigned integer
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, uint value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(uint), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(uint)] : buffer2!.Value.Memory;
            value.GetBytes(bufferMem.Span);
            await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a signed long
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void Write(this Stream stream, in long value)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(long), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(long)];
#endif
            value.GetBytes(bufferSpan);
            stream.Write(bufferSpan);
        }

        /// <summary>
        /// Write a signed long
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, long value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(long), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(long)] : buffer2!.Value.Memory;
            value.GetBytes(bufferMem.Span);
            await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write an unsigned long
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void Write(this Stream stream, in ulong value)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(ulong), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(ulong)];
#endif
            value.GetBytes(bufferSpan);
            stream.Write(bufferSpan);
        }

        /// <summary>
        /// Write an unsigned long
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, ulong value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(ulong), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(ulong)] : buffer2!.Value.Memory;
            value.GetBytes(bufferMem.Span);
            await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a half
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void Write(this Stream stream, in Half value)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: 2, clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[2];
#endif
            value.GetBytes(bufferSpan);
            stream.Write(bufferSpan);
        }

        /// <summary>
        /// Write a half
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, Half value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: 2, clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..2] : buffer2!.Value.Memory;
            value.GetBytes(bufferMem.Span);
            await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a float
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void Write(this Stream stream, in float value)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(float), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(float)];
#endif
            value.GetBytes(bufferSpan);
            stream.Write(bufferSpan);
        }

        /// <summary>
        /// Write a float
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, float value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(float), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(float)] : buffer2!.Value.Memory;
            value.GetBytes(bufferMem.Span);
            await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a double
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void Write(this Stream stream, in double value)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(double), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(double)];
#endif
            value.GetBytes(bufferSpan);
            stream.Write(bufferSpan);
        }

        /// <summary>
        /// Write a double
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, double value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(double), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(double)] : buffer2!.Value.Memory;
            value.GetBytes(bufferMem.Span);
            await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a decimal
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void Write(this Stream stream, in decimal value)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: sizeof(decimal), clean: false);
            Span<byte> bufferSpan = buffer.Span;
#else
            Span<byte> bufferSpan = stackalloc byte[sizeof(decimal)];
#endif
            value.GetBytes(bufferSpan);
            stream.Write(bufferSpan);
        }

        /// <summary>
        /// Write a decimal
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, decimal value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(len: sizeof(decimal), clean: false);
            Memory<byte> bufferMem = buffer.HasValue ? buffer.Value[..sizeof(decimal)] : buffer2!.Value.Memory;
            value.GetBytes(bufferMem.Span);
            await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write a big integer
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        public static void Write(this Stream stream, in BigInteger value)
        {
            using RentedMemoryRef<byte> buffer = new(value.GetByteCount(), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            if (!value.TryWriteBytes(bufferSpan, out int len)) throw new InvalidProgramException("Failed to write the big integer into a buffer");
            stream.WriteWithLengthInfo(bufferSpan[..len]);
        }

        /// <summary>
        /// Write a big integer
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, BigInteger value, Memory<byte>? buffer = null, CancellationToken cancellationToken = default)
        {
            using RentedMemory<byte>? buffer2 = buffer.HasValue ? null : new(value.GetByteCount(), clean: false);
            Memory<byte> bufferMem = buffer ?? buffer2!.Value.Memory;
            if (!value.TryWriteBytes(bufferMem.Span, out int len)) throw new OutOfMemoryException("Buffer too small");
            await stream.WriteWithLengthInfoAsync(bufferMem[..len], cancellationToken).DynamicContext();
        }
    }
}
