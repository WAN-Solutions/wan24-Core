namespace wan24.Core
{
    // Reading
    public partial class BlockingBufferStream
    {
        /// <inheritdoc/>
        public override int ReadByte() => this.GenericReadByte();

        /// <inheritdoc/>
        public sealed override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            if (_IsEndOfFile && Available == 0) return 0;
            int res = 0;
            for (int read = 1; buffer.Length > 0 && read > 0 && EnsureUndisposed();)
            {
                DataEvent.Wait();
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = BufferSync.SyncContext();
                EnsureUndisposed();
                read = Math.Min(buffer.Length, Available);
                if (read == 0)
                {
                    if (_IsEndOfFile)
                        break;
                    DataEvent.Reset();
                    RaiseOnNeedData();
                    if (_IsEndOfFile || ((res != 0 || !ReadIncomplete) && !AggressiveReadBlocking))
                        break;
                    read = 1;
                    continue;
                }
                try
                {
                    Buffer.Span.Slice(ReadOffset, read).CopyTo(buffer);
                    buffer = buffer[read..];
                    res += read;
                    ReadOffset += read;
                    _Position += read;
                    ResetBuffer();
                }
                finally
                {
                    if (Available == 0 && !_IsEndOfFile)
                    {
                        DataEvent.Reset();
                        RaiseOnNeedData();
                    }
                }
                if (!AggressiveReadBlocking ||(Available == 0 && _IsEndOfFile))
                    break;
            }
            if (((UseFlush && Available == 0) || (!UseFlush && res != 0)) && AutoReorg) ReorganizeBuffer();
            return res;
        }

        /// <summary>
        /// Try reading as much data as available without blocking
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes written into the buffer</returns>
        public virtual int TryRead(Span<byte> buffer)
        {
            EnsureUndisposed();
            if (_IsEndOfFile && Available == 0) return 0;
            int res = 0;
            for (int read = 1; buffer.Length > 0 && read > 0 && EnsureUndisposed();)
            {
                using SemaphoreSyncContext ssc = BufferSync.SyncContext();
                EnsureUndisposed();
                read = Math.Min(buffer.Length, Available);
                if (read == 0)
                {
                    if (_IsEndOfFile)
                        break;
                    DataEvent.Reset();
                    RaiseOnNeedData();
                    break;
                }
                try
                {
                    Buffer.Span.Slice(ReadOffset, read).CopyTo(buffer);
                    buffer = buffer[read..];
                    res += read;
                    ReadOffset += read;
                    _Position += read;
                    ResetBuffer();
                }
                finally
                {
                    if (Available == 0 && !_IsEndOfFile)
                    {
                        DataEvent.Reset();
                        RaiseOnNeedData();
                    }
                }
                if (Available == 0 && _IsEndOfFile)
                    break;
            }
            if (((UseFlush && Available == 0) || (!UseFlush && res != 0)) && AutoReorg) ReorganizeBuffer();
            return res;
        }

        /// <inheritdoc/>
        public sealed override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (_IsEndOfFile && Available == 0) return 0;
            int res = 0;
            for (int read = 1; buffer.Length > 0 && read > 0 && EnsureUndisposed();)
            {
                await DataEvent.WaitAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
                read = Math.Min(buffer.Length, Available);
                if (read == 0)
                {
                    if (_IsEndOfFile)
                        break;
                    DataEvent.Reset(CancellationToken.None);
                    RaiseOnNeedData();
                    if (_IsEndOfFile || ((res != 0 || !ReadIncomplete) && !AggressiveReadBlocking))
                        break;
                    read = 1;
                    continue;
                }
                try
                {
                    Buffer.Span.Slice(ReadOffset, read).CopyTo(buffer.Span);
                    buffer = buffer[read..];
                    res += read;
                    ReadOffset += read;
                    _Position += read;
                    ResetBuffer();
                }
                finally
                {
                    if (Available == 0 && !_IsEndOfFile)
                    {
                        DataEvent.Reset(CancellationToken.None);
                        RaiseOnNeedData();
                    }
                }
                if (!AggressiveReadBlocking || (Available == 0 && _IsEndOfFile))
                    break;
            }
            if (((UseFlush && Available == 0) || (!UseFlush && res != 0)) && AutoReorg) await ReorganizeBufferAsync(cancellationToken).DynamicContext();
            return res;
        }

        /// <summary>
        /// Try reading as much data as available without blocking
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of bytes written into the buffer</returns>
        public virtual async ValueTask<int> TryReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (_IsEndOfFile && Available == 0) return 0;
            int res = 0;
            for (int read = 1; buffer.Length > 0 && read > 0 && EnsureUndisposed();)
            {
                using SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
                read = Math.Min(buffer.Length, Available);
                if (read == 0)
                {
                    if (_IsEndOfFile)
                        break;
                    DataEvent.Reset(CancellationToken.None);
                    RaiseOnNeedData();
                    break;
                }
                try
                {
                    Buffer.Span.Slice(ReadOffset, read).CopyTo(buffer.Span);
                    buffer = buffer[read..];
                    res += read;
                    ReadOffset += read;
                    _Position += read;
                    ResetBuffer();
                }
                finally
                {
                    if (Available == 0 && !_IsEndOfFile)
                    {
                        DataEvent.Reset(CancellationToken.None);
                        RaiseOnNeedData();
                    }
                }
                if (Available == 0 && _IsEndOfFile)
                    break;
            }
            if (((UseFlush && Available == 0) || (!UseFlush && res != 0)) && AutoReorg) await ReorganizeBufferAsync(cancellationToken).DynamicContext();
            return res;
        }
    }
}
