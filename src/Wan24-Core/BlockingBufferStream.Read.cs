using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            bool blocking = false;
            for (int read = 1; buffer.Length > 0 && read > 0 && EnsureUndisposed();)
            {
                DataEvent.Wait();
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = BufferSync.SyncContext();
                EnsureUndisposed();
                read = Math.Min(buffer.Length, Available);
                if (read == 0)
                {
                    if (_IsEndOfFile || ((res != 0 || !ReadIncomplete) && !AggressiveReadBlocking))
                    {
                        blocking = true;
                        break;
                    }
                    DataEvent.Reset();
                    ssc.Dispose();
                    RaiseOnNeedData();
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
                    if (Available == 0)
                    {
                        blocking = true;
                        DataEvent.Reset();
                    }
                    else
                    {
                        blocking = false;
                    }
                }
                if (!AggressiveReadBlocking) break;
            }
            if (blocking) RaiseOnNeedData();
            if (Available == 0 && AutoReorg) ReorganizeBuffer();
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
            bool blocking = false;
            for (int read = 1; buffer.Length > 0 && read > 0 && EnsureUndisposed();)
            {
                using SemaphoreSyncContext ssc = BufferSync.SyncContext();
                EnsureUndisposed();
                read = Math.Min(buffer.Length, Available);
                if (read == 0) break;
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
                    if (Available == 0)
                    {
                        blocking = true;
                        DataEvent.Reset();
                    }
                    else
                    {
                        blocking = false;
                    }
                }
            }
            if (blocking) RaiseOnNeedData();
            if (Available == 0 && AutoReorg) ReorganizeBuffer();
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
            bool blocking = false;
            for (int read = 1; buffer.Length > 0 && read > 0 && EnsureUndisposed();)
            {
                await DataEvent.WaitAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
                read = Math.Min(buffer.Length, Available);
                if (read == 0)
                {
                    if (_IsEndOfFile || ((res != 0 || !ReadIncomplete) && !AggressiveReadBlocking))
                    {
                        blocking = true;
                        break;
                    }
                    DataEvent.Reset(cancellationToken);
                    ssc.Dispose();
                    RaiseOnNeedData();
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
                    if (Available == 0)
                    {
                        blocking = true;
                        DataEvent.Reset(cancellationToken);
                    }
                    else
                    {
                        blocking = false;
                    }
                }
                if (!AggressiveReadBlocking) break;
            }
            if (blocking) RaiseOnNeedData();
            if (Available == 0 && AutoReorg) await ReorganizeBufferAsync(cancellationToken).DynamicContext();
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
            bool blocking = false;
            for (int read = 1; buffer.Length > 0 && read > 0 && EnsureUndisposed();)
            {
                using SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
                read = Math.Min(buffer.Length, Available);
                if (read == 0) break;
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
                    if (Available == 0)
                    {
                        blocking = true;
                        DataEvent.Reset(cancellationToken);
                    }
                    else
                    {
                        blocking = false;
                    }
                }
            }
            if (blocking) RaiseOnNeedData();
            if (Available == 0 && AutoReorg) await ReorganizeBufferAsync(cancellationToken).DynamicContext();
            return res;
        }
    }
}
