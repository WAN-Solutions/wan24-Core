using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Asynchronous enumerable extensions
    /// </summary>
    public static class AsyncEnumerableExtensions
    {
        /// <summary>
        /// Get as array
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Array</returns>
        public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken = default)
            => (await enumerable.ToListAsync(cancellationToken).DynamicContext()).ToArray();

        /// <summary>
        /// Get as list
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List</returns>
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken = default)
        {
            List<T> res = new();
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken)) res.Add(item);
            return res;
        }

        /// <summary>
        /// Get as array
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Array</returns>
        public static async Task<T[]> ToArrayAsync<T>(this ConfiguredCancelableAsyncEnumerable<T> enumerable, CancellationToken cancellationToken = default)
            => (await enumerable.ToListAsync(cancellationToken).DynamicContext()).ToArray();

        /// <summary>
        /// Get as list
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List</returns>
        public static async Task<List<T>> ToListAsync<T>(this ConfiguredCancelableAsyncEnumerable<T> enumerable, CancellationToken cancellationToken = default)
        {
            List<T> res = new();
            await foreach (T item in enumerable.WithCancellation(cancellationToken)) res.Add(item);
            return res;
        }

        /// <summary>
        /// Return to the awaiting context
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumeration">Enumeration</param>
        /// <returns>Enumeration</returns>
        public static ConfiguredCancelableAsyncEnumerable<T> FixedContext<T>(this IAsyncEnumerable<T> enumeration) => enumeration.ConfigureAwait(continueOnCapturedContext: true);

        /// <summary>
        /// Return in any thread context
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumeration">Enumeration</param>
        /// <returns>Enumeration</returns>
        public static ConfiguredCancelableAsyncEnumerable<T> DynamicContext<T>(this IAsyncEnumerable<T> enumeration) => enumeration.ConfigureAwait(continueOnCapturedContext: false);
    }
}
