using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Background stream (allows writing using a background service; will copy each chunk for writing; not seek- and readable!)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="maxMemory">Max. memory for the background service in byte (will block, if exceeded; may overflow!)</param>
    /// <param name="queueSize">Max. write queue size (will block, if exceeded!)</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class BackgroundStream(in Stream baseStream, in int maxMemory = int.MaxValue, in int queueSize = 100, in bool leaveOpen = false)
        : BackgroundStream<Stream>(baseStream, maxMemory, queueSize, leaveOpen)
    {
    }

    /// <summary>
    /// Background stream (allows writing using a background service; will copy each chunk for writing; not seek- and readable!)
    /// </summary>
    /// <typeparam name="T">Base stream type</typeparam>
    public class BackgroundStream<T> : WrapperStream<T> where T : Stream
    {
        /// <summary>
        /// Write queue
        /// </summary>
        protected readonly WriteService Queue;
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Queue counter
        /// </summary>
        protected readonly CounterEvent QueueCounter = new()
        {
            MinCounter = 0
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="maxMemory">Max. memory for the background service in byte (will block, if exceeded; may overflow!)</param>
        /// <param name="queueSize">Max. write queue size (will block, if exceeded!)</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public BackgroundStream(in T baseStream, in int maxMemory = int.MaxValue, in int queueSize = 100, in bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            Queue = new(this, queueSize);
            ArgumentOutOfRangeException.ThrowIfLessThan(maxMemory, 1);
            MaxMemory = maxMemory;
            UseOriginalByteIO = true;
            UseOriginalCopyTo = true;
            UseOriginalBeginWrite = true;
            Queue.OnException += (s, e) => RaiseOnError();
            Queue.StartAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Max. memory for the background service in byte (<see cref="BackgroundStream"/> will block, if exceeded; may overflow!)
        /// </summary>
        public int MaxMemory { get; }

        /// <summary>
        /// Current background service memory usage in byte (may be larger than <see cref="MaxMemory"/>)
        /// </summary>
        public int CurrentMemory => QueueCounter.Counter;

        /// <summary>
        /// Last exception of the background writing service
        /// </summary>
        public Exception? LastException => Queue.LastException;

        /// <inheritdoc/>
        public sealed override bool CanRead => false;

        /// <inheritdoc/>
        public sealed override bool CanSeek => false;

        /// <summary>
        /// Wait for the background service to finish writing the queue
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public void WaitWritten(CancellationToken cancellationToken = default) => QueueCounter.WaitCounterEquals(value: 0, cancellationToken);

        /// <summary>
        /// Wait for the background service to finish writing the queue
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task WaitWrittenAsync(CancellationToken cancellationToken = default) => QueueCounter.WaitCounterEqualsAsync(value: 0, cancellationToken);

        /// <inheritdoc/>
        public sealed override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (buffer.Length == 0) return;
            using SemaphoreSyncContext ssc = Sync;
            EnsureUndisposed();
            QueueCounter.WaitCounterLower(MaxMemory);
            EnsureUndisposed();
            if (LastException is not null) throw new IOException(LastException.Message, LastException);
            bool countMemory = false;
            RentedArray<byte> queuedBuffer = new(len: buffer.Length, clean: false)
            {
                Clear = true
            };
            try
            {
                buffer.CopyTo(queuedBuffer.Span);
                QueueCounter.Count(buffer.Length);
                countMemory = true;
                Queue.EnqueueAsync(queuedBuffer).AsTask().GetAwaiter().GetResult();
            }
            catch
            {
                queuedBuffer.Dispose();
                if (countMemory) QueueCounter.Count(-buffer.Length);
                throw;
            }
            if (LastException is not null) throw new IOException(LastException.Message, LastException);
        }

        /// <inheritdoc/>
        public sealed override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (buffer.Length == 0) return;
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            await QueueCounter.WaitCounterLowerAsync(MaxMemory, cancellationToken).DynamicContext();
            EnsureUndisposed();
            if (LastException is not null) throw new IOException(LastException.Message, LastException);
            bool countMemory = false;
            RentedArray<byte> queuedBuffer = new(len: buffer.Length, clean: false)
            {
                Clear = true
            };
            try
            {
                buffer.Span.CopyTo(queuedBuffer.Span);
                await QueueCounter.CountAsync(buffer.Length, cancellationToken: CancellationToken.None).DynamicContext();
                countMemory = true;
                await Queue.EnqueueAsync(queuedBuffer, cancellationToken).DynamicContext();
            }
            catch
            {
                queuedBuffer.Dispose();
                if (countMemory) await QueueCounter.CountAsync(-buffer.Length, cancellationToken: CancellationToken.None).DynamicContext();
                throw;
            }
            if (LastException is not null) throw new IOException(LastException.Message, LastException);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Queue.Dispose();
            base.Dispose(disposing);
            QueueCounter.Dispose();
            Sync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await Queue.DisposeAsync().DynamicContext();
            await base.DisposeCore().DynamicContext();
            await QueueCounter.DisposeAsync().DynamicContext();
            Sync.Dispose();
        }

        /// <summary>
        /// Delegate for an <see cref="OnError"/> event handler
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="e">Arguments</param>
        public delegate void Error_Delegate(BackgroundStream<T> stream, ErrorEventArgs e);
        /// <summary>
        /// Raised on background writing error
        /// </summary>
        public event Error_Delegate? OnError;
        /// <summary>
        /// Raise the <see cref="OnError"/> event
        /// </summary>
        protected virtual void RaiseOnError() => OnError?.Invoke(this, new(Queue.LastException!));

        /// <summary>
        /// Arguments for the <see cref="OnError"/> event
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="ex">Exception</param>
        public sealed class ErrorEventArgs(Exception ex) : EventArgs()
        {
            /// <summary>
            /// Exception
            /// </summary>
            public Exception Exception { get; } = ex;
        }

        /// <summary>
        /// Write service
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="target">Target stream</param>
        /// <param name="queueSize">Max. queue size</param>
        protected sealed class WriteService(in BackgroundStream<T> target, in int queueSize) : ItemQueueWorkerBase<RentedArray<byte>>(queueSize)
        {
            /// <summary>
            /// Target stream
            /// </summary>
            public BackgroundStream<T> Target { get; } = target;

            /// <inheritdoc/>
            protected override async Task ProcessItem(RentedArray<byte> item, CancellationToken cancellationToken)
            {
                using RentedArray<byte> buffer = item;
                try
                {
                    if (_LastException is null && !IsDisposing) await Target.BaseStream.WriteAsync(buffer.Memory, cancellationToken).DynamicContext();
                }
                catch(Exception ex)
                {
                    LastException = ex;
                    _ = DisposeAsync().DynamicContext();
                    RaiseOnException();
                }
                finally
                {
                    await Target.QueueCounter.CountAsync(-buffer.Length, cancellationToken: CancellationToken.None).DynamicContext();
                }
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (Queue.Reader.Count == 0) return;
                if (_LastException is null)
                {
                    _LastException = new ObjectDisposedException(GetType().ToString());
                    if (Warning) Logging.WriteWarning($"{GetType()} of {Target.GetType()} had queued unwritten data which will be discarded due to early disposing");
                    RaiseOnException();
                }
                while (Queue.Reader.TryRead(out Task_Delegate? task)) task(CancellationToken.None).AsTask().GetAwaiter().GetResult();
            }

            /// <inheritdoc/>
            protected override async Task DisposeCore()
            {
                await base.DisposeCore().DynamicContext();
                if (Queue.Reader.Count == 0) return;
                if (_LastException is null)
                {
                    _LastException = new ObjectDisposedException(GetType().ToString());
                    if (Warning) Logging.WriteWarning($"{GetType()} of {Target.GetType()} had queued unwritten data which will be discarded due to early disposing");
                    RaiseOnException();
                }
                while (Queue.Reader.TryRead(out Task_Delegate? task)) await task(CancellationToken.None).DynamicContext();
            }
        }
    }
}
