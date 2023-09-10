using System.Runtime;

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
        public ChangeCallback(in Action<object?> callback, in object? state) : base()
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
        [TargetedPatchingOptOut("Just a method adapter")]
        public virtual void Invoke() => Callback(State);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) { }
    }
}
