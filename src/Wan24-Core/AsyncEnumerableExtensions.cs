using System.Runtime;
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
        [TargetedPatchingOptOut("Just a method adapter")]
        public static ConfiguredCancelableAsyncEnumerable<T> FixedContext<T>(this IAsyncEnumerable<T> enumeration)
            => enumeration.ConfigureAwait(continueOnCapturedContext: true);

        /// <summary>
        /// Return in any thread context
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumeration">Enumeration</param>
        /// <returns>Enumeration</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static ConfiguredCancelableAsyncEnumerable<T> DynamicContext<T>(this IAsyncEnumerable<T> enumeration)
            => enumeration.ConfigureAwait(continueOnCapturedContext: false);

        /// <summary>
        /// Combine enumerables
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerables">Enumerables</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Combined enumerable</returns>
        public static async IAsyncEnumerable<T> CombineAsync<T>(
            this IAsyncEnumerable<IEnumerable<T>> enumerables,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (IEnumerable<T> e in enumerables.DynamicContext().WithCancellation(cancellationToken))
                foreach (T item in e)
                    yield return item;
        }

        /// <summary>
        /// Combine enumerables
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerables">Enumerables</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Combined enumerable</returns>
        public static async IAsyncEnumerable<T> CombineAsync<T>(
            this IAsyncEnumerable<IAsyncEnumerable<T>> enumerables,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (IAsyncEnumerable<T> e in enumerables.DynamicContext().WithCancellation(cancellationToken))
                await foreach (T item in e.DynamicContext().WithCancellation(cancellationToken))
                    yield return item;
        }

        /// <summary>
        /// Chunk an enumerable
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="chunkSize">Chunk size</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Chunks</returns>
        public static async IAsyncEnumerable<T[]> ChunkEnumAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            int chunkSize,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (chunkSize < 1) throw new ArgumentOutOfRangeException(nameof(chunkSize));
            List<T> res = new(chunkSize);
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                res.Add(item);
                if (res.Count < chunkSize) continue;
                yield return res.ToArray();
                res.Clear();
            }
            if (res.Count > 0) yield return res.ToArray();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="disposables">Disposables</param>
        public static DisposingAsyncEnumerator<T> ToDipsposingAsyncEnumerator<T>(
            this IAsyncEnumerable<T> enumerable,
            in CancellationToken cancellationToken,
            params object?[] disposables
            )
            => new(enumerable, cancellationToken, disposables);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="disposables">Disposables</param>
        public static DisposingAsyncEnumerator<T> ToDipsposingAsyncEnumerable<T>(
            this IAsyncEnumerable<T> enumerable,
            params object?[] disposables
            )
            => new(enumerable, disposables);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerator">Enumerator (will be disposed)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="disposables">Disposables</param>
        public static DisposingAsyncEnumerator<T> ToDipsposingAsyncEnumerator<T>(
            this IAsyncEnumerator<T> enumerator,
            in CancellationToken cancellationToken,
            params object?[] disposables
            )
            => new(enumerator, cancellationToken, disposables);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumerator">Enumerator (will be disposed)</param>
        /// <param name="disposables">Disposables</param>
        public static DisposingAsyncEnumerator<T> ToDipsposingAsyncEnumerable<T>(
            this IAsyncEnumerator<T> enumerator,
            params object?[] disposables
            )
            => new(enumerator, disposables);
    }
}
