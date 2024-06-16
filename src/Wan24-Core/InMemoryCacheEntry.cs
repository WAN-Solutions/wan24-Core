namespace wan24.Core
{
    /// <summary>
    /// In-memory cache entry
    /// </summary>
    /// <typeparam name="T">Cached item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="key">Cache entry key</param>
    /// <param name="item">Cached item</param>
    /// <param name="size">Size (may be any number which indicates the <c>item</c> size, if you work with sizes)</param>
    public class InMemoryCacheEntry<T>(in string key, in T item, in int size = 1)
    {
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        protected readonly object SyncObject = new();
        /// <summary>
        /// Size
        /// </summary>
        protected readonly int _Size = size;
        /// <summary>
        /// If added to the cache
        /// </summary>
        protected bool WasAdded = false;
        /// <summary>
        /// If removed from the cache
        /// </summary>
        protected bool WasRemoved = false;

        /// <summary>
        /// Hosting cache
        /// </summary>
        public required InMemoryCache<T> Cache { get; init; }

        /// <summary>
        /// If the item can be used
        /// </summary>
        public virtual bool CanUse
        {
            get
            {
                if (IsTimeout) return false;
                if (Cache.Options.AgeLimit > TimeSpan.Zero && Age > Cache.Options.AgeLimit) return false;
                if (Cache.Options.IdleLimit > TimeSpan.Zero && Idle > Cache.Options.IdleLimit) return false;
                return true;
            }
        }

        /// <summary>
        /// Type
        /// </summary>
        public InMemoryCacheEntryTypes Type { get; init; } = InMemoryCacheEntryTypes.Variable;

        /// <summary>
        /// Absolute timeout
        /// </summary>
        public DateTime? AbsoluteTimeout { get; init; }

        /// <summary>
        /// Timeout (if <see cref="Type"/> is <see cref="InMemoryCacheEntryTypes.Timeout"/>)
        /// </summary>
        public TimeSpan Timeout { get; init; }

        /// <summary>
        /// Is the <see cref="Timeout"/> a sliding timeout? (has only an effect if <see cref="Type"/> is <see cref="InMemoryCacheEntryTypes.Timeout"/>)
        /// </summary>
        public bool IsSlidingTimeout { get; init; }

        /// <summary>
        /// If timeout (cache entry defined <see cref="Timeout"/>/<see cref="AbsoluteTimeout"/> only (the cache configuration may limit the entries configuration))
        /// </summary>
        public bool IsTimeout
            => (AbsoluteTimeout.HasValue && AbsoluteTimeout.Value <= DateTime.Now) ||
                (Type == InMemoryCacheEntryTypes.Timeout && (IsSlidingTimeout ? Idle > Timeout : Age > Timeout));

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime Created { get; protected set; } = DateTime.Now;

        /// <summary>
        /// Age
        /// </summary>
        public TimeSpan Age => DateTime.Now - Created;

        /// <summary>
        /// Last accessed
        /// </summary>
        public DateTime Accessed { get; protected set; } = DateTime.Now;

        /// <summary>
        /// Cast as idle time
        /// </summary>
        public TimeSpan Idle => DateTime.Now - Accessed;

        /// <summary>
        /// Number of access
        /// </summary>
        public long AccessCount { get; protected set; }

        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; } = key;

        /// <summary>
        /// Item
        /// </summary>
        public virtual T Item { get; } = item;

        /// <summary>
        /// Size
        /// </summary>
        public virtual int Size => (Item as IInMemoryCacheItem)?.Size ?? _Size;

        /// <summary>
        /// Observe item disposing (works only for <see cref="IDisposableObject"/> types; disposing items will be removed from the cache automatic)
        /// </summary>
        public bool ObserveDisposing { get; init; }

        /// <summary>
        /// Initialize after this was added to the cache
        /// </summary>
        public virtual void OnAdded()
        {
            lock (SyncObject)
            {
                if (WasRemoved)
                    throw new InvalidOperationException("Has been removed already");
                if (WasAdded)
                    return;
                WasAdded = true;
            }
            if (ObserveDisposing && Item is IDisposableObject disposable)
            {
                if (disposable.IsDisposing)
                    throw new InvalidOperationException("Item is disposed already");
                disposable.OnDisposing += HandleItemDisposing;
            }
        }

        /// <summary>
        /// Count an access
        /// </summary>
        public virtual void OnAccess()
        {
            lock (SyncObject)
            {
                Accessed = DateTime.Now;
                AccessCount++;
            }
        }

        /// <summary>
        /// Refresh the entry (postporne timeouts; won't refresh if not usable already)
        /// </summary>
        /// <param name="resetAccessCounter">If to reset the <see cref="AccessCount"/> to zero</param>
        /// <returns>If refreshed</returns>
        public virtual bool Refresh(in bool resetAccessCounter = false)
        {
            if (!CanUse)
                return false;
            lock (SyncObject)
            {
                Accessed = Created = DateTime.Now;
                if (resetAccessCounter)
                    AccessCount = 0;
            }
            return true;
        }

        /// <summary>
        /// Handle this removed from the cache
        /// </summary>
        public virtual void OnRemoved()
        {
            lock (SyncObject)
            {
                if (WasRemoved)
                    return;
                WasRemoved = true;
                if (!WasAdded)
                    return;
            }
            if (ObserveDisposing && Item is IDisposableObject disposable)
                disposable.OnDisposing -= HandleItemDisposing;
        }

        /// <summary>
        /// Handle the item disposing
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected virtual void HandleItemDisposing(IDisposableObject sender, EventArgs e) => Cache.Remove(this);

        /// <summary>
        /// Cast as item
        /// </summary>
        /// <param name="entry">Cache entry</param>
        public static implicit operator T(in InMemoryCacheEntry<T> entry) => entry.Item;

        /// <summary>
        /// Cast as size
        /// </summary>
        /// <param name="entry">Cache entry</param>
        public static implicit operator int(in InMemoryCacheEntry<T> entry) => entry.Size;

        /// <summary>
        /// Cast as <see cref="CanUse"/> flag
        /// </summary>
        /// <param name="entry">Cache entry</param>
        public static implicit operator bool(in InMemoryCacheEntry<T> entry) => entry.CanUse;

        /// <summary>
        /// Cast as created time
        /// </summary>
        /// <param name="entry">Cache entry</param>
        public static implicit operator DateTime(in InMemoryCacheEntry<T> entry) => entry.Created;

        /// <summary>
        /// Cast as age
        /// </summary>
        /// <param name="entry">Cache entry</param>
        public static implicit operator TimeSpan(in InMemoryCacheEntry<T> entry) => entry.Age;
    }
}
