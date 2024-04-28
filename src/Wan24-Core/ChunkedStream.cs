using System.Collections.ObjectModel;

namespace wan24.Core
{
    /// <summary>
    /// Chunked stream
    /// </summary>
    public partial class ChunkedStream : StreamBase
    {
        /// <summary>
        /// Chunk stream factory
        /// </summary>
        protected readonly StreamFactory_Delegate? ChunkStreamFactory;
        /// <summary>
        /// Asynchronous chunk stream factory
        /// </summary>
        protected readonly AsyncStreamFactory_Delegate? AsyncChunkStreamFactory;
        /// <summary>
        /// Delete a chunk
        /// </summary>
        protected readonly DeleteChunk_Delegate? DeleteChunk;
        /// <summary>
        /// Delete a chunk
        /// </summary>
        protected readonly AsyncDeleteChunk_Delegate? AsyncDeleteChunk;
        /// <summary>
        /// Can write?
        /// </summary>
        protected readonly bool _CanWrite;
        /// <summary>
        /// Chunk streams
        /// </summary>
        protected readonly Dictionary<int, Stream> ChunkStreams;
        /// <summary>
        /// Modified chunk indexes
        /// </summary>
        protected readonly HashSet<int> _ModifiedChunks = [];
        /// <summary>
        /// Length in bytes
        /// </summary>
        protected long _Length;
        /// <summary>
        /// Position byte offset
        /// </summary>
        protected long _Position = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="writable">Writable?</param>
        /// <param name="chunkSize">Chunk size in bytes</param>
        /// <param name="chunkStreamFactory">Chunk stream factory</param>
        /// <param name="deleteChunk">Delete chunk</param>
        /// <param name="asyncChunkStreamFactory">Asynchronous chunk stream factory</param>
        /// <param name="asyncDeleteChunk">Delete chunk asynchronous</param>
        /// <param name="numberOfChunks">Number of existing chunks</param>
        /// <param name="lastChunkLength">Length of the last chunk in bytes</param>
        public ChunkedStream(
            in bool writable,
            in long chunkSize,
            in StreamFactory_Delegate? chunkStreamFactory,
            in DeleteChunk_Delegate? deleteChunk,
            in AsyncStreamFactory_Delegate? asyncChunkStreamFactory = null,
            in AsyncDeleteChunk_Delegate? asyncDeleteChunk = null,
            in int numberOfChunks = 0,
            in long lastChunkLength = 0
            ) : base()
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1);
            if (chunkStreamFactory is null && asyncChunkStreamFactory is null) throw new ArgumentNullException(nameof(chunkStreamFactory));
            if (deleteChunk is null && asyncDeleteChunk is null) throw new ArgumentNullException(nameof(deleteChunk));
            ArgumentOutOfRangeException.ThrowIfNegative(numberOfChunks);
            ArgumentOutOfRangeException.ThrowIfNegative(lastChunkLength);
            ChunkStreamFactory = chunkStreamFactory;
            AsyncChunkStreamFactory = asyncChunkStreamFactory;
            DeleteChunk = deleteChunk;
            AsyncDeleteChunk = asyncDeleteChunk;
            _CanWrite = writable;
            ChunkStreams = new(numberOfChunks);
            _Length = numberOfChunks == 0 ? 0 : (numberOfChunks - 1) * chunkSize + lastChunkLength;
            NumberOfChunks = numberOfChunks;
            ChunkSize = chunkSize;
        }

        /// <summary>
        /// Number of existing chunks
        /// </summary>
        public int NumberOfChunks { get; protected set; }

        /// <summary>
        /// Chunk size in bytes
        /// </summary>
        public long ChunkSize { get; }

        /// <summary>
        /// Current number of chunks
        /// </summary>
        public int CurrentNumberOfChunks => Math.Max(NumberOfChunks, ChunkStreams.Count == 0 ? 0 : ChunkStreams.Keys.Max() + 1);

        /// <summary>
        /// Current chunk index
        /// </summary>
        public int CurrentChunk => _Position == 0 ? 0 : (int)Math.Ceiling((double)_Position / ChunkSize);

        /// <summary>
        /// Current chunk position
        /// </summary>
        public long CurrentChunkPosition => _Position < ChunkSize ? 0 : _Position - (CurrentChunk * ChunkSize);

        /// <summary>
        /// Last chunk length in bytes
        /// </summary>
        public long LastChunkLength => CurrentNumberOfChunks == 0 ? 0 : _Length - (CurrentNumberOfChunks - 1) * ChunkSize;

        /// <summary>
        /// Indexes of modified chunks
        /// </summary>
        public ReadOnlyCollection<int> ModifiedChunks => _ModifiedChunks.AsReadOnly();

        /// <summary>
        /// Is committed?
        /// </summary>
        public bool IsCommitted => _ModifiedChunks.Count == 0 && NumberOfChunks == CurrentNumberOfChunks;

        /// <summary>
        /// Auto-commit on close?
        /// </summary>
        public bool CommitOnClose { get; set; } = true;

        /// <summary>
        /// Auto-commit on flush?
        /// </summary>
        public bool CommitOnFlush { get; set; }

        /// <summary>
        /// Commit changes
        /// </summary>
        public virtual void Commit()
        {
            EnsureUndisposed(allowDisposing: true);
            _ModifiedChunks.Clear();
            NumberOfChunks = CurrentNumberOfChunks;
        }

        /// <summary>
        /// Commit changes
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual Task CommitAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed(allowDisposing: true);
            _ModifiedChunks.Clear();
            NumberOfChunks = CurrentNumberOfChunks;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get a chunk stream
        /// </summary>
        /// <param name="index">Chunk index</param>
        /// <returns>Chunk stream</returns>
        protected Stream GetChunkStream(in int index)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            if (ChunkStreams.TryGetValue(index, out Stream? res)) return res;
            return ChunkStreams[index] = ChunkStreamFactory is null 
                ? AsyncChunkStreamFactory!(this, index, default).GetAwaiter().GetResult() 
                : ChunkStreamFactory(this, index);
        }

        /// <summary>
        /// Get a chunk stream
        /// </summary>
        /// <param name="index">Chunk index</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Chunk stream</returns>
        protected async Task<Stream> GetChunkStreamAsync(int index, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            if (ChunkStreams.TryGetValue(index, out Stream? res)) return res;
            return ChunkStreams[index] = ChunkStreamFactory is null
                ? await AsyncChunkStreamFactory!(this, index, cancellationToken).DynamicContext()
                : ChunkStreamFactory(this, index);
        }

        /// <summary>
        /// Delete a chunk
        /// </summary>
        /// <param name="index">Chunk index</param>
        protected virtual void DeleteChunkSync(in int index)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (ChunkStreams.TryGetValue(index, out Stream? stream))
            {
                stream.Dispose();
                ChunkStreams.Remove(index);
            }
            if (DeleteChunk is not null)
            {
                DeleteChunk(this, index);
            }
            else
            {
                AsyncDeleteChunk!(this, index, default).GetAwaiter().GetResult();
            }
            _ModifiedChunks.Remove(index);
        }

        /// <summary>
        /// Delete a chunk
        /// </summary>
        /// <param name="index">Chunk index</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task DeleteChunkAsync(int index, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (ChunkStreams.TryGetValue(index, out Stream? stream))
            {
                await stream.DisposeAsync().DynamicContext();
                ChunkStreams.Remove(index);
            }
            if (AsyncDeleteChunk is not null)
            {
                await AsyncDeleteChunk(this, index, cancellationToken).DynamicContext();
            }
            else
            {
                DeleteChunk!(this, index);
            }
            _ModifiedChunks.Remove(index);
        }

        /// <summary>
        /// Delegate for a chunk stream factory
        /// </summary>
        /// <param name="stream">Chunked stream</param>
        /// <param name="chunk">Chunk index</param>
        /// <returns>Chunk stream</returns>
        public delegate Stream StreamFactory_Delegate(ChunkedStream stream, int chunk);
        /// <summary>
        /// Delegate for an asynchronous chunk stream factory
        /// </summary>
        /// <param name="stream">Chunked stream</param>
        /// <param name="chunk">Chunk index</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Chunk stream</returns>
        public delegate Task<Stream> AsyncStreamFactory_Delegate(ChunkedStream stream, int chunk, CancellationToken cancellationToken);
        /// <summary>
        /// Delegate for deleting a chunk
        /// </summary>
        /// <param name="stream">Chunked stream</param>
        /// <param name="chunk">Chunk index</param>
        public delegate void DeleteChunk_Delegate(ChunkedStream stream, int chunk);
        /// <summary>
        /// Delegate for deleting a chunk
        /// </summary>
        /// <param name="stream">Chunked stream</param>
        /// <param name="chunk">Chunk index</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate Task AsyncDeleteChunk_Delegate(ChunkedStream stream, int chunk, CancellationToken cancellationToken);
    }
}
