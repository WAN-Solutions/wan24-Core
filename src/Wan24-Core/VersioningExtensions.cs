using System.Collections.Frozen;
using System.Collections.Immutable;

namespace wan24.Core
{
    /// <summary>
    /// Extension methods for <see cref="IVersioning"/> and <see cref="IVersioningExt"/>
    /// </summary>
    public static class VersioningExtensions
    {
        /// <summary>
        /// Filter by version (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="version">Version</param>
        /// <param name="requireVersionFilter">Require the item to implement a version filter (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)?</param>
        /// <returns>Filtered items</returns>
        public static IEnumerable<T> FilterByVersion<T>(this IEnumerable<T> enumerable, int version, bool requireVersionFilter = false)
            => enumerable.Where(
                i => (i is not IVersioning v || (v.FromVersion <= version && v.ToVersion >= version)) && 
                    (i is not IVersioningExt ve || ve.IsIncluded(version)) && 
                    (!requireVersionFilter || i is IVersioningExt || i is IVersioning)
                );

        /// <summary>
        /// Filter by version (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="version">Version</param>
        /// <param name="requireVersionFilter">Require the item to implement a version filter (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)?</param>
        /// <returns>Filtered items</returns>
        public static IEnumerable<T> FilterByVersion<T>(this Memory<T> enumerable, int version, bool requireVersionFilter = false)
            => FilterByVersion((ReadOnlyMemory<T>)enumerable, version, requireVersionFilter);

        /// <summary>
        /// Filter by version (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="version">Version</param>
        /// <param name="requireVersionFilter">Require the item to implement a version filter (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)?</param>
        /// <returns>Filtered items</returns>
        public static IEnumerable<T> FilterByVersion<T>(this ReadOnlyMemory<T> enumerable, int version, bool requireVersionFilter = false)
        {
            T item;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                item = enumerable.Span[i];
                if (
                    (item is not IVersioning v || (v.FromVersion <= version && v.ToVersion >= version)) &&
                    (item is not IVersioningExt ve || ve.IsIncluded(version)) &&
                    (!requireVersionFilter || item is IVersioningExt || item is IVersioning)
                    )
                    yield return item;
            }
        }

        /// <summary>
        /// Filter by version (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="version">Version</param>
        /// <param name="requireVersionFilter">Require the item to implement a version filter (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)?</param>
        /// <returns>Filtered items</returns>
        public static IEnumerable<T> FilterByVersion<T>(this T[] enumerable, int version, bool requireVersionFilter = false)
            => FilterByVersion((ReadOnlyMemory<T>)enumerable, version, requireVersionFilter);

        /// <summary>
        /// Filter by version (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="version">Version</param>
        /// <param name="requireVersionFilter">Require the item to implement a version filter (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)?</param>
        /// <returns>Filtered items</returns>
        public static IEnumerable<T> FilterByVersion<T>(this IList<T> enumerable, int version, bool requireVersionFilter = false)
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable[i];
                if (
                    (item is not IVersioning v || (v.FromVersion <= version && v.ToVersion >= version)) &&
                    (item is not IVersioningExt ve || ve.IsIncluded(version)) &&
                    (!requireVersionFilter || item is IVersioningExt || item is IVersioning)
                    )
                    yield return item;
            }
        }

        /// <summary>
        /// Filter by version (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="version">Version</param>
        /// <param name="requireVersionFilter">Require the item to implement a version filter (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)?</param>
        /// <returns>Filtered items</returns>
        public static IEnumerable<T> FilterByVersion<T>(this List<T> enumerable, int version, bool requireVersionFilter = false)
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable[i];
                if (
                    (item is not IVersioning v || (v.FromVersion <= version && v.ToVersion >= version)) &&
                    (item is not IVersioningExt ve || ve.IsIncluded(version)) &&
                    (!requireVersionFilter || item is IVersioningExt || item is IVersioning)
                    )
                    yield return item;
            }
        }

        /// <summary>
        /// Filter by version (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="version">Version</param>
        /// <param name="requireVersionFilter">Require the item to implement a version filter (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)?</param>
        /// <returns>Filtered items</returns>
        public static IEnumerable<T> FilterByVersion<T>(this ImmutableArray<T> enumerable, int version, bool requireVersionFilter = false)
        {
            T item;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                item = enumerable[i];
                if (
                    (item is not IVersioning v || (v.FromVersion <= version && v.ToVersion >= version)) &&
                    (item is not IVersioningExt ve || ve.IsIncluded(version)) &&
                    (!requireVersionFilter || item is IVersioningExt || item is IVersioning)
                    )
                    yield return item;
            }
        }

        /// <summary>
        /// Filter by version (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="version">Version</param>
        /// <param name="requireVersionFilter">Require the item to implement a version filter (using <see cref="IVersioning"/> and <see cref="IVersioningExt"/>)?</param>
        /// <returns>Filtered items</returns>
        public static IEnumerable<T> FilterByVersion<T>(this FrozenSet<T> enumerable, int version, bool requireVersionFilter = false)
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable.Items[i];
                if (
                    (item is not IVersioning v || (v.FromVersion <= version && v.ToVersion >= version)) &&
                    (item is not IVersioningExt ve || ve.IsIncluded(version)) &&
                    (!requireVersionFilter || item is IVersioningExt || item is IVersioning)
                    )
                    yield return item;
            }
        }
    }
}
