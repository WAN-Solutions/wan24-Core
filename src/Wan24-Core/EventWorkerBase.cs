namespace wan24.Core
{
    /// <summary>
    /// Base class for an event worker
    /// </summary>
    public abstract class EventWorkerBase : HostedServiceBase
    {
        /// <summary>
        /// Work event (raised when having work)
        /// </summary>
        protected readonly ResetEvent WorkEvent = new();

        /// <summary>
        /// Constructor
        /// </summary>
        protected EventWorkerBase() : base() => CanPause = true;

        /// <summary>
        /// Is working?
        /// </summary>
        public virtual bool IsWorking => EnsureUndisposed() && IsRunning && !IsPaused && WorkEvent.IsSet;

        /// <summary>
        /// Raise the work event
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual void RaiseWorkEvent(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            WorkEvent.Set(cancellationToken);
        }

        /// <summary>
        /// Raise the work event
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task RaiseWorkEventAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await WorkEvent.SetAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
        {
            for (; !CancelToken.IsCancellationRequested;)
            {
                await PauseEvent.WaitAsync(CancelToken).DynamicContext();
                await WorkEvent.WaitAndResetAsync(CancelToken).DynamicContext();
                for (; !CancelToken.IsCancellationRequested;)
                {
                    await EventWorkerAsync().DynamicContext();
                    if (!WorkEvent.IsSet) break;
                    await PauseEvent.WaitAsync(CancelToken).DynamicContext();
                    if (!WorkEvent.IsSet || CancelToken.IsCancellationRequested) break;
                    await WorkEvent.ResetAsync(CancelToken).DynamicContext();
                }
            }
        }

        /// <summary>
        /// Worker method (to override and implement the actual work to do, if the work event was raised)
        /// </summary>
        protected abstract Task EventWorkerAsync();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            WorkEvent.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            await WorkEvent.DisposeAsync().DynamicContext();
        }
    }
}
