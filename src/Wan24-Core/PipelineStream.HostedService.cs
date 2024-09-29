namespace wan24.Core
{
    public partial class PipelineStream : IServiceWorker
    {
        /// <inheritdoc/>
        public bool IsRunning => Queue.IsRunning;

        /// <inheritdoc/>
        public bool IsPaused => Queue.IsPaused || !PauseEvent.IsSet;

        /// <inheritdoc/>
        public bool CanPause => true;

        /// <inheritdoc/>
        public DateTime Started => Queue.Started;

        /// <inheritdoc/>
        public DateTime Paused => Queue.Paused;

        /// <inheritdoc/>
        public DateTime Stopped => Queue.Stopped;

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            await SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            await Queue.StartAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            await SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            await Queue.StopAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public async Task StartAsync()
        {
            EnsureUndisposed();
            await SyncEvent.WaitAsync(Cancellation.Token).DynamicContext();
            await StartAsync(Cancellation.Token).DynamicContext();
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            EnsureUndisposed();
            await SyncEvent.WaitAsync(Cancellation.Token).DynamicContext();
            await StopAsync(Cancellation.Token).DynamicContext();
        }

        /// <inheritdoc/>
        public async Task RestartAsync()
        {
            EnsureUndisposed();
            await SyncEvent.WaitAsync(Cancellation.Token).DynamicContext();
            await ((IServiceWorker)Queue).RestartAsync().DynamicContext();
        }

        /// <inheritdoc/>
        public async Task PauseAsync()
        {
            EnsureUndisposed();
            await SyncEvent.WaitAsync(Cancellation.Token).DynamicContext();
            await PauseEvent.SetAsync(Cancellation.Token).DynamicContext();
            await Queue.PauseAsync(Cancellation.Token).DynamicContext();
        }

        /// <inheritdoc/>
        public async Task ResumeAsync()
        {
            EnsureUndisposed();
            await SyncEvent.WaitAsync(Cancellation.Token).DynamicContext();
            await Queue.ResumeAsync(Cancellation.Token).DynamicContext();
            await PauseEvent.ResetAsync(Cancellation.Token).DynamicContext();
        }
    }
}
