using System.Runtime;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Pooled temporary stream (hosts written data in memory first, then switches to a temporary file when exceeding the memory limit)
    /// </summary>
    public sealed class PooledTempStream : WrapperStream
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
        /// Used memory stream pool
        /// </summary>
        private readonly StreamPool<PooledMemoryStream> UsedMemoryStreamPool;
        /// <summary>
        /// Used file stream pool
        /// </summary>
        private readonly StreamPool<PooledTempFileStream> UsedFileStreamPool;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="estimatedLength">Estimated length in bytes</param>
        /// <param name="memoryStreamPool">Memory stream pool to use</param>
        /// <param name="fileStreamPool">File stream pool to use</param>
        public PooledTempStream(in long estimatedLength = 0, in StreamPool<PooledMemoryStream>? memoryStreamPool = null, in StreamPool<PooledTempFileStream>? fileStreamPool = null)
            : base(leaveOpen: true)
        {
            UsedMemoryStreamPool = memoryStreamPool ?? MemoryStreamPool;
            UsedFileStreamPool = fileStreamPool ?? FileStreamPool;
            BaseStream = estimatedLength > MaxLengthInMemory ? UsedFileStreamPool.Rent() : UsedMemoryStreamPool.Rent();
            UseOriginalBeginWrite = true;
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
                if (_MemoryStreamPool is not null) return _MemoryStreamPool;
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
                if (_FileStreamPool is not null) return _FileStreamPool;
                lock (StaticSyncObject) return _FileStreamPool ??= new(FileStreamCapacity)
                {
                    Name = "Temporary file streams"
                };
            }
        }

        /// <summary>
        /// <see cref="PooledMemoryStream"/> (do not dispose!)
        /// </summary>
        public PooledMemoryStream? MemoryStream
        {
            [TargetedPatchingOptOut("Tiny method")]
            get => _BaseStream as PooledMemoryStream;
        }

        /// <summary>
        /// <see cref="PooledTempFileStream"/> (do not dispose!)
        /// </summary>
        public PooledTempFileStream? FileStream
        {
            [TargetedPatchingOptOut("Tiny method")]
            get => _BaseStream as PooledTempFileStream;
        }

        /// <summary>
        /// Is the data stored in a <see cref="PooledMemoryStream"/>
        /// </summary>
        public bool IsInMemory
        {
            [TargetedPatchingOptOut("Tiny method")]
            get => _BaseStream is PooledMemoryStream;
        }

        /// <inheritdoc/>
        public override IEnumerable<Status> State
        {
            get
            {
                foreach (Status status in base.State) yield return status;
                yield return new(__("In memory"), IsInMemory, __("Is the data hosted in memory?"));
            }
        }

        /// <inheritdoc/>
        public override bool LeaveOpen
        {
            get => true;
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            if (IsInMemory && value > MaxLengthInMemory) CreateTempFile();
            _BaseStream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (IsInMemory && Position + buffer.Length > MaxLengthInMemory) CreateTempFile();
            _BaseStream.Write(buffer);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            if (IsInMemory && Position + 1 > MaxLengthInMemory) CreateTempFile();
            _BaseStream.WriteByte(value);
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (IsInMemory && Position + buffer.Length > MaxLengthInMemory) await CreateTempFileAsync(cancellationToken).DynamicContext();
            await _BaseStream.WriteAsync(buffer, cancellationToken).DynamicContext(); ;
        }

        /// <inheritdoc/>
        public override void Close()
        {
            if (!IsDisposing) return;
            base.Close();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            ReturnBaseStream();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override Task DisposeCore()
        {
            ReturnBaseStream();
            return base.DisposeCore();
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
            if (_BaseStream == Null) return;
            if (MemoryStream is not null)
            {
                UsedMemoryStreamPool.Return(MemoryStream);
            }
            else
            {
                UsedFileStreamPool.Return(FileStream!);
            }
            _BaseStream = Null;
        }
    }
}
