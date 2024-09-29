namespace wan24.Core
{
    /// <summary>
    /// Interface for an event throttle
    /// </summary>
    public interface IEventThrottle : IBasicDisposableObject, IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Raise the event
        /// </summary>
        /// <returns>Was raised?</returns>
        bool Raise();
    }
}
