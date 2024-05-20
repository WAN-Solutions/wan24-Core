namespace wan24.Core
{
    /// <summary>
    /// Base class for a lazy volatile value which will timeout
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class LazyVolatileTimeoutValueBase<T> : LazyVolatileValueBase<T>
    {
        /// <summary>
        /// Timeout timer (should be stopped on reset and started when the current value was set)
        /// </summary>
        protected readonly Timeout Timer;

        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="timeout">Timeout</param>
        protected LazyVolatileTimeoutValueBase(in TimeSpan timeout) : base()
        {
            Timer = new(timeout);
            Timer.OnTimeout += HandleTimeout;
        }

        /// <inheritdoc/>
        public override Task<T> CurrentValue
        {
            get
            {
                if (!IsDisposed)
                    Timer.Reset();
                return base.CurrentValue;
            }
        }

        /// <summary>
        /// Handle a timeout
        /// </summary>
        /// <param name="timer">Timer</param>
        /// <param name="e">Arguments</param>
        protected virtual void HandleTimeout(Timeout timer, EventArgs e) => Reset();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Timer.Dispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await Timer.DisposeAsync().DynamicContext();
            await base.DisposeCore().DynamicContext();
        }
    }
}
