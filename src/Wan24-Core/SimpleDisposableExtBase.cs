namespace wan24.Core
{
    /// <summary>
    /// Extended simple disposable base class which implements <see cref="IWillDispose"/>
    /// </summary>
    public abstract class SimpleDisposableExtBase() : SimpleDisposableBase(), IWillDispose
    {
        /// <summary>
        /// Registered disposables
        /// </summary>
        protected readonly HashSet<object> RegisteredDisposables = [];

        /// <inheritdoc/>
        public virtual void RegisterForDispose<T>(in T disposable)
        {
            lock (SyncDispose)
            {
                EnsureUndisposed();
                ArgumentNullException.ThrowIfNull(disposable);
                if (disposable is not IDisposable && disposable is not IAsyncDisposable)
                    throw new ArgumentException("Not a disposable", nameof(disposable));
                RegisteredDisposables.Add(disposable);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if(RegisteredDisposables.Count>0)
                foreach(object obj in RegisteredDisposables)
                    switch (obj)
                    {
                        case IDisposableObject disposable:
                            if (!disposable.IsDisposing) disposable.Dispose();
                            break;
                        case IDisposable disposable:
                            disposable.Dispose();
                            break;
                        case IAsyncDisposable disposable:
                            disposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                            break;
                    }
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (RegisteredDisposables.Count > 0)
                foreach (object obj in RegisteredDisposables)
                    switch (obj)
                    {
                        case IDisposableObject disposable:
                            if (!disposable.IsDisposing) await disposable.DisposeAsync().DynamicContext();
                            break;
                        case IAsyncDisposable disposable:
                            await disposable.DisposeAsync().DynamicContext();
                            break;
                        case IDisposable disposable:
                            disposable.Dispose();
                            break;
                    }
        }
    }
}
