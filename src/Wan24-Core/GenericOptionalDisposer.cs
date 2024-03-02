namespace wan24.Core
{
    /// <summary>
    /// Generic optional disposer
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="obj">Object to dispose</param>
    /// <param name="dispose">Dispose the object?</param>
    public sealed class GenericOptionalDisposer<T>(in T? obj, in bool dispose = true) : DisposableBase() where T : notnull
    {
        /// <summary>
        /// Object to dispose
        /// </summary>
        private T? _Object = obj;

        /// <summary>
        /// Object to dispose
        /// </summary>
        public T? Object
        {
            get => IfUndisposed(_Object, allowDisposing: true);
            set
            {
                EnsureUndisposed();
                _Object = value;
            }
        }

        /// <summary>
        /// Dispose the object?
        /// </summary>
        public bool DisposeObject { get; set; } = dispose;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => _Object?.Equals(obj) ?? obj is null;

        /// <inheritdoc/>
        public override int GetHashCode() => _Object?.GetHashCode() ?? 0;

        /// <inheritdoc/>
        public override string ToString() => _Object?.ToString() ?? JsonHelper.NULL;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!DisposeObject || _Object is null) return;
            _Object.TryDispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (!DisposeObject || _Object is null) return;
            await _Object.TryDisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Cast as object
        /// </summary>
        /// <param name="disposer">Disposer</param>
        public static implicit operator T(in GenericOptionalDisposer<T> disposer) => disposer.Object ?? throw new InvalidOperationException("Can't cast NULL to object");

        /// <summary>
        /// Cast as disposer
        /// </summary>
        /// <param name="obj">Object</param>
        public static implicit operator GenericOptionalDisposer<T>(in T? obj) => new(obj);

        /// <summary>
        /// Cast as disposing flag
        /// </summary>
        /// <param name="disposer">Disposer</param>
        public static implicit operator bool(in GenericOptionalDisposer<T> disposer) => disposer.IsDisposing;
    }
}
