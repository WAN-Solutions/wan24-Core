namespace wan24.Core
{
    /// <summary>
    /// Disposable adapter
    /// </summary>
    public sealed class DisposableAdapter : DisposableBase
    {
        /// <summary>
        /// Disposer
        /// </summary>
        private readonly DisposableWrapper<DisposableAdapter>.Dispose_Delegate Disposer;
        /// <summary>
        /// Asynchronous disposer
        /// </summary>
        private readonly DisposableWrapper<DisposableAdapter>.DisposeAsync_Delegate? AsyncDisposer;
        /// <summary>
        /// Disposed from the destructor?
        /// </summary>
        private bool DisposedFromDestructor = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="disposer">Disposer</param>
        /// <param name="asyncDisposer">Asynchronous disposer</param>
        public DisposableAdapter(
            in DisposableWrapper<DisposableAdapter>.Dispose_Delegate disposer,
            in DisposableWrapper<DisposableAdapter>.DisposeAsync_Delegate? asyncDisposer = null
            ) : base(asyncDisposing: asyncDisposer is not null)
        {
            Disposer = disposer;
            AsyncDisposer = asyncDisposer;
        }

        /// <summary>
        /// Dispose at destruction time
        /// </summary>
        public void DisposeFromDestructor()
        {
            try
            {
                using (SemaphoreSyncContext ssc = new(DisposeSyncObject))
                {
                    ssc.Sync();
                    if (IsDisposing || DisposedFromDestructor) return;
                    DisposedFromDestructor = true;
                }
                Dispose();
            }
            catch (Exception ex)
            {
                ErrorHandling.Handle(new(ex, ErrorHandling.DISPOSABLE_ADAPTER_ERROR));
                Dispose();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Disposer(disposing && !DisposedFromDestructor);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (AsyncDisposer is not null)
            {
                await AsyncDisposer().DynamicContext();
            }
            else
            {
                Dispose(disposing: true);
            }
        }
    }
}
