namespace wan24.Core
{
    /// <summary>
    /// Change callback
    /// </summary>
    public class ChangeCallback : DisposableBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State</param>
        public ChangeCallback(Action<object?> callback, object? state) : base()
        {
            Callback = callback;
            State = state;
        }

        /// <summary>
        /// Callback
        /// </summary>
        public Action<object?> Callback { get; }

        /// <summary>
        /// State
        /// </summary>
        public object? State { get; }

        /// <summary>
        /// Invoke the callback
        /// </summary>
        public virtual void Invoke() => Callback(State);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) { }
    }
}
