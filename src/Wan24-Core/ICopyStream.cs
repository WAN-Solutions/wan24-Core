namespace wan24.Core
{
    /// <summary>
    /// Interface for a <see cref="CopyStream"/>
    /// </summary>
    public interface ICopyStream : IStream
    {
        /// <summary>
        /// Target stream
        /// </summary>
        Stream CopyTarget { get; }
        /// <summary>
        /// If to leave the target stream open when disposing
        /// </summary>
        bool LeaveTargetOpen { get; set; }
        /// <summary>
        /// If to dispose when the copy task did finish
        /// </summary>
        bool AutoDispose { get; set; }
        /// <summary>
        /// Copy background task
        /// </summary>
        Task CopyTask { get; }
        /// <summary>
        /// Last copy background task exception
        /// </summary>
        Exception? LastException { get; }
        /// <summary>
        /// Cancellation token (as given to the constructor)
        /// </summary>
        CancellationToken CancelToken { get; }
        /// <summary>
        /// The copy cancellation token (canceled using <see cref="CancelCopy"/>)
        /// </summary>
        CancellationToken CopyCancellation { get; }
        /// <summary>
        /// Combined cancellation token (<see cref="CancelToken"/> and <see cref="CopyCancellation"/>)
        /// </summary>
        CancellationToken Cancellations { get; }
        /// <summary>
        /// If the copy did complete
        /// </summary>
        bool IsCopyCompleted { get; }
        /// <summary>
        /// Cancel the copy background task
        /// </summary>
        void CancelCopy();
        /// <summary>
        /// Raised on error
        /// </summary>
        event CopyStream_Delegate? OnError;
        /// <summary>
        /// Raised on completed
        /// </summary>
        event CopyStream_Delegate? OnComplete;
        /// <summary>
        /// Delegate for an event handler
        /// </summary>
        /// <param name="stream">Sender</param>
        /// <param name="e">Arguments</param>
        public delegate void CopyStream_Delegate(ICopyStream stream, EventArgs e);
    }
}
