﻿namespace wan24.Core
{
    /// <summary>
    /// Blocking buffer stream (reading blocks until (all, if aggressive) data is available, writing until the buffer was red completely; reading/writing is synchronized)
    /// </summary>
    public partial class BlockingBufferStream : StreamBase, IStatusProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="clear">Clear the buffer when disposing?</param>
        public BlockingBufferStream(in int bufferSize, in bool clear = false) : base()
        {
            if (bufferSize < 1) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            BufferSize = bufferSize;
            Buffer = new(bufferSize, clean: false)
            {
                Clear = clear
            };
        }

        /// <summary>
        /// Buffer size in bytes
        /// </summary>
        public int BufferSize { get; }

        /// <summary>
        /// Data in bytes available for reading unblocked
        /// </summary>
        public int Available => IfUndisposed(WriteOffset - ReadOffset);

        /// <summary>
        /// Space in bytes left in the buffer for writing unblocked
        /// </summary>
        public int SpaceLeft => IfUndisposed(BufferSize - WriteOffset);

        /// <summary>
        /// Block until the requested amount of data was red?
        /// </summary>
        public bool AggressiveReadBlocking { get; set; } = true;

        /// <summary>
        /// Read incomplete sequences? (will return incomplete sequences with <see cref="AggressiveReadBlocking"/> on <see langword="false"/>, but blocks when nothing was red)
        /// </summary>
        public bool ReadIncomplete { get; set; }

        /// <summary>
        /// Is reading blocked?
        /// </summary>
        public bool IsReadBlocked => IfUndisposed(() => !DataEvent.IsSet);

        /// <summary>
        /// Is writing blocked?
        /// </summary>
        public bool IsWriteBlocked => IfUndisposed(() => !SpaceEvent.IsSet);

        /// <summary>
        /// Automatic reorganize the buffer?
        /// </summary>
        public bool AutoReorg { get; set; } = true;

        /// <summary>
        /// Is at the end of the file?
        /// </summary>
        public bool IsEndOfFile
        {
            get => _IsEndOfFile;
            set
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = BufferSync.SyncContext();
                if (_IsEndOfFile || !value) throw new InvalidOperationException();
                _IsEndOfFile = value;
                SpaceEvent.Set();
                DataEvent.Set();
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new("Name", Name, "Name of the stream");
                yield return new("Type", GetType().ToString(), "Stream type");
                yield return new("Available", Available, "Available number of bytes for reading");
                yield return new("Space", SpaceLeft, "Space left in bytes for writing");
                yield return new("Reading blocks", IsReadBlocked, "Is reading blocked?");
                yield return new("Writing blocks", IsWriteBlocked, "Is writing blocked?");
            }
        }

        /// <inheritdoc/>
        public sealed override bool CanRead => true;

        /// <inheritdoc/>
        public sealed override bool CanSeek => false;

        /// <inheritdoc/>
        public sealed override bool CanWrite => true;

        /// <inheritdoc/>
        public sealed override long Length => IfUndisposed(_Length);

        /// <inheritdoc/>
        public sealed override long Position
        {
            get => IfUndisposed(_Position);
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Set the end of the file
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SetIsEndOfFileAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext();
            if (_IsEndOfFile) throw new InvalidOperationException();
            _IsEndOfFile = true;
            await SpaceEvent.SetAsync(cancellationToken).DynamicContext();
            await DataEvent.SetAsync(cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Reorganize the buffer (may give more space for writing and unblock)
        /// </summary>
        /// <returns>If more space for writing is available now</returns>
        public bool ReorganizeBuffer()
        {
            EnsureUndisposed();
            bool hadSpace;
            using (SemaphoreSyncContext ssc = BufferSync.SyncContext())
            {
                EnsureUndisposed();
                hadSpace = !IsWriteBlocked;
                if (ReadOffset == 0) return false;
                Array.Copy(Buffer, ReadOffset, Buffer, 0, WriteOffset - ReadOffset);
                WriteOffset -= ReadOffset;
                ReadOffset = 0;
            }
            if (!hadSpace)
            {
                SpaceEvent.Set();
                RaiseOnSpaceAvailable();
            }
            return true;
        }

        /// <summary>
        /// Reorganize the buffer (may give more space for writing and unblock)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If more space for writing is available now</returns>
        public async Task<bool> ReorganizeBufferAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            bool hadSpace;
            using (SemaphoreSyncContext ssc = await BufferSync.SyncContextAsync(cancellationToken).DynamicContext())
            {
                EnsureUndisposed();
                hadSpace = !IsWriteBlocked;
                if (ReadOffset == 0) return false;
                Array.Copy(Buffer, ReadOffset, Buffer, 0, WriteOffset - ReadOffset);
                WriteOffset -= ReadOffset;
                ReadOffset = 0;
            }
            if (!hadSpace)
            {
                SpaceEvent.Set(cancellationToken);
                RaiseOnSpaceAvailable();
            }
            return true;
        }

        /// <summary>
        /// Wait for buffer space for writing
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public void WaitSpace(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            SpaceEvent.Wait(cancellationToken);
        }

        /// <summary>
        /// Wait for buffer space for writing
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task WaitSpaceAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await SpaceEvent.WaitAsync(cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Wait for data being available
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public void WaitData(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            DataEvent.Wait(cancellationToken);
        }

        /// <summary>
        /// Wait for data being available
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task WaitDataAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await DataEvent.WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public sealed override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    }
}
