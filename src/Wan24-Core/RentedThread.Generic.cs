using System.Diagnostics.Contracts;

namespace wan24.Core
{
    /// <summary>
    /// Rented thread (can be used with <see cref="ObjectPool{T}"/> and <see cref="RentedObject{T}"/>)
    /// </summary>
    /// <typeparam name="T">Worker return value type</typeparam>
    public class RentedThread<T> : SimpleDisposableBase, IObjectPoolItem
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Work event (raised when work is available; reset when work was done)
        /// </summary>
        protected readonly ResetEvent WorkEvent = new();
        /// <summary>
        /// Work done event (raised when work was done; reset when working)
        /// </summary>
        protected readonly ResetEvent WorkDoneEvent = new(initialState: true);
        /// <summary>
        /// Default options
        /// </summary>
        protected readonly RentedThreadOptions Options;
        /// <summary>
        /// Managed thread
        /// </summary>
        protected readonly Thread ManagedThread;
        /// <summary>
        /// Worker
        /// </summary>
        protected Worker_Delegate? Worker = null;
        /// <summary>
        /// Worker completion
        /// </summary>
        protected volatile TaskCompletionSource<T>? WorkerCompletion = null;
        /// <summary>
        /// Worker cancellation token
        /// </summary>
        protected CancellationToken WorkerCancellation = default;

        /// <summary>
        /// Constructor
        /// </summary>
        public RentedThread() : this(options: null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Options</param>
        public RentedThread(RentedThreadOptions? options) : base()
        {
            Options = options ?? new();
            ManagedThread = new(ThreadWorker);
            ApplyDefaultOptions();
        }

        /// <summary>
        /// Thread name (will be reset, if the rented thread is returned to the pool)
        /// </summary>
        public string? Name
        {
            get => IfUndisposed(ManagedThread.Name);
            set => IfUndisposed(() => ManagedThread.Name = value);
        }

        /// <summary>
        /// If the managed thread is a background thread
        /// </summary>
        public bool IsBackground
        {
            get => IfUndisposed(ManagedThread.IsBackground);
            set => IfUndisposed(() => ManagedThread.IsBackground = value);
        }

        /// <summary>
        /// Managed thread priority
        /// </summary>
        public ThreadPriority Priority
        {
            get => IfUndisposed(ManagedThread.Priority);
            set => IfUndisposed(() => ManagedThread.Priority = value);
        }

        /// <summary>
        /// Any tagged object (will be set to <see langword="null"/> when returned to the pool)
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Last exception of the thread worker
        /// </summary>
        public Exception? LastException { get; protected set; }

        /// <summary>
        /// If working
        /// </summary>
        public virtual bool IsWorking => !IsDisposed && WorkEvent.IsSet;

        /// <summary>
        /// If can work
        /// </summary>
        public virtual bool CanWork => !IsDisposing && WorkDoneEvent.IsSet;

        /// <summary>
        /// Current worker task
        /// </summary>
        public Task<T>? CurrentWorkerTask => IfUndisposed(WorkerCompletion?.Task);

        /// <summary>
        /// Work
        /// </summary>
        /// <param name="worker">Worker</param>
        /// <param name="cancellationToken">Cancellation token (used for thread synchronization only)</param>
        /// <returns>Worker return value</returns>
        public virtual async Task<T> WorkAsync(Worker_Delegate worker, CancellationToken cancellationToken = default)
        {
            while (EnsureUndisposed())
            {
                // Wait for another work
                await WorkDoneEvent.WaitAsync(cancellationToken).DynamicContext();
                EnsureUndisposed();
                // Try using the thread
                using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
                if (!CanWork) continue;
                await WorkDoneEvent.ResetAsync(CancellationToken.None).DynamicContext();
                Worker = worker;
                WorkerCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
                WorkerCancellation = cancellationToken;
                break;
            }
            try
            {
                // Start working and wait for the worker to finish
                Contract.Assert(WorkerCompletion is not null);
                LastException = null;
                if (!ManagedThread.IsAlive) ManagedThread.Start();
                await WorkEvent.SetAsync(CancellationToken.None).DynamicContext();
                return await WorkerCompletion.Task.DynamicContext();
            }
            finally
            {
                // Reset this object for the next worker
                WorkerCancellation = default;
                WorkerCompletion = null;
                Worker = null;
                await WorkDoneEvent.SetAsync(CancellationToken.None).DynamicContext();
            }
        }

        /// <inheritdoc/>
        void IObjectPoolItem.Reset()
        {
            EnsureUndisposed();
            WorkDoneEvent.Wait();
            ApplyDefaultOptions();
            Name = null;
            Tag = null;
        }

        /// <summary>
        /// Apply the default options
        /// </summary>
        protected virtual void ApplyDefaultOptions()
        {
            ManagedThread.IsBackground = Options.IsBackground;
            ManagedThread.Priority = Options.Priority;
        }

        /// <summary>
        /// Thread worker
        /// </summary>
        protected virtual void ThreadWorker()
        {
            using CancellationTokenSource cts = new();
            void HandleDisposing(IDisposableObject sender, EventArgs e) => cts.Cancel();
            OnDisposing += HandleDisposing;
            try
            {
                while (EnsureUndisposed())
                {
                    // Wait for work
                    WorkEvent.Wait();
                    EnsureUndisposed();
                    // Run the worker
                    Contract.Assert(Worker is not null);
                    Contract.Assert(WorkerCompletion is not null);
                    try
                    {
                        using CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, WorkerCancellation);
                        T value = Worker.Invoke(this, cancellation.Token);
                        WorkEvent.Reset();
                        WorkerCompletion.SetResult(value);
                    }
                    catch (Exception ex)
                    {
                        WorkEvent.Reset();
                        WorkerCompletion.SetException(ex);
                    }
                }
            }
            catch (ObjectDisposedException) when (IsDisposing)
            {
            }
            catch (Exception ex)
            {
                LastException = ex;
                ErrorHandling.Handle(new(ex, Options.ErrorSource, this));
                WorkerCompletion?.TrySetException(ex);
            }
            finally
            {
                WorkEvent.Reset();
                OnDisposing -= HandleDisposing;
                WorkerCompletion?.TrySetException(new InvalidProgramException("Worker thread finalizing"));
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            WorkEvent.Set();
            WorkDoneEvent.Wait();
            Sync.Dispose();
            ManagedThread.Join();
            WorkEvent.Dispose();
            WorkDoneEvent.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await WorkEvent.SetAsync().DynamicContext();
            await WorkDoneEvent.WaitAsync().DynamicContext();
            Sync.Dispose();
            ManagedThread.Join();
            await WorkEvent.DisposeAsync().DynamicContext();
            await WorkDoneEvent.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Delegate for a worker
        /// </summary>
        /// <param name="thread">Rented thread</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate T Worker_Delegate(RentedThread<T> thread, CancellationToken cancellationToken);
    }
}
