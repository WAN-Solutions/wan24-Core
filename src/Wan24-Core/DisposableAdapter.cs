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
        /// Any tagged object
        /// </summary>
        public object? Tag { get; set; }

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
                ErrorHandling.Handle(new(ex, ErrorHandling.DISPOSABLE_ADAPTER_ERROR, this));
                Dispose();
            }
        }

        /// <summary>
        /// Ensure an undisposed state
        /// </summary>
        /// <param name="allowDisposing">Allow disposing?</param>
        /// <param name="throwException">Throw an axception when disposing/disposed?</param>
        /// <returns>if disposing/disposed</returns>
        public bool EnsureNotDisposed(in bool allowDisposing = false, in bool throwException = true) => EnsureUndisposed(allowDisposing, throwException);

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
