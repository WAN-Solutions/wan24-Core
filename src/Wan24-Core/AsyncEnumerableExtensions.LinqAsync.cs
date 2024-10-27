using System.Collections.Generic;
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
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<T> WhereAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, bool> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (predicate(item))
                    yield return item;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
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
        /// Where
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
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
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="selector">Selector</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<tResult> SelectAsync<tItem, tResult>(
            this IAsyncEnumerable<tItem> enumerable,
            Func<tItem, tResult> selector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            await foreach (tItem item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                yield return selector(item);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="selector">Selector</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
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
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="selector">Selector</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
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
        /// Count
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of items</returns>
        public static async Task<int> CountAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            CancellationToken cancellationToken = default
            )
        {
            int res = 0;
            IAsyncEnumerator<T> enumerator = enumerable.GetAsyncEnumerator(cancellationToken);
            await using (enumerator.DynamicContext())
                while (await enumerator.MoveNextAsync().DynamicContext())
                    res++;
            return res;
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of items</returns>
        public static async Task<int> CountAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, bool> predicate,
            CancellationToken cancellationToken = default
            )
        {
            int res = 0;
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (predicate(item))
                    res++;
            return res;
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of items</returns>
        public static async Task<int> CountAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            CancellationToken cancellationToken = default
            )
        {
            int res = 0;
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (await predicate(item, cancellationToken).DynamicContext())
                    res++;
            return res;
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of items</returns>
        public static async Task<int> CountAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            CancellationToken cancellationToken = default
            )
        {
            int res = 0;
            foreach (T item in enumerable)
                if (await predicate(item, cancellationToken).DynamicContext())
                    res++;
            return res;
        }

        /// <summary>
        /// Contains
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="item">Item</param>
        /// <param name="cancellationToken">Cancellation</param>
        /// <returns>If the item is contained</returns>
        public static async Task<bool> ContainsAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            T item,
            CancellationToken cancellationToken = default
            )
        {
            bool isNull = item is null;
            int hc = item?.GetHashCode() ?? 0;
            await foreach (T current in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if ((isNull && current is null) || (!isNull && current is not null && hc == current.GetHashCode() && current.Equals(item)))
                    return true;
            return false;
        }

        /// <summary>
        /// Contains all
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="items">Items</param>
        /// <returns>If all items are contained</returns>
        public static async Task<bool> ContainsAllAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            CancellationToken cancellationToken,
            params T[] items
            )
        {
            int itemsLen = items.Length;// Number of objects to look for
            if (itemsLen == 0) return false;
            int i = 0;// Index counter
            using RentedArrayStructSimple<int> hashCodes = new(itemsLen, clean: false);// Hash codes of objects to look for
            int[] hashCodesArray = hashCodes.Array;// Hash codes span
            for (; i < itemsLen; hashCodes[i] = items[i]?.GetHashCode() ?? 0, i++) ;
            using RentedArrayStructSimple<bool> seen = new(itemsLen);// Found object indicators
            bool[] seenArray = seen.Array;// Found object indicators span
            bool isItemNull;// If the current item is NULL
            T item;// Current object
            int seenCnt = 0,// Number of seen objects
                hc;// Current object hash code
            await foreach(T current in enumerable.DynamicContext())
                for (i = 0, hc = current?.GetHashCode() ?? 0; i < itemsLen && !cancellationToken.GetIsCancellationRequested(); i++)
                    if (
                        !seenArray[i] && hashCodesArray[i] == hc &&
                        (
                            ((isItemNull = current is null) && items[i] is null) ||
                            (!isItemNull && (item = items[i]) is not null && item.Equals(current))
                        )
                        )
                    {
                        if (++seenCnt >= itemsLen) return true;
                        seenArray[i] = true;
                    }
            return false;
        }

        /// <summary>
        /// Contains any
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="items">Items</param>
        /// <returns>If any item is contained</returns>
        public static async Task<bool> ContainsAnyAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            CancellationToken cancellationToken,
            params T[] items
            )
        {
            int itemsLen = items.Length;// Number of objects to look for
            if (itemsLen == 0) return false;
            int i = 0;// Index counter
            using RentedArrayStructSimple<int> hashCodes = new(itemsLen, clean: false);// Hash codes of objects to look for
            int[] hashCodesArray = hashCodes.Array;// Hash codes span
            for (; i < itemsLen; hashCodes[i] = items[i]?.GetHashCode() ?? 0, i++) ;
            bool isItemNull;// If the current item is NULL
            T obj;// Current object
            int hc;// Current object hash code
            await foreach(T current in enumerable.DynamicContext().WithCancellation(cancellationToken))
                for (i = 0, hc = current?.GetHashCode() ?? 0; i < itemsLen; i++)
                    if (
                        hashCodesArray[i] == hc &&
                        (
                            ((isItemNull = current is null) && items[i] is null) ||
                            (!isItemNull && (obj = items[i]) is not null && obj.Equals(current))
                        )
                        )
                        return true;
            return false;
        }

        /// <summary>
        /// Contains at least
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="count">Count</param>
        /// <param name="cancellationToken">Cancellation</param>
        /// <returns>If contains at least <c>count</c></returns>
        public static async Task<bool> ContainsAtLeast<T>(
            this IAsyncEnumerable<T> enumerable,
            int count,
            CancellationToken cancellationToken = default
            )
        {
            int cnt = 0;
            await foreach (T _ in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (++cnt >= count)
                    return true;
            return false;
        }

        /// <summary>
        /// Contains at most
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="count">Count</param>
        /// <param name="cancellationToken">Cancellation</param>
        /// <returns>If contains at most <c>count</c></returns>
        public static async Task<bool> ContainsAtMost<T>(
            this IAsyncEnumerable<T> enumerable,
            int count,
            CancellationToken cancellationToken = default
            )
        {
            int cnt = 0;
            await foreach (T _ in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (++cnt > count)
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
            Func<T, bool> predicate,
            CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (!predicate(item))
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
        /// Any
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If the enumerable contains any items</returns>
        public static async Task<bool> AnyAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            CancellationToken cancellationToken = default
            )
        {
            IAsyncEnumerator<T> enumerator = enumerable.GetAsyncEnumerator(cancellationToken);
            await using (enumerator.DynamicContext())
                return await enumerator.MoveNextAsync().DynamicContext();
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
            Func<T, bool> predicate,
            CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (predicate(item))
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
        /// Distinct
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<T> DistinctAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            List<int> seenHashCodes = [];
            List<T> seen = [];
            bool useItem,// If to use the current item
                isItemNull;// If the current item is NULL
            T seenItem;// Seen item
            int seenCnt = 0,
                hc,
                i;
            await foreach(T current in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                for (hc = current?.GetHashCode() ?? 0, useItem = true, i = 0; i < seenCnt; i++)
                    if (seenHashCodes[i] == hc && (((isItemNull = current is null) && seen[i] is null) || (!isItemNull && (seenItem = seen[i]) is not null && seenItem.Equals(current))))
                    {
                        useItem = false;
                        break;
                    }
                if (!useItem) continue;
                seenHashCodes.Add(hc);
                seen.Add(current);
                seenCnt++;
                yield return current;
            }
        }

        /// <summary>
        /// Distinct by
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<tItem> DistinctByAsync<tItem, tKey>(
            this IAsyncEnumerable<tItem> enumerable,
            Func<tItem, tKey> keySelector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            List<int> seenHashCodes = [];
            List<tKey> seen = [];
            bool useItem,// If to use the current item
                isKeyNull;// If the current key is NULL
            tKey key,// Current key
                seenKey;// Seen key
            int seenCnt = 0,
                hc,
                i;
            await foreach(tItem current in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                for (key = keySelector(current), hc = key?.GetHashCode() ?? 0, useItem = true, i = 0; i < seenCnt; i++)
                    if (seenHashCodes[i] == hc && (((isKeyNull = key is null) && seen[i] is null) || (!isKeyNull && (seenKey = seen[i]) is not null && seenKey.Equals(key))))
                    {
                        useItem = false;
                        break;
                    }
                if (!useItem) continue;
                seenHashCodes.Add(hc);
                seen.Add(key);
                seenCnt++;
                yield return current;
            }
        }

        /// <summary>
        /// Distinct by
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<tItem> DistinctByAsync<tItem, tKey>(
            this IAsyncEnumerable<tItem> enumerable,
            Func<tItem, CancellationToken, Task<tKey>> keySelector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            List<int> seenHashCodes = [];
            List<tKey> seen = [];
            bool useItem,// If to use the current item
                isKeyNull;// If the current key is NULL
            tKey key,// Current key
                seenKey;// Seen key
            int seenCnt = 0,
                hc,
                i;
            await foreach (tItem current in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                for (key = await keySelector(current, cancellationToken).DynamicContext(), hc = key?.GetHashCode() ?? 0, useItem = true, i = 0; i < seenCnt; i++)
                    if (seenHashCodes[i] == hc && (((isKeyNull = key is null) && seen[i] is null) || (!isKeyNull && (seenKey = seen[i]) is not null && seenKey.Equals(key))))
                    {
                        useItem = false;
                        break;
                    }
                if (!useItem) continue;
                seenHashCodes.Add(hc);
                seen.Add(key);
                seenCnt++;
                yield return current;
            }
        }

        /// <summary>
        /// Distinct by
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<tItem> DistinctByAsync<tItem, tKey>(
            this IEnumerable<tItem> enumerable,
            Func<tItem, CancellationToken, Task<tKey>> keySelector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            List<int> seenHashCodes = [];
            List<tKey> seen = [];
            bool useItem,// If to use the current item
                isKeyNull;// If the current key is NULL
            tKey key,// Current key
                seenKey;// Seen key
            int seenCnt = 0,
                hc,
                i;
            foreach (tItem current in enumerable)
            {
                for (key = await keySelector(current, cancellationToken).DynamicContext(), hc = key?.GetHashCode() ?? 0, useItem = true, i = 0; i < seenCnt; i++)
                    if (seenHashCodes[i] == hc && (((isKeyNull = key is null) && seen[i] is null) || (!isKeyNull && (seenKey = seen[i]) is not null && seenKey.Equals(key))))
                    {
                        useItem = false;
                        break;
                    }
                if (!useItem) continue;
                seenHashCodes.Add(hc);
                seen.Add(key);
                seenCnt++;
                yield return current;
            }
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item</returns>
        public static async Task<T> FirstAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            CancellationToken cancellationToken = default
            )
        {
            IAsyncEnumerator<T> enumerator = enumerable.GetAsyncEnumerator(cancellationToken);
            await using (enumerator.DynamicContext())
                return await enumerator.MoveNextAsync().DynamicContext()
                    ? enumerator.Current
                    : throw new InvalidOperationException("The sequence contains no items");
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item</returns>
        public static async Task<T> FirstAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, bool> predicate,
            CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (predicate(item))
                    return item;
            throw new InvalidOperationException("The sequence contains no items");
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item</returns>
        public static async Task<T> FirstAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (await predicate(item, cancellationToken).DynamicContext())
                    return item;
            throw new InvalidOperationException("The sequence contains no items");
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item</returns>
        public static async Task<T> FirstAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            CancellationToken cancellationToken = default
            )
        {
            foreach (T item in enumerable)
                if (await predicate(item, cancellationToken).DynamicContext())
                    return item;
            throw new InvalidOperationException("The sequence contains no items");
        }

        /// <summary>
        /// First or default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item or the <c>defaultValue</c></returns>
        public static async Task<T?> FirstOrDefaultAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            T? defaultValue = default,
            CancellationToken cancellationToken = default
            )
        {
            IAsyncEnumerator<T> enumerator = enumerable.GetAsyncEnumerator(cancellationToken);
            await using (enumerator.DynamicContext())
                return await enumerator.MoveNextAsync().DynamicContext()
                    ? enumerator.Current
                    : defaultValue;
        }

        /// <summary>
        /// First or default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item or the <c>defaultValue</c></returns>
        public static async Task<T?> FirstOrDefaultAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, bool> predicate,
            T? defaultValue = default,
            CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (predicate(item))
                    return item;
            return defaultValue;
        }

        /// <summary>
        /// First or default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item or the <c>defaultValue</c></returns>
        public static async Task<T?> FirstOrDefaultAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            T? defaultValue = default,
            CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (await predicate(item, cancellationToken).DynamicContext())
                    return item;
            return defaultValue;
        }

        /// <summary>
        /// First or default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First item or the <c>defaultValue</c></returns>
        public static async Task<T?> FirstOrDefaultAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            T? defaultValue = default,
            CancellationToken cancellationToken = default
            )
        {
            foreach (T item in enumerable)
                if (await predicate(item, cancellationToken).DynamicContext())
                    return item;
            return defaultValue;
        }

        /// <summary>
        /// Skip
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="count">Count</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<T> SkipAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            int count,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                if (count > 0)
                {
                    count++;
                    continue;
                }
                yield return item;
            }
        }

        /// <summary>
        /// Skip while
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<T> SkipWhileAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, bool> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            bool skip = true;
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                if (skip)
                {
                    if (predicate(item)) continue;
                    skip = false;
                }
                yield return item;
            }
        }

        /// <summary>
        /// Skip while
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<T> SkipWhileAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            bool skip = true;
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                if (skip)
                {
                    if (await predicate(item, cancellationToken).DynamicContext()) continue;
                    skip = false;
                }
                yield return item;
            }
        }

        /// <summary>
        /// Skip while
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<T> SkipWhileAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            bool skip = true;
            foreach (T item in enumerable)
            {
                if (skip)
                {
                    if (await predicate(item, cancellationToken).DynamicContext()) continue;
                    skip = false;
                }
                yield return item;
            }
        }

        /// <summary>
        /// Take
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="count">Count</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<T> TakeAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            int count,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            if (count < 1) yield break;
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                yield return item;
                if (--count < 1) yield break;
            }
        }

        /// <summary>
        /// Take while
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<T> TakeWhileAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, bool> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                if (!predicate(item)) yield break;
                yield return item;
            }
        }

        /// <summary>
        /// Take while
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<T> TakeWhileAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                if (!await predicate(item, cancellationToken).DynamicContext()) yield break;
                yield return item;
            }
        }

        /// <summary>
        /// Take while
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable</returns>
        public static async IAsyncEnumerable<T> TakeWhileAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<bool>> predicate,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            foreach (T item in enumerable)
            {
                if (!await predicate(item, cancellationToken).DynamicContext()) yield break;
                yield return item;
            }
        }
    }
}
