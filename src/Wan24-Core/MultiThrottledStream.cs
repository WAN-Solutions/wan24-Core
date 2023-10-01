namespace wan24.Core
{
    /// <summary>
    /// Multiple combined throttled streams (will balance a total read/write limit to all hosted streams; all streams need to use the same time restrictions for this)
    /// </summary>
    public class MultiThrottledStream : DisposableBase, IStatusProvider
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Streams
        /// </summary>
        protected readonly HashSet<IThrottledStream> _Streams = new();
        /// <summary>
        /// Read count total limit
        /// </summary>
        protected int _TotalReadLimit;
        /// <summary>
        /// Write count total limit
        /// </summary>
        protected int _TotalWriteLimit;
        /// <summary>
        /// Last read count limit
        /// </summary>
        protected int LastReadLimit = -1;
        /// <summary>
        /// Last write count limit
        /// </summary>
        protected int LastWriteLimit = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="totalReadLimit">Reading count total limit</param>
        /// <param name="totalWriteLimit">Writing count total limit</param>
        public MultiThrottledStream(in int totalReadLimit, in int totalWriteLimit) : base()
        {
            _TotalReadLimit = totalReadLimit;
            _TotalWriteLimit = totalWriteLimit;
        }

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Reading count total limit
        /// </summary>
        public virtual int TotalReadLimit
        {
            get => IfUndisposed(_TotalReadLimit);
            set
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                value = Math.Max(0, value);
                if (_TotalReadLimit == value) return;
                _TotalReadLimit = value;
                UpdateCurrentLimit();
            }
        }

        /// <summary>
        /// Writing count total limit
        /// </summary>
        public virtual int TotalWriteLimit
        {
            get => IfUndisposed(_TotalReadLimit);
            set
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                value = Math.Max(0, value);
                if (_TotalWriteLimit == value) return;
                _TotalWriteLimit = value;
                UpdateCurrentLimit();
            }
        }

        /// <summary>
        /// Hosted streams
        /// </summary>
        public IThrottledStream[] Streams
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                return _Streams.ToArray();
            }
        }

        /// <summary>
        /// Number of hosted streams
        /// </summary>
        public int Count => IfUndisposed(() => _Streams.Count);

        /// <summary>
        /// Current reading count limit per hosted stream
        /// </summary>
        public virtual int CurrentReadLimit
        {
            get
            {
                EnsureUndisposed();
                int limit = _TotalReadLimit;
                if (limit < 1) return 0;
                int count = _Streams.Count;
                return count < 1 ? limit : (int)Math.Ceiling(Math.Max(1, (float)limit / count));
            }
        }

        /// <summary>
        /// Current writing count limit per hosted stream
        /// </summary>
        public virtual int CurrentWriteLimit
        {
            get
            {
                EnsureUndisposed();
                int limit = _TotalWriteLimit;
                if (limit < 1) return 0;
                int count = _Streams.Count;
                return count < 1 ? limit : (int)Math.Ceiling(Math.Max(1, (float)limit / count));
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                EnsureUndisposed();
                yield return new("GUID", GUID, "Unique ID of the object");
                yield return new("Name", Name, "Name of the object");
                yield return new("Count", _Streams.Count, "Number of hosted streams");
                yield return new("Total read limit", _TotalReadLimit, "Reading count total limit");
                yield return new("Total write limit", _TotalWriteLimit, "Writing count total limit");
                yield return new("Current read limit", CurrentReadLimit, "Reading count limit per hosted stream");
                yield return new("Current write limit", CurrentWriteLimit, "Writing count limit per hosted stream");
            }
        }

        /// <summary>
        /// Set new limits
        /// </summary>
        /// <param name="totalReadLimit">New reading count total limit</param>
        /// <param name="totalWriteLimit">New writing count total limit</param>
        public void SetLimits(int totalReadLimit, int totalWriteLimit)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            totalReadLimit = Math.Max(0, totalReadLimit);
            totalWriteLimit = Math.Max(0, totalWriteLimit);
            if (_TotalReadLimit == totalReadLimit && _TotalWriteLimit == totalWriteLimit) return;
            _TotalReadLimit = totalReadLimit;
            _TotalWriteLimit = totalWriteLimit;
            UpdateCurrentLimit();
        }

        /// <summary>
        /// Set new limits
        /// </summary>
        /// <param name="totalReadLimit">New reading count total limit</param>
        /// <param name="totalWriteLimit">New writing count total limit</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SetLimitsAsync(int totalReadLimit, int totalWriteLimit, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            totalReadLimit = Math.Max(0, totalReadLimit);
            totalWriteLimit = Math.Max(0, totalWriteLimit);
            if (_TotalReadLimit == totalReadLimit && _TotalWriteLimit == totalWriteLimit) return;
            _TotalReadLimit = totalReadLimit;
            _TotalWriteLimit = totalWriteLimit;
            UpdateCurrentLimit();
        }

        /// <summary>
        /// Add a stream
        /// </summary>
        /// <typeparam name="T">Stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <returns>Stream</returns>
        public T AddStream<T>(T stream) where T : IThrottledStream
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            if (_Streams.Add(stream))
            {
                stream.OnDisposing += HandleDisposedStream;
                UpdateCurrentLimit();
            }
            return stream;
        }

        /// <summary>
        /// Add a stream
        /// </summary>
        /// <typeparam name="T">Stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Stream</returns>
        public async Task<T> AddStreamAsync<T>(T stream, CancellationToken cancellationToken = default) where T : IThrottledStream
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (_Streams.Add(stream))
            {
                stream.OnDisposing += HandleDisposedStream;
                UpdateCurrentLimit();
            }
            return stream;
        }

        /// <summary>
        /// Remove a stream
        /// </summary>
        /// <typeparam name="T">Stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <returns>Stream</returns>
        public T RemoveStream<T>(T stream) where T : IThrottledStream
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            if (_Streams.Remove(stream))
            {
                stream.OnDisposing -= HandleDisposedStream;
                UpdateCurrentLimit();
            }
            return stream;
        }

        /// <summary>
        /// Remove a stream
        /// </summary>
        /// <typeparam name="T">Stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Stream</returns>
        public async Task<T> RemoveStreamAsync<T>(T stream, CancellationToken cancellationToken = default) where T : IThrottledStream
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (_Streams.Remove(stream))
            {
                stream.OnDisposing -= HandleDisposedStream;
                UpdateCurrentLimit();
            }
            return stream;
        }

        /// <summary>
        /// Remove all streams
        /// </summary>
        /// <returns>Removed streams</returns>
        public IThrottledStream[] Clear()
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            try
            {
                IThrottledStream[] res = _Streams.ToArray();
                foreach (IThrottledStream stream in res) stream.OnDisposing -= HandleDisposedStream;
                return res;
            }
            finally
            {
                _Streams.Clear();
                UpdateCurrentLimit();
            }
        }

        /// <summary>
        /// Remove all streams
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Removed streams</returns>
        public async Task<IThrottledStream[]> ClearAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            try
            {
                IThrottledStream[] res = _Streams.ToArray();
                foreach (IThrottledStream stream in res) stream.OnDisposing -= HandleDisposedStream;
                return res;
            }
            finally
            {
                _Streams.Clear();
                UpdateCurrentLimit();
            }
        }

        /// <summary>
        /// Update the current processing count limit for all hosted streams (<see cref="Sync"/> should be synchronized)
        /// </summary>
        protected virtual void UpdateCurrentLimit()
        {
            int readLimit = CurrentReadLimit,
                writeLimit = CurrentWriteLimit;
            if (readLimit == LastReadLimit && writeLimit == LastWriteLimit) return;
            LastReadLimit = readLimit;
            LastWriteLimit = writeLimit;
            foreach (IThrottledStream stream in _Streams)
            {
                if (stream.ReadCount != readLimit) stream.ReadCount = readLimit;
                if (stream.WriteCount != writeLimit) stream.WriteCount = writeLimit;
            }
        }

        /// <summary>
        /// Handle a disposed stream
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="e">Arguments</param>
        protected virtual void HandleDisposedStream(IDisposableObject obj, EventArgs e)
        {
            IThrottledStream stream = (obj as IThrottledStream)!;
            RemoveStream(stream);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Clear().DisposeAll();
            Sync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await (await ClearAsync().DynamicContext()).DisposeAllAsync().DynamicContext();
            await Sync.DisposeAsync().DynamicContext();
        }
    }
}
