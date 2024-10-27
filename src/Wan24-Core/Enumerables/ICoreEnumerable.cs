/*
 * Where
 * Select
 * Count(Async)
 * Contains(All|Any|AtLeast|AtMost)
 * All(Async)
 * Any(Async)
 * ExecuteForAll(Async)
 * DiscardAll(Async)
 * Distinct(By(Async))
 * First(OrDefault)(Async)
 * (Skip|Take)(While)(Async)
 * To(Array|Buffer|List)
 * Process
 */

namespace wan24.Core.Enumerables
{
    /// <summary>
    /// Interface for a <c>wan24-Core</c> enumerable
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public interface ICoreEnumerable<T> : IEnumerable<T>
    {
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        ICoreEnumerable<T> Where(Func<T, bool> predicate);
        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="selector">Selector</param>
        /// <returns>Enumerable</returns>
        ICoreEnumerable<tResult> Select<tResult>(Func<T, tResult> selector);
        /// <summary>
        /// Count
        /// </summary>
        /// <returns>Count</returns>
        int Count();
        /// <summary>
        /// Count
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Count</returns>
        int Count(Func<T, bool> predicate);
        /// <summary>
        /// Count
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Count</returns>
        Task<int> CountAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default);
        /// <summary>
        /// Determine if an item is contained
        /// </summary>
        /// <param name="obj">Item</param>
        /// <returns>If the item is contained</returns>
        bool Contains(T obj);
        /// <summary>
        /// Determine if all items are contained
        /// </summary>
        /// <param name="objs">Items</param>
        /// <returns>If all items are contained</returns>
        bool ContainsAll(params T[] objs);
        /// <summary>
        /// Determine if any item is contained
        /// </summary>
        /// <param name="objs">Items</param>
        /// <returns>If any item is contained</returns>
        bool ContainsAny(params T[] objs);
        /// <summary>
        /// Determine if at least N items are contained
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>If at least <c>count</c> items are contained</returns>
        bool ContainsAtLeast(in int count);
        /// <summary>
        /// Determine if at most N items are contained
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>If at most <c>count</c> items are contained</returns>
        bool ContainsAtMost(in int count);
        /// <summary>
        /// All
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>If the predicate returned <see langword="true"/> for all items</returns>
        bool All(in Func<T, bool> predicate);
        /// <summary>
        /// All
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If the predicate returned <see langword="true"/> for all items</returns>
        Task<bool> AllAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default);
        /// <summary>
        /// Any
        /// </summary>
        /// <returns>If any</returns>
        bool Any();
        /// <summary>
        /// Any
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>If the predicate returned <see langword="true"/> for all items</returns>
        bool Any(Func<T, bool> predicate);
        /// <summary>
        /// Any
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If the predicate returned <see langword="true"/> for all items</returns>
        Task<bool> AnyAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default);
        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <param name="action">Action</param>
        void ExecuteForAll(in Action<T> action);
        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tReturn">Return value type</typeparam>
        /// <param name="action">Action</param>
        /// <returns>Return values</returns>
        IEnumerable<tReturn> ExecuteForAll<tReturn>(Func<T, EnumerableExtensions.ExecuteResult<tReturn>> action);
        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ExecuteForAllAsync(Func<T, CancellationToken, Task> action, CancellationToken cancellationToken = default);
        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tReturn">Return value type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return values</returns>
        IAsyncEnumerable<tReturn> ExecuteForAllAsync<tReturn>(
            Func<T, CancellationToken, Task<EnumerableExtensions.ExecuteResult<tReturn>>> action,
            CancellationToken cancellationToken = default
            );
        /// <summary>
        /// Discard all items
        /// </summary>
        /// <param name="dispose">If to dispose all disposable items</param>
        /// <returns>Number of items</returns>
        int DiscardAll(in bool dispose = true);
        /// <summary>
        /// Discard all items
        /// </summary>
        /// <param name="dispose">If to dispose all disposable items</param>
        /// <returns>Number of items</returns>
        Task<int> DiscardAllAsync(bool dispose = true);
        /// <summary>
        /// Distinct
        /// </summary>
        /// <returns>Items</returns>
        IEnumerable<T> Distinct();
        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="keySelector">Key selector</param>
        /// <returns>Items</returns>
        IEnumerable<T> DistinctBy<tKey>(Func<T, tKey> keySelector);
        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="keySelector">Key selector</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        IAsyncEnumerable<T> DistinctByAsync<tKey>(
            Func<T, CancellationToken, Task<tKey>> keySelector,
            CancellationToken cancellationToken = default
            );
        /// <summary>
        /// First
        /// </summary>
        /// <returns>Item</returns>
        T First();
        /// <summary>
        /// First
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Item</returns>
        T First(Func<T, bool> predicate);
        /// <summary>
        /// First
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Item</returns>
        Task<T> FirstAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default);
        /// <summary>
        /// First or default
        /// </summary>
        /// <param name="defaultValue">Default</param>
        /// <returns>Item</returns>
        T? FirstOrDefault(T? defaultValue = default);
        /// <summary>
        /// First or default
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultValue">Default</param>
        /// <returns>Item</returns>
        T? FirstOrDefault(Func<T, bool> predicate, T? defaultValue = default);
        /// <summary>
        /// First or default
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultValue">Default</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Item</returns>
        Task<T?> FirstOrDefaultAsync(Func<T, CancellationToken, Task<bool>> predicate, T? defaultValue = default, CancellationToken cancellationToken = default);
        /// <summary>
        /// Skip
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Enumerable</returns>
        ICoreEnumerable<T> Skip(int count);
        /// <summary>
        /// Skip while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        ICoreEnumerable<T> SkipWhile(Func<T, bool> predicate);
        /// <summary>
        /// Skip while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        IAsyncEnumerable<T> SkipWhileAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default);
        /// <summary>
        /// Take
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Enumerable</returns>
        ICoreEnumerable<T> Take(int count);
        /// <summary>
        /// Take while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Enumerable</returns>
        ICoreEnumerable<T> TakeWhile(Func<T, bool> predicate);
        /// <summary>
        /// Take while
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        IAsyncEnumerable<T> TakeWhileAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default);
        /// <summary>
        /// To array
        /// </summary>
        /// <returns>Array</returns>
        T[] ToArray();
        /// <summary>
        /// To buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of items written to the buffer</returns>
        /// <exception cref="OutOfMemoryException">Buffer too small</exception>
        int ToBuffer(in Span<T> buffer);
        /// <summary>
        /// To list
        /// </summary>
        /// <returns>List</returns>
        List<T> ToList();
        /// <summary>
        /// Process the enumeration and get a new basic array enumerable
        /// </summary>
        /// <returns>Enumerable</returns>
        ArrayEnumerable<T> Process();
    }
}
