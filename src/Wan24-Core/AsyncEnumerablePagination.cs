﻿using System.Runtime.CompilerServices;

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
    public sealed class AsyncEnumerablePagination<T>(in IAsyncEnumerator<T> enumerator, in int itemsPerPage) : DisposableBase()
    {
        /// <summary>
        /// Enumerator
        /// </summary>
        private readonly IAsyncEnumerator<T> Enumerator = enumerator;
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
        /// Current page
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
        /// Get the next page (<see cref="CurrentPage"/> will be increased, if <c>page</c> wasn't given)
        /// </summary>
        /// <param name="page">Specific page to get (will be set to <see cref="CurrentPage"/>; must be greater than the present <see cref="CurrentPage"/> value)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable (having a maximum number of <see cref="ItemsPerPage"/> items)</returns>
        public async IAsyncEnumerable<T> NextPageAsync(int? page = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (ItemsPerPage < 1) throw new InvalidOperationException("No items per page");
            await InterruptCurrentEnumerationAsync().DynamicContext();
            if (IsDone) yield break;
            if (CurrentPageItemIndex < ItemsPerPage) await MoveForwardAsync(ItemsPerPage - CurrentPageItemIndex).DynamicContext();
            if (IsDone) yield break;
            int prevPage = CurrentPage;
            if (page.HasValue) CurrentPage = page.Value;
            if (CurrentPage < 1) CurrentPage = 1;
            if (prevPage >= CurrentPage)
            {
                CurrentPage = prevPage;
                throw new InvalidOperationException("Can't enumerate backward");
            }
            using PageEnumerator enumerator = new(this);
            CurrentEnumerator = enumerator;
            while (await enumerator.MoveNextAsync().DynamicContext())
            {
                if (cancellationToken.IsCancellationRequested) yield break;
                yield return enumerator.Current;
            }
            if (CurrentEnumerator == enumerator) CurrentEnumerator = null;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            InterruptCurrentEnumerationAsync().GetAwaiter().GetResult();
            Enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await InterruptCurrentEnumerationAsync().DynamicContext();
            await Enumerator.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Interrupt the current enumeration
        /// </summary>
        private async Task InterruptCurrentEnumerationAsync()
        {
            if (CurrentEnumerator is PageEnumerator enumerator)
            {
                await enumerator.DisposeAsync().DynamicContext();
                CurrentEnumerator = null;
            }
        }

        /// <summary>
        /// Move N items forward
        /// </summary>
        /// <param name="count">Count</param>
        private async Task MoveForwardAsync(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!await Enumerator.MoveNextAsync().DynamicContext())
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
        /// Page enumerator
        /// </summary>
        /// <param name="pagination">Pagination</param>
        private sealed class PageEnumerator(in AsyncEnumerablePagination<T> pagination) : DisposableBase(asyncDisposing: false), IAsyncEnumerator<T>
        {
            /// <summary>
            /// Pagination
            /// </summary>
            private readonly AsyncEnumerablePagination<T> Pagination = pagination;
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
            public async ValueTask<bool> MoveNextAsync()
            {
                if (!EnsureUndisposed(throwException: false)) return false;
                EnsureValidState();
                if (Pagination.IsDone || CurrentItemIndex >= Pagination.ItemsPerPage) return false;
                if (await Pagination.Enumerator.MoveNextAsync().DynamicContext())
                {
                    CurrentItemIndex++;
                    return true;
                }
                Pagination.IsDone = true;
                return false;
            }

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