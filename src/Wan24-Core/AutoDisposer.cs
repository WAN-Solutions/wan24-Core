namespace wan24.Core
{
    /// <summary>
    /// Automatic disposer (disposes an object and itself after use)
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="obj">Object</param>
    public class AutoDisposer<T>(in T obj) : BasicAllDisposableBase()
    {
        /// <summary>
        /// No usage event (raised when the <see cref="Object"/> isn't in use)
        /// </summary>
        protected readonly ResetEvent NoUsageEvent = new(initialState: true);
        /// <summary>
        /// <see cref="Object"/> usage count
        /// </summary>
        protected volatile int _UsageCount = 0;
        /// <summary>
        /// If the <see cref="Object"/> should be disposed after use
        /// </summary>
        protected volatile bool _ShouldDispose = false;

        /// <summary>
        /// Hosted object
        /// </summary>
        public T Object { get; } = obj ?? throw new ArgumentNullException(nameof(obj));

        /// <summary>
        /// If only exclusive object usage is allowed
        /// </summary>
        public bool OnlyExclusiveObjectUsage { get; init; }

        /// <summary>
        /// <see cref="Object"/> usage count
        /// </summary>
        public int UsageCount => _UsageCount;

        /// <summary>
        /// If the <see cref="Object"/> is in use at present
        /// </summary>
        public bool IsInUse => _UsageCount > 0;

        /// <summary>
        /// Active contexts
        /// </summary>
        public ConcurrentList<Context> ActiveContexts { get; } = [];

        /// <summary>
        /// If the <see cref="Object"/> should be disposed after use (can only be set to <see langword="true"/>!)
        /// </summary>
        public virtual bool ShouldDispose
        {
            get => _ShouldDispose;
            set
            {
                if (_ShouldDispose || !EnsureUndisposed(allowDisposing: false, throwException: false))
                {
                    if (!value) throw new ArgumentException("Value must be TRUE", nameof(value));
                    return;
                }
                if (!value) throw new ArgumentException("Value must be TRUE", nameof(value));
                _ShouldDispose = true;
                if (_UsageCount < 1)
                    Dispose();
            }
        }

        /// <summary>
        /// Set <see cref="ShouldDispose"/> to <see langword="true"/> and dispose, if possible
        /// </summary>
        public virtual async Task SetShouldDisposeAsync()
        {
            if (_ShouldDispose || !EnsureUndisposed(allowDisposing: false, throwException: false))
                return;
            _ShouldDispose = true;
            if (_UsageCount < 1)
                await DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Use the <see cref="Object"/>
        /// </summary>
        /// <param name="usage">Usage</param>
        /// <returns>Usage context (don't forget to dispose to release the <see cref="Object"/> after use!)</returns>
        /// <exception cref="InvalidOperationException">Should be disposed</exception>
        public virtual Context UseObject(in string? usage = null)
        {
            EnsureUndisposed();
            if (OnlyExclusiveObjectUsage) return UseObjectExclusive(usage);
            NoUsageEvent.Reset();
            if (_ShouldDispose)
                throw new InvalidOperationException();
            _UsageCount++;
            if (_ShouldDispose && --_UsageCount < 1)
                Dispose();
            EnsureUndisposed();
            Context res = new(this, usage);
            ActiveContexts.Add(res);
            return res;
        }

        /// <summary>
        /// Use the <see cref="Object"/>
        /// </summary>
        /// <param name="usage">Usage</param>
        /// <returns>Usage context (don't forget to dispose to release the <see cref="Object"/> after use!)</returns>
        /// <exception cref="InvalidOperationException">Should be disposed</exception>
        public virtual async Task<Context> UseObjectAsync(string? usage = null)
        {
            EnsureUndisposed();
            if (OnlyExclusiveObjectUsage) return await UseObjectExclusiveAsync(usage).DynamicContext();
            await NoUsageEvent.ResetAsync().DynamicContext();
            if (_ShouldDispose)
                throw new InvalidOperationException();
            _UsageCount++;
            if (_ShouldDispose && --_UsageCount < 1)
                await DisposeAsync().DynamicContext();
            EnsureUndisposed();
            Context res = new(this, usage);
            ActiveContexts.Add(res);
            return res;
        }

        /// <summary>
        /// Use the <see cref="Object"/> exclusive
        /// </summary>
        /// <param name="usage">Usage</param>
        /// <returns>Usage context (don't forget to dispose to release the <see cref="Object"/> after use!)</returns>
        /// <exception cref="InvalidOperationException">Should be disposed</exception>
        public virtual Context UseObjectExclusive(in string? usage = null)
        {
            EnsureUndisposed();
            NoUsageEvent.WaitAndReset();
            if (_ShouldDispose)
                throw new InvalidOperationException();
            _UsageCount++;
            if (_ShouldDispose && --_UsageCount < 1)
                Dispose();
            EnsureUndisposed();
            Context res = new(this, usage);
            ActiveContexts.Add(res);
            return res;
        }

        /// <summary>
        /// Use the <see cref="Object"/> exclusive
        /// </summary>
        /// <param name="usage">Usage</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Usage context (don't forget to dispose to release the <see cref="Object"/> after use!)</returns>
        /// <exception cref="InvalidOperationException">Should be disposed</exception>
        public virtual async Task<Context> UseObjectExclusiveAsync(string? usage = null, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await NoUsageEvent.WaitAndResetAsync(cancellationToken).DynamicContext();
            if (_ShouldDispose)
                throw new InvalidOperationException();
            _UsageCount++;
            if (_ShouldDispose && --_UsageCount < 1)
                await DisposeAsync().DynamicContext();
            EnsureUndisposed();
            Context res = new(this, usage);
            ActiveContexts.Add(res);
            return res;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            NoUsageEvent.Dispose();
            Object.TryDispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await NoUsageEvent.DisposeAsync().DynamicContext();
            await Object.TryDisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Cast as object
        /// </summary>
        /// <param name="disposer">Disposer</param>
        public static implicit operator T(in AutoDisposer<T> disposer) => disposer.Object;

        /// <summary>
        /// Usage context
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="disposer">Disposer</param>
        /// <param name="usage">Usage</param>
        public class Context(in AutoDisposer<T> disposer, in string? usage) : BasicAllDisposableBase()
        {
            /// <summary>
            /// Disposer
            /// </summary>
            public AutoDisposer<T> Disposer { get; } = disposer;

            /// <summary>
            /// Object
            /// </summary>
            public T Object => Disposer.Object;

            /// <summary>
            /// Usage
            /// </summary>
            public string? Usage { get; set; } = usage;

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                Disposer.ActiveContexts.Remove(this);
                if (--Disposer._UsageCount < 1)
                {
                    Disposer.NoUsageEvent.Set();
                    if (Disposer._ShouldDispose)
                        Disposer.Dispose();
                }
            }

            /// <inheritdoc/>
            protected override async Task DisposeCore()
            {
                Disposer.ActiveContexts.Remove(this);
                if (--Disposer._UsageCount < 1)
                {
                    await Disposer.NoUsageEvent.SetAsync().DynamicContext();
                    if (Disposer._ShouldDispose)
                        await Disposer.DisposeAsync().DynamicContext();
                }
            }

            /// <summary>
            /// Cast as object
            /// </summary>
            /// <param name="context">Context</param>
            public static implicit operator T(in Context context) => context.Object;
        }
    }
}
