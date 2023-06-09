﻿namespace wan24.Core
{
    /// <summary>
    /// Pooled temporary stream (hosts written data in memory first, then switches to a temporary file when exceeding the memory limit)
    /// </summary>
    public sealed class PooledTempStream : Stream
    {
        /// <summary>
        /// An object for static thread synchronization
        /// </summary>
        private static readonly object StaticSyncObject = new();
        /// <summary>
        /// Memory stream pool
        /// </summary>
        private static StreamPool<PooledMemoryStream>? _MemoryStreamPool = null;
        /// <summary>
        /// Memory stream pool
        /// </summary>
        private static StreamPool<PooledTempFileStream>? _FileStreamPool = null;

        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        private readonly object SyncObject = new();
        /// <summary>
        /// Used memory stream pool
        /// </summary>
        private readonly StreamPool<PooledMemoryStream> UsedMemoryStreamPool;
        /// <summary>
        /// Used file stream pool
        /// </summary>
        private readonly StreamPool<PooledTempFileStream> UsedFileStreamPool;
        /// <summary>
        /// Is disposed?
        /// </summary>
        private bool IsDisposed = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="estimatedLength">Estimated length in bytes</param>
        /// <param name="memoryStreamPool">Memory stream pool to use</param>
        /// <param name="fileStreamPool">File stream pool to use</param>
        public PooledTempStream(long estimatedLength = 0, StreamPool<PooledMemoryStream>? memoryStreamPool = null, StreamPool<PooledTempFileStream>? fileStreamPool = null) : base()
        {
            UsedMemoryStreamPool = memoryStreamPool ?? MemoryStreamPool;
            UsedFileStreamPool = fileStreamPool ?? FileStreamPool;
            BaseStream = estimatedLength > MaxLengthInMemory ? UsedFileStreamPool.Rent() : UsedMemoryStreamPool.Rent();
        }

        /// <summary>
        /// Maximum number of bytes to store in a <see cref="MemoryStream"/>
        /// </summary>
        public static int MaxLengthInMemory { get; set; } = Settings.BufferSize;

        /// <summary>
        /// Memory stream pool capacity
        /// </summary>
        public static int MemoryPoolCapacity { get; set; } = 100;

        /// <summary>
        /// File stream pool capacity
        /// </summary>
        public static int FileStreamCapacity { get; set; } = 20;

        /// <summary>
        /// Memory stream pool
        /// </summary>
        public static StreamPool<PooledMemoryStream> MemoryStreamPool
        {
            get
            {
                if (_MemoryStreamPool != null) return _MemoryStreamPool;
                lock (StaticSyncObject) return _MemoryStreamPool ??= new(MemoryPoolCapacity)
                {
                    Name = "Temporary memory streams"
                };
            }
        }

        /// <summary>
        /// File stream pool
        /// </summary>
        public static StreamPool<PooledTempFileStream> FileStreamPool
        {
            get
            {
                if (_FileStreamPool != null) return _FileStreamPool;
                lock (StaticSyncObject) return _FileStreamPool ??= new(FileStreamCapacity)
                {
                    Name = "Temporary file streams"
                };
            }
        }

        /// <summary>
        /// Base stream (do not dispose!)
        /// </summary>
        public Stream BaseStream { get; private set; }

        /// <summary>
        /// <see cref="PooledMemoryStream"/> (do not dispose!)
        /// </summary>
        public PooledMemoryStream? MemoryStream => BaseStream as PooledMemoryStream;

        /// <summary>
        /// <see cref="PooledTempFileStream"/> (do not dispose!)
        /// </summary>
        public PooledTempFileStream? FileStream => BaseStream as PooledTempFileStream;

        /// <summary>
        /// Is the data stored in a <see cref="PooledMemoryStream"/>
        /// </summary>
        public bool IsInMemory => BaseStream is PooledMemoryStream;

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => BaseStream.Length;

        /// <inheritdoc/>
        public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

        /// <inheritdoc/>
        public override void Flush() => BaseStream.Flush();

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken) => BaseStream.FlushAsync(cancellationToken);

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer) => BaseStream.Read(buffer);

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => BaseStream.ReadAsync(buffer, offset, count, cancellationToken);

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => BaseStream.ReadAsync(buffer, cancellationToken);

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            if (IsInMemory && value > MaxLengthInMemory) CreateTempFile();
            BaseStream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (IsInMemory && Position + buffer.Length > MaxLengthInMemory) CreateTempFile();
            BaseStream.Write(buffer);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            if (IsInMemory && Position + 1 > MaxLengthInMemory) CreateTempFile();
            BaseStream.WriteByte(value);
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (IsInMemory && Position + buffer.Length > MaxLengthInMemory) await CreateTempFileAsync(cancellationToken).DynamicContext();
            await BaseStream.WriteAsync(buffer, cancellationToken).DynamicContext(); ;
        }

        /// <inheritdoc/>
        public override void Close()
        {
            lock (SyncObject)
            {
                if (IsDisposed) return;
                IsDisposed = true;
            }
            base.Close();
            ReturnBaseStream();
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            await Task.Yield();
            lock (SyncObject)
            {
                if (IsDisposed) return;
                IsDisposed = true;
            }
            await base.DisposeAsync().DynamicContext();
            ReturnBaseStream();
        }

        /// <summary>
        /// Create a temporary file
        /// </summary>
        private void CreateTempFile()
        {
            EnsureUndisposed();
            long offset = Position;
            try
            {
                Position = 0;
                PooledMemoryStream ms = MemoryStream!;
                PooledTempFileStream fs = UsedFileStreamPool.Rent();
                try
                {
                    CopyTo(fs);
                    BaseStream = fs;
                    UsedMemoryStreamPool.Return(ms);
                }
                catch
                {
                    UsedFileStreamPool.Return(fs);
                    throw;
                }
            }
            finally
            {
                Position = offset;
            }
        }

        /// <summary>
        /// Create a temporary file
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        private async Task CreateTempFileAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            long offset = Position;
            try
            {
                Position = 0;
                PooledMemoryStream ms = MemoryStream!;
                PooledTempFileStream fs = UsedFileStreamPool.Rent();
                try
                {
                    await CopyToAsync(fs, cancellationToken).DynamicContext();
                    BaseStream = fs;
                    UsedMemoryStreamPool.Return(ms);
                }
                catch
                {
                    UsedFileStreamPool.Return(fs);
                    throw;
                }
            }
            finally
            {
                Position = offset;
            }
        }

        /// <summary>
        /// Return the base stream
        /// </summary>
        private void ReturnBaseStream()
        {
            if (MemoryStream != null)
            {
                UsedMemoryStreamPool.Return(MemoryStream);
            }
            else
            {
                UsedFileStreamPool.Return(FileStream!);
            }
            BaseStream = null!;
        }

        /// <summary>
        /// Ensure undisposed state
        /// </summary>
        private void EnsureUndisposed()
        {
            if (!IsDisposed) return;
            throw new ObjectDisposedException(GetType().ToString());
        }
    }
}
