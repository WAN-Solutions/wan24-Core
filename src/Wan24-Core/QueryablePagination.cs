using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Queryable pagination
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="Queryable"></param>
    /// <param name="ItemsPerPage"></param>
    [StructLayout(LayoutKind.Sequential)]
    public record struct QueryablePagination<T>(in IQueryable<T> Queryable, in int ItemsPerPage)
    {
        /// <summary>
        /// Items per page
        /// </summary>
        public int ItemsPerPage = ItemsPerPage;
        /// <summary>
        /// Current page
        /// </summary>
        public int CurrentPage = 0;
        /// <summary>
        /// Queryable
        /// </summary>
        public IQueryable<T> Queryable = Queryable;

        /// <summary>
        /// Get the next page (<see cref="CurrentPage"/> will be increased, if <c>page</c> wasn't given)
        /// </summary>
        /// <param name="page">Specific page to get (will be set to <see cref="CurrentPage"/>)</param>
        /// <returns>Queryable (having a maximum number of <see cref="ItemsPerPage"/> items)</returns>
        public IQueryable<T> NextPage(in int? page = null)
        {
            if (ItemsPerPage < 1) throw new InvalidOperationException("No items per page");
            if (page.HasValue)
            {
                CurrentPage = page.Value;
            }
            else
            {
                CurrentPage++;
            }
            if (CurrentPage < 1) CurrentPage = 1;
            return Queryable.Skip(CurrentPage * ItemsPerPage).Take(ItemsPerPage);
        }

        /// <summary>
        /// Reset the pagination
        /// </summary>
        /// <param name="queryable">New queryable to use</param>
        /// <param name="itemsPerPage">New items per page</param>
        public void Reset(in IQueryable<T>? queryable = null, in int? itemsPerPage = null)
        {
            if (queryable is not null) Queryable = queryable;
            CurrentPage = 0;
            if (itemsPerPage.HasValue) ItemsPerPage = itemsPerPage.Value;
        }
    }
}
