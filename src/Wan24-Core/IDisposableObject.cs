namespace wan24.Core
{
    /// <summary>
    /// Interface for a disposable object
    /// </summary>
    public interface IDisposableObject : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Is disposing?
        /// </summary>
        bool IsDisposing { get; }
        /// <summary>
        /// Is disposed?
        /// </summary>
        bool IsDisposed { get; }
        /// <summary>
        /// Delegate for the disposing events
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        public delegate void Dispose_Delegate(object sender, EventArgs e);
        /// <summary>
        /// Raised when disposing
        /// </summary>
        event Dispose_Delegate? OnDisposing;
        /// <summary>
        /// Raised when disposed
        /// </summary>
        event Dispose_Delegate? OnDisposed;
    }
}
