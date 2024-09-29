using System.Collections;

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
    public sealed class CachedEnumerablePagination<T>(in IEnumerator<T> enumerator, in int itemsPerPage, in int? cacheCapacity = null) : DisposableBase(asyncDisposing: false)
    {
        /// <summary>
        /// Enumerator
        /// </summary>
        private readonly IEnumerator<T> Enumerator = enumerator;
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
        /// Last exception (if not <see langword="null"/>, <see cref="NextPage(int?)"/> can't be used anymore)
        /// </summary>
        public Exception? LastException { get; private set; }

        /// <summary>
        /// Get the next page (<see cref="CurrentPage"/> will be increased, if <c>page</c> wasn't given)
        /// </summary>
        /// <param name="page">Specific page to get (will be set to <see cref="CurrentPage"/>; must be greater than the present <see cref="CurrentPage"/> value)</param>
        /// <returns>Enumerable (having a maximum number of <see cref="ItemsPerPage"/> items)</returns>
        public IEnumerable<T> NextPage(int? page = null)
        {
            EnsureUndisposed();
            if (LastException is not null) throw new AggregateException(LastException);
            if (ItemsPerPage < 1) throw new InvalidOperationException("No items per page");
            try
            {
                InterruptCurrentEnumeration();
                bool increasePage = true;
                if (CurrentPage > 0 && CurrentPageItemIndex < ItemsPerPage)
                {
                    MoveForward(ItemsPerPage - CurrentPageItemIndex);
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
                if (prevPage > 0 && prevPage >= CurrentPage) MoveBackward((CurrentPage - 1) * ItemsPerPage);
                if (IsDone) yield break;
            }
            catch (Exception ex)
            {
                LastException = ex;
                throw new AggregateException(ex);
            }
            using PageEnumerator enumerator = new(this);
            CurrentEnumerator = enumerator;
            while (true)
            {
                try
                {
                    if (!enumerator.MoveNext()) break;
                }
                catch(Exception ex)
                {
                    LastException = ex;
                    throw new AggregateException(ex);
                }
                yield return enumerator.Current;
            }
            if (CurrentEnumerator == enumerator) CurrentEnumerator = null;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            InterruptCurrentEnumeration();
            Enumerator.Dispose();
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
        private void MoveForward(int count)
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
                if (!Enumerator.MoveNext())
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
        private void MoveBackward(in int index)
        {
            IsDone = false;
            CurrentPage = 1;
            CurrentPageItemIndex = 0;
            if (index > 0) MoveForward(index);
        }

        /// <summary>
        /// Page enumerator
        /// </summary>
        /// <param name="pagination">Pagination</param>
        private sealed class PageEnumerator(in CachedEnumerablePagination<T> pagination) : BasicDisposableBase(), IEnumerator<T>
        {
            /// <summary>
            /// Pagination
            /// </summary>
            private readonly CachedEnumerablePagination<T> Pagination = pagination;
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
            object IEnumerator.Current => Current!;

            /// <inheritdoc/>
            public bool MoveNext()
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
                if (Pagination.Enumerator.MoveNext())
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
            public void Reset() { }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing) { }

            /// <summary>
            /// Ensure a valid pagination state
            /// </summary>
            /// <exception cref="InvalidOperationException">The pagination moved on with another page already</exception>
            private void EnsureValidState()
            {
                if (Pagination.LastException is not null) throw new AggregateException(Pagination.LastException);
                if (Pagination.CurrentPage != Page) throw new InvalidOperationException("The pagination moved on with another page already");
            }
        }
    }
}
