namespace wan24.Core
{
    // Writing
    public partial class BlockingBufferStream
    {
        /// <inheritdoc/>
        public sealed override void Flush() => EnsureUndisposed();

        /// <inheritdoc/>
        public sealed override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void WriteByte(byte value) => this.GenericWriteByte(value);

        /// <inheritdoc/>
        public sealed override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (_IsEndOfFile) throw new InvalidOperationException();
            bool blocking = false,
                haveData = false;
            for (int write; buffer.Length > 0 && EnsureUndisposed();)
            {
                SpaceEvent.Wait();
                if (_IsEndOfFile) throw new InvalidOperationException();
                EnsureUndisposed();
                using (SemaphoreSyncContext ssc = BufferSync.SyncContext())
                {
                    EnsureUndisposed();
                    write = Math.Min(SpaceLeft, buffer.Length);
                    buffer[..write].CopyTo(Buffer.Span[WriteOffset..]);
                    buffer = buffer[write..];
                    WriteOffset += write;
                    _Length += write;
                    if (SpaceLeft == 0)
                    {
                        blocking = true;
                        SpaceEvent.Reset();
                    }
                    haveData = !DataEvent.IsSet;
                    DataEvent.Set();
                }
                if (haveData)
                {
                    RaiseOnDataAvailable();
                    haveData = false;
                }
                if (blocking)
                {
                    RaiseOnNeedSpace();
                    blocking = false;
                }
                if (AutoReorg && SpaceLeft == 0) ReorganizeBuffer();
            }
        }

        /// <summary>
        /// Try writing a buffer without blocking
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes written</returns>
        public virtual int TryWrite(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (_IsEndOfFile) throw new InvalidOperationException();
            int res = 0;
            bool blocking = false,
                haveData = false;
            for (int write; buffer.Length > 0 && EnsureUndisposed();)
            {
                using (SemaphoreSyncContext ssc = BufferSync.SyncContext())
                {
                    EnsureUndisposed();
                    write = Math.Min(SpaceLeft, buffer.Length);
                    if (write == 0) return res;
                    buffer[..write].CopyTo(Buffer.Span[WriteOffset..]);
                    buffer = buffer[write..];
                    WriteOffset += write;
                    _Length += write;
                    res += write;
                    if (SpaceLeft == 0)
                    {
                        blocking = true;
                        SpaceEvent.Reset();
                    }
                    haveData = !DataEvent.IsSet;
                    DataEvent.Set();
                }
                if (haveData)
                {
                    RaiseOnDataAvailable();
                    haveData = false;
                }
                if (blocking)
                {
                    RaiseOnNeedSpace();
                    blocking = false;
                }
                if (AutoReorg && SpaceLeft == 0) ReorganizeBuffer();
            }
            return res;
        }

        /// <inheritdoc/>
        public sealed override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (_IsEndOfFile) throw new InvalidOperationException();
            bool blocking = false,
                haveData = false;
            for (int write; buffer.Length > 0 && EnsureUndisposed();)
            {
                await SpaceEvent.WaitAsync(cancellationToken).DynamicContext();
                if (_IsEndOfFile) throw new InvalidOperationException();
                EnsureUndisposed();
                using (SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext())
                {
                    EnsureUndisposed();
                    write = Math.Min(SpaceLeft, buffer.Length);
                    buffer.Span[..write].CopyTo(Buffer.Span[WriteOffset..]);
                    buffer = buffer[write..];
                    WriteOffset += write;
                    _Length += write;
                    if (SpaceLeft == 0)
                    {
                        blocking = true;
                        SpaceEvent.Reset(cancellationToken);
                    }
                    haveData = !DataEvent.IsSet;
                    DataEvent.Set(cancellationToken);
                }
                if (haveData)
                {
                    RaiseOnDataAvailable();
                    haveData = false;
                }
                if (blocking)
                {
                    RaiseOnNeedSpace();
                    blocking = false;
                }
                if (AutoReorg && SpaceLeft == 0) await ReorganizeBufferAsync(cancellationToken).DynamicContext();
            }
        }

        /// <summary>
        /// Try writing a buffer without blocking
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of bytes written</returns>
        public virtual async ValueTask<int> TryWriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (_IsEndOfFile) throw new InvalidOperationException();
            int res = 0;
            bool blocking = false,
                haveData = false;
            for (int write; buffer.Length > 0 && EnsureUndisposed();)
            {
                using (SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext())
                {
                    EnsureUndisposed();
                    write = Math.Min(SpaceLeft, buffer.Length);
                    if (write == 0) return res;
                    buffer.Span[..write].CopyTo(Buffer.Span[WriteOffset..]);
                    buffer = buffer[write..];
                    WriteOffset += write;
                    _Length += write;
                    res += write;
                    if (SpaceLeft == 0)
                    {
                        blocking = true;
                        SpaceEvent.Reset(cancellationToken);
                    }
                    haveData = !DataEvent.IsSet;
                    DataEvent.Set(cancellationToken);
                }
                if (haveData)
                {
                    RaiseOnDataAvailable();
                    haveData = false;
                }
                if (blocking)
                {
                    RaiseOnNeedSpace();
                    blocking = false;
                }
                if (AutoReorg && SpaceLeft == 0) await ReorganizeBufferAsync(cancellationToken).DynamicContext();
            }
            return res;
        }
    }
}
