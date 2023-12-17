using System.Runtime;

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
            foreach(T item in enumerable)
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
    }
}
