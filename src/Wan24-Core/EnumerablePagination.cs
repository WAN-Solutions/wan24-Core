using System.Collections;

namespace wan24.Core
{
    /// <summary>
    /// Enumerable pagination
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="enumerator">Enumerator</param>
    /// <param name="itemsPerPage">Items per page</param>
    public sealed class EnumerablePagination<T>(in IEnumerator<T> enumerator, in int itemsPerPage) : DisposableBase(asyncDisposing: false)
    {
        /// <summary>
        /// Enumerator
        /// </summary>
        private readonly IEnumerator<T> Enumerator = enumerator;
        /// <summary>
        /// Current page item index
        /// </summary>
        private int CurrentPageItemIndex = 0;
        /// <summary>
        /// Current page enumerator
        /// </summary>
        private PageEnumerator? CurrentEnumerator = null;

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
        /// If enumerating
        /// </summary>
        public bool IsEnumerating => CurrentEnumerator is not null;

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
            if (ItemsPerPage < 1) throw new InvalidOperationException("No items per page");
            if (LastException is not null) throw new AggregateException(LastException);
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
        private void MoveForward(in int count)
        {
            if (count < 1) return;
            for (int i = 0; i < count; i++)
            {
                if (!Enumerator.MoveNext())
                {
                    IsDone = true;
                    return;
                }
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
            Enumerator.Reset();
            IsDone = false;
            CurrentPage = 1;
            CurrentPageItemIndex = 0;
            if (index > 0) MoveForward(index);
        }

        /// <summary>
        /// Page enumerator
        /// </summary>
        /// <param name="pagination">Pagination</param>
        private sealed class PageEnumerator(in EnumerablePagination<T> pagination) : DisposableBase(asyncDisposing: false), IEnumerator<T>
        {
            /// <summary>
            /// Pagination
            /// </summary>
            private readonly EnumerablePagination<T> Pagination = pagination;
            /// <summary>
            /// Page
            /// </summary>
            private readonly int Page = pagination.CurrentPage;
            /// <summary>
            /// Current item index
            /// </summary>
            private int CurrentItemIndex = 0;

            /// <inheritdoc/>
            public T Current
            {
                get
                {
                    EnsureUndisposed();
                    EnsureValidState();
                    return Pagination.Enumerator.Current;
                }
            }

            /// <inheritdoc/>
            object IEnumerator.Current => Current!;

            /// <inheritdoc/>
            public bool MoveNext()
            {
                if (!EnsureUndisposed(throwException: false)) return false;
                EnsureValidState();
                if (Pagination.IsDone || CurrentItemIndex >= Pagination.ItemsPerPage) return false;
                if (Pagination.Enumerator.MoveNext())
                {
                    CurrentItemIndex++;
                    return true;
                }
                Pagination.IsDone = true;
                return false;
            }

            /// <inheritdoc/>
            public void Reset() { }

            /// <inheritdoc/>
            protected override void Dispose(bool disposing) => Pagination.CurrentPageItemIndex += CurrentItemIndex;

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
