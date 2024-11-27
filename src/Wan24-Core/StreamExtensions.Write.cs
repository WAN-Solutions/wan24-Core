using System.Buffers;

namespace wan24.Core
{
    // Write
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Write zero
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes</param>
        public static void WriteZero(this Stream stream, in long count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return;
            using ZeroStream zero = new();
            zero.SetLength(count);
            zero.CopyTo(stream);
        }

        /// <summary>
        /// Write zero
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteZeroAsync(this Stream stream, long count, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return;
            using ZeroStream zero = new();
            zero.SetLength(count);
            await zero.CopyToAsync(stream, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write random bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes</param>
        public static void WriteRandom(this Stream stream, in long count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return;
            RandomStream.Instance.CopyPartialTo(stream, count);
        }

        /// <summary>
        /// Write random bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteRandomAsync(this Stream stream, long count, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return;
            await RandomStream.Instance.CopyPartialToAsync(stream, count, cancellationToken: cancellationToken).DynamicContext();
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
        public static void WriteNullableWithLengthInfo(this Stream stream, in Stream? other)
        {
            if(other is null)
            {
                stream.Write((long)-1);
            }
            else
            {
                stream.WriteWithLengthInfo(other);
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
        /// Write an even not seekable stream with length info (one stream must be seekable)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="other">Source stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteNullableWithLengthInfoAsync(this Stream stream, Stream? other, CancellationToken cancellationToken = default)
        {
            if(other is null)
            {
                await stream.WriteAsync((long)-1, cancellationToken: cancellationToken).DynamicContext();
            }
            else
            {
                await stream.WriteWithLengthInfoAsync(other, cancellationToken).DynamicContext();
            }
        }

        /// <summary>
        /// Write with length info
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        public static void WriteWithLengthInfo(this Stream stream, in ReadOnlySpan<byte> data)
        {
            stream.WriteNumeric(data.Length);
            stream.Write(data);
        }

        /// <summary>
        /// Write with length info
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        public static void WriteNullableWithLengthInfo(this Stream stream, in ReadOnlyMemory<byte>? data)
        {
            if (data.HasValue)
            {
                stream.WriteWithLengthInfo(data.Value.Span);
            }
            else
            {
                stream.Write((byte)NumericTypes.None);
            }
        }

        /// <summary>
        /// Write with length info (can be red using <see cref="ReadStreamWithLengthInfo"/>)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of written bytes including length information (4 bytes)</returns>
        public static async Task<long> WriteWithLengthInfoAsync(this Stream stream, ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            await stream.WriteNumericAsync(data.Length, cancellationToken: cancellationToken).DynamicContext();
            await stream.WriteAsync(data, cancellationToken).DynamicContext();
            return data.Length + sizeof(int);
        }

        /// <summary>
        /// Write with length info (can be red using <see cref="ReadStreamNullableWithLengthInfo"/>)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteNullableWithLengthInfoAsync(this Stream stream, ReadOnlyMemory<byte>? data, CancellationToken cancellationToken = default)
        {
            if (data.HasValue)
            {
                await stream.WriteWithLengthInfoAsync(data.Value, cancellationToken).DynamicContext();
            }
            else
            {
                await stream.WriteAsync((byte)NumericTypes.None, cancellationToken: cancellationToken).DynamicContext();
            }
        }

        /// <summary>
        /// Write with length info (can be red using <see cref="ReadStreamWithLengthInfoAsync"/>)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        public static void WriteWithLengthInfo(this Stream stream, in ReadOnlySequence<byte> data)
        {
            stream.WriteNumeric(data.Length);
            using MemorySequenceStream ms = new(data);
            ms.CopyTo(stream);
        }

        /// <summary>
        /// Write with length info (can be red using <see cref="ReadStreamNullableWithLengthInfoAsync"/>)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        public static void WriteNullableWithLengthInfo(this Stream stream, in ReadOnlySequence<byte>? data)
        {
            if (data.HasValue)
            {
                stream.WriteWithLengthInfo(data.Value);
            }
            else
            {
                stream.Write((byte)NumericTypes.None);
            }
        }

        /// <summary>
        /// Write with length info
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteWithLengthInfoAsync(this Stream stream, ReadOnlySequence<byte> data, CancellationToken cancellationToken = default)
        {
            await stream.WriteNumericAsync(data.Length, cancellationToken: cancellationToken).DynamicContext();
            using MemorySequenceStream ms = new(data);
            await ms.CopyToAsync(stream, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Write with length info
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteNullableWithLengthInfoAsync(this Stream stream, ReadOnlySequence<byte>? data, CancellationToken cancellationToken = default)
        {
            if (data.HasValue)
            {
                await stream.WriteWithLengthInfoAsync(data.Value, cancellationToken).DynamicContext();
            }
            else
            {
                await stream.WriteAsync((byte)NumericTypes.None, cancellationToken: cancellationToken).DynamicContext();
            }
        }

        /// <summary>
        /// Write a stream with length information or chunked
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="other">Source stream</param>
        public static void Write(this Stream stream, in Stream other)
        {
            if (stream.CanSeek || other.CanSeek)
            {
                stream.WriteByte(0);
                stream.WriteWithLengthInfo(other);
            }
            else
            {
                stream.WriteByte(1);
                using CutStream target = new(stream, leaveOpen: true);
                using ChunkStream chunker = ChunkStream.CreateNew(target, Settings.BufferSize, leaveOpen: true);
                other.CopyTo(chunker);
                chunker.FlushFinalChunk();
            }
        }

        /// <summary>
        /// Write a stream with length information or chunked
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="other">Source stream</param>
        public static void WriteNullable(this Stream stream, in Stream? other)
        {
            if (other is null)
            {
                stream.Write((byte)2);
            }
            else
            {
                stream.Write(other);
            }
        }

        /// <summary>
        /// Write a stream with length information or chunked
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="other">Source stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, Stream other, CancellationToken cancellationToken = default)
        {
            if (stream.CanSeek || other.CanSeek)
            {
                await stream.WriteAsync((byte)0, cancellationToken: cancellationToken).DynamicContext();
                await stream.WriteWithLengthInfoAsync(other, cancellationToken).DynamicContext();
            }
            else
            {
                stream.WriteByte(1);
                using CutStream target = new(stream, leaveOpen: true);
                using ChunkStream chunker = await ChunkStream.CreateNewAsync(target, Settings.BufferSize, leaveOpen: true, cancellationToken: cancellationToken)
                    .DynamicContext();
                await other.CopyToAsync(chunker, cancellationToken).DynamicContext();
                await chunker.FlushFinalChunkAsync(cancellationToken).DynamicContext();
            }
        }

        /// <summary>
        /// Write a stream with length information or chunked
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="other">Source stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteNullableAsync(this Stream stream, Stream? other, CancellationToken cancellationToken = default)
        {
            if(other is null)
            {
                await stream.WriteAsync((byte)2, cancellationToken: cancellationToken).DynamicContext();
            }
            else
            {
                await stream.WriteAsync(other, cancellationToken).DynamicContext();
            }
        }
    }
}
