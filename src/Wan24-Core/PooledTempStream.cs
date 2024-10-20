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
        /// Memory limit context
        /// </summary>
        private readonly PooledTempStreamMemoryLimit.Context? MemoryLimitContext;
        /// <summary>
        /// If the limit is a dynamic shared limit, which defines a total memory limit for all streams
        /// </summary>
        private readonly bool IsDynamicSharedLimit;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="estimatedLength">Estimated length in bytes</param>
        /// <param name="memoryStreamPool">Memory stream pool to use</param>
        /// <param name="fileStreamPool">File stream pool to use</param>
        /// <param name="limit">Memory limit (isn't available for browser apps (WASM)!)</param>
        /// <param name="limitContext">Existing memory limit context (will be disposed; consumed memory should match <see cref="MaxLengthInMemory"/>; isn't available for browser apps 
        /// (WASM)!)</param>
        /// <param name="isDynamicSharedLimit">If the memory limit is a dynamic shared limit, which defines a total memory limit for all streams (giving <c>limitContext</c> isn't allowed, 
        /// if this value is <see langword="true"/>; a dynamic shared memory limit isn't available for browser apps (WASM)!)</param>
        public PooledTempStream(
            in long estimatedLength = 0, 
            in StreamPool<PooledMemoryStream>? memoryStreamPool = null, 
            in StreamPool<PooledTempFileStream>? fileStreamPool = null,
            in PooledTempStreamMemoryLimit? limit = null,
            in PooledTempStreamMemoryLimit.Context? limitContext = null,
            in bool isDynamicSharedLimit = false
            )
            : base(leaveOpen: true)
        {
            if ((limit is not null || limitContext is not null) && ENV.IsBrowserApp)
                throw new PlatformNotSupportedException("Memory limit isn't available for browser apps");
            if (isDynamicSharedLimit)
            {
                if (limitContext is not null)
                    throw new ArgumentException("Existing memory limit context isn't allowed for a shared dynamic memory limit", nameof(limitContext));
                if (ENV.IsBrowserApp) throw new PlatformNotSupportedException("Dynamic shared memory limit isn't available for browser apps");
            }
            UsedMemoryStreamPool = memoryStreamPool ?? MemoryStreamPool;
            UsedFileStreamPool = fileStreamPool ?? FileStreamPool;
            IsDynamicSharedLimit = isDynamicSharedLimit;
            int usedMemory = MaxLengthInMemory;
            if (!isDynamicSharedLimit) MemoryLimitContext = limitContext ?? limit?.UseMemory(MaxLengthInMemory, out usedMemory);
            BaseStream = (!isDynamicSharedLimit && usedMemory < MaxLengthInMemory) || estimatedLength > MaxLengthInMemory 
                ? UsedFileStreamPool.Rent() 
                : UsedMemoryStreamPool.Rent();
            UseOriginalBeginWrite = true;
            if (isDynamicSharedLimit && limit is not null && MemoryStream is PooledMemoryStream ms)
            {
                MemoryLimitContext = limit.UseMemory((int)ms.BufferLength, out usedMemory);
                if (usedMemory < ms.BufferLength) CreateTempFile();
            }
        }

        /// <summary>
        /// Maximum number of bytes to store in a <see cref="MemoryStream"/>
        /// </summary>
        public static int MaxLengthInMemory { get; set; } = ENV.IsBrowserApp ? int.MaxValue : Settings.BufferSize;

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
                    Name = "Temporary memory stream"
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
                    Name = "Temporary file stream"
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
            PooledMemoryStream? ms = MemoryStream;
            long len = ms?.Length ?? 0;
            bool isInMemory = ms is not null,
                increaseLength = value > len;
            if (isInMemory && value == len) return;
            if (!isInMemory || increaseLength || !IsDynamicSharedLimit || MemoryLimitContext is null || ENV.IsBrowserApp)
            {
                if (isInMemory && increaseLength)
                {
                    HandleLengthChange(value);
                }
                else
                {
                    _BaseStream.SetLength(value);
                }
                return;
            }
            long oldBufferLength = ms!.BufferLength;
            ms.SetLength(value);
            long newBufferLength = ms.BufferLength;
            if (newBufferLength < oldBufferLength) MemoryLimitContext.Decrease((int)(oldBufferLength - newBufferLength));
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            HandleLengthChange(Position + buffer.Length);
            _BaseStream.Write(buffer);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            HandleLengthChange(Position + 1);
            _BaseStream.WriteByte(value);
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await HandleLengthChangeAsync(Position + buffer.Length, cancellationToken).DynamicContext();
            await _BaseStream.WriteAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override void Close()
        {
            ReturnBaseStream();
            base.Close();
            if (!IsDisposing) MemoryLimitContext?.Dispose();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            ReturnBaseStream();
            base.Dispose(disposing);
            MemoryLimitContext?.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            ReturnBaseStream();
            await base.DisposeCore().DynamicContext();
            if (MemoryLimitContext is not null) await MemoryLimitContext.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Handle a stream length change
        /// </summary>
        /// <param name="newLength">New length in bytes</param>
        private void HandleLengthChange(in long newLength)
        {
            if (MemoryStream is not PooledMemoryStream ms || newLength <= ms.Length) return;
            bool isBrowserApp = ENV.IsBrowserApp;
            if (isBrowserApp || !IsDynamicSharedLimit)
            {
                if (!isBrowserApp && newLength > MaxLengthInMemory) CreateTempFile();
                return;
            }
            long oldBufferLength = ms.BufferLength;
            if (MemoryLimitContext is not null && newLength >= oldBufferLength)
            {
                ms.SetLength(newLength);
                long bufferLengthDiff = ms.BufferLength - oldBufferLength;
                if (bufferLengthDiff > 0 && !MemoryLimitContext.Increase((int)bufferLengthDiff)) CreateTempFile();
            }
        }

        /// <summary>
        /// Handle a stream length change
        /// </summary>
        /// <param name="newLength">New length in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private async Task HandleLengthChangeAsync(long newLength, CancellationToken cancellationToken = default)
        {
            if (MemoryStream is not PooledMemoryStream ms || newLength <= ms.Length) return;
            bool isBrowserApp = ENV.IsBrowserApp;
            if (isBrowserApp || !IsDynamicSharedLimit)
            {
                if (!isBrowserApp && newLength > MaxLengthInMemory) await CreateTempFileAsync(cancellationToken).DynamicContext();
                return;
            }
            long oldBufferLength = ms.BufferLength;
            if (MemoryLimitContext is not null && newLength >= oldBufferLength)
            {
                ms.SetLength(newLength);
                long bufferLengthDiff = ms.BufferLength - oldBufferLength;
                if (bufferLengthDiff > 0 && !MemoryLimitContext.Increase((int)bufferLengthDiff)) await CreateTempFileAsync(cancellationToken).DynamicContext();
            }
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
                    if (ms.Length > 0) CopyTo(fs);
                    BaseStream = fs;
                    UsedMemoryStreamPool.Return(ms);
                    MemoryLimitContext?.Dispose();
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
                    if (ms.Length > 0) await CopyToAsync(fs, cancellationToken).DynamicContext();
                    BaseStream = fs;
                    UsedMemoryStreamPool.Return(ms);
                    if (MemoryLimitContext is not null) await MemoryLimitContext.DisposeAsync().DynamicContext();
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
            if (MemoryStream is PooledMemoryStream ms)
            {
                UsedMemoryStreamPool.Return(ms);
            }
            else if(FileStream is PooledTempFileStream fs)
            {
                UsedFileStreamPool.Return(fs);
            }
            else
            {
                _BaseStream?.Dispose();
                Logging.WriteWarning($"Base stream \"{_BaseStream?.GetType()}\" of {GetType()} (\"{Name}\") couldn't be returned to the used pool");
            }
            _BaseStream = Null;
        }
    }
}
