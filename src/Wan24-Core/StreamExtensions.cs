using System.Collections;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="Stream"/> extensions
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Get the number of remaining bytes until the streams end
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Remaining number of bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static long GetRemainingBytes(this Stream stream) => stream.Length - stream.Position;

        /// <summary>
        /// Copy partial to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="target">Target stream</param>
        /// <param name="count">Number of bytes to copy</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="progress">Progress</param>
        /// <returns>Number of left bytes ('cause the source stream didn't deliver enough data)</returns>
        public static long CopyPartialTo(this Stream stream, in Stream target, long count, int? bufferSize = null, ProcessingProgress? progress = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return 0;
            bufferSize ??= Settings.BufferSize;
            bufferSize = (int)Math.Min(count, bufferSize.Value);
            using RentedArrayRefStruct<byte> buffer = new(bufferSize.Value, clean: false);
            for (int red = 1; count > 0 && red > 0; count -= red)
            {
                red = stream.Read(buffer.Span[..(int)Math.Min(count, bufferSize.Value)]);
                if (red != 0)
                {
                    target.Write(buffer.Span[..red]);
                    progress?.Update(red);
                }
            }
            return count;
        }

        /// <summary>
        /// Copy partial to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="target">Target stream</param>
        /// <param name="count">Number of bytes to copy</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="progress">Progress</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of left bytes ('cause the source stream didn't deliver enough data)</returns>
        public static async Task<long> CopyPartialToAsync(
            this Stream stream, 
            Stream target, 
            long count, 
            int? bufferSize = null, 
            ProcessingProgress? progress = null, 
            CancellationToken cancellationToken = default
            )
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return 0;
            if (progress is not null) cancellationToken = progress.GetCancellationToken(cancellationToken);
            bufferSize ??= Settings.BufferSize;
            bufferSize = (int)Math.Min(count, bufferSize.Value);
            using RentedArrayStructSimple<byte> buffer = new(bufferSize.Value, clean: false);
            for (int red = 1; count > 0 && red > 0; count -= red)
            {
                red = await stream.ReadAsync(buffer.Memory[..(int)Math.Min(count, bufferSize.Value)], cancellationToken).DynamicContext();
                if (red != 0)
                {
                    await target.WriteAsync(buffer.Memory[..red], cancellationToken).DynamicContext();
                    progress?.Update(red);
                }
            }
            return count;
        }

        /// <summary>
        /// Copy partial to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="target">Target stream</param>
        /// <param name="count">Number of bytes to copy</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="progress">Progress</param>
        public static void CopyExactlyPartialTo(this Stream stream, in Stream target, long count, int? bufferSize = null, ProcessingProgress? progress = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return;
            bufferSize ??= Settings.BufferSize;
            bufferSize = (int)Math.Min(count, bufferSize.Value);
            using RentedArrayRefStruct<byte> buffer = new(bufferSize.Value, clean: false);
            for (int red = 1; count > 0 && red > 0; count -= red)
            {
                red = stream.Read(buffer.Span[..(int)Math.Min(count, bufferSize.Value)]);
                if (red != 0)
                {
                    target.Write(buffer.Span[..red]);
                    progress?.Update(red);
                }
            }
            if (count != 0) throw new IOException("Not enough data");
        }

        /// <summary>
        /// Copy partial to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="target">Target stream</param>
        /// <param name="count">Number of bytes to copy</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="progress">Progress</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task CopyExactlyPartialToAsync(
            this Stream stream,
            Stream target,
            long count,
            int? bufferSize = null,
            ProcessingProgress? progress = null,
            CancellationToken cancellationToken = default
            )
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0) return;
            if (progress is not null) cancellationToken = progress.GetCancellationToken(cancellationToken);
            bufferSize ??= Settings.BufferSize;
            bufferSize = (int)Math.Min(count, bufferSize.Value);
            using RentedArrayStructSimple<byte> buffer = new(bufferSize.Value, clean: false);
            for (int red = 1; count > 0 && red > 0; count -= red)
            {
                red = await stream.ReadAsync(buffer.Memory[..(int)Math.Min(count, bufferSize.Value)], cancellationToken).DynamicContext();
                if (red != 0)
                {
                    await target.WriteAsync(buffer.Memory[..red], cancellationToken).DynamicContext();
                    progress?.Update(red);
                }
            }
            if (count != 0) throw new IOException("Not enough data");
        }

        /// <summary>
        /// Seek
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="offset">Offset</param>
        /// <param name="origin">Origin</param>
        /// <returns>Position</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static long GenericSeek(this Stream stream, in long offset, in SeekOrigin origin) => stream.Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => stream.Position + offset,
            SeekOrigin.End => stream.Length + offset,
            _ => throw new ArgumentException("Invalid seek origin", nameof(origin))
        };

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
        /// Generic copy to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="destination">Target</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="progress">Progress</param>
        public static void GenericCopyTo(this Stream stream, in Stream destination, in int bufferSize = 81_920, ProcessingProgress? progress = null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, 1);
            using RentedArrayRefStruct<byte> buffer = new(bufferSize, clean: false);
            for (int red = bufferSize; red == bufferSize;)
            {
                red = stream.Read(buffer.Span);
                if (red != 0)
                {
                    destination.Write(buffer.Span[..red]);
                    progress?.Update(red);
                }
            }
        }

        /// <summary>
        /// Generic copy to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="destination">Target</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="progress">Progress</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task GenericCopyToAsync(
            this Stream stream, 
            Stream destination, 
            int bufferSize = 81_920, 
            ProcessingProgress? progress = null, 
            CancellationToken cancellationToken = default
            )
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, 1);
            if (progress is not null) cancellationToken = progress.GetCancellationToken(cancellationToken);
            using RentedArrayStructSimple<byte> buffer = new(bufferSize, clean: false);
            for (int red = bufferSize; red == bufferSize;)
            {
                red = await stream.ReadAsync(buffer.Memory, cancellationToken).DynamicContext();
                if (red != 0)
                {
                    await destination.WriteAsync(buffer.Memory[..red], cancellationToken).DynamicContext();
                    progress?.Update(red);
                }
            }
        }

        /// <summary>
        /// Generic read byte
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Byte or <c>-1</c>, if read failed</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static int GenericReadByte(this Stream stream)
        {
#if NO_UNSAFE
            using RentedArrayRefStruct<byte> buffer = new(len: 1, clean: false);
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
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void GenericWriteByte(this Stream stream, in byte value)
        {
#if NO_UNSAFE
            using RentedArrayRefStruct<byte> buffer = new(len: 1);
            buffer[0] = value;
            stream.Write(buffer.Span);
#else
            Span<byte> buffer = [value];
            stream.Write(buffer);
#endif
        }

        /// <summary>
        /// Read a number of bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes to read</param>
        /// <returns>Red bytes</returns>
        public static byte[] Read(this Stream stream, in int count)
        {
            using RentedArray<byte> buffer = new(count, clean: false);
            int red = stream.Read(buffer.Span);
            if (red < 1) return [];
            byte[] res = new byte[red];
            buffer.Span[..red].CopyTo(res);
            return res;
        }

        /// <summary>
        /// Read a number of bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes to read</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Red bytes</returns>
        public static async Task<byte[]> ReadAsync(this Stream stream, int count, CancellationToken cancellationToken = default)
        {
            using RentedArray<byte> buffer = new(count, clean: false);
            int red = await stream.ReadAsync(buffer.Memory, cancellationToken).DynamicContext();
            if (red < 1) return [];
            byte[] res = new byte[red];
            buffer.Span[..red].CopyTo(res);
            return res;
        }

        /// <summary>
        /// Read an exact number of bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes to read</param>
        /// <returns>Red bytes</returns>
        public static byte[] ReadExactly(this Stream stream, in int count)
        {
            byte[] res = new byte[count];
            stream.ReadExactly(res);
            return res;
        }

        /// <summary>
        /// Read an exact number of bytes
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="count">Number of bytes to read</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Red bytes</returns>
        public static async Task<byte[]> ReadExactlyAsync(this Stream stream, int count, CancellationToken cancellationToken = default)
        {
            byte[] res = new byte[count];
            await stream.ReadExactlyAsync(res, cancellationToken).DynamicContext();
            return res;
        }

        /// <summary>
        /// Create stream chunks
        /// </summary>
        /// <param name="stream">Stream (will be chunked from position <c>0</c>)</param>
        /// <param name="chunkSize">Chunk size in bytes</param>
        /// <returns>Chunks (should be used directly, and the yielded partial chunk streams, too (their position will change as soon as a new stream was yielded)!)</returns>
        public static IEnumerable<StreamBase> Chunk(this Stream stream, in long chunkSize)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1);
            return new StreamChunkEnumerator(stream, chunkSize);
        }

        /// <summary>
        /// Read to the end
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cleanBuffer">If to clean used buffers</param>
        /// <param name="progress">Progress</param>
        /// <returns>Red bytes</returns>
        public static byte[] ReadToEnd(this Stream stream, in bool cleanBuffer = false, in ProcessingProgress? progress = null)
        {
            if (stream.CanSeek)
            {
                long remaining = GetRemainingBytes(stream);
                if (remaining < 1) return [];
                if (remaining > int.MaxValue) throw new OutOfMemoryException();
                if (progress is not null) progress.Total += remaining;
                byte[] res = new byte[remaining];
                stream.ReadExactly(res);
                progress?.Update((int)remaining);
                return res;
            }
            else
            {
                using MemoryPoolStream ms = new()
                {
                    CleanReturned = cleanBuffer
                };
                stream.GenericCopyTo(ms, progress: progress);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Read to the end
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cleanBuffer">If to clean used buffers</param>
        /// <param name="progress">Progress</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Red bytes</returns>
        public static async Task<byte[]> ReadToEndAsync(this Stream stream, bool cleanBuffer = false, ProcessingProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (stream.CanSeek)
            {
                long remaining = GetRemainingBytes(stream);
                if (remaining < 1) return [];
                if (remaining > int.MaxValue) throw new OutOfMemoryException();
                if (progress is not null) progress.Total += remaining;
                byte[] res = new byte[remaining];
                await stream.ReadExactlyAsync(res, cancellationToken).DynamicContext();
                progress?.Update((int)remaining);
                return res;
            }
            else
            {
                using MemoryPoolStream ms = new()
                {
                    CleanReturned = cleanBuffer
                };
                await stream.GenericCopyToAsync(ms, progress: progress, cancellationToken: cancellationToken).DynamicContext();
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Stream chunk enumerator
        /// </summary>
        internal sealed class StreamChunkEnumerator : DisposableBase, IEnumerable<StreamBase>, IEnumerator<StreamBase>
        {
            /// <summary>
            /// Stream
            /// </summary>
            private readonly Stream Stream;
            /// <summary>
            /// Chunk size in bytes
            /// </summary>
            private readonly long ChunkSize;
            /// <summary>
            /// Remaining stream length in bytes
            /// </summary>
            private long Length;
            /// <summary>
            /// Past stream length in bytes
            /// </summary>
            private long Past = 0;
            /// <summary>
            /// Current stream
            /// </summary>
            private StreamBase? _Current = null;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="stream">Stream</param>
            /// <param name="chunkSize">Chunk size in bytes</param>
            internal StreamChunkEnumerator(in Stream stream, in long chunkSize) : base(asyncDisposing: false)
            {
                stream.Position = 0;
                Stream = stream;
                ChunkSize = chunkSize;
                Length = stream.Length;
            }

            /// <inheritdoc/>
            public StreamBase Current
            {
                get
                {
                    EnsureUndisposed();
                    if (_Current is not null) throw new InvalidOperationException();
                    Stream.Position = Past;
                    long len = Math.Min(Length, ChunkSize);
                    _Current = new PartialStream(Stream, len);
                    Length -= len;
                    Past += len;
                    return _Current;
                }
            }

            /// <inheritdoc/>
            object IEnumerator.Current => Current;

            /// <inheritdoc/>
            IEnumerator<StreamBase> IEnumerable<StreamBase>.GetEnumerator() => IfUndisposed(this);

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() => IfUndisposed(this);

            /// <inheritdoc/>
            bool IEnumerator.MoveNext()
            {
                EnsureUndisposed();
                if (Length == 0)
                {
                    Dispose();
                    return false;
                }
                _Current?.Dispose();
                _Current = null;
                return true;
            }

            /// <inheritdoc/>
            void IEnumerator.Reset() => throw new NotSupportedException();

            /// <inheritdoc/>
            protected override void Dispose(bool disposing) => _Current?.Dispose();
        }
    }
}
