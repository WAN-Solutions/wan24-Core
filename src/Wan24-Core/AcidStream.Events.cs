namespace wan24.Core
{
    // Events
    public partial class AcidStream<T> where T : Stream
    {
        /// <summary>
        /// Delegate for an ACID stream event handler
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="e">Arguments</param>
        public delegate void AcidStreamEvent_Delegate(AcidStream<T> stream, EventArgs e);

        /// <summary>
        /// Raised before committing
        /// </summary>
        public event AcidStreamEvent_Delegate? OnBeforeCommit;
        /// <summary>
        /// Raise the <see cref="OnBeforeCommit"/> event
        /// </summary>
        protected virtual void RaiseOnBeforeCommit() => OnBeforeCommit?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raised after committed
        /// </summary>
        public event AcidStreamEvent_Delegate? OnAfterCommit;
        /// <summary>
        /// Raise the <see cref="OnAfterCommit"/> event
        /// </summary>
        protected virtual void RaiseOnAfterCommit() => OnAfterCommit?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raised before rolling back
        /// </summary>
        public event AcidStreamEvent_Delegate? OnBeforeRollback;
        /// <summary>
        /// Raise the <see cref="OnBeforeRollback"/> event
        /// </summary>
        protected virtual void RaiseOnBeforeRollback() => OnBeforeRollback?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raised after rolled back
        /// </summary>
        public event AcidStreamEvent_Delegate? OnAfterRollback;
        /// <summary>
        /// Raise the <see cref="OnAfterRollback"/> event
        /// </summary>
        protected virtual void RaiseOnAfterRollback() => OnAfterRollback?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raised when a commit is required (only when <see cref="AutoCommit"/> is <see langword="false"/>, and only once until committed)
        /// </summary>
        public event AcidStreamEvent_Delegate? OnNeedCommit;
        /// <summary>
        /// Raise the <see cref="OnNeedCommit"/> event
        /// </summary>
        protected virtual void RaiseOnNeedCommit() => OnNeedCommit?.Invoke(this, EventArgs.Empty);
    }
}
