namespace wan24.Core
{
    // StreamBase
    public partial class ChunkedStream : StreamBase
    {
        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => _CanWrite;

        /// <inheritdoc/>
        public override long Length => _Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _Position;
            set
            {
                EnsureUndisposed();
                if (value < 0 || value > _Length) throw new ArgumentOutOfRangeException(nameof(value));
                _Position = value;
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            EnsureUndisposed();
            if (ChunkStreams.TryGetValue(CurrentChunk, out Stream? stream)) stream.Flush();
            if (CommitOnFlush) Commit();
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            if (ChunkStreams.TryGetValue(CurrentChunk, out Stream? stream)) await stream.FlushAsync(cancellationToken).DynamicContext();
            if (CommitOnFlush) await CommitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => this.GenericSeek(offset, origin);

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            if (value == _Length) return;
            if (value == 0)
            {
                foreach (Stream chunk in ChunkStreams.Values) chunk.Dispose();
                ChunkStreams.Clear();
                for (int i = CurrentNumberOfChunks - 1; i >= 0; i--) DeleteChunkSync(i);
                _Position = _Length = 0;
            }
            else if (value > _Length)
            {
                Stream chunk;
                long total = _Length - value,
                    extend;
                for (int currentChunk = NumberOfChunks - 1; total != 0; total -= extend, currentChunk++)
                {
                    chunk = GetChunkStream(currentChunk);
                    extend = Math.Min(ChunkSize, total);
                    if (extend != 0) chunk.SetLength(extend);
                }
                _Length = value;
            }
            else
            {
                int currentChunk = CurrentNumberOfChunks - 1,
                    lastChunk = currentChunk;
                long len = _Length + ChunkSize;
                for (; len > value; len -= currentChunk == lastChunk ? LastChunkLength : ChunkSize, currentChunk--)
                    DeleteChunkSync(currentChunk);
                Stream chunk = GetChunkStream(currentChunk);
                long shrink = value - chunk.Length - currentChunk * ChunkSize;
                if (shrink != 0) chunk.SetLength(shrink);
                _Length = value;
                if (_Position > value) _Position = value;
            }
        }

        /// <summary>
        /// Set the stream length
        /// </summary>
        /// <param name="value">Length in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SetLengthAsync(long value, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            if (value == _Length) return;
            if (value == 0)
            {
                for (int i = CurrentNumberOfChunks - 1; i >= 0; i--) await DeleteChunkAsync(i, cancellationToken).DynamicContext();
                _Position = _Length = 0;
            }
            else if (value > _Length)
            {
                Stream chunk;
                long total = _Length - value,
                    extend;
                for (int currentChunk = NumberOfChunks - 1; total != 0; total -= extend, currentChunk++)
                {
                    chunk = await GetChunkStreamAsync(currentChunk, cancellationToken).DynamicContext();
                    extend = Math.Min(ChunkSize, total);
                    if (extend != 0) chunk.SetLength(extend);
                }
                _Length = value;
            }
            else
            {
                int currentChunk = CurrentNumberOfChunks - 1,
                    lastChunk = currentChunk;
                long len = _Length + ChunkSize;
                for (; len > value; len -= currentChunk == lastChunk ? LastChunkLength : ChunkSize, currentChunk--)
                    await DeleteChunkAsync(currentChunk, cancellationToken).DynamicContext();
                Stream chunk = await GetChunkStreamAsync(currentChunk, cancellationToken).DynamicContext();
                long shrink = value - chunk.Length - currentChunk * ChunkSize;
                if (shrink != 0) chunk.SetLength(shrink);
                _Length = value;
                if (_Position > value) _Position = value;
            }
        }

        /// <inheritdoc/>
        public override int ReadByte() => this.GenericReadByte();

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            int res = 0;
            Stream chunk;
            long pos;
            for (int read, red; buffer.Length != 0 && _Position != _Length; _Position += red, buffer = buffer[red..], res += red)
            {
                chunk = GetChunkStream(CurrentChunk);
                pos = CurrentChunkPosition;
                chunk.Position = pos;
                read = (int)Math.Min(buffer.Length, chunk.Length - pos);
                if (read == 0) return res;
                red = chunk.Read(buffer[..read]);
                if (red != read) throw new IOException($"Failed to read {read} bytes (got only {red} bytes)");
            }
            return res;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            int res = 0;
            Stream chunk;
            long pos;
            for (int read, red; buffer.Length != 0 && _Position != _Length; _Position += red, buffer = buffer[red..], res += red)
            {
                chunk = await GetChunkStreamAsync(CurrentChunk, cancellationToken).DynamicContext();
                pos = CurrentChunkPosition;
                chunk.Position = pos;
                read = (int)Math.Min(buffer.Length, chunk.Length - pos);
                if (read == 0) return res;
                red = await chunk.ReadAsync(buffer[..read], cancellationToken).DynamicContext();
                if (red != read) throw new IOException($"Failed to read {read} bytes (got only {red} bytes)");
            }
            return res;
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value) => this.GenericWriteByte(value);

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            Stream chunk;
            long pos;
            for (int write, currentChunk; buffer.Length != 0; buffer = buffer[write..])
            {
                currentChunk = CurrentChunk;
                chunk = GetChunkStream(currentChunk);
                pos = CurrentChunkPosition;
                chunk.Position = pos;
                write = (int)Math.Min(buffer.Length, ChunkSize - pos);
                _ModifiedChunks.Add(currentChunk);
                chunk.Write(buffer[..write]);
                _Position += write;
                if (_Position > _Length) _Length = _Position;
            }
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            Stream chunk;
            long pos;
            for (int write, currentChunk; buffer.Length != 0; buffer = buffer[write..])
            {
                currentChunk = CurrentChunk;
                chunk = await GetChunkStreamAsync(currentChunk, cancellationToken).DynamicContext();
                pos = CurrentChunkPosition;
                chunk.Position = pos;
                write = (int)Math.Min(buffer.Length, ChunkSize - pos);
                _ModifiedChunks.Add(currentChunk);
                await chunk.WriteAsync(buffer[..write], cancellationToken).DynamicContext();
                _Position += write;
                if (_Position > _Length) _Length = _Position;
            }
        }

        /// <inheritdoc/>
        public override void Close()
        {
            if (!DoClose()) return;
            if (CommitOnClose) Commit();
            base.Close();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            ChunkStreams.Values.DisposeAll();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (CommitOnClose) await CommitAsync().DynamicContext();
            await ChunkStreams.Values.DisposeAllAsync().DynamicContext();
            await base.DisposeCore().DynamicContext();
        }
    }
}
