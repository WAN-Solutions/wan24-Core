using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Change callback
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="callback">Callback</param>
    /// <param name="state">State</param>
    public class ChangeCallback(in Action<object?> callback, in object? state) : DisposableBase()
    {
        /// <summary>
        /// Callback
        /// </summary>
        public Action<object?> Callback { get; } = callback;

        /// <summary>
        /// State
        /// </summary>
        public object? State { get; } = state;

        /// <summary>
        /// Invoke the callback
        /// </summary>
        [TargetedPatchingOptOut("Just a method adapter")]
        public virtual void Invoke() => Callback(State);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) { }
    }
}
