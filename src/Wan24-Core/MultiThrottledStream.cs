using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Multiple combined throttled streams (will balance a total read/write limit to all hosted streams, based on quotas; all streams need to use the same time 
    /// restrictions for this)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="totalReadLimit">Reading count total limit</param>
    /// <param name="totalWriteLimit">Writing count total limit</param>
    public class MultiThrottledStream(in int totalReadLimit, in int totalWriteLimit) : DisposableBase(), IStatusProvider, IStreamThrottle
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Streams
        /// </summary>
        protected readonly HashSet<IStreamThrottle> _Streams = [];
        /// <summary>
        /// Parent <see cref="MultiThrottledStream"/>
        /// </summary>
        protected MultiThrottledStream? _Parent = null;
        /// <summary>
        /// Read count total limit
        /// </summary>
        protected int _TotalReadLimit = totalReadLimit;
        /// <summary>
        /// Write count total limit
        /// </summary>
        protected int _TotalWriteLimit = totalWriteLimit;

        /// <summary>
        /// Root <see cref="MultiThrottledStream"/>
        /// </summary>
        protected MultiThrottledStream Root => _Parent?.Root ?? this;

        /// <summary>
        /// Parent <see cref="MultiThrottledStream"/>
        /// </summary>
        protected MultiThrottledStream Parent => _Parent ?? this;

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
                using (SemaphoreSyncContext ssc = Sync)
                {
                    value = Math.Max(0, value);
                    if (_TotalReadLimit == value) return;
                    _TotalReadLimit = value;
                }
                Parent.UpdateCurrentLimit(syncRoot: true);
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
                using (SemaphoreSyncContext ssc = Sync)
                {
                    value = Math.Max(0, value);
                    if (_TotalWriteLimit == value) return;
                    _TotalWriteLimit = value;
                }
                Parent.UpdateCurrentLimit(syncRoot: true);
            }
        }

        /// <summary>
        /// Hosted streams (may contain nested <see cref="MultiThrottledStream"/>)
        /// </summary>
        public IStreamThrottle[] Streams
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                return [.. _Streams];
            }
        }

        /// <summary>
        /// Number of hosted streams (recursive, excluding nested <see cref="MultiThrottledStream"/>, but including their hosted stream count)
        /// </summary>
        public int Count
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                int res = 0;
                foreach (IStreamThrottle throttle in _Streams)
                    if (throttle is MultiThrottledStream mt) res += mt.Count;
                    else res++;
                return res;
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                EnsureUndisposed();
                yield return new(__("GUID"), GUID, __("Unique ID of the object"));
                yield return new(__("Name"), Name, __("Name of the object"));
                yield return new(__("Count"), _Streams.Count, __("Number of hosted streams"));
                yield return new(__("Total read limit"), _TotalReadLimit, __("Reading count total limit"));
                yield return new(__("Total write limit"), _TotalWriteLimit, __("Writing count total limit"));
            }
        }

        /// <inheritdoc/>
        public int ReadCountQuota => (int)GetLongReadCountQuotaUnblocked();

        /// <summary>
        /// Read count limit quota as 64 bit integer
        /// </summary>
        public long LongReadCountQuota
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                return GetLongReadCountQuotaUnblocked();
            }
        }

        /// <inheritdoc/>
        public int ReadCount
        {
            get => TotalReadLimit;
            set => TotalReadLimit = value;
        }

        /// <inheritdoc/>
        public int WriteCountQuota => (int)GetLongWriteCountQuotaUnblocked();

        /// <summary>
        /// Write count limit quota as 64 bit integer
        /// </summary>
        public long LongWriteCountQuota
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync;
                return GetLongWriteCountQuotaUnblocked();
            }
        }

        /// <inheritdoc/>
        public int WriteCount
        {
            get => TotalWriteLimit;
            set => TotalWriteLimit = value;
        }

        /// <summary>
        /// Set new limits
        /// </summary>
        /// <param name="totalReadLimit">New reading count total limit</param>
        /// <param name="totalWriteLimit">New writing count total limit</param>
        public void SetLimits(int totalReadLimit, int totalWriteLimit)
        {
            EnsureUndisposed();
            using (SemaphoreSyncContext ssc = Sync) SetLimitsUnblocked(totalReadLimit, totalWriteLimit);
            Parent.UpdateCurrentLimit(syncRoot: true);
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
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext())
                SetLimitsUnblocked(totalReadLimit, totalWriteLimit);
            await Parent.UpdateCurrentLimitAsync(syncRoot: true).DynamicContext();
        }

        /// <summary>
        /// Add a stream
        /// </summary>
        /// <typeparam name="T">Stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <returns>Stream</returns>
        public T AddStream<T>(T stream) where T : IStreamThrottle
        {
            EnsureUndisposed();
            using (SemaphoreSyncContext ssc = Sync)
            {
                if (!_Streams.Add(stream)) return stream;
                if (stream is MultiThrottledStream mt) mt._Parent = this;
                stream.OnDisposing += HandleDisposedStream;
            }
            Parent.UpdateCurrentLimit(syncRoot: true);
            return stream;
        }

        /// <summary>
        /// Add a stream
        /// </summary>
        /// <typeparam name="T">Stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Stream</returns>
        public async Task<T> AddStreamAsync<T>(T stream, CancellationToken cancellationToken = default) where T : IStreamThrottle
        {
            EnsureUndisposed();
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext())
            {
                if (!_Streams.Add(stream)) return stream;
                if (stream is MultiThrottledStream mt) mt._Parent = this;
                stream.OnDisposing += HandleDisposedStream;
            }
            await Parent.UpdateCurrentLimitAsync(syncRoot: true).DynamicContext();
            return stream;
        }

        /// <summary>
        /// Remove a stream
        /// </summary>
        /// <typeparam name="T">Stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <returns>Stream</returns>
        public T RemoveStream<T>(T stream) where T : IStreamThrottle
        {
            EnsureUndisposed();
            using (SemaphoreSyncContext ssc = Sync)
            {
                if (!_Streams.Remove(stream)) return stream;
                if (stream is MultiThrottledStream mt) mt._Parent = null;
                stream.OnDisposing -= HandleDisposedStream;
            }
            Parent.UpdateCurrentLimit(syncRoot: true);
            return stream;
        }

        /// <summary>
        /// Remove a stream
        /// </summary>
        /// <typeparam name="T">Stream type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Stream</returns>
        public async Task<T> RemoveStreamAsync<T>(T stream, CancellationToken cancellationToken = default) where T : IStreamThrottle
        {
            EnsureUndisposed();
            using (SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext())
            {
                if (!_Streams.Remove(stream)) return stream;
                if (stream is MultiThrottledStream mt) mt._Parent = null;
                stream.OnDisposing -= HandleDisposedStream;
            }
            await Parent.UpdateCurrentLimitAsync(syncRoot: true).DynamicContext();
            return stream;
        }

        /// <summary>
        /// Remove all streams
        /// </summary>
        /// <returns>Removed streams</returns>
        public IStreamThrottle[] Clear()
        {
            EnsureUndisposed(allowDisposing: true);
            try
            {
                using SemaphoreSyncContext ssc = Sync;
                return ClearUnblocked();
            }
            finally
            {
                Parent.UpdateCurrentLimit(syncRoot: true);
            }
        }

        /// <summary>
        /// Remove all streams
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Removed streams</returns>
        public async Task<IStreamThrottle[]> ClearAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed(allowDisposing: true);
            bool didClear = false;
            try
            {
                using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
                didClear = true;
                return ClearUnblocked();
            }
            finally
            {
                if (didClear) await Parent.UpdateCurrentLimitAsync(syncRoot: true).DynamicContext();
            }
        }

        /// <summary>
        /// Get the read count quota as 64 bit integer
        /// </summary>
        /// <returns>Read count quota</returns>
        protected long GetLongReadCountQuotaUnblocked()
            => _Streams.Sum(s =>
            {
                if (s is not MultiThrottledStream mt) return Math.Max(0, s.ReadCountQuota);
                using SemaphoreSyncContext ssc = mt.Sync;
                return mt.GetLongReadCountQuotaUnblocked();
            });

        /// <summary>
        /// Get the write count quota as 64 bit integer
        /// </summary>
        /// <returns>Write count quota</returns>
        protected long GetLongWriteCountQuotaUnblocked()
            => _Streams.Sum(s =>
            {
                if (s is not MultiThrottledStream mt) return Math.Max(0, s.WriteCountQuota);
                using SemaphoreSyncContext ssc = mt.Sync;
                return mt.GetLongWriteCountQuotaUnblocked();
            });

        /// <summary>
        /// Set new limits
        /// </summary>
        /// <param name="totalReadLimit">New reading count total limit</param>
        /// <param name="totalWriteLimit">New writing count total limit</param>
        protected void SetLimitsUnblocked(int totalReadLimit, int totalWriteLimit)
        {
            totalReadLimit = Math.Max(0, totalReadLimit);
            totalWriteLimit = Math.Max(0, totalWriteLimit);
            if (_TotalReadLimit == totalReadLimit && _TotalWriteLimit == totalWriteLimit) return;
            _TotalReadLimit = totalReadLimit;
            _TotalWriteLimit = totalWriteLimit;
        }

        /// <summary>
        /// Remove all streams
        /// </summary>
        /// <returns>Removed streams</returns>
        protected IStreamThrottle[] ClearUnblocked()
        {
            IStreamThrottle[] res = [.. _Streams];
            foreach (IStreamThrottle stream in res)
            {
                if (stream is MultiThrottledStream mt) mt._Parent = null;
                stream.OnDisposing -= HandleDisposedStream;
            }
            _Streams.Clear();
            return res;
        }

        /// <summary>
        /// Update the current processing count limit for all hosted streams (<see cref="Sync"/> should be synchronized)
        /// </summary>
        /// <param name="syncRoot">Synchronize the root?</param>
        protected virtual void UpdateCurrentLimit(bool syncRoot)
        {
            using SemaphoreSyncContext? rootSsc = syncRoot && _Parent is not null ? Root.Sync.SyncContext() : null;
            long totalReadQuota = GetLongReadCountQuotaUnblocked(),
                totalWriteQuota = GetLongWriteCountQuotaUnblocked(),
                longReadQuota,
                longWriteQuota;
            float oneReadLimit = (float)_TotalReadLimit / 100,
                oneWriteLimit = (float)_TotalWriteLimit / 100,
                readQuotaFactor = oneReadLimit == 0 ? 1 : (float)((double)totalReadQuota / oneReadLimit),
                writeQuotaFactor = oneWriteLimit == 0 ? 1 : (float)((double)totalWriteQuota / oneWriteLimit);
            int readLimit,
                writeLimit;
            foreach (IStreamThrottle throttle in _Streams)
                if (throttle is MultiThrottledStream mt)
                {
                    using SemaphoreSyncContext ssc = mt.Sync;
                    longReadQuota = mt._TotalReadLimit == 0 ? 0 : mt.GetLongReadCountQuotaUnblocked();
                    longWriteQuota = mt._TotalWriteLimit == 0 ? 0 : mt.GetLongWriteCountQuotaUnblocked();
                    if (longReadQuota < 1 && longWriteQuota < 1) continue;
                    readLimit = mt._TotalReadLimit == 0
                        ? 0
                        : (int)Math.Min(int.MaxValue, Math.Max(1, Math.Ceiling(Math.Min(longReadQuota, longReadQuota * readQuotaFactor))));
                    writeLimit = mt._TotalWriteLimit == 0
                        ? 0
                        : (int)Math.Min(int.MaxValue, Math.Max(1, Math.Ceiling(Math.Min(longWriteQuota, longWriteQuota * writeQuotaFactor))));
                    mt.SetLimitsUnblocked(readLimit, writeLimit);
                    mt.UpdateCurrentLimit(syncRoot: false);
                }
                else
                {
                    if (throttle.ReadCountQuota < 1 && throttle.WriteCountQuota < 1) continue;
                    throttle.ReadCount = throttle.ReadCount == 0
                        ? 0
                        : (int)Math.Max(1, Math.Ceiling(Math.Min(throttle.ReadCountQuota, throttle.ReadCountQuota * readQuotaFactor)));
                    throttle.WriteCount = throttle.WriteCount == 0
                        ? 0
                        : (int)Math.Max(1, Math.Ceiling(Math.Min(throttle.WriteCountQuota, throttle.WriteCountQuota * writeQuotaFactor)));
                }
        }

        /// <summary>
        /// Update the current processing count limit for all hosted streams (<see cref="Sync"/> should be synchronized)
        /// </summary>
        /// <param name="syncRoot">Synchronize the root?</param>
        protected virtual async Task UpdateCurrentLimitAsync(bool syncRoot)
        {
            using SemaphoreSyncContext? rootSsc = syncRoot && _Parent is not null ? await Root.Sync.SyncContextAsync().DynamicContext() : null;
            long totalReadQuota = GetLongReadCountQuotaUnblocked(),
                totalWriteQuota = GetLongWriteCountQuotaUnblocked(),
                longReadQuota,
                longWriteQuota;
            float oneReadLimit = (float)_TotalReadLimit / 100,
                oneWriteLimit = (float)_TotalWriteLimit / 100,
                readQuotaFactor = oneReadLimit == 0 ? 1 : (float)((double)totalReadQuota / oneReadLimit),
                writeQuotaFactor = oneWriteLimit == 0 ? 1 : (float)((double)totalWriteQuota / oneWriteLimit);
            int readLimit,
                writeLimit;
            foreach (IStreamThrottle throttle in _Streams)
                if (throttle is MultiThrottledStream mt)
                {
                    using SemaphoreSyncContext ssc = await mt.Sync.SyncContextAsync().DynamicContext();
                    longReadQuota = mt._TotalReadLimit == 0 ? 0 : mt.GetLongReadCountQuotaUnblocked();
                    longWriteQuota = mt._TotalWriteLimit == 0 ? 0 : mt.GetLongWriteCountQuotaUnblocked();
                    if (longReadQuota < 1 && longWriteQuota < 1) continue;
                    readLimit = mt._TotalReadLimit == 0
                        ? 0
                        : (int)Math.Min(int.MaxValue, Math.Max(1, Math.Ceiling(Math.Min(longReadQuota, longReadQuota * readQuotaFactor))));
                    writeLimit = mt._TotalWriteLimit == 0
                        ? 0
                        : (int)Math.Min(int.MaxValue, Math.Max(1, Math.Ceiling(Math.Min(longWriteQuota, longWriteQuota * writeQuotaFactor))));
                    mt.SetLimitsUnblocked(readLimit, writeLimit);
                    await mt.UpdateCurrentLimitAsync(syncRoot: false).DynamicContext();
                }
                else
                {
                    if (throttle.ReadCountQuota < 1 && throttle.WriteCountQuota < 1) continue;
                    throttle.ReadCount = throttle.ReadCount == 0
                        ? 0
                        : (int)Math.Max(1, Math.Ceiling(Math.Min(throttle.ReadCountQuota, throttle.ReadCountQuota * readQuotaFactor)));
                    throttle.WriteCount = throttle.WriteCount == 0
                        ? 0
                        : (int)Math.Max(1, Math.Ceiling(Math.Min(throttle.WriteCountQuota, throttle.WriteCountQuota * writeQuotaFactor)));
                }
        }

        /// <summary>
        /// Handle a disposed stream
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="e">Arguments</param>
        protected virtual void HandleDisposedStream(IDisposableObject obj, EventArgs e) => RemoveStream((IStreamThrottle)obj);

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
