namespace wan24.Core
{
    /// <summary>
    /// Multiple combined process throttles (will balance a total limit to all hosted throttles; all throttles need to use the same time restriction for this)
    /// </summary>
    public class MultiProcessThrottle : DisposableBase, IStatusProvider
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Throttles
        /// </summary>
        protected readonly HashSet<ProcessThrottle> _Throttles = new();
        /// <summary>
        /// Processing count total limit
        /// </summary>
        protected int _TotalLimit;
        /// <summary>
        /// Last processing count limit
        /// </summary>
        protected int LastLimit = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="totalLimit">Processing count total limit</param>
        public MultiProcessThrottle(in int totalLimit) : base()
        {
            if (totalLimit < 1) throw new ArgumentOutOfRangeException(nameof(totalLimit));
            _TotalLimit = totalLimit;
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
        /// Processing count total limit
        /// </summary>
        public virtual int TotalLimit
        {
            get => IfUndisposed(_TotalLimit);
            set
            {
                EnsureUndisposed();
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(value));
                using SemaphoreSyncContext ssc = Sync;
                if (_TotalLimit == value) return;
                _TotalLimit = value;
                UpdateCurrentLimit();
            }
        }

        /// <summary>
        /// Hosted throttles
        /// </summary>
        public ProcessThrottle[] Throttles
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                return _Throttles.ToArray();
            }
        }

        /// <summary>
        /// Number of hosted throttles
        /// </summary>
        public int Count => IfUndisposed(() => _Throttles.Count);

        /// <summary>
        /// Current processing count limit per hosted throttle
        /// </summary>
        public virtual int CurrentLimit
        {
            get
            {
                EnsureUndisposed();
                int count = _Throttles.Count;
                return count < 1 ? _TotalLimit : (int)Math.Ceiling(Math.Max(1, (float)_TotalLimit / count));
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
                yield return new("Count", _Throttles.Count, "Number of hosted streams");
                yield return new("Total limit", _TotalLimit, "Processing count total limit");
                yield return new("Current limit", CurrentLimit, "Processing count limit per throttle");
            }
        }

        /// <summary>
        /// Set a new limit
        /// </summary>
        /// <param name="totalLimit">Processing count total limit</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SetLimitAsync(int totalLimit, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (totalLimit < 1) throw new ArgumentOutOfRangeException(nameof(totalLimit));
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (_TotalLimit == totalLimit) return;
            _TotalLimit = totalLimit;
            UpdateCurrentLimit();
        }

        /// <summary>
        /// Add a throttle
        /// </summary>
        /// <typeparam name="T">Throttle type</typeparam>
        /// <param name="throttle">Throttle</param>
        /// <returns>Throttle</returns>
        public T AddThrottle<T>(T throttle) where T : ProcessThrottle
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            if (_Throttles.Add(throttle))
            {
                throttle.OnDisposing += HandleDisposedThrottle;
                UpdateCurrentLimit();
            }
            return throttle;
        }

        /// <summary>
        /// Add a throttle
        /// </summary>
        /// <typeparam name="T">Throttle type</typeparam>
        /// <param name="throttle">Throttle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Throttle</returns>
        public async Task<T> AddThrottleAsync<T>(T throttle, CancellationToken cancellationToken = default) where T : ProcessThrottle
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (_Throttles.Add(throttle))
            {
                throttle.OnDisposing += HandleDisposedThrottle;
                UpdateCurrentLimit();
            }
            return throttle;
        }

        /// <summary>
        /// Remove a throttle
        /// </summary>
        /// <typeparam name="T">Throttle type</typeparam>
        /// <param name="throttle">Throttle</param>
        /// <returns>Throttle</returns>
        public T RemoveThrottle<T>(T throttle) where T : ProcessThrottle
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = Sync;
            if (_Throttles.Remove(throttle))
            {
                throttle.OnDisposing -= HandleDisposedThrottle;
                UpdateCurrentLimit();
            }
            return throttle;
        }

        /// <summary>
        /// Remove a throttle
        /// </summary>
        /// <typeparam name="T">Throttle type</typeparam>
        /// <param name="throttle">Throttle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Throttle</returns>
        public async Task<T> RemoveThrottleAsync<T>(T throttle, CancellationToken cancellationToken = default) where T : ProcessThrottle
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            if (_Throttles.Remove(throttle))
            {
                throttle.OnDisposing -= HandleDisposedThrottle;
                UpdateCurrentLimit();
            }
            return throttle;
        }

        /// <summary>
        /// Remove all throttles
        /// </summary>
        /// <returns>Removed throttles</returns>
        public ProcessThrottle[] Clear()
        {
            EnsureUndisposed(allowDisposing: true);
            using SemaphoreSyncContext ssc = Sync;
            try
            {
                ProcessThrottle[] res = _Throttles.ToArray();
                foreach (ProcessThrottle throttle in res) throttle.OnDisposing -= HandleDisposedThrottle;
                return res;
            }
            finally
            {
                _Throttles.Clear();
                UpdateCurrentLimit();
            }
        }

        /// <summary>
        /// Remove all throttles
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Removed throttles</returns>
        public async Task<ProcessThrottle[]> ClearAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed(allowDisposing: true);
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            try
            {
                ProcessThrottle[] res = _Throttles.ToArray();
                foreach (ProcessThrottle throttle in res) throttle.OnDisposing -= HandleDisposedThrottle;
                return res;
            }
            finally
            {
                _Throttles.Clear();
                UpdateCurrentLimit();
            }
        }

        /// <summary>
        /// Update the current processing count limit for all hosted throttles (<see cref="Sync"/> should be synchronized)
        /// </summary>
        protected virtual void UpdateCurrentLimit()
        {
            int limit = CurrentLimit;
            if (limit == LastLimit) return;
            LastLimit = limit;
            foreach (ProcessThrottle throttle in _Throttles) throttle.SetLimit(limit, throttle.Timeout);
        }

        /// <summary>
        /// Handle a disposed throttle
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="e">Arguments</param>
        protected virtual void HandleDisposedThrottle(IDisposableObject obj, EventArgs e)
        {
            ProcessThrottle throttle = (obj as ProcessThrottle)!;
            RemoveThrottle(throttle);
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
