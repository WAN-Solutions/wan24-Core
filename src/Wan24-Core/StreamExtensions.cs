using System.Buffers;
using System.Collections;
using System.Runtime;

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
        /// <returns>Number of left bytes ('cause the source stream didn't deliver enough data)</returns>
        public static long CopyPartialTo(this Stream stream, Stream target, long count, int? bufferSize = null)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return 0;
            bufferSize ??= Settings.BufferSize;
            bufferSize = (int)Math.Min(count, bufferSize.Value);
            using RentedArray<byte> buffer = new(bufferSize.Value, clean: false);
            for (int red = 1; count != 0 && red != 0; count -= red)
            {
                red = stream.Read(buffer.Span[..(int)Math.Min(count, bufferSize.Value)]);
                if (red != 0) target.Write(buffer.Span[..red]);
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of left bytes ('cause the source stream didn't deliver enough data)</returns>
        public static async Task<long> CopyPartialToAsync(this Stream stream, Stream target, long count, int? bufferSize = null, CancellationToken cancellationToken = default)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return 0;
            bufferSize ??= Settings.BufferSize;
            bufferSize = (int)Math.Min(count, bufferSize.Value);
            using RentedArray<byte> buffer = new(bufferSize.Value, clean: false);
            for (int red = 1; count != 0 && red != 0; count -= red)
            {
                red = await stream.ReadAsync(buffer.Memory[..(int)Math.Min(count, bufferSize.Value)], cancellationToken).DynamicContext();
                if (red != 0) await target.WriteAsync(buffer.Memory[..red], cancellationToken).DynamicContext();
            }
            return count;
        }

        /// <summary>
        /// Seek
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="offset">Offset</param>
        /// <param name="origin">Origin</param>
        /// <returns>Position</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static long GenericSeek(this Stream stream, long offset, SeekOrigin origin) => stream.Position = origin switch
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
        public static void WriteZero(this Stream stream, long count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
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
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
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
        public static void WriteRandom(this Stream stream, long count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
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
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return;
            await RandomStream.Instance.CopyPartialToAsync(stream, count, cancellationToken: cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Generic copy to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="destination">Target</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        public static void GenericCopyTo(this Stream stream, Stream destination, int bufferSize = 81_920)
        {
            if (bufferSize < 1) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            using RentedArray<byte> buffer = new(bufferSize, clean: false);
            for (int red = bufferSize; red == bufferSize;)
            {
                red = stream.Read(buffer.Span);
                if (red != 0) destination.Write(buffer.Span[..red]);
            }
        }

        /// <summary>
        /// Generic copy to another stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="destination">Target</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task GenericCopyToAsync(this Stream stream, Stream destination, int bufferSize = 81_920, CancellationToken cancellationToken = default)
        {
            if (bufferSize < 1) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            using RentedArray<byte> buffer = new(bufferSize, clean: false);
            for (int red = bufferSize; red == bufferSize;)
            {
                red = await stream.ReadAsync(buffer.Memory, cancellationToken).DynamicContext();
                if (red != 0) await destination.WriteAsync(buffer.Memory[..red], cancellationToken).DynamicContext();
            }
        }

        /// <summary>
        /// Create stream chunks
        /// </summary>
        /// <param name="stream">Stream (will be chunked from position <c>0</c>)</param>
        /// <param name="chunkSize">Chunk size in bytes</param>
        /// <returns>Chunks (should be used directly, and the yielded partial chunk streams, too (their position will change as soon as a new stream was yielded)!)</returns>
        public static IEnumerable<StreamBase> Chunk(this Stream stream, long chunkSize)
        {
            if (chunkSize < 1) throw new ArgumentOutOfRangeException(nameof(chunkSize));
            return new StreamChunkEnumerator(stream, chunkSize);
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
            internal StreamChunkEnumerator(Stream stream, long chunkSize) : base(asyncDisposing: false)
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
