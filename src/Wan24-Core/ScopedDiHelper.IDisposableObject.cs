namespace wan24.Core
{
    // IDisposableObject implementation
    public partial class ScopedDiHelper : IDisposableObject
    {
        /// <summary>
        /// Disposable adapter
        /// </summary>
        protected readonly DisposableAdapter DisposableAdapter;

        /// <summary>
        /// Destructor
        /// </summary>
        ~ScopedDiHelper() => DisposableAdapter.DisposeFromDestructor();

        /// <inheritdoc/>
        public bool IsDisposing => DisposableAdapter.IsDisposing;

        /// <inheritdoc/>
        public bool IsDisposed => DisposableAdapter.IsDisposed;

        /// <inheritdoc/>
        public void Dispose()
        {
            DisposableAdapter.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await DisposableAdapter.DisposeAsync().DynamicContext();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposing
        {
            add => DisposableAdapter.OnDisposing += value;
            remove => DisposableAdapter.OnDisposing -= value;
        }

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposed
        {
            add => DisposableAdapter.OnDisposed += value;
            remove => DisposableAdapter.OnDisposed -= value;
        }
    }
}
