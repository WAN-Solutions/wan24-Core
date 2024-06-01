namespace wan24.Core
{
    /// <summary>
    /// Data event stream (reading blocks; will read more on data event)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream (<see cref="Stream.Position"/> and <see cref="Stream.Length"/> must be readable)</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class DataEventStream(in Stream baseStream, in bool leaveOpen = false) : DataEventStream<Stream>(baseStream, leaveOpen)
    {
    }

    /// <summary>
    /// Data event stream (reading blocks; will read more on data event)
    /// </summary>
    /// <typeparam name="T">Base stream type</typeparam>
    public class DataEventStream<T> : WrapperStream<T> where T : Stream
    {
        /// <summary>
        /// Data event (raised when having data for reading)
        /// </summary>
        protected readonly ResetEvent DataEvent = new(initialState: true);
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream (<see cref="Stream.Position"/> and <see cref="Stream.Length"/> must be readable)</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public DataEventStream(in T baseStream, in bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            if (!baseStream.CanRead)
                throw new ArgumentException("Readable base stream required", nameof(baseStream));
            _ = baseStream.Length;
            _ = baseStream.Position;
            UseOriginalBeginRead = true;
            UseOriginalByteIO = true;
            UseOriginalCopyTo = true;
        }

        /// <summary>
        /// If there is no more input data
        /// </summary>
        public virtual bool IsEndOfStream { get; protected set; }

        /// <summary>
        /// If there's data available for reading
        /// </summary>
        public bool IsDataAvailable => DataEvent.IsSet && (!IsEndOfStream || Position < Length);

        /// <inheritdoc/>
        public sealed override bool CanSeek => false;

        /// <inheritdoc/>
        public sealed override bool CanWrite => false;

        /// <inheritdoc/>
        public sealed override long Position
        {
            get => base.Position;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Set end of stream
        /// </summary>
        public virtual void SetEndOfStream()
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            if (IsEndOfStream)
                throw new InvalidOperationException();
            IsEndOfStream = true;
            DataEvent.Set();
        }

        /// <summary>
        /// Raise the data event
        /// </summary>
        public virtual void RaiseDataEvent()
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            if (IsEndOfStream)
                throw new InvalidOperationException();
            DataEvent.Set();
        }

        /// <inheritdoc/>
        public sealed override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public sealed override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public sealed override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            if (buffer.Length < 1)
                return 0;
            int res = 0;
            for (int red; buffer.Length > 0 && EnsureUndisposed(); DataEvent.Wait())
            {
                red = base.Read(buffer);
                if (red > 0)
                {
                    res += red;
                    if (buffer.Length <= red)
                        break;
                    buffer = buffer[red..];
                    continue;
                }
                using (SemaphoreSyncContext ssc = Sync)
                {
                    if (Position < Length)
                        continue;
                    if (IsEndOfStream)
                        break;
                    DataEvent.Reset();
                }
                RaiseOnNeedData();
            }
            return res;
        }

        /// <inheritdoc/>
        public sealed override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (buffer.Length < 1)
                return 0;
            int res = 0;
            for (int red; buffer.Length > 0 && EnsureUndisposed(); await DataEvent.WaitAsync(cancellationToken).DynamicContext())
            {
                red = await base.ReadAsync(buffer, cancellationToken).DynamicContext();
                if (red > 0)
                {
                    res += red;
                    if (buffer.Length <= red)
                        break;
                    buffer = buffer[red..];
                    continue;
                }
                using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext())
                {
                    if (Position < Length)
                        continue;
                    if (IsEndOfStream)
                        break;
                    DataEvent.Reset(CancellationToken.None);
                }
                RaiseOnNeedData();
            }
            return res;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DataEvent.Dispose();
            Sync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            await DataEvent.DisposeAsync().DynamicContext();
            await Sync.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Delegate for a write event stream event handler
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="e">Arguments</param>
        public delegate void WriteEventStream_Delegate(DataEventStream<T> stream, EventArgs e);
        /// <summary>
        /// Raised when more data is required for a reading operation
        /// </summary>
        public event WriteEventStream_Delegate? OnNeedData;
        /// <summary>
        /// Raise the <see cref="OnNeedData"/> event
        /// </summary>
        protected virtual void RaiseOnNeedData() => OnNeedData?.Invoke(this, new());
    }
}
