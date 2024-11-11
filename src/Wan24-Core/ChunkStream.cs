using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Chunk stream (writes data in chunks including chunk meta data; an existing stream can't be extended; not seekable during writing)
    /// </summary>
    public sealed class ChunkStream : ChunkStream<Stream, ChunkStream>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="chunkSize">Chunk size in bytes</param>
        /// <param name="writable">If writable (if <see langword="true"/>, reading isn't possible unless flushed finally)</param>
        /// <param name="writeChunkSizeHeader">If to write the chunk size to the beginning of the stream</param>
        /// <param name="clearBuffers">If to clear buffers (if <see langword="null"/>, <see cref="Settings.ClearBuffers"/> will be used)</param>
        /// <param name="leaveOpen">If to leave the base stream open when disposing</param>
        [Constructor]
#pragma warning disable IDE0079 // Not required suppression
#pragma warning disable IDE0051 // Ununsed
        private ChunkStream(
            in Stream baseStream,
            in int chunkSize,
            in bool writable,
            in bool writeChunkSizeHeader,
            in bool? clearBuffers,
            in bool leaveOpen
            )
            : base(baseStream, chunkSize, writable, writeChunkSizeHeader, clearBuffers, leaveOpen)
        { }
#pragma warning restore IDE0051 // Ununsed
#pragma warning restore IDE0079 // Not required suppression
    }

    /// <summary>
    /// Chunk stream (writes data in chunks including chunk meta data; an existing stream can't be extended; not seekable during writing; extending type needs to have a 
    /// constructor which uses the <see cref="ConstructorAttribute"/>)
    /// </summary>
    /// <typeparam name="tStream">Stream type</typeparam>
    /// <typeparam name="tFinal">Final type</typeparam>
    public partial class ChunkStream<tStream, tFinal> : WrapperStream<tStream>
        where tStream : Stream
        where tFinal : ChunkStream<tStream, tFinal>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="chunkSize">Chunk size in bytes</param>
        /// <param name="writable">If writable (if <see langword="true"/>, reading isn't possible unless flushed finally)</param>
        /// <param name="writeChunkSizeHeader">If to write the chunk size to the beginning of the stream</param>
        /// <param name="clearBuffers">If to clear buffers (if <see langword="null"/>, <see cref="Settings.ClearBuffers"/> will be used)</param>
        /// <param name="leaveOpen">If to leave the base stream open when disposing</param>
        protected ChunkStream(
            in tStream baseStream,
            in int chunkSize,
            in bool writable,
            in bool writeChunkSizeHeader,
            in bool? clearBuffers,
            in bool leaveOpen
            )
            : base(baseStream, leaveOpen)
        {
            if (writable && !baseStream.CanWrite) throw new ArgumentException("Writable stream required", nameof(baseStream));
            if (!writable && !baseStream.CanRead) throw new ArgumentException("Readable stream required", nameof(baseStream));
            UseBaseStream = false;
            ChunkSize = chunkSize;
            IsFinalChunk = writable;
            IsFlushedFinally = !writable;
            WriteChunkSizeHeader = writeChunkSizeHeader;
            Buffer = writable
                ? new(bufferSize: Math.Min(Settings.BufferSize, chunkSize + 1))
                {
                    CleanReturned = clearBuffers ?? Settings.ClearBuffers
                }
                : null;
        }

        /// <summary>
        /// Chunk size in bytes
        /// </summary>
        public int ChunkSize { get; }

        /// <summary>
        /// If to write the chunk size to the beginning of the stream
        /// </summary>
        public bool WriteChunkSizeHeader { get; }

        /// <summary>
        /// Last chunk index (is the last chunk index only when writing or all data was red (<see cref="RedAll"/>))
        /// </summary>
        public long LastChunk { get; protected set; }

        /// <summary>
        /// Last chunk length in bytes
        /// </summary>
        public int LastChunkLength { get; protected set; }

        /// <summary>
        /// If the last chunk is the final chunk
        /// </summary>
        public bool IsFinalChunk { get; protected set; }

        /// <summary>
        /// If the final chunk was flushed finally
        /// </summary>
        public bool IsFlushedFinally { get; protected set; }

        /// <summary>
        /// Current chunk index
        /// </summary>
        public long CurrentChunk { get; protected set; }

        /// <summary>
        /// Current chunk length in bytes
        /// </summary>
        public int CurrentChunkLength { get; protected set; }

        /// <summary>
        /// Current chunk position in bytes
        /// </summary>
        public int CurrentChunkPosition { get; protected set; }

        /// <summary>
        /// If all chunk meta data was red (is <see langword="true"/> when writing)
        /// </summary>
        public bool RedAll { get; protected set; }

        /// <summary>
        /// If the stream was writable at any time
        /// </summary>
        public bool WasWritable
        {
            [MemberNotNullWhen(returnValue: true, nameof(Buffer))]
            get => Buffer is not null;
        }

        /// <summary>
        /// If it's required to flush the final chunk
        /// </summary>
        public bool NeedsFinalFlush
        {
            [MemberNotNullWhen(returnValue: true, nameof(Buffer))]
            get => Buffer is not null && !IsFlushedFinally;
        }

        /// <inheritdoc/>
        public override bool CanRead => IfUndisposed(() => !NeedsFinalFlush && BaseStream.CanRead, allowDisposing: true);

        /// <inheritdoc/>
        public override bool CanSeek => IfUndisposed(() => _Length > -1 && RedAll && CanRead && BaseStream.CanSeek, allowDisposing: true);

        /// <inheritdoc/>
        public override sealed bool CanWrite
        {
            [MemberNotNullWhen(returnValue: true, nameof(Buffer))]
            get => IfUndisposed(() => Buffer is not null && !IsFlushedFinally, allowDisposing: true);
        }

        /// <inheritdoc/>
        public override sealed long Length
        {
            get
            {
                EnsureUndisposed();
                if (_Length < 0 || !IsFlushedFinally) throw new InvalidOperationException();
                return GetDataOffset(_Length);
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get => IfUndisposed(() => !IsFlushedFinally ? LastChunk * ChunkSize + LastChunkLength : GetDataOffset(_Position));
            set
            {
                EnsureUndisposed();
                if (RedAll)
                {
                    EnsureSeekable();
                }
                else
                {
                    throw new InvalidOperationException();
                }
                ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(value));
                long pos = GetStreamPosition(value);
                if (pos > _Length) throw new ArgumentOutOfRangeException(nameof(value));
                BaseStream.Position = pos;
                _Position = pos;
                SetChunkOffset(value);
            }
        }

        /// <summary>
        /// Flush the final chunk (calls <see cref="Stream.Flush"/> also)
        /// </summary>
        public virtual void FlushFinalChunk()
        {
            EnsureUndisposed(allowDisposing: true);
            EnsureWritable();
            if (IsFlushedFinally) throw new InvalidOperationException();
            IsFinalChunk = true;
            int len = LastChunkLength;
            bool isFullChunk = len == ChunkSize;
            ChunkTypes chunkType = isFullChunk ? ChunkTypes.Full | ChunkTypes.Final : ChunkTypes.Partial | ChunkTypes.Final;
            BaseStream.WriteByte((byte)chunkType);
            _Position++;
            if (!isFullChunk)
                using (RentedMemoryRef<byte> buffer = new(sizeof(int), clean: false))
                {
                    len.GetBytes(buffer.Span);
                    BaseStream.Write(buffer.Span);
                    _Position += sizeof(int);
                }
            if (len > 0)
            {
                Buffer!.Position = 0;
                Buffer.CopyTo(BaseStream);
            }
            _Length = _Position;
            IsFlushedFinally = true;
            Buffer!.Dispose();
            RaiseOnChunkWritten(LastChunk, chunkType, len);
            Flush();
        }

        /// <summary>
        /// Flush the final chunk (calls <see cref="Stream.FlushAsync(CancellationToken)"/> also)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task FlushFinalChunkAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed(allowDisposing: true);
            EnsureWritable();
            if (IsFlushedFinally) throw new InvalidOperationException();
            IsFinalChunk = true;
            int len = LastChunkLength;
            bool isFullChunk = len == ChunkSize;
            ChunkTypes chunkType = isFullChunk ? ChunkTypes.Full | ChunkTypes.Final : ChunkTypes.Partial | ChunkTypes.Final;
            BaseStream.WriteByte((byte)chunkType);
            _Position++;
            if (!isFullChunk)
                using (RentedMemory<byte> buffer = new(sizeof(int), clean: false))
                {
                    len.GetBytes(buffer.Memory.Span);
                    await BaseStream.WriteAsync(buffer.Memory, cancellationToken).DynamicContext();
                    _Position += sizeof(int);
                }
            if (len > 0)
            {
                Buffer!.Position = 0;
                await Buffer.CopyToAsync(BaseStream, cancellationToken).DynamicContext();
                _Position += len;
            }
            _Length = _Position;
            IsFlushedFinally = true;
            Buffer!.Dispose();
            RaiseOnChunkWritten(LastChunk, chunkType, len);
            await FlushAsync(cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Discard the buffered data of the current chunk
        /// </summary>
        public virtual void DiscardChunkBuffer()
        {
            EnsureUndisposed();
            EnsureWritable();
            Buffer!.SetLength(0);
        }

        /// <inheritdoc/>
        public sealed override void SetLength(long value)
        {
            EnsureUndisposed();
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            EnsureSeekable();
            offset = offset < 0
                ? 0 - GetStreamPosition(Math.Abs(offset))
                : GetStreamPosition(offset);
            return GetDataOffset(this.GenericSeek(offset, origin));
        }

        /// <inheritdoc/>
        public sealed override int ReadByte()
        {
            EnsureUndisposed();
            EnsureReadable();
            return this.GenericReadByte();
        }

        /// <inheritdoc/>
        public sealed override void WriteByte(byte value)
        {
            EnsureUndisposed();
            EnsureWritable();
            this.GenericWriteByte(value);
        }

        /// <inheritdoc/>
        public override void Close()
        {
            if (!IsDisposed && NeedsFinalFlush) FlushFinalChunk();
            base.Close();
        }
    }
}
