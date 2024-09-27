namespace wan24.Core
{
    /// <summary>
    /// Asynchronous event interface
    /// </summary>
    /// <typeparam name="tSender">Sender type</typeparam>
    /// <typeparam name="tArgs">Arguments type</typeparam>
    public interface IAsyncEvent<tSender, tArgs>
        where tSender : class
        where tArgs : EventArgs, new()
    {
        /// <summary>
        /// Sender
        /// </summary>
        tSender? Sender { get; }
        /// <summary>
        /// Event handler timeout
        /// </summary>
        TimeSpan? Timeout { get; set; }
        /// <summary>
        /// Cancellation
        /// </summary>
        CancellationToken? Cancellation { get; set; }
        /// <summary>
        /// Event handlers
        /// </summary>
        HashSet<EventHandler_Delegate> EventHandlers { get; }
        /// <summary>
        /// Event handlers
        /// </summary>
        HashSet<EventHandlerAsync_Delegate> AsyncEventHandlers { get; }
        /// <summary>
        /// Detach all event handlers
        /// </summary>
        void DetachAll();
        /// <summary>
        /// Raise the event
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RaiseEventAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Raise the event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RaiseEventAsync(tSender sender, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Raise the event
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RaiseEventAsync(tArgs args, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Raise the event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RaiseEventAsync(tSender sender, tArgs args, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Event handler delegate
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate void EventHandler_Delegate(tSender sender, tArgs args, CancellationToken cancellationToken);
        /// <summary>
        /// Event handler delegate
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate Task EventHandlerAsync_Delegate(tSender sender, tArgs args, CancellationToken cancellationToken);
    }
}
