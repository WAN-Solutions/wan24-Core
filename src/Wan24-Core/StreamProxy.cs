using System.Diagnostics.Contracts;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Stream proxy (will copy data bi-directional between two streams; if one channel is done, the other channel will be canceled)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class StreamProxy : BasicAllDisposableBase
    {
        /// <summary>
        /// Cancellation
        /// </summary>
        protected readonly CancellationTokenSource Cancellation = new();
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// A2B task
        /// </summary>
        protected Task? A2B = null;
        /// <summary>
        /// B2A task
        /// </summary>
        protected Task? B2A = null;
        /// <summary>
        /// Observer task
        /// </summary>
        protected Task? Observer = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="a">A (should be non-blocking; will be disposed per default)</param>
        /// <param name="b">B (should be non-blocking; will be disposed per default)</param>
        /// <param name="run">Run now?</param>
        public StreamProxy(in Stream a, in Stream b, in bool run = true) : base()
        {
            A = a;
            B = b;
            if (run) Run();
        }

        /// <summary>
        /// Started time
        /// </summary>
        public DateTime Started { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Stopped time
        /// </summary>
        public DateTime Stopped { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// A (will be disposed per default)
        /// </summary>
        public Stream A { get; }

        /// <summary>
        /// B (will be disposed per default)
        /// </summary>
        public Stream B { get; }

        /// <summary>
        /// Leave the streams open when disposing?
        /// </summary>
        public bool LeaveOpen { get; set; }

        /// <summary>
        /// Is running?
        /// </summary>
        public bool IsRunning
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => !IsDisposing && A2B is not null && !A2B.IsCompleted && B2A is not null && !B2A.IsCompleted;
        }

        /// <summary>
        /// Is exceptional?
        /// </summary>
        public bool IsExceptional
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [TargetedPatchingOptOut("Tiny method")]
            get => A2BException is not null || B2AException is not null;
        }

        /// <summary>
        /// Delay to wait for data, if the source stream didn't read anything
        /// </summary>
        public TimeSpan DataDelay { get; set; } = TimeSpan.FromMilliseconds(50);

        /// <summary>
        /// Buffer size per channel
        /// </summary>
        public int BufferSize { get; set; } = Settings.BufferSize;

        /// <summary>
        /// A2B exception
        /// </summary>
        public AggregateException? A2BException { get; protected set; }

        /// <summary>
        /// B2A exception
        /// </summary>
        public AggregateException? B2AException { get; protected set; }

        /// <summary>
        /// Run (can't run twice!)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual void Run(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
            EnsureUndisposed();
            if (A2B is not null) throw new InvalidOperationException();
            Started = DateTime.Now;
            RunCommunication();
            Observer = RunObserverAsync();
        }

        /// <summary>
        /// Run (can't run twice!)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task RunAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            if (A2B is not null) throw new InvalidOperationException();
            Started = DateTime.Now;
            RunCommunication();
            Observer = RunObserverAsync();
        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual void Cancel(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
            EnsureUndisposed();
            if (Observer is null) throw new InvalidOperationException();
            if (!Cancellation.IsCancellationRequested) Cancellation.Cancel();
        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task CancelAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            if (Observer is null) throw new InvalidOperationException();
            if (!Cancellation.IsCancellationRequested) await Cancellation.CancelAsync().DynamicContext();
        }

        /// <summary>
        /// Wait
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task WaitAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed(allowDisposing: true);
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            EnsureUndisposed(allowDisposing: true);
            if (Observer is null) throw new InvalidOperationException();
            await Observer.WaitAsync(cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Throw an exception, if exceptional
        /// </summary>
        /// <exception cref="AggregateException">Processing exceptions</exception>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [TargetedPatchingOptOut("Tiny method")]
        public void ThrowIfExceptional()
        {
            if (!IsExceptional) return;
            List<AggregateException> exceptions = [];
            if (A2BException is not null) exceptions.Add(A2BException);
            if (B2AException is not null) exceptions.Add(B2AException);
            throw new AggregateException(exceptions);
        }

        /// <summary>
        /// Run the communication
        /// </summary>
        protected virtual void RunCommunication()
        {
            A2B = PermanentCopyNonBlockingAsync(A, B);
            B2A = PermanentCopyNonBlockingAsync(B, A);
        }

        /// <summary>
        /// Copy a non-blocking source stream to a target stream until interrupted
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="target">Target</param>
        protected async Task PermanentCopyNonBlockingAsync(Stream source, Stream target)
        {
            try
            {
                using RentedMemory<byte> buffer = new(BufferSize, clean: false);
                int red;
                while (!Cancellation.IsCancellationRequested)
                {
                    red = await source.ReadAsync(buffer.Memory, Cancellation.Token).DynamicContext();
                    if (red > 0)
                    {
                        await target.WriteAsync(buffer.Memory[..red], Cancellation.Token).DynamicContext();
                        continue;
                    }
                    await Task.Delay(DataDelay, Cancellation.Token).DynamicContext();
                }
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken.IsEqualTo(Cancellation.Token))
            {
            }
        }

        /// <summary>
        /// Run the observer
        /// </summary>
        protected virtual async Task RunObserverAsync()
        {
            Contract.Assert(A2B is not null && B2A is not null);
            await Task.WhenAny(A2B, B2A).DynamicContext();
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync(CancellationToken.None).DynamicContext())
                if (!Cancellation.IsCancellationRequested)
                    await Cancellation.CancelAsync().DynamicContext();
            await Task.WhenAll(A2B, B2A).DynamicContext();
            Stopped = DateTime.Now;
            A2BException = A2B.Exception;
            B2AException = B2A.Exception;
            if (IsExceptional) RaiseOnError();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            using (SemaphoreSyncContext ssc = Sync)
                if (!Cancellation.IsCancellationRequested)
                    Cancellation.Cancel();
            Observer?.GetAwaiter().GetResult();
            Cancellation.Dispose();
            Sync.Dispose();
            if (LeaveOpen) return;
            A.Dispose();
            B.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync().DynamicContext())
                if (!Cancellation.IsCancellationRequested)
                    await Cancellation.CancelAsync().DynamicContext();
            if (Observer is not null) await Observer.DynamicContext();
            Cancellation.Dispose();
            Sync.Dispose();
            if (LeaveOpen) return;
            await A.DisposeAsync().DynamicContext();
            await B.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Delegate for an <see cref="OnError"/> handler
        /// </summary>
        /// <param name="proxy">Proxy</param>
        /// <param name="e">Arguments</param>
        public delegate void Error_Delegate(StreamProxy proxy, EventArgs e);
        /// <summary>
        /// Raised on error
        /// </summary>
        public event Error_Delegate? OnError;
        /// <summary>
        /// Raise the <see cref="OnError"/> event
        /// </summary>
        protected virtual void RaiseOnError() => OnError?.Invoke(this, EventArgs.Empty);
    }
}
