using System.Collections.Frozen;
using System.Collections.Immutable;

namespace wan24.Core
{
    // LINQ
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Items</returns>
        public static IEnumerable<T> Where<T>(this ImmutableArray<T> arr, Func<T, bool> predicate)
        {
            T item;
            for (int i = 0, len = arr.Length; i < len; i++)
            {
                item = arr[i];
                if (predicate(item))
                    yield return item;
            }
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Items</returns>
        public static IEnumerable<T> Where<T>(this FrozenSet<T> arr, Func<T, bool> predicate)
        {
            T item;
            for (int i = 0, len = arr.Count; i < len; i++)
            {
                item = arr.Items[i];
                if (predicate(item))
                    yield return item;
            }
        }

        /// <summary>
        /// Skip
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="skip">Items to skip</param>
        /// <returns>Items</returns>
        public static IEnumerable<T> Skip<T>(this ImmutableArray<T> arr, int skip)
        {
            int len = arr.Length;
            if (len <= skip) yield break;
            for (int i = 0; i < len; i++)
                if (i >= skip)
                    yield return arr[i];
        }

        /// <summary>
        /// Skip
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="skip">Items to skip</param>
        /// <returns>Items</returns>
        public static IEnumerable<T> Skip<T>(this FrozenSet<T> arr, int skip)
        {
            int len = arr.Count;
            if (len <= skip) yield break;
            for (int i = 0; i < len; i++)
                if (i >= skip)
                    yield return arr.Items[i];
        }

        /// <summary>
        /// Take
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="take">Items to take</param>
        /// <returns>Items</returns>
        public static IEnumerable<T> Take<T>(this ImmutableArray<T> arr, int take)
        {
            int len = arr.Length;
            return len <= take 
                ? arr.Enumerate() 
                : Skip(arr, len - take);
        }

        /// <summary>
        /// Take
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="take">Items to take</param>
        /// <returns>Items</returns>
        public static IEnumerable<T> Take<T>(this FrozenSet<T> arr, int take)
        {
            int len = arr.Count;
            return len <= take 
                ? arr.Enumerate() 
                : Skip(arr, len - take);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="selector">Selector</param>
        /// <returns>Result items</returns>
        public static IEnumerable<tResult> Select<tItem, tResult>(this ImmutableArray<tItem> arr, Func<tItem, tResult> selector)
        {
            for (int i = 0, len = arr.Length; i < len; i++)
                yield return selector(arr[i]);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="selector">Selector</param>
        /// <returns>Result items</returns>
        public static IEnumerable<tResult> Select<tItem, tResult>(this FrozenSet<tItem> arr, Func<tItem, tResult> selector)
        {
            for (int i = 0, len = arr.Count; i < len; i++)
                yield return selector(arr.Items[i]);
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>Items</returns>
        public static IEnumerable<T> Distinct<T>(this ImmutableArray<T> arr)
        {
            HashSet<int> seen = [];
            T item;
            for (int i = 0, len = arr.Length; i < len; i++)
            {
                item = arr[i];
                if (seen.Add(item is null ? 0 : HashCode.Combine(item.GetType(), item)))
                    yield return item;
            }
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="keySelector">Key selector</param>
        /// <returns>Items</returns>
        public static IEnumerable<tItem> DistinctBy<tItem, tKey>(this ImmutableArray<tItem> arr, Func<tItem, tKey> keySelector)
        {
            HashSet<int> seen = [];
            tItem item;
            tKey key;
            for (int i = 0, len = arr.Length; i < len; i++)
            {
                item = arr[i];
                if(item is null)
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
        /// <param name="arr">Array</param>
        /// <param name="keySelector">Key selector</param>
        /// <returns>Items</returns>
        public static IEnumerable<tItem> DistinctBy<tItem, tKey>(this FrozenSet<tItem> arr, Func<tItem, tKey> keySelector)
        {
            HashSet<int> seen = [];
            tItem item;
            tKey key;
            for (int i = 0, len = arr.Count; i < len; i++)
            {
                item = arr.Items[i];
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
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>First item</returns>
        public static T First<T>(this ImmutableArray<T> arr)
        {
            if (arr.Length < 1) throw new InvalidOperationException("Sequence contains no elements");
            return arr[0];
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>First item</returns>
        public static T First<T>(this FrozenSet<T> arr)
        {
            if (arr.Count < 1) throw new InvalidOperationException("Sequence contains no elements");
            return arr.Items[0];
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>First item</returns>
        public static T First<T>(this ImmutableArray<T> arr, Func<T, bool> predicate)
        {
            T item;
            for (int i = 0, len = arr.Length; i < len; i++)
            {
                item = arr[i];
                if (predicate(item))
                    return item;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <summary>
        /// First
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>First item</returns>
        public static T First<T>(this FrozenSet<T> arr, Func<T, bool> predicate)
        {
            T item;
            for (int i = 0, len = arr.Count; i < len; i++)
            {
                item = arr.Items[i];
                if (predicate(item))
                    return item;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }

        /// <summary>
        /// First or the default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="defaultResult">Default result</param>
        /// <returns>First item or the <c>defaultResult</c></returns>
        public static T? FirstOrDefault<T>(this ImmutableArray<T> arr, T? defaultResult = default)
            => arr.Length < 1 ? defaultResult : arr[0];

        /// <summary>
        /// First or the default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="defaultResult">Default result</param>
        /// <returns>First item or the <c>defaultResult</c></returns>
        public static T? FirstOrDefault<T>(this FrozenSet<T> arr, T? defaultResult = default)
            => arr.Count < 1 ? defaultResult : arr.Items[0];

        /// <summary>
        /// First or the default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultResult">Default result</param>
        /// <returns>First item or the <c>defaultResult</c></returns>
        public static T? FirstOrDefault<T>(this ImmutableArray<T> arr, Func<T, bool> predicate, T? defaultResult = default)
        {
            T item;
            for (int i = 0, len = arr.Length; i < len; i++)
            {
                item = arr[i];
                if (predicate(item))
                    return item;
            }
            return defaultResult;
        }

        /// <summary>
        /// First or the default
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultResult">Default result</param>
        /// <returns>First item or the <c>defaultResult</c></returns>
        public static T? FirstOrDefault<T>(this FrozenSet<T> arr, Func<T, bool> predicate, T? defaultResult = default)
        {
            T item;
            for (int i = 0, len = arr.Count; i < len; i++)
            {
                item = arr.Items[i];
                if (predicate(item))
                    return item;
            }
            return defaultResult;
        }
    }
}
