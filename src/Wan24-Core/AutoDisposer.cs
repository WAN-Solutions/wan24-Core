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
    public class AutoDisposer<T>(in T obj) : DisposableBase()
    {
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
        /// <see cref="Object"/> usage count
        /// </summary>
        public int UsageCount => _UsageCount;

        /// <summary>
        /// If the <see cref="Object"/> is in use at present
        /// </summary>
        public bool IsInUse => _UsageCount > 0;

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
        /// <returns>Usage context (don't forget to dispose to release the <see cref="Object"/> after use!)</returns>
        /// <exception cref="InvalidOperationException">Should be disposed</exception>
        public virtual Context UseObject()
        {
            EnsureUndisposed();
            if (_ShouldDispose)
                throw new InvalidOperationException();
            _UsageCount++;
            if (_ShouldDispose && --_UsageCount < 1)
                Dispose();
            EnsureUndisposed();
            return new(this);
        }

        /// <summary>
        /// Use the <see cref="Object"/>
        /// </summary>
        /// <returns>Usage context (don't forget to dispose to release the <see cref="Object"/> after use!)</returns>
        /// <exception cref="InvalidOperationException">Should be disposed</exception>
        public virtual async Task<Context> UseObjectAsync()
        {
            EnsureUndisposed();
            if (_ShouldDispose)
                throw new InvalidOperationException();
            _UsageCount++;
            if (_ShouldDispose && --_UsageCount < 1)
                await DisposeAsync().DynamicContext();
            EnsureUndisposed();
            return new(this);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Object.TryDispose();

        /// <inheritdoc/>
        protected override async Task DisposeCore() => await Object.TryDisposeAsync().DynamicContext();

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
        public class Context(in AutoDisposer<T> disposer) : DisposableBase()
        {
            /// <summary>
            /// Disposer
            /// </summary>
            public AutoDisposer<T> Disposer { get; } = disposer;

            /// <summary>
            /// Object
            /// </summary>
            public T Object => Disposer.Object;

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                if (--Disposer._UsageCount < 1 && Disposer._ShouldDispose)
                    Disposer.Dispose();
            }

            /// <inheritdoc/>
            protected override async Task DisposeCore()
            {
                if (--Disposer._UsageCount < 1 && Disposer._ShouldDispose)
                    await Disposer.DisposeAsync().DynamicContext();
            }

            /// <summary>
            /// Cast as object
            /// </summary>
            /// <param name="context">Context</param>
            public static implicit operator T(in Context context) => context.Object;
        }
    }
}
