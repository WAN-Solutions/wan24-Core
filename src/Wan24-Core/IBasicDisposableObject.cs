namespace wan24.Core
{
    /// <summary>
    /// Interface for a basic disposable object (must implement <see cref="IDisposable"/> and/or <see cref="IAsyncDisposable"/>)
    /// </summary>
    public interface IBasicDisposableObject
    {
        /// <summary>
        /// Is disposing? (is <see langword="true"/> when disposed already, too!)
        /// </summary>
        bool IsDisposing { get; }
        /// <summary>
        /// Is disposed? (final object disposing state)
        /// </summary>
        bool IsDisposed { get; }
    }
}
