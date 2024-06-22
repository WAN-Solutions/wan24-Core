namespace wan24.Core
{
    // Events
    public partial class InMemoryCache<T>
    {
        /// <summary>
        /// Handle entry added
        /// </summary>
        /// <param name="entry">Cache entry</param>
        protected virtual void HandleEntryAdded(InMemoryCacheEntry<T> entry) { }

        /// <summary>
        /// Handle entry added
        /// </summary>
        /// <param name="entry">Cache entry</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual Task HandleEntryAddedAsync(InMemoryCacheEntry<T> entry, CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Get an entry
        /// </summary>
        /// <param name="key">Unique cache entry key</param>
        /// <returns>Cache entry</returns>
        protected virtual InMemoryCacheEntry<T>? GetEntry(string key) => null;

        /// <summary>
        /// Get an entry
        /// </summary>
        /// <param name="key">Unique cache entry key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cache entry</returns>
        protected virtual Task<InMemoryCacheEntry<T>?> GetEntryAsync(string key, CancellationToken cancellationToken)
            => Task.FromResult<InMemoryCacheEntry<T>?>(null);

        /// <summary>
        /// Handle entry removed
        /// </summary>
        /// <param name="entry">Cache entry</param>
        protected virtual void HandleEntryRemoved(InMemoryCacheEntry<T> entry) { }

        /// <summary>
        /// Handle entry removed
        /// </summary>
        /// <param name="entry">Cache entry</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual Task HandleEntryRemovedAsync(InMemoryCacheEntry<T> entry, CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Delegate for an event handler
        /// </summary>
        /// <param name="cache">Cache</param>
        /// <param name="e">Arguments</param>
        public delegate void InMemoryCacheEvent_Delegate(IInMemoryCache cache, EntryEventArgs e);

        /// <summary>
        /// Raised when an entry was added
        /// </summary>
        public event InMemoryCacheEvent_Delegate? OnEntryAdded;
        /// <summary>
        /// Raise the <see cref="OnEntryAdded"/> event
        /// </summary>
        /// <param name="entry">Cache entry</param>
        protected virtual void RaiseOnEntryAdded(in InMemoryCacheEntry<T> entry) => OnEntryAdded?.Invoke(this, new(entry));

        /// <summary>
        /// Raised when an entry was removed
        /// </summary>
        public event InMemoryCacheEvent_Delegate? OnEntryRemoved;
        /// <summary>
        /// Raise the <see cref="OnEntryRemoved"/> event
        /// </summary>
        /// <param name="entry">Cache entry</param>
        protected virtual void RaiseOnEntryRemoved(in InMemoryCacheEntry<T> entry) => OnEntryRemoved?.Invoke(this, new(entry));

        /// <summary>
        /// Cache entry event arguments
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="entry">Cache entry</param>
        public class EntryEventArgs(in InMemoryCacheEntry<T> entry) : EventArgs()
        {
            /// <summary>
            /// Cache entry
            /// </summary>
            public T Entry { get; } = entry;
        }
    }
}
