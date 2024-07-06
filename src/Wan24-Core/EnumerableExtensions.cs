﻿using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Enumerable extensions
    /// </summary>
    public static class EnumerableExtensions
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
    }
}
