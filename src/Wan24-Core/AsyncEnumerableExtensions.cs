using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Asynchronous enumerable extensions
    /// </summary>
    public static partial class AsyncEnumerableExtensions
    {
        /// <summary>
        /// Get as array
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Array</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken = default)
        {
            List<T> items = [];
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken)) items.Add(item);
            return [.. items];
        }

        /// <summary>
        /// Get as list
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken = default)
        {
            List<T> res = [];
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken)) res.Add(item);
            return res;
        }

        /// <summary>
        /// Get as array
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Array</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<T[]> ToArrayAsync<T>(this ConfiguredCancelableAsyncEnumerable<T> enumerable)
        {
            List<T> items = [];
            await foreach (T item in enumerable) items.Add(item);
            return [.. items];
        }

        /// <summary>
        /// Get as list
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>List</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<List<T>> ToListAsync<T>(this ConfiguredCancelableAsyncEnumerable<T> enumerable)
        {
            List<T> res = [];
            await foreach (T item in enumerable) res.Add(item);
            return res;
        }

        /// <summary>
        /// Return to the awaiting context
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumeration">Enumeration</param>
        /// <returns>Enumeration</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ConfiguredCancelableAsyncEnumerable<T> FixedContext<T>(this IAsyncEnumerable<T> enumeration)
            => enumeration.ConfigureAwait(continueOnCapturedContext: true);

        /// <summary>
        /// Return in any thread context
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumeration">Enumeration</param>
        /// <returns>Enumeration</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ConfiguredCancelableAsyncEnumerable<T> DynamicContext<T>(this IAsyncEnumerable<T> enumeration)
            => enumeration.ConfigureAwait(continueOnCapturedContext: false);

        /// <summary>
        /// Combine enumerables
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerables">Enumerables</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Combined enumerable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async IAsyncEnumerable<T[]> ChunkEnumAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            int chunkSize,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1);
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static DisposingAsyncEnumerator<T> ToDisposingAsyncEnumerator<T>(
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static DisposingAsyncEnumerator<T> ToDisposingAsyncEnumerator<T>(
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static DisposingAsyncEnumerator<T> ToDisposingAsyncEnumerator<T>(
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static DisposingAsyncEnumerator<T> ToDisposingAsyncEnumerator<T>(
            this IAsyncEnumerator<T> enumerator,
            params object?[] disposables
            )
            => new(enumerator, disposables);

        /// <summary>
        /// Filter non-<see langword="null"/> items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Non-<see langword="null"/> items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async IAsyncEnumerable<T> WhereNotNullAsync<T>(
            this IAsyncEnumerable<T?> enumerable,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            await foreach (T? item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (item is not null)
                    yield return item;
        }

        /// <summary>
        /// Collect <see cref="XY"/> values from objects (Y values are summarized and grouped by X)
        /// </summary>
        /// <typeparam name="T">Source object type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="collector">Collector</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="XY"/> values</returns>
        public static async Task<XY[]> CollectXyAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<XY?>> collector,
            CancellationToken cancellationToken = default
            )
        {
            Dictionary<double, double> values = [];
            XY? xy;
            foreach (T item in enumerable)
            {
                xy = await collector(item, cancellationToken).DynamicContext();
                if (xy.HasValue && !values.TryAdd(xy.Value.X, xy.Value.Y)) values[xy.Value.X] += xy.Value.Y;
            }
            return [.. values.Select(kvp => new XY(kvp.Key, kvp.Value))];
        }

        /// <summary>
        /// Collect <see cref="XYZ"/> values from objects (Y values are summarized and grouped by Z (result index) and X)
        /// </summary>
        /// <typeparam name="T">Source object type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="collector">Collector</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="XYZ"/> values (key is the Z value)</returns>
        public static async Task<Dictionary<double, XYZ[]>> CollectXyzAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<XYZ?>> collector,
            CancellationToken cancellationToken = default
            )
        {
            Dictionary<double, Dictionary<double, double>> values = [];
            Dictionary<double, double>? values2;
            XYZ? xyz;
            foreach (T item in enumerable)
            {
                xyz = await collector(item, cancellationToken).DynamicContext();
                if (!xyz.HasValue) continue;
                if (!values.TryGetValue(xyz.Value.Z, out values2))
                {
                    values2 = values[xyz.Value.Z] = new()
                    {
                        { xyz.Value.X, xyz.Value.Y }
                    };
                }
                else if (!values2.TryAdd(xyz.Value.X, xyz.Value.Y))
                {
                    values2[xyz.Value.X] += xyz.Value.Y;
                }
            }
            return new(values.Select(kvp => new KeyValuePair<double, XYZ[]>(kvp.Key, [.. kvp.Value.Select(kvp2 => new XYZ(kvp.Key, kvp2.Key, kvp2.Value))])));
        }

        /// <summary>
        /// Collect <see cref="XYInt"/> values from objects (Y values are summarized and grouped by X)
        /// </summary>
        /// <typeparam name="T">Source object type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="collector">Collector</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="XYInt"/> values</returns>
        public static async Task<XYInt[]> CollectXyIntAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<XYInt?>> collector,
            CancellationToken cancellationToken = default
            )
        {
            Dictionary<int, int> values = [];
            XYInt? xy;
            foreach (T item in enumerable)
            {
                xy = await collector(item, cancellationToken).DynamicContext();
                if (xy.HasValue && !values.TryAdd(xy.Value.X, xy.Value.Y)) values[xy.Value.X] += xy.Value.Y;
            }
            return [.. values.Select(kvp => new XYInt(kvp.Key, kvp.Value))];
        }

        /// <summary>
        /// Collect <see cref="XYZInt"/> values from objects (Y values are summarized and grouped by Z (result index) and X)
        /// </summary>
        /// <typeparam name="T">Source object type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="collector">Collector</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="XYZInt"/> values (key is the Z value)</returns>
        public static async Task<Dictionary<double, XYZInt[]>> CollectXyzIntAsync<T>(
            this IEnumerable<T> enumerable,
            Func<T, CancellationToken, Task<XYZInt?>> collector,
            CancellationToken cancellationToken = default
            )
        {
            Dictionary<int, Dictionary<int, int>> values = [];
            Dictionary<int, int>? values2;
            XYZInt? xyz;
            foreach (T item in enumerable)
            {
                xyz = await collector(item, cancellationToken).DynamicContext();
                if (!xyz.HasValue) continue;
                if (!values.TryGetValue(xyz.Value.Z, out values2))
                {
                    values2 = values[xyz.Value.Z] = new()
                    {
                        { xyz.Value.X, xyz.Value.Y }
                    };
                }
                else if (!values2.TryAdd(xyz.Value.X, xyz.Value.Y))
                {
                    values2[xyz.Value.X] += xyz.Value.Y;
                }
            }
            return new(values.Select(kvp => new KeyValuePair<double, XYZInt[]>(kvp.Key, [.. kvp.Value.Select(kvp2 => new XYZInt(kvp.Key, kvp2.Key, kvp2.Value))])));
        }

        /// <summary>
        /// Filter all objects which are assignable to a type
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="type">Type</param>
        /// <param name="extended">If to match the generic type definition, too</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async IAsyncEnumerable<tItem> WhereIsAssignableToAsync<tItem>(
            this IAsyncEnumerable<tItem> enumerable,
            Type type,
            bool extended = false,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            await foreach (tItem item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (item is not null && (extended ? type.IsAssignableFromExt(item.GetType()) : type.IsAssignableFrom(item.GetType())))
                    yield return item;
        }

        /// <summary>
        /// Filter all objects which are assignable to a type
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tType">Type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="extended">If to match the generic type definition, too</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async IAsyncEnumerable<tItem> WhereIsAssignableToAsync<tItem, tType>(
            this IAsyncEnumerable<tItem> enumerable,
            bool extended = false,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            Type type = typeof(tType);
            await foreach (tItem item in enumerable.DynamicContext().WithCancellation(cancellationToken))
                if (item is not null && (extended ? type.IsAssignableFromExt(item.GetType()) : type.IsAssignableFrom(item.GetType())))
                    yield return item;
        }

        /// <summary>
        /// Discard all items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="dispose">Dispose disposables?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of discarded items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<int> DiscardAllAsync<T>(this IAsyncEnumerable<T> enumerable, bool dispose = true, CancellationToken cancellationToken = default)
        {
            int res = 0;
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                res++;
                if (dispose && item is not null) await item.TryDisposeAsync().DynamicContext();
            }
            return res;
        }
    }
}
