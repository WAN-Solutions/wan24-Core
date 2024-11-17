using System.Buffers;
using System.Runtime;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="Stream"/> extensions
    /// </summary>
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Get the number of remaining bytes until the streams end
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Remaining number of bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static long GetRemainingBytes(this Stream stream) => stream.Length - stream.Position;

        /// <summary>
        /// Seek
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="offset">Offset</param>
        /// <param name="origin">Origin</param>
        /// <returns>Position</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static long GenericSeek(this Stream stream, in long offset, in SeekOrigin origin) => stream.Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => stream.Position + offset,
            SeekOrigin.End => stream.Length + offset,
            _ => throw new ArgumentException("Invalid seek origin", nameof(origin))
        };

        /// <summary>
        /// Generic read byte
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Byte or <c>-1</c>, if read failed</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static int GenericReadByte(this Stream stream)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: 1, clean: false);
            return stream.Read(buffer.Span) == 0 ? -1 : buffer.Span[0];
#else
            Span<byte> buffer = stackalloc byte[1];
            return stream.Read(buffer) == 0 ? -1 : buffer[0];
#endif
        }

        /// <summary>
        /// Generic write byte
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void GenericWriteByte(this Stream stream, in byte value)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: 1);
            Span<byte> bufferSpan = buffer.Span;
            bufferSpan[0] = value;
            stream.Write(bufferSpan);
#else
            Span<byte> buffer = [value];
            stream.Write(buffer);
#endif
        }

        /// <summary>
        /// Write an even not seekable stream with length info (one stream must be seekable)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="other">Source stream</param>
        /// <returns>Number of written bytes including length information (8 bytes)</returns>
        public static long WriteWithLengthInfo(this Stream stream, in Stream other)
        {
            bool streamCanSeek = stream.CanSeek,
                otherCanSeek = other.CanSeek;
            if (!streamCanSeek && !otherCanSeek) throw new ArgumentException("One stream must be seekable", nameof(other));
            if (streamCanSeek && !otherCanSeek)
            {
                using RentedMemoryRef<byte> buffer = new(len: sizeof(long));
                Span<byte> bufferSpan = buffer.Span;
                long pos = stream.Position;
                stream.Write(bufferSpan);
                other.CopyTo(stream);
                long end = stream.Position,
                    res = end - pos;
                stream.Position = pos;
                res.GetBytes(bufferSpan);
                stream.Write(bufferSpan);
                stream.Position = end;
                return res;
            }
            else
            {
                long res = other.GetRemainingBytes();
                using (RentedMemoryRef<byte> buffer = new(len: sizeof(long), clean: false))
                {
                    Span<byte> bufferSpan = buffer.Span;
                    res.GetBytes(bufferSpan);
                    stream.Write(bufferSpan);
                }
                other.CopyExactlyPartialTo(stream, res);
                return res;
            }
        }

        /// <summary>
        /// Write an even not seekable stream with length info (one stream must be seekable)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="other">Source stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of written bytes including length information (8 bytes)</returns>
        public static async Task<long> WriteWithLengthInfoAsync(this Stream stream, Stream other, CancellationToken cancellationToken = default)
        {
            bool streamCanSeek = stream.CanSeek,
                otherCanSeek = other.CanSeek;
            if (!streamCanSeek && !otherCanSeek) throw new ArgumentException("One stream must be seekable", nameof(other));
            if (streamCanSeek && !otherCanSeek)
            {
                using RentedMemory<byte> buffer = new(len: sizeof(long));
                Memory<byte> bufferMem = buffer.Memory;
                long pos = stream.Position;
                await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
                await other.CopyToAsync(stream, cancellationToken).DynamicContext();
                long end = stream.Position,
                    res = end - pos;
                stream.Position = pos;
                res.GetBytes(bufferMem.Span);
                await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
                stream.Position = end;
                return res;
            }
            else
            {
                long res = other.GetRemainingBytes();
                using (RentedMemory<byte> buffer = new(len: sizeof(long), clean: false))
                {
                    Memory<byte> bufferMem = buffer.Memory;
                    res.GetBytes(bufferMem.Span);
                    await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
                }
                await other.CopyExactlyPartialToAsync(stream, res, cancellationToken: cancellationToken).DynamicContext();
                return res;
            }
        }

        /// <summary>
        /// Write with length info
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <returns>Number of written bytes including length information (4 bytes)</returns>
        public static long WriteWithLengthInfo(this Stream stream, in ReadOnlySpan<byte> data)
        {
            using (RentedMemoryRef<byte> buffer = new(len: sizeof(int), clean: false))
            {
                Span<byte> bufferSpan = buffer.Span;
                data.Length.GetBytes(bufferSpan);
                stream.Write(bufferSpan);
            }
            stream.Write(data);
            return data.Length + sizeof(int);
        }

        /// <summary>
        /// Write with length info (can be red using <see cref="ReadStreamWithLengthInfo{T}(T)"/>)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of written bytes including length information (4 bytes)</returns>
        public static async Task<long> WriteWithLengthInfoAsync(this Stream stream, ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            using (RentedMemory<byte> buffer = new(len: sizeof(int), clean: false))
            {
                Memory<byte> bufferMem = buffer.Memory;
                data.Length.GetBytes(bufferMem.Span);
                await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
            }
            await stream.WriteAsync(data, cancellationToken).DynamicContext();
            return data.Length + sizeof(int);
        }

        /// <summary>
        /// Write with length info (can be red using <see cref="ReadStreamWithLengthInfoAsync{T}(T, CancellationToken)"/>)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <returns>Number of written bytes including length information (8 bytes)</returns>
        public static long WriteWithLengthInfo(this Stream stream, in ReadOnlySequence<byte> data)
        {
            using (RentedMemoryRef<byte> buffer = new(len: sizeof(long), clean: false))
            {
                Span<byte> bufferSpan = buffer.Span;
                data.Length.GetBytes(bufferSpan);
                stream.Write(bufferSpan);
            }
            using (MemorySequenceStream ms = new(data))
                ms.CopyTo(stream);
            return data.Length + sizeof(long);
        }

        /// <summary>
        /// Write with length info
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of written bytes including length information (8 bytes)</returns>
        public static async Task<long> WriteWithLengthInfoAsync(this Stream stream, ReadOnlySequence<byte> data, CancellationToken cancellationToken = default)
        {
            using (RentedMemory<byte> buffer = new(len: sizeof(long), clean: false))
            {
                Memory<byte> bufferMem = buffer.Memory;
                data.Length.GetBytes(bufferMem.Span);
                await stream.WriteAsync(bufferMem, cancellationToken).DynamicContext();
            }
            using (MemorySequenceStream ms = new(data))
                await ms.CopyToAsync(stream, cancellationToken).DynamicContext();
            return data.Length + sizeof(long);
        }

        /// <summary>
        /// Read a stream with length information (8 byte)
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <returns>Partial stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static PartialStream<T> ReadStreamWithLengthInfo<T>(this T stream) where T : Stream
        {
            using RentedMemoryRef<byte> buffer = new(len: sizeof(long));
            Span<byte> bufferSpan = buffer.Span;
            stream.ReadExactly(bufferSpan);
            return new(stream, bufferSpan.ToLong(), leaveOpen: true);
        }

        /// <summary>
        /// Read a stream with length information (8 byte)
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Partial stream (must be fully red before reading more from <c>stream</c>)</returns>
        public static async Task<PartialStream<T>> ReadStreamWithLengthInfoAsync<T>(this T stream, CancellationToken cancellationToken = default) where T : Stream
        {
            using RentedMemory<byte> buffer = new(len: sizeof(long));
            Memory<byte> bufferMem = buffer.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            return new(stream, bufferMem.Span.ToLong(), leaveOpen: true);
        }

        /// <summary>
        /// Read data with length information (4 byte)
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of red bytes into the buffer</returns>
        public static int ReadDataWithLengthInfo<T>(this T stream, Span<byte> buffer) where T : Stream
        {
            using RentedMemoryRef<byte> buffer2 = new(len: sizeof(int));
            Span<byte> bufferSpan = buffer2.Span;
            stream.ReadExactly(bufferSpan);
            int res = bufferSpan.ToInt();
            stream.ReadExactly(buffer[..res]);
            return res;
        }

        /// <summary>
        /// Read data with length information (4 byte)
        /// </summary>
        /// <typeparam name="T">Source stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of red bytes into the buffer</returns>
        public static async Task<int> ReadDataWithLengthInfoAsync<T>(
            this T stream, 
            Memory<byte> buffer,
            CancellationToken cancellationToken = default
            )
            where T : Stream
        {
            using RentedMemory<byte> buffer2 = new(len: sizeof(int));
            Memory<byte> bufferMem = buffer2.Memory;
            await stream.ReadExactlyAsync(bufferMem, cancellationToken).DynamicContext();
            int res = bufferMem.Span.ToInt();
            await stream.ReadExactlyAsync(buffer[..res], cancellationToken).DynamicContext();
            return res;
        }
    }
}
