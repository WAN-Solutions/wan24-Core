using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Asynchronous event
    /// </summary>
    /// <typeparam name="tSender">Sender type</typeparam>
    /// <typeparam name="tArgs">Arguments type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="sender">Sender</param>
    /// <param name="timeout">Event handler timeout</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public sealed class AsyncEvent<tSender, tArgs>(in tSender? sender = null, in TimeSpan? timeout = null, in CancellationToken? cancellationToken = null) : IAsyncEvent<tSender, tArgs>
        where tSender : class
        where tArgs : EventArgs, new()
    {
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        private readonly object SyncObject = new();
        /// <summary>
        /// Event handlers
        /// </summary>
        private readonly HashSet<IAsyncEvent<tSender, tArgs>.EventHandler_Delegate> EventHandlers = [];
        /// <summary>
        /// Event handlers
        /// </summary>
        private readonly HashSet<IAsyncEvent<tSender, tArgs>.EventHandlerAsync_Delegate> AsyncEventHandlers = [];
        /// <summary>
        /// Sender
        /// </summary>
        private readonly tSender? Sender = sender;
        /// <summary>
        /// Event handler timeout
        /// </summary>
        private TimeSpan? Timeout = timeout;
        /// <summary>
        /// Cancellation
        /// </summary>
        private CancellationToken? Cancellation = cancellationToken;
        /// <summary>
        /// Number of times the event was raised
        /// </summary>
        private volatile int _RaiseCount = 0;

        /// <summary>
        /// First raised
        /// </summary>
        public DateTime FirstRaised { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Last raised
        /// </summary>
        public DateTime LastRaised { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Number of times the event was raised
        /// </summary>
        public int RaiseCount => _RaiseCount;

        /// <summary>
        /// Has event handlers?
        /// </summary>
        public bool HasHandlers => EventHandlers.Count != 0 || AsyncEventHandlers.Count != 0;

        /// <summary>
        /// Get the abstract interface of this instance
        /// </summary>
        public IAsyncEvent<tSender, tArgs> Abstract => this;

        /// <inheritdoc/>
        tSender? IAsyncEvent<tSender, tArgs>.Sender => Sender;

        /// <inheritdoc/>
        TimeSpan? IAsyncEvent<tSender, tArgs>.Timeout { get => Timeout; set => Timeout = value; }

        /// <inheritdoc/>
        CancellationToken? IAsyncEvent<tSender, tArgs>.Cancellation { get => Cancellation; set => Cancellation = value; }

        /// <inheritdoc/>
        HashSet<IAsyncEvent<tSender, tArgs>.EventHandler_Delegate> IAsyncEvent<tSender, tArgs>.EventHandlers => EventHandlers;

        /// <inheritdoc/>
        HashSet<IAsyncEvent<tSender, tArgs>.EventHandlerAsync_Delegate> IAsyncEvent<tSender, tArgs>.AsyncEventHandlers => AsyncEventHandlers;

        /// <summary>
        /// Add an event handler
        /// </summary>
        /// <param name="handler">Handler</param>
        /// <returns>Added?</returns>
        public bool Listen(in IAsyncEvent<tSender, tArgs>.EventHandler_Delegate handler)
        {
            lock (SyncObject) return EventHandlers.Add(handler);
        }

        /// <summary>
        /// Add an event handler
        /// </summary>
        /// <param name="handler">Handler</param>
        /// <returns>Added?</returns>
        public bool Listen(in IAsyncEvent<tSender, tArgs>.EventHandlerAsync_Delegate handler)
        {
            lock (SyncObject) return AsyncEventHandlers.Add(handler);
        }

        /// <summary>
        /// Detach an event handler
        /// </summary>
        /// <param name="handler">Handler</param>
        /// <returns>Detached?</returns>
        public bool Detach(in IAsyncEvent<tSender, tArgs>.EventHandler_Delegate handler)
        {
            lock (SyncObject) return EventHandlers.Remove(handler);
        }

        /// <summary>
        /// Detach an event handler
        /// </summary>
        /// <param name="handler">Handler</param>
        /// <returns>Detached?</returns>
        public bool Detach(in IAsyncEvent<tSender, tArgs>.EventHandlerAsync_Delegate handler)
        {
            lock (SyncObject) return AsyncEventHandlers.Remove(handler);
        }

        /// <summary>
        /// Detach all event handlers
        /// </summary>
        private void DetachAll()
        {
            lock (SyncObject)
            {
                EventHandlers.Clear();
                AsyncEventHandlers.Clear();
            }
        }

        /// <summary>
        /// Raise the event
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private Task RaiseEventAsync(in TimeSpan? timeout = null, in CancellationToken cancellationToken = default)
            => RaiseEventAsync(Sender ?? throw new InvalidOperationException(), new(), timeout, cancellationToken);

        /// <summary>
        /// Raise the event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private Task RaiseEventAsync(in tSender sender, in TimeSpan? timeout = null, in CancellationToken cancellationToken = default)
            => RaiseEventAsync(sender, new(), timeout, cancellationToken);

        /// <summary>
        /// Raise the event
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private Task RaiseEventAsync(in tArgs args, in TimeSpan? timeout = null, in CancellationToken cancellationToken = default)
            => RaiseEventAsync(Sender ?? throw new InvalidOperationException(), args, timeout, cancellationToken);

        /// <summary>
        /// Raise the event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private async Task RaiseEventAsync(tSender sender, tArgs args, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            lock (SyncObject)
            {
                if (FirstRaised == DateTime.MinValue) FirstRaised = LastRaised = DateTime.Now;
                LastRaised = DateTime.Now;
                _RaiseCount++;
            }
            if (!HasHandlers) return;
            timeout ??= Timeout;
            if (Cancellation.HasValue && Equals(cancellationToken, default)) cancellationToken = Cancellation.Value;
            CancellationToken cancellation = !Cancellation.HasValue || cancellationToken == Cancellation ? cancellationToken : Cancellation.Value;
            DateTime started = DateTime.Now;
            TimeSpan to = timeout ?? TimeSpan.Zero;
            void throwTimeoutException()
            {
                TimeoutException ex = new();
                ex.Data[timeout!.Value] = true;
                throw ex;
            }
            {
                IAsyncEvent<tSender, tArgs>.EventHandlerAsync_Delegate[] handlers;
                lock (SyncObject) handlers = [.. AsyncEventHandlers];
                foreach (IAsyncEvent<tSender, tArgs>.EventHandlerAsync_Delegate handler in handlers)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    cancellation.ThrowIfCancellationRequested();
                    if (timeout.HasValue)
                    {
                        if (!TimeSpanHelper.UpdateTimeout(ref started, ref to)) throwTimeoutException();
                        await handler(sender, args, cancellationToken).WaitAsync(timeout.Value, cancellation).DynamicContext();
                    }
                    else
                    {
                        await handler(sender, args, cancellationToken).WaitAsync(cancellation).DynamicContext();
                    }
                }
            }
            {
                IAsyncEvent<tSender, tArgs>.EventHandler_Delegate[] handlers;
                lock (SyncObject) handlers = [.. EventHandlers];
                foreach (IAsyncEvent<tSender, tArgs>.EventHandler_Delegate handler in handlers)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    cancellation.ThrowIfCancellationRequested();
                    if (timeout.HasValue && !TimeSpanHelper.UpdateTimeout(ref started, ref to)) throwTimeoutException();
                    handler(sender, args, cancellationToken);
                }
            }
        }

        /// <inheritdoc/>
        void IAsyncEvent<tSender, tArgs>.DetachAll() => DetachAll();

        /// <inheritdoc/>
        Task IAsyncEvent<tSender, tArgs>.RaiseEventAsync(TimeSpan? timeout, CancellationToken cancellationToken)
            => RaiseEventAsync(timeout, cancellationToken);

        /// <inheritdoc/>
        Task IAsyncEvent<tSender, tArgs>.RaiseEventAsync(tSender sender, TimeSpan? timeout, CancellationToken cancellationToken)
            => RaiseEventAsync(sender, timeout, cancellationToken);

        /// <inheritdoc/>
        Task IAsyncEvent<tSender, tArgs>.RaiseEventAsync(tArgs args, TimeSpan? timeout, CancellationToken cancellationToken)
            => RaiseEventAsync(args, timeout, cancellationToken);

        /// <inheritdoc/>
        Task IAsyncEvent<tSender, tArgs>.RaiseEventAsync(tSender sender, tArgs args, TimeSpan? timeout, CancellationToken cancellationToken)
            => RaiseEventAsync(sender, args, timeout, cancellationToken);

        /// <summary>
        /// Cast as <see cref="HasHandlers"/>
        /// </summary>
        /// <param name="e">Event</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator bool(in AsyncEvent<tSender, tArgs> e) => e.HasHandlers;

        /// <summary>
        /// Add an event handler
        /// </summary>
        /// <param name="e">Event</param>
        /// <param name="handler">Handler</param>
        /// <returns>Event</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static AsyncEvent<tSender, tArgs> operator +(in AsyncEvent<tSender, tArgs> e, in IAsyncEvent<tSender, tArgs>.EventHandler_Delegate handler)
        {
            e.Listen(handler);
            return e;
        }

        /// <summary>
        /// Remove an event handler
        /// </summary>
        /// <param name="e">Event</param>
        /// <param name="handler">Handler</param>
        /// <returns>Event</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static AsyncEvent<tSender, tArgs> operator -(in AsyncEvent<tSender, tArgs> e, in IAsyncEvent<tSender, tArgs>.EventHandler_Delegate handler)
        {
            e.Detach(handler);
            return e;
        }

        /// <summary>
        /// Add an event handler
        /// </summary>
        /// <param name="e">Event</param>
        /// <param name="handler">Handler</param>
        /// <returns>Event</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static AsyncEvent<tSender, tArgs> operator +(in AsyncEvent<tSender, tArgs> e, in IAsyncEvent<tSender, tArgs>.EventHandlerAsync_Delegate handler)
        {
            e.Listen(handler);
            return e;
        }

        /// <summary>
        /// Remove an event handler
        /// </summary>
        /// <param name="e">Event</param>
        /// <param name="handler">Handler</param>
        /// <returns>Event</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static AsyncEvent<tSender, tArgs> operator -(in AsyncEvent<tSender, tArgs> e, in IAsyncEvent<tSender, tArgs>.EventHandlerAsync_Delegate handler)
        {
            e.Detach(handler);
            return e;
        }
    }
}
