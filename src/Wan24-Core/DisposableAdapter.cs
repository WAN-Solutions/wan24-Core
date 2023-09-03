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
        /// Constructor
        /// </summary>
        /// <param name="disposer">Disposer</param>
        /// <param name="asyncDisposer">Asynchronous disposer</param>
        public DisposableAdapter(
            in DisposableWrapper<DisposableAdapter>.Dispose_Delegate disposer,
            in DisposableWrapper<DisposableAdapter>.DisposeAsync_Delegate? asyncDisposer = null
            ) : base()
        {
            Disposer = disposer;
            AsyncDisposer = asyncDisposer;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Disposer(disposing);

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
