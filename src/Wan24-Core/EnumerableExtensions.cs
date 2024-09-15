using System.Collections;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Enumerable extensions
    /// </summary>
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// Combine enumerables
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerables">Enumerables</param>
        /// <returns>Combined enumerable</returns>
        public static IEnumerable<T> Combine<T>(this IEnumerable<IEnumerable<T>> enumerables)
        {
            foreach (IEnumerable<T> e in enumerables)
                foreach (T item in e)
                    yield return item;
        }

        /// <summary>
        /// Chunk an enumerable
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="chunkSize">Chunk size</param>
        /// <returns>Chunks</returns>
        public static IEnumerable<T[]> ChunkEnum<T>(this IEnumerable<T> enumerable, int chunkSize)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1);
            List<T> res = new(chunkSize);
            foreach (T item in enumerable)
            {
                res.Add(item);
                if (res.Count < chunkSize) continue;
                yield return res.ToArray();
                res.Clear();
            }
            if (res.Count > 0) yield return res.ToArray();
        }

        /// <summary>
        /// Determine if all values are contained
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="values">Required values</param>
        /// <returns>All contained?</returns>
        public static bool ContainsAll<T>(this IEnumerable<T> enumerable, params T[] values)
        {
            int valuesLen = values.Length;
            if (valuesLen == 0) return true;
            T[] arr = enumerable.ToArray();
            int len = arr.Length;
            if (len < valuesLen) return false;
            bool[] found = new bool[valuesLen];
            for (int i = 0, index; i < len; i++)
            {
                index = values.IndexOf(arr[i]);
                if (index != -1) found[index] = true;
            }
            for (int i = 0; i < valuesLen; i++) if (!found[i]) return false;
            return true;
        }

        /// <summary>
        /// Determine if any of the values are contained
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="values">Values</param>
        /// <returns>Any contained?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool ContainsAny<T>(this IEnumerable<T> enumerable, params T[] values)
        {
            if (values.Length == 0) return false;
            foreach (T value in enumerable) if (values.IndexOf(value) != -1) return true;
            return false;
        }

        /// <summary>
        /// Get the element index
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="value">Element</param>
        /// <returns>Element index or <c>-1</c>, if not enumerated</returns>
        public static int ElementIndex<T>(this IEnumerable<T> enumerable, in T value)
        {
            int res = 0;
            bool found = false;
            foreach (T item in enumerable)
            {
                if (item?.Equals(value) ?? false)
                {
                    found = true;
                    break;
                }
                res++;
            }
            return found ? res : -1;
        }

        /// <summary>
        /// Filter non-<see langword="null"/> items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Non-<see langword="null"/> items</returns>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) => enumerable.Where(i => i is not null).Cast<T>();

        /// <summary>
        /// Filter non-<see langword="null"/> items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Non-<see langword="null"/> items</returns>
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
        /// <returns><see cref="XY"/> values</returns>
        public static XY[] CollectXy<T>(this IEnumerable<T> enumerable, in Func<T, XY?> collector)
        {
            Dictionary<double, double> values = [];
            XY? xy;
            foreach (T item in enumerable)
            {
                xy = collector(item);
                if (xy.HasValue && !values.TryAdd(xy.Value.X, xy.Value.Y)) values[xy.Value.X] += xy.Value.Y;
            }
            return [.. values.Select(kvp => new XY(kvp.Key, kvp.Value))];
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
        /// <returns><see cref="XYZ"/> values (key is the Z value)</returns>
        public static Dictionary<double, XYZ[]> CollectXyz<T>(this IEnumerable<T> enumerable, in Func<T, XYZ?> collector)
        {
            Dictionary<double, Dictionary<double, double>> values = [];
            Dictionary<double, double>? values2;
            XYZ? xyz;
            foreach (T item in enumerable)
            {
                xyz = collector(item);
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
        /// <returns><see cref="XYInt"/> values</returns>
        public static XYInt[] CollectXyInt<T>(this IEnumerable<T> enumerable, in Func<T, XYInt?> collector)
        {
            Dictionary<int, int> values = [];
            XYInt? xy;
            foreach (T item in enumerable)
            {
                xy = collector(item);
                if (xy.HasValue && !values.TryAdd(xy.Value.X, xy.Value.Y)) values[xy.Value.X] += xy.Value.Y;
            }
            return [.. values.Select(kvp => new XYInt(kvp.Key, kvp.Value))];
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
        /// <returns><see cref="XYZInt"/> values (key is the Z value)</returns>
        public static Dictionary<double, XYZInt[]> CollectXyzInt<T>(this IEnumerable<T> enumerable, in Func<T, XYZInt?> collector)
        {
            Dictionary<int, Dictionary<int, int>> values = [];
            Dictionary<int, int>? values2;
            XYZInt? xyz;
            foreach (T item in enumerable)
            {
                xyz = collector(item);
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
        /// <returns>Items</returns>
        public static IEnumerable<tItem> WhereIsAssignableTo<tItem>(this IEnumerable<tItem> enumerable, Type type, bool extended = false)
        {
            foreach (tItem item in enumerable)
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
        /// <returns>Items</returns>
        public static IEnumerable<tItem> WhereIsAssignableTo<tItem, tType>(this IEnumerable<tItem> enumerable, bool extended = false)
        {
            Type type = typeof(tType);
            foreach (tItem item in enumerable)
                if (item is not null && (extended ? type.IsAssignableFromExt(item.GetType()) : type.IsAssignableFrom(item.GetType())))
                    yield return item;
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
        /// Get the common base type
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="includeInterfaces">If interfaces are included</param>
        /// <param name="includeMainType">If to include the main type</param>
        /// <returns>Common base type</returns>
        public static Type? CommonBaseType(this IEnumerable enumerable, in bool includeInterfaces = false, in bool includeMainType = true)
        {
            HashSet<Type> baseTypes = [];
            Type type;
            Type[] types;
            // Find all base types and interfaces
            foreach (object? item in enumerable)
            {
                if (item is null) continue;
                type = item.GetType();
                types = [.. type.GetBaseTypes()];
                if (includeMainType) baseTypes.Add(type);
                baseTypes.AddRange(types);
                if (!includeInterfaces) continue;
                baseTypes.AddRange(type.GetInterfaces());
                foreach (Type t in types) baseTypes.AddRange(t.GetInterfaces());
            }
            // Filter out incompatible base types and interfaces
            foreach(object? item in enumerable)
            {
                if (item is null) continue;
                type = item.GetType();
                foreach (Type incompatible in baseTypes.Where(t => !t.IsAssignableFrom(type)).ToArray())
                    baseTypes.Remove(incompatible);
            }
            return baseTypes.FirstOrDefault();
        }

        /// <summary>
        /// Discard all items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="dispose">Dispose disposables?</param>
        /// <returns>Number of discarded items</returns>
        public static int DiscardAll<T>(this IEnumerable<T> enumerable, bool dispose = true)
        {
            int res = 0;
            foreach (T item in enumerable)
            {
                res++;
                if (dispose)
                    if (item is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    else if (item is IAsyncDisposable asyncDisposable)
                    {
                        asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                    }
            }
            return res;
        }

        /// <summary>
        /// Discard all items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="dispose">Dispose disposables?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of discarded items</returns>
        public static async Task<int> DiscardAllAsync<T>(this IAsyncEnumerable<T> enumerable, bool dispose = true, CancellationToken cancellationToken = default)
        {
            int res = 0;
            await foreach (T item in enumerable.DynamicContext().WithCancellation(cancellationToken))
            {
                res++;
                if (dispose)
                    if (item is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync().DynamicContext();
                    }
                    else if (item is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
            }
            return res;
        }

        /// <summary>
        /// Create a builder
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Builder</returns>
        public static EnumerableBuilder<T> CreateBuilder<T>(this IEnumerable<T> enumerable) => new(enumerable);

        /// <summary>
        /// Create a builder
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="queryable">Queryable</param>
        /// <returns>Builder</returns>
        public static QueryableBuilder<T> CreateBuilder<T>(this IQueryable<T> queryable) => new(queryable);
    }
}
