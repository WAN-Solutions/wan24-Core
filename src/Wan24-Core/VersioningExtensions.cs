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
    }
}
