using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Distinct
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        public static async IAsyncEnumerable<T> DistinctAsync<T>(this IAsyncEnumerable<T> enumerable, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            HashSet<T> seen = [];
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (seen.Add(item))
                    yield return item;
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        public static async IAsyncEnumerable<tItem> DistinctByAsync<tItem, tKey>(
            this IAsyncEnumerable<tItem> enumerable, 
            Func<tItem, tKey> keySelector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            HashSet<tKey> seen = [];
            await foreach (tItem item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (seen.Add(keySelector(item)))
                    yield return item;
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        public static async IAsyncEnumerable<tItem> DistinctByAsync<tItem, tKey>(
            this IAsyncEnumerable<tItem> enumerable,
            Func<tItem, CancellationToken, Task<tKey>> keySelector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            HashSet<tKey> seen = [];
            await foreach (tItem item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (seen.Add(await keySelector(item, cancellationToken).DynamicContext()))
                    yield return item;
        }
    }
}
