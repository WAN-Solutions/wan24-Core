using System.Collections;

namespace wan24.Core
{
    // Stream chunk enumerator
    public static partial class StreamExtensions
    {
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
        /// Stream chunk enumerator
        /// </summary>
        internal sealed class StreamChunkEnumerator : BasicDisposableBase, IEnumerable<StreamBase>, IEnumerator<StreamBase>
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
            internal StreamChunkEnumerator(in Stream stream, in long chunkSize) : base()
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
