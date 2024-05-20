namespace wan24.Core
{
    /// <summary>
    /// Base class for a lazy volatile value
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public abstract class LazyVolatileValueBase<T>() : VolatileValueBase<T>()
    {
        /// <summary>
        /// Value request event (raised when <see cref="CurrentValue"/> was accessed; used from <see cref="VolatileValueBase{T}.SetCurrentValue"/>)
        /// </summary>
        protected readonly ResetEvent ValueRequestEvent = new();

        /// <inheritdoc/>
        public override Task<T> CurrentValue
        {
            get
            {
                if (!IsDisposing)
                    ValueRequestEvent.Set();
                return base.CurrentValue;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            ValueRequestEvent.Set();
            base.Dispose(disposing);
            ValueRequestEvent.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await ValueRequestEvent.SetAsync().DynamicContext();
            await base.DisposeCore().DynamicContext();
            await ValueRequestEvent.DisposeAsync().DynamicContext();
        }
    }
}
