namespace wan24.Core
{
    /// <summary>
    /// File system events
    /// </summary>
    public class FileSystemEvents : HostedServiceBase
    {
        /// <summary>
        /// Event throttle
        /// </summary>
        protected readonly FileSystemEventThrottle Throttle;
        /// <summary>
        /// File system watcher
        /// </summary>
        protected readonly FileSystemWatcher Watcher;
        /// <summary>
        /// Watcher event (raised when having an event)
        /// </summary>
        protected readonly ResetEvent WatcherEvent = new();
        /// <summary>
        /// Events synchronization
        /// </summary>
        protected readonly SemaphoreSync EventSync = new();
        /// <summary>
        /// File system event arguments
        /// </summary>
        protected readonly List<FileSystemEventArgs> Arguments = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="folder">Folder</param>
        /// <param name="pattern">Pattern</param>
        /// <param name="filters">Filters</param>
        /// <param name="events">Event types</param>
        /// <param name="throttle">Throttling time in ms</param>
        /// <param name="recursive">Watch recursive?</param>
        public FileSystemEvents(
            in string folder,
            in string pattern,
            in NotifyFilters filters,
            in int throttle,
            in bool recursive = true,
            in FileSystemEventTypes events = FileSystemEventTypes.All
            )
            : base()
        {
            CanPause = true;
            Throttle = new(throttle, this);
            Watcher = new(folder, pattern)
            {
                NotifyFilter = filters,
                IncludeSubdirectories = recursive
            };
            Events = events;
            if ((events & FileSystemEventTypes.Changes) == FileSystemEventTypes.Changes) Watcher.Changed += HandleWatcherEvent;
            if ((events & FileSystemEventTypes.Created) == FileSystemEventTypes.Created) Watcher.Created += HandleWatcherEvent;
            if ((events & FileSystemEventTypes.Deleted) == FileSystemEventTypes.Deleted) Watcher.Deleted += HandleWatcherEvent;
            if ((events & FileSystemEventTypes.Renamed) == FileSystemEventTypes.Renamed) Watcher.Renamed += HandleWatcherEvent;
        }

        /// <summary>
        /// Events
        /// </summary>
        public FileSystemEventTypes Events { get; }

        /// <summary>
        /// Watched folder
        /// </summary>
        public string Folder => Watcher.Path;

        /// <summary>
        /// Watched pattern
        /// </summary>
        public string Pattern => Watcher.Filter;

        /// <summary>
        /// Recursive?
        /// </summary>
        public bool Recursive => Watcher.IncludeSubdirectories;

        /// <summary>
        /// Filters
        /// </summary>
        public NotifyFilters Filters => Watcher.NotifyFilter;

        /// <summary>
        /// Event throttle timeout in ms
        /// </summary>
        public int ThrottleTimeout => Throttle.Timeout;

        /// <summary>
        /// Number of currently collected event arguments
        /// </summary>
        public int CurrentEvents
        {
            get
            {
                using SemaphoreSyncContext ssc = EventSync;
                return Arguments.Count;
            }
        }

        /// <summary>
        /// Number of times events have been raised during throttline
        /// </summary>
        public int EventCount => Throttle.RaisedCount;

        /// <summary>
        /// Time when the first event was raised during throttling
        /// </summary>
        public DateTime EventRaised => Throttle.RaisedTime;

        /// <summary>
        /// Last event data
        /// </summary>
        public FileSystemEventsArgs? Last { get; protected set; }

        /// <summary>
        /// Wait for an event (canceled when the service is stopping)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Event data</returns>
        public virtual FileSystemEventsArgs WaitEvent(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureRunning();
            using Cancellations cancellation = new(cancellationToken, CancelToken);
            WatcherEvent.Wait(cancellation);
            return Last ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// Wait for an event (canceled when the service is stopping)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Event data</returns>
        public virtual async Task<FileSystemEventsArgs> WaitEventAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureRunning();
            using Cancellations cancellation = new(cancellationToken, CancelToken);
            await WatcherEvent.WaitAsync(cancellation).DynamicContext();
            return Last ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// Handle a file system watcher event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected virtual void HandleWatcherEvent(object sender, FileSystemEventArgs e)
        {
            using (SemaphoreSyncContext ssc = EventSync) Arguments.Add(e);
            if (!IsPaused) Throttle.Raise();
        }

        /// <inheritdoc/>
        protected override async Task AfterPauseAsync(CancellationToken cancellationToken)
        {
            bool raise;
            using (SemaphoreSyncContext ssc = await EventSync.SyncContextAsync(cancellationToken).DynamicContext())
                raise = Arguments.Count > 0;
            if (raise) Throttle.Raise();
            await base.AfterPauseAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
        {
            try
            {
                Watcher.EnableRaisingEvents = true;
                await CancelToken;
            }
            finally
            {
                Watcher.EnableRaisingEvents = false;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Watcher.Changed -= HandleWatcherEvent;
            Watcher.Created -= HandleWatcherEvent;
            Watcher.Deleted -= HandleWatcherEvent;
            Watcher.Renamed -= HandleWatcherEvent;
            base.Dispose(disposing);
            Watcher.Dispose();
            WatcherEvent.Dispose();
            Throttle.Dispose();
            EventSync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            Watcher.Changed -= HandleWatcherEvent;
            Watcher.Created -= HandleWatcherEvent;
            Watcher.Deleted -= HandleWatcherEvent;
            Watcher.Renamed -= HandleWatcherEvent;
            await base.DisposeCore().DynamicContext();
            Watcher.Dispose();
            await WatcherEvent.DisposeAsync().DynamicContext();
            await Throttle.DisposeAsync().DynamicContext();
            await EventSync.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Delegate for an event hander
        /// </summary>
        /// <param name="events">Events</param>
        /// <param name="e">Arguments</param>
        public delegate void FileSystemEvents_Delegate(FileSystemEvents events, FileSystemEventsArgs e);
        /// <summary>
        /// Raised on event
        /// </summary>
        public event FileSystemEvents_Delegate? OnEvents;
        /// <summary>
        /// Raise the <see cref="OnEvents"/> event
        /// </summary>
        /// <param name="raised">Raised time</param>
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
        /// <param name="arguments">File system event arguments</param>
        /// <param name="raised">Raised time</param>
        public class FileSystemEventsArgs(in IReadOnlyList<FileSystemEventArgs> arguments, in DateTime raised) : EventArgs()
        {
            /// <summary>
            /// File system event arguments
            /// </summary>
            public IReadOnlyList<FileSystemEventArgs> Arguments { get; } = arguments;

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
        protected class FileSystemEventThrottle(in int throttle, in FileSystemEvents events) : EventThrottle(throttle)
        {
            /// <summary>
            /// Events
            /// </summary>
            protected readonly FileSystemEvents Events = events;

            /// <inheritdoc/>
            protected override void HandleEvent(in DateTime raised, in int raisedCount) => Events.RaiseOnEvents(raised);
        }
    }
}
