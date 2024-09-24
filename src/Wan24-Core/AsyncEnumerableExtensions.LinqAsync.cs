using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // LINQ asynchronous
    public static partial class AsyncEnumerableExtensions
    {
        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        public static async IAsyncEnumerable<T> WhereAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            foreach (T item in enumerable)
                if (await predicate(item, cancellationToken).DynamicContext())
                    yield return item;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        public static async IAsyncEnumerable<T> WhereAsync<T>(
            this T[] enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            T item;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                item = enumerable[i];
                if (await predicate(item, cancellationToken).DynamicContext())
                    yield return item;
            }
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        public static async IAsyncEnumerable<T> WhereAsync<T>(
            this ImmutableArray<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            T item;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                item = enumerable[i];
                if (await predicate(item, cancellationToken).DynamicContext())
                    yield return item;
            }
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        public static async IAsyncEnumerable<T> WhereAsync<T>(
            this FrozenSet<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable.Items[i];
                if (await predicate(item, cancellationToken).DynamicContext())
                    yield return item;
            }
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        public static async IAsyncEnumerable<T> WhereAsync<T>(
            this IList<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable[i];
                if (await predicate(item, cancellationToken).DynamicContext())
                    yield return item;
            }
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        public static async IAsyncEnumerable<T> WhereAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (await predicate(item, cancellationToken).DynamicContext())
                    yield return item;
        }

        /// <summary>
        /// Skip
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="skip">Number of items to skip</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        public static async IAsyncEnumerable<T> SkipAsync<T>(this IAsyncEnumerable<T> enumerable, int skip, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (--skip < 0)
                    yield return item;
        }

        /// <summary>
        /// Take
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="take">Number of items to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        public static async IAsyncEnumerable<T> TakeAsync<T>(this IAsyncEnumerable<T> enumerable, int take, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                if (--take < 0) break;
                yield return item;
            }
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="selector">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result items</returns>
        public static async IAsyncEnumerable<tResult> SelectAsync<tItem, tResult>(
            this IEnumerable<tItem> enumerable,
            Func<tItem, CancellationToken, Task<tResult>> selector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            foreach (tItem item in enumerable)
                yield return await selector(item, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="selector">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result items</returns>
        public static async IAsyncEnumerable<tResult> SelectAsync<tItem, tResult>(
            this tItem[] enumerable,
            Func<tItem, CancellationToken, Task<tResult>> selector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = enumerable.Length; i < len; i++)
                yield return await selector(enumerable[i], cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="selector">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result items</returns>
        public static async IAsyncEnumerable<tResult> SelectAsync<tItem, tResult>(
            this ImmutableArray<tItem> enumerable,
            Func<tItem, CancellationToken, Task<tResult>> selector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = enumerable.Length; i < len; i++)
                yield return await selector(enumerable[i], cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="selector">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result items</returns>
        public static async IAsyncEnumerable<tResult> SelectAsync<tItem, tResult>(
            this FrozenSet<tItem> enumerable,
            Func<tItem, CancellationToken, Task<tResult>> selector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = enumerable.Count; i < len; i++)
                yield return await selector(enumerable.Items[i], cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="selector">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result items</returns>
        public static async IAsyncEnumerable<tResult> SelectAsync<tItem, tResult>(
            this IList<tItem> enumerable,
            Func<tItem, CancellationToken, Task<tResult>> selector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            for (int i = 0, len = enumerable.Count; i < len; i++)
                yield return await selector(enumerable[i], cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="selector">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result items</returns>
        public static async IAsyncEnumerable<tResult> SelectAsync<tItem, tResult>(
            this IAsyncEnumerable<tItem> enumerable,
            Func<tItem, CancellationToken, Task<tResult>> selector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            await foreach (tItem item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                yield return await selector(item, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        public static async IAsyncEnumerable<T> DistinctAsync<T>(this IAsyncEnumerable<T> enumerable, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            HashSet<int> seen = [];
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (seen.Add(item is null ? 0 : HashCode.Combine(item.GetType(), item)))
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
            HashSet<int> seen = [];
            tKey key;
            await foreach (tItem item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                if (item is null)
                {
                    if (seen.Add(0))
                        yield return item;
                    continue;
                }
                key = keySelector(item);
                if (seen.Add(key is null ? 0 : HashCode.Combine(key.GetType(), key)))
                    yield return item;
            }
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
            HashSet<int> seen = [];
            tKey key;
            await foreach (tItem item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                if (item is null)
                {
                    if (seen.Add(0))
                        yield return item;
                    continue;
                }
                key = await keySelector(item, cancellationToken).DynamicContext();
                if (seen.Add(key is null ? 0 : HashCode.Combine(key.GetType(), key)))
                    yield return item;
            }
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item</returns>
        public static async Task<T> FirstAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken = default)
        {
            IAsyncEnumerator<T> enumerator = enumerable.GetAsyncEnumerator(cancellationToken);
            await using (enumerator.DynamicContext())
            {
                if (!await enumerator.MoveNextAsync().DynamicContext())
                    throw new InvalidOperationException("Sequence contains no elements");
                return enumerator.Current;
            }
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item</returns>
        public static async Task<T> FirstAsync<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> predicate, CancellationToken cancellationToken = default)
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (predicate(item))
                    return item;
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item</returns>
        public static async Task<T> FirstAsync<T>(this IAsyncEnumerable<T> enumerable, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (await predicate(item, cancellationToken).DynamicContext())
                    return item;
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <summary>
        /// First or the default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="defaultResult">Default result</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item or the <c>defaultResult</c></returns>
        public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable, T? defaultResult = default, CancellationToken cancellationToken = default)
        {
            IAsyncEnumerator<T> enumerator = enumerable.GetAsyncEnumerator(cancellationToken);
            await using (enumerator.DynamicContext())
            {
                if (!await enumerator.MoveNextAsync().DynamicContext()) return defaultResult;
                return enumerator.Current;
            }
        }

        /// <summary>
        /// First or the default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultResult">Default result</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item or the <c>defaultResult</c></returns>
        public static async Task<T?> FirstOrDefaultAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, bool> predicate,
            T? defaultResult = default,
            CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (predicate(item))
                    return item;
            return defaultResult;
        }

        /// <summary>
        /// First or the default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultResult">Default result</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item or the <c>defaultResult</c></returns>
        public static async Task<T?> FirstOrDefaultAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            T? defaultResult = default,
            CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (await predicate(item, cancellationToken).DynamicContext())
                    return item;
            return defaultResult;
        }

        /// <summary>
        /// All
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If the predicate returned <see langword="true"/> for all items</returns>
        public static async Task<bool> AllAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            CancellationToken cancellationToken = default
            )
        {
            foreach (T item in enumerable)
                if (!await predicate(item, cancellationToken).DynamicContext())
                    return false;
            return true;
        }

        /// <summary>
        /// All
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If the predicate returned <see langword="true"/> for all items</returns>
        public static async Task<bool> AllAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (!await predicate(item, cancellationToken).DynamicContext())
                    return false;
            return true;
        }

        /// <summary>
        /// Any
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If the predicate returned <see langword="true"/> for any item</returns>
        public static async Task<bool> AnyAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            CancellationToken cancellationToken = default
            )
        {
            foreach (T item in enumerable)
                if (await predicate(item, cancellationToken).DynamicContext())
                    return true;
            return false;
        }

        /// <summary>
        /// Any
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If the predicate returned <see langword="true"/> for any item</returns>
        public static async Task<bool> AnyAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (await predicate(item, cancellationToken).DynamicContext())
                    return true;
            return false;
        }
    }
}
