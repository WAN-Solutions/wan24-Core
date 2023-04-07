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
            if (chunkSize < 1) throw new ArgumentOutOfRangeException(nameof(chunkSize));
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
    }
}
