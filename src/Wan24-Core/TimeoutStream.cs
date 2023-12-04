namespace wan24.Core
{
    /// <summary>
    /// Timeout stream (async reading/writing methods can timeout)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="readTimeout">Read timeout (<see cref="TimeSpan.Zero"/> to disable)</param>
    /// <param name="writeTimeout">Write timeout (<see cref="TimeSpan.Zero"/> to disable)</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class TimeoutStream(in Stream baseStream, in TimeSpan readTimeout, in TimeSpan? writeTimeout = null, in bool leaveOpen = false)
        : TimeoutStream<Stream>(baseStream, readTimeout, writeTimeout, leaveOpen)
    {
    }

    /// <summary>
    /// Timeout stream (async reading/writing methods can timeout)
    /// </summary>
    /// <typeparam name="T">Wrapped stream type</typeparam>
    public class TimeoutStream<T> : WrapperStream<T> where T:Stream
    {
        /// <summary>
        /// Read timeout
        /// </summary>
        protected TimeSpan _ReadTimeout;
        /// <summary>
        /// Write timeout
        /// </summary>
        protected TimeSpan _WriteTimeout;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="readTimeout">Read timeout (<see cref="TimeSpan.Zero"/> to disable)</param>
        /// <param name="writeTimeout">Write timeout (<see cref="TimeSpan.Zero"/> to disable)</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public TimeoutStream(in T baseStream, in TimeSpan readTimeout, in TimeSpan? writeTimeout = null, in bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            _ReadTimeout = readTimeout;
            _WriteTimeout = writeTimeout ?? TimeSpan.Zero;
            UseOriginalCopyTo = true;
            UseOriginalBeginRead = true;
            UseOriginalBeginWrite = true;
        }

        /// <inheritdoc/>
        public sealed override bool CanTimeout => true;

        /// <inheritdoc/>
        public override int ReadTimeout
        {
            get => (int)_ReadTimeout.TotalMilliseconds;
            set => _ReadTimeout = TimeSpan.FromMilliseconds(value);
        }

        /// <inheritdoc/>
        public override int WriteTimeout
        {
            get => (int)_WriteTimeout.TotalMilliseconds;
            set => _WriteTimeout = TimeSpan.FromMilliseconds(value);
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            if (_WriteTimeout != TimeSpan.Zero)
            {
                await Target.FlushAsync(cancellationToken).WaitAsync(_WriteTimeout, cancellationToken).DynamicContext();
            }
            else
            {
                await Target.FlushAsync(cancellationToken).DynamicContext();
            }
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => _ReadTimeout != TimeSpan.Zero
                ? await Target.ReadAsync(buffer, offset, count, cancellationToken).WaitAsync(_ReadTimeout, cancellationToken).DynamicContext()
                : await Target.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => _ReadTimeout != TimeSpan.Zero
                ? await Target.ReadAsync(buffer, cancellationToken).AsTask().WaitAsync(_ReadTimeout, cancellationToken).DynamicContext()
                : await Target.ReadAsync(buffer, cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_WriteTimeout != TimeSpan.Zero)
            {
                await Target.WriteAsync(buffer, offset, count, cancellationToken).WaitAsync(_WriteTimeout, cancellationToken).DynamicContext();
            }
            else
            {
                await Target.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
            }
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_WriteTimeout != TimeSpan.Zero)
            {
                await Target.WriteAsync(buffer, cancellationToken).AsTask().WaitAsync(_WriteTimeout, cancellationToken).DynamicContext();
            }
            else
            {
                await Target.WriteAsync(buffer, cancellationToken).DynamicContext();
            }
        }
    }
}
