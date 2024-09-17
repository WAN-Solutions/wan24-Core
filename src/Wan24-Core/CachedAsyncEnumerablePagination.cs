using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Cached enumerable pagination
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="enumerator">Enumerator</param>
    /// <param name="itemsPerPage">Items per page</param>
    /// <param name="cacheCapacity">Cache initial capacity</param>
    public sealed class CachedAsyncEnumerablePagination<T>(in IAsyncEnumerator<T> enumerator, in int itemsPerPage, in int? cacheCapacity = null) : DisposableBase()
    {
        /// <summary>
        /// Enumerator
        /// </summary>
        private readonly IAsyncEnumerator<T> Enumerator = enumerator;
        /// <summary>
        /// Cache
        /// </summary>
        private readonly FreezableList<T> Cache = cacheCapacity.HasValue ? new(cacheCapacity.Value) : [];
        /// <summary>
        /// Current page item index
        /// </summary>
        private int CurrentPageItemIndex = 0;
        /// <summary>
        /// Current page enumerator
        /// </summary>
        private PageEnumerator? CurrentEnumerator = null;

        /// <summary>
        /// Cache index of the current item
        /// </summary>
        private int CurrentCacheIndex => (CurrentPage - 1) * ItemsPerPage + CurrentPageItemIndex;

        /// <summary>
        /// Items per page
        /// </summary>
        public int ItemsPerPage { get; } = itemsPerPage;

        /// <summary>
        /// Current page (is zero, if not started enumerating yet)
        /// </summary>
        public int CurrentPage { get; private set; } = 0;

        /// <summary>
        /// If done (no more items will be enumerated)
        /// </summary>
        public bool IsDone { get; private set; }

        /// <summary>
        /// If fully enumerated once
        /// </summary>
        public bool FullyEnumerated { get; private set; }

        /// <summary>
        /// If enumerating
        /// </summary>
        public bool IsEnumerating => CurrentEnumerator is not null;

        /// <summary>
        /// Number of cached items
        /// </summary>
        public int CacheCount => Cache.Count;

        /// <summary>
        /// Last exception (if not <see langword="null"/>, <see cref="NextPageAsync(int?, CancellationToken)"/> can't be used anymore)
        /// </summary>
        public Exception? LastException { get; private set; }

        /// <summary>
        /// Get the next page (<see cref="CurrentPage"/> will be increased, if <c>page</c> wasn't given)
        /// </summary>
        /// <param name="page">Specific page to get (will be set to <see cref="CurrentPage"/>; must be greater than the present <see cref="CurrentPage"/> value)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable (having a maximum number of <see cref="ItemsPerPage"/> items)</returns>
        public async IAsyncEnumerable<T> NextPageAsync(int? page = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (LastException is not null) throw new AggregateException(LastException);
            try
            {
                if (ItemsPerPage < 1) throw new InvalidOperationException("No items per page");
                InterruptCurrentEnumeration();
                bool increasePage = true;
                if (CurrentPage > 0 && CurrentPageItemIndex < ItemsPerPage)
                {
                    await MoveForwardAsync(ItemsPerPage - CurrentPageItemIndex).DynamicContext();
                    increasePage = false;
                }
                int prevPage = CurrentPage;
                if (page.HasValue)
                {
                    CurrentPage = page.Value;
                }
                else if (increasePage)
                {
                    CurrentPage++;
                    CurrentPageItemIndex = 0;
                }
                if (CurrentPage < 1) CurrentPage = 1;
                if (prevPage > 0 && prevPage >= CurrentPage) await MoveBackwardAsync((CurrentPage - 1) * ItemsPerPage).DynamicContext();
                if (IsDone) yield break;
            }
            catch (Exception ex)
            {
                LastException = ex;
                throw new AggregateException(ex);
            }
            using PageEnumerator enumerator = new(this);
            CurrentEnumerator = enumerator;
            while (await enumerator.MoveNextAsync().DynamicContext())
            {
                try
                {
                    if (!await enumerator.MoveNextAsync().DynamicContext()) break;
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    throw new AggregateException(ex);
                }
                if (cancellationToken.IsCancellationRequested) break;
                yield return enumerator.Current;
            }
            if (CurrentEnumerator == enumerator) CurrentEnumerator = null;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            InterruptCurrentEnumeration();
            Enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
            if (Cache.IsFrozen) Cache.Unfreeze();
            Cache.Clear();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            InterruptCurrentEnumeration();
            await Enumerator.DisposeAsync().DynamicContext();
            if (Cache.IsFrozen) Cache.Unfreeze();
            Cache.Clear();
        }

        /// <summary>
        /// Interrupt the current enumeration
        /// </summary>
        private void InterruptCurrentEnumeration()
        {
            if (CurrentEnumerator is PageEnumerator enumerator)
            {
                enumerator.Dispose();
                CurrentEnumerator = null;
            }
        }

        /// <summary>
        /// Move N items forward
        /// </summary>
        /// <param name="count">Count</param>
        private async Task MoveForwardAsync(int count)
        {
            if (count < 1) return;
            int currentCacheIndex = CurrentCacheIndex,
                newCacheIndex = currentCacheIndex + count;
            if (newCacheIndex < Cache.Count)
            {
                CurrentPageItemIndex += count;
                CurrentPage += (int)Math.Floor((float)CurrentPageItemIndex / ItemsPerPage);
                CurrentPageItemIndex %= ItemsPerPage;
                if (FullyEnumerated && CurrentCacheIndex == Cache.Count - 1) IsDone = true;
                return;
            }
            if (currentCacheIndex < Cache.Count)
            {
                int cached = count - (Cache.Count - currentCacheIndex);
                CurrentPageItemIndex += cached;
                CurrentPage += (int)Math.Floor((float)CurrentPageItemIndex / ItemsPerPage);
                CurrentPageItemIndex %= ItemsPerPage;
                count -= cached;
            }
            if (FullyEnumerated)
            {
                IsDone = true;
                return;
            }
            for (int i = 0; i < count; i++)
            {
                if (!await Enumerator.MoveNextAsync().DynamicContext())
                {
                    IsDone = true;
                    FullyEnumerated = true;
                    Cache.Freeze();
                    return;
                }
                Cache.Add(Enumerator.Current);
                CurrentPageItemIndex++;
                if (CurrentPageItemIndex >= ItemsPerPage)
                {
                    CurrentPage++;
                    CurrentPageItemIndex = 0;
                }
            }
        }

        /// <summary>
        /// Move backward to item index N
        /// </summary>
        /// <param name="index">Index</param>
        private async Task MoveBackwardAsync(int index)
        {
            IsDone = false;
            CurrentPage = 1;
            CurrentPageItemIndex = 0;
            if (index > 0) await MoveForwardAsync(index).DynamicContext();
        }

        /// <summary>
        /// Page enumerator
        /// </summary>
        /// <param name="pagination">Pagination</param>
        private sealed class PageEnumerator(in CachedAsyncEnumerablePagination<T> pagination) : DisposableBase(asyncDisposing: false), IAsyncEnumerator<T>
        {
            /// <summary>
            /// Pagination
            /// </summary>
            private readonly CachedAsyncEnumerablePagination<T> Pagination = pagination;
            /// <summary>
            /// Page
            /// </summary>
            private readonly int Page = pagination.CurrentPage;

            /// <inheritdoc/>
            public T Current
            {
                get
                {
                    EnsureUndisposed();
                    EnsureValidState();
                    return Pagination.Cache[Pagination.CurrentCacheIndex - 1];
                }
            }

            /// <inheritdoc/>
            public async ValueTask<bool> MoveNextAsync()
            {
                if (!EnsureUndisposed(throwException: false)) return false;
                EnsureValidState();
                if (Pagination.IsDone || Pagination.CurrentPageItemIndex >= Pagination.ItemsPerPage) return false;
                if (Pagination.CurrentCacheIndex < Pagination.Cache.Count)
                {
                    Pagination.CurrentPageItemIndex++;
                    return true;
                }
                if (Pagination.FullyEnumerated)
                {
                    Pagination.IsDone = true;
                    return false;
                }
                if (await Pagination.Enumerator.MoveNextAsync().DynamicContext())
                {
                    Pagination.CurrentPageItemIndex++;
                    Pagination.Cache.Add(Pagination.Enumerator.Current);
                    return true;
                }
                Pagination.IsDone = true;
                Pagination.FullyEnumerated = true;
                Pagination.Cache.Freeze();
                return false;
            }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing) { }

            /// <summary>
            /// Ensure a valid pagination state
            /// </summary>
            /// <exception cref="InvalidOperationException">The pagination moved on with another page already</exception>
            private void EnsureValidState()
            {
                if (Pagination.CurrentPage != Page) throw new InvalidOperationException("The pagination moved on with another page already");
            }
        }
    }
}
