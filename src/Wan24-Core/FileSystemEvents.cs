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
        /// Events synchronization
        /// </summary>
        protected readonly SemaphoreSync EventSync = new();
        /// <summary>
        /// Changed paths
        /// </summary>
        protected readonly HashSet<string> Paths = [];
        /// <summary>
        /// Changes
        /// </summary>
        protected WatcherChangeTypes Changes = default;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="folder">Folder</param>
        /// <param name="pattern">Pattern</param>
        /// <param name="filters">Filters</param>
        /// <param name="throttle">Throttling time in ms</param>
        /// <param name="recursive">Watch recursive?</param>
        public FileSystemEvents(in string folder, in string pattern, in NotifyFilters filters, in int throttle, in bool recursive = true) : base()
        {
            CanPause = true;
            Throttle = new(throttle, this);
            Watcher = new(folder, pattern)
            {
                NotifyFilter = filters,
                IncludeSubdirectories = recursive
            };
            Watcher.Changed += HandleWatherEvent;
            Watcher.Created += HandleWatherEvent;
            Watcher.Deleted += HandleWatherEvent;
            Watcher.Renamed += HandleWatherEvent;
        }

        /// <summary>
        /// Handle a file system watcher event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected virtual void HandleWatherEvent(object sender, FileSystemEventArgs e)
        {
            using (SemaphoreSyncContext ssc = EventSync)
            {
                Changes |= e.ChangeType;
                Paths.Add(e.FullPath);
            }
            Throttle.Raise();
        }

        /// <inheritdoc/>
        protected override async Task AfterPauseAsync(CancellationToken cancellationToken)
        {
            bool raise;
            using (SemaphoreSyncContext ssc = await EventSync.SyncContextAsync(cancellationToken).DynamicContext())
                raise = Paths.Count > 0;
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
            Watcher.Changed -= HandleWatherEvent;
            Watcher.Created -= HandleWatherEvent;
            Watcher.Deleted -= HandleWatherEvent;
            Watcher.Renamed -= HandleWatherEvent;
            base.Dispose(disposing);
            Watcher.Dispose();
            Throttle.Dispose();
            EventSync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            Watcher.Changed -= HandleWatherEvent;
            Watcher.Created -= HandleWatherEvent;
            Watcher.Deleted -= HandleWatherEvent;
            Watcher.Renamed -= HandleWatherEvent;
            await base.DisposeCore().DynamicContext();
            Watcher.Dispose();
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
        /// <param name="raisedCount">Raised count</param>
        protected virtual void RaiseOnEvents(in DateTime raised, in int raisedCount)
        {
            if (!IsRunning)
            {
                using SemaphoreSyncContext ssc = EventSync;
                Changes = default;
                Paths.Clear();
                return;
            }
            FileSystemEventsArgs e;
            using (SemaphoreSyncContext ssc = EventSync)
            {
                if (Paths.Count < 1) return;
                e = new(Changes, [.. Paths], raised, raisedCount);
                Changes = default;
                Paths.Clear();
            }
            OnEvents?.Invoke(this, e);
        }

        /// <summary>
        /// File system events <see cref="OnEvents"/> arguments
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="changes">Recorded changes</param>
        /// <param name="paths">Changed paths</param>
        /// <param name="raised">Raised time</param>
        /// <param name="raisedCount">Raised count</param>
        public class FileSystemEventsArgs(in WatcherChangeTypes changes, in IReadOnlyList<string> paths, in DateTime raised, in int raisedCount) : EventArgs()
        {
            /// <summary>
            /// Reorded changes
            /// </summary>
            public WatcherChangeTypes Changes { get; } = changes;

            /// <summary>
            /// CHanged paths
            /// </summary>
            public IReadOnlyList<string> Paths { get; } = paths;

            /// <summary>
            /// Raised time
            /// </summary>
            public DateTime Raised { get; } = raised;

            /// <summary>
            /// Raised count
            /// </summary>
            public int RaisedCount { get; } = raisedCount;
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
            protected override void HandleEvent(in DateTime raised, in int raisedCount) => Events.RaiseOnEvents(raised, raisedCount);
        }
    }
}
