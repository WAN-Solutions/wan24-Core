namespace wan24.Core
{
    /// <summary>
    /// Optional disposer
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="obj">Object to dispose</param>
    /// <param name="dispose">Dispose the object?</param>
    public sealed class OptionalDisposer(in object? obj, in bool dispose = true) : DisposableBase()
    {
        /// <summary>
        /// Object to dispose
        /// </summary>
        private object? _Object = obj;

        /// <summary>
        /// Object to dispose
        /// </summary>
        public object? Object
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
        public override string ToString() => _Object?.ToString()! ?? JsonHelper.NULL;

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
        /// Cast as disposing flag
        /// </summary>
        /// <param name="disposer">Disposer</param>
        public static implicit operator bool(in OptionalDisposer disposer) => disposer.IsDisposing;
    }
}
