﻿namespace wan24.Core
{
    // Writing
    public partial class BlockingBufferStream
    {
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
            for (int write; buffer.Length > 0 && EnsureUndisposed();)
            {
                SpaceEvent.Wait();
                if (_IsEndOfFile) throw new InvalidOperationException();
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = BufferSync.SyncContext();
                if (_IsEndOfFile) throw new InvalidOperationException();
                EnsureUndisposed();
                write = Math.Min(SpaceLeft, buffer.Length);
                buffer[..write].CopyTo(Buffer.Memory.Span[WriteOffset..]);
                buffer = buffer[write..];
                WriteOffset += write;
                _Length += write;
                if (SpaceLeft == 0)
                {
                    SpaceEvent.Reset();
                    RaiseOnNeedSpace();
                }
                if (!DataEvent.IsSet && (!UseFlush || SpaceLeft == 0))
                {
                    DataEvent.Set();
                    RaiseOnDataAvailable();
                }
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
            for (int write; buffer.Length > 0 && EnsureUndisposed();)
            {
                using SemaphoreSyncContext ssc = BufferSync.SyncContext();
                if (_IsEndOfFile) throw new InvalidOperationException();
                EnsureUndisposed();
                write = Math.Min(SpaceLeft, buffer.Length);
                if (write == 0) return res;
                buffer[..write].CopyTo(Buffer.Memory.Span[WriteOffset..]);
                buffer = buffer[write..];
                WriteOffset += write;
                _Length += write;
                res += write;
                if (SpaceLeft == 0)
                {
                    SpaceEvent.Reset();
                    RaiseOnNeedSpace();
                }
                if (!DataEvent.IsSet && (!UseFlush || SpaceLeft == 0))
                {
                    DataEvent.Set();
                    RaiseOnDataAvailable();
                }
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
            for (int write; buffer.Length > 0 && EnsureUndisposed();)
            {
                await SpaceEvent.WaitAsync(cancellationToken).DynamicContext();
                if (_IsEndOfFile) throw new InvalidOperationException();
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext();
                if (_IsEndOfFile) throw new InvalidOperationException();
                EnsureUndisposed();
                write = Math.Min(SpaceLeft, buffer.Length);
                buffer.Span[..write].CopyTo(Buffer.Memory.Span[WriteOffset..]);
                buffer = buffer[write..];
                WriteOffset += write;
                _Length += write;
                if (SpaceLeft == 0)
                {
                    SpaceEvent.Reset(CancellationToken.None);
                    RaiseOnNeedSpace();
                }
                if (!DataEvent.IsSet && (!UseFlush || SpaceLeft == 0))
                {
                    DataEvent.Set(CancellationToken.None);
                    RaiseOnDataAvailable();
                }
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
            for (int write; buffer.Length > 0 && EnsureUndisposed();)
            {
                using SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext();
                if (_IsEndOfFile) throw new InvalidOperationException();
                EnsureUndisposed();
                write = Math.Min(SpaceLeft, buffer.Length);
                if (write == 0) return res;
                buffer.Span[..write].CopyTo(Buffer.Memory.Span[WriteOffset..]);
                buffer = buffer[write..];
                WriteOffset += write;
                _Length += write;
                res += write;
                if (SpaceLeft == 0)
                {
                    SpaceEvent.Reset(CancellationToken.None);
                    RaiseOnNeedSpace();
                }
                if (!DataEvent.IsSet && (!UseFlush || SpaceLeft == 0))
                {
                    DataEvent.Set(CancellationToken.None);
                    RaiseOnDataAvailable();
                }
            }
            return res;
        }
    }
}
