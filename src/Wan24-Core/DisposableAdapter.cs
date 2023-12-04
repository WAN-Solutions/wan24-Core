namespace wan24.Core
{
    /// <summary>
    /// Disposable adapter
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="disposer">Disposer</param>
    /// <param name="asyncDisposer">Asynchronous disposer</param>
    public sealed class DisposableAdapter(
        in DisposableWrapper<DisposableAdapter>.Dispose_Delegate disposer,
        in DisposableWrapper<DisposableAdapter>.DisposeAsync_Delegate? asyncDisposer = null
            )
        : DisposableBase(asyncDisposing: asyncDisposer is not null)
    {
        /// <summary>
        /// Disposer
        /// </summary>
        private readonly DisposableWrapper<DisposableAdapter>.Dispose_Delegate Disposer = disposer;
        /// <summary>
        /// Asynchronous disposer
        /// </summary>
        private readonly DisposableWrapper<DisposableAdapter>.DisposeAsync_Delegate? AsyncDisposer = asyncDisposer;
        /// <summary>
        /// Disposed from the destructor?
        /// </summary>
        private bool DisposedFromDestructor = false;

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
