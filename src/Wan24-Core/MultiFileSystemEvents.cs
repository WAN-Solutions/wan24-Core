using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Multi file system events
    /// </summary>
    public class MultiFileSystemEvents : HostedServiceBase
    {
        /// <summary>
        /// Event throttle
        /// </summary>
        protected readonly MultiFileSystemEventThrottle? Throttle;
        /// <summary>
        /// File system watchers
        /// </summary>
        protected readonly ConcurrentDictionary<string, FileSystemEvents> Watchers = [];
        /// <summary>
        /// Watcher event (raised when having an event)
        /// </summary>
        protected readonly ResetEvent WatcherEvent = new();
        /// <summary>
        /// Events synchronization
        /// </summary>
        protected readonly SemaphoreSync EventSync = new();
        /// <summary>
        /// File system event senders and arguments
        /// </summary>
        protected readonly List<(FileSystemEvents, FileSystemEvents.FileSystemEventsArgs)> Arguments = [];

        /// <summary>
        /// Constructor
        /// </summary>
        public MultiFileSystemEvents(in int throttle = 0) : base()
        {
            CanPause = true;
            Throttle = throttle < 1 ? null : new(throttle, this);
        }

        /// <summary>
        /// Get a file system events watcher
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>File system events watcher</returns>
        public FileSystemEvents? this[string id] => IfUndisposed(() => Watchers.TryGetValue(id, out FileSystemEvents? res) ? res : null);

        /// <summary>
        /// Number of hosted file system events watchers
        /// </summary>
        public int Count => IfUndisposed(Watchers.Count);

        /// <summary>
        /// Hosted file system events watcher IDs
        /// </summary>
        public IEnumerable<string> Ids => IfUndisposed(() => Watchers.Keys);

        /// <summary>
        /// Last event data
        /// </summary>
        public MultiFileSystemEventsArgs? Last { get; protected set; }

        /// <summary>
        /// Wait for an event (canceled when the service is stopping)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Event data</returns>
        public virtual MultiFileSystemEventsArgs WaitEvent(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureRunning();
            using CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancelToken);
            WatcherEvent.Wait(cancellation.Token);
            return Last ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// Wait for an event (canceled when the service is stopping)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Event data</returns>
        public virtual async Task<MultiFileSystemEventsArgs> WaitEventAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureRunning();
            using CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancelToken);
            await WatcherEvent.WaitAsync(cancellation.Token).DynamicContext();
            return Last ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// Add a file system events watcher
        /// </summary>
        /// <param name="watcher">File system events watcher (will be disposed, if not removed before disposing!)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ID</returns>
        public virtual string Add(in FileSystemEvents watcher, in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            string res = Guid.NewGuid().ToString();
            Watchers[res] = watcher;
            if (IsPaused && !watcher.IsPaused)
            {
                if (watcher.IsRunning)
                {
                    watcher.PauseAsync(cancellationToken).Wait(CancellationToken.None);
                }
                else
                {
                    watcher.StartAsync(cancellationToken).Wait(CancellationToken.None);
                    watcher.PauseAsync(cancellationToken).Wait(CancellationToken.None);
                }
            }
            else if (IsRunning)
            {
                if (watcher.IsPaused)
                {
                    watcher.ResumeAsync(cancellationToken).Wait(CancellationToken.None);
                }
                else if (!watcher.IsRunning)
                {
                    watcher.StartAsync(cancellationToken).Wait(CancellationToken.None);
                }
            }
            watcher.OnEvents += HandleEvent;
            return res;
        }

        /// <summary>
        /// Add a file system events watcher
        /// </summary>
        /// <param name="watcher">File system events watcher (will be disposed, if not removed before disposing!)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ID</returns>
        public virtual async Task<string> AddAsync(FileSystemEvents watcher, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            string res = Guid.NewGuid().ToString();
            Watchers[res] = watcher;
            if (IsPaused && !watcher.IsPaused)
            {
                if (watcher.IsRunning)
                {
                    await watcher.PauseAsync(cancellationToken).DynamicContext();
                }
                else
                {
                    await watcher.StartAsync(cancellationToken).DynamicContext();
                    await watcher.PauseAsync(cancellationToken).DynamicContext();
                }
            }
            else if (IsRunning)
            {
                if (watcher.IsPaused)
                {
                    await watcher.ResumeAsync(cancellationToken).DynamicContext();
                }
                else if (!watcher.IsRunning)
                {
                    await watcher.StartAsync(cancellationToken).DynamicContext();
                }
            }
            watcher.OnEvents += HandleEvent;
            return res;
        }

        /// <summary>
        /// Remove a file system events watcher
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Removed file system events watcher (won't be disposed)</returns>
        public virtual FileSystemEvents Remove(in string id)
        {
            EnsureUndisposed();
            if (!Watchers.TryRemove(id, out FileSystemEvents? res)) throw new ArgumentException("ID not found", nameof(id));
            res.OnEvents -= HandleEvent;
            return res;
        }

        /// <summary>
        /// Remove a file system events watcher
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="result">Removed file system events watcher (won't be disposed)</param>
        /// <returns>If succeed</returns>
        public virtual bool TryRemove(in string id, [NotNullWhen(returnValue: true)] out FileSystemEvents? result)
        {
            if (IsDisposing || !Watchers.TryRemove(id, out result))
            {
                result = null;
                return false;
            }
            result.OnEvents -= HandleEvent;
            return true;
        }

        /// <summary>
        /// Handle an event
        /// </summary>
        /// <param name="watcher">Watcher</param>
        /// <param name="e">Arguments</param>
        protected virtual void HandleEvent(FileSystemEvents watcher, FileSystemEvents.FileSystemEventsArgs e)
        {
            using (SemaphoreSyncContext ssc = EventSync) Arguments.Add((watcher, e));
            if (!IsPaused)
                if (Throttle is not null)
                {
                    Throttle.Raise();
                }
                else
                {
                    RaiseOnEvents(DateTime.Now);
                }
        }

        /// <inheritdoc/>
        protected override async Task AfterStartAsync(CancellationToken cancellationToken)
        {
            await base.AfterStartAsync(cancellationToken).DynamicContext();
            List<Task> tasks = new(Watchers.Values.Select(w => !w.IsPaused ? w.StartAsync(CancellationToken.None) : Task.CompletedTask));
            await Task.WhenAll(tasks).WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task BeforeStopAsync(CancellationToken cancellationToken)
        {
            await base.BeforeStopAsync(cancellationToken).DynamicContext();
            List<Task> tasks = new(Watchers.Values.Select(w => w.StopAsync(CancellationToken.None)));
            await Task.WhenAll(tasks).WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task AfterPauseAsync(CancellationToken cancellationToken)
        {
            await base.AfterPauseAsync(cancellationToken).DynamicContext();
            List<Task> tasks = new(Watchers.Values.Select(w => w.IsRunning && !w.IsPaused ? w.PauseAsync(CancellationToken.None) : Task.CompletedTask));
            await Task.WhenAll(tasks).WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task AfterResumeAsync(CancellationToken cancellationToken)
        {
            await base.AfterResumeAsync(cancellationToken).DynamicContext();
            bool raise;
            using (SemaphoreSyncContext ssc = await EventSync.SyncContextAsync(cancellationToken).DynamicContext())
                raise = Arguments.Count > 0;
            if (raise)
                if (Throttle is not null)
                {
                    Throttle.Raise();
                }
                else
                {
                    RaiseOnEvents(DateTime.Now);
                }
            List<Task> tasks = new(Watchers.Values.Select(w => w.IsPaused ? w.ResumeAsync(CancellationToken.None) : Task.CompletedTask));
            await Task.WhenAll(tasks).WaitAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync() => await CancelToken;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Throttle?.Dispose();
            WatcherEvent.Dispose();
            EventSync.Dispose();
            Watchers.Values.DisposeAll();
            foreach (FileSystemEvents watcher in Watchers.Values) watcher.OnEvents -= HandleEvent;
            Watchers.Clear();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            if (Throttle is not null) await Throttle.DisposeAsync().DynamicContext();
            await WatcherEvent.DisposeAsync().DynamicContext();
            EventSync.Dispose();
            await Watchers.Values.DisposeAllAsync().DynamicContext();
            foreach (FileSystemEvents watcher in Watchers.Values) watcher.OnEvents -= HandleEvent;
            Watchers.Clear();
        }

        /// <summary>
        /// Delegate for an event hander
        /// </summary>
        /// <param name="events">Events</param>
        /// <param name="e">Arguments</param>
        public delegate void MultiFileSystemEvents_Delegate(MultiFileSystemEvents events, MultiFileSystemEventsArgs e);
        /// <summary>
        /// Raised on event
        /// </summary>
        public event MultiFileSystemEvents_Delegate? OnEvents;
        /// <summary>
        /// Raise the <see cref="OnEvents"/> event
        /// </summary>
        /// <param name="raised">Time when raised first</param>
        protected virtual void RaiseOnEvents(in DateTime raised)
        {
            using (SemaphoreSyncContext ssc = EventSync)
            {
                if (Arguments.Count < 1) return;
                Last = new([.. Arguments], raised);
                Arguments.Clear();
            }
            if (!IsRunning) return;
            WatcherEvent.Set();
            OnEvents?.Invoke(this, Last);
            WatcherEvent.Reset();
        }

        /// <summary>
        /// File system events <see cref="OnEvents"/> arguments
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="arguments">File system event senders and arguments</param>
        /// <param name="raised">Raised time</param>
        public class MultiFileSystemEventsArgs(
            in IReadOnlyList<(FileSystemEvents, FileSystemEvents.FileSystemEventsArgs)> arguments,
            in DateTime raised
            )
            : EventArgs()
        {
            /// <summary>
            /// File system event senders and arguments
            /// </summary>
            public IReadOnlyList<(FileSystemEvents, FileSystemEvents.FileSystemEventsArgs)> Arguments { get; } = arguments;

            /// <summary>
            /// Raised time
            /// </summary>
            public DateTime Raised { get; } = raised;
        }

        /// <summary>
        /// Event throttle
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="throttle">Throttle time in ms</param>
        /// <param name="events">Events</param>
        protected class MultiFileSystemEventThrottle(in int throttle, in MultiFileSystemEvents events) : EventThrottle(throttle)
        {
            /// <summary>
            /// Events
            /// </summary>
            protected readonly MultiFileSystemEvents Events = events;

            /// <inheritdoc/>
            protected override void HandleEvent(in DateTime raised, in int raisedCount) => Events.RaiseOnEvents(raised);
        }
    }
}
