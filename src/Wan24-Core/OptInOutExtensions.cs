namespace wan24.Core
{
    /// <summary>
    /// Extension methods for <see cref="IOptInOut"/> and <see cref="OptInOut"/>
    /// </summary>
    public static class OptInOutExtensions
    {
        /// <summary>
        /// Determine if the opt direction is opt in
        /// </summary>
        /// <param name="optInOut">Opt direction</param>
        /// <returns>If opt in</returns>
        public static bool IsOptIn(this OptInOut optInOut) => optInOut == OptInOut.OptIn;

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        public static IEnumerable<T> FilterOptIn<T>(this IEnumerable<T> enumerable) where T : IOptInOut => enumerable.Where(i => i.Opt.IsOptIn());

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        public static IEnumerable<T> FilterOptInOnly<T>(this IEnumerable<T> enumerable) => enumerable.Where(i => i is IOptInOut oio && oio.Opt.IsOptIn());

        /// <summary>
        /// Filter all opt out items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items (or items with no opt direction)</returns>
        public static IEnumerable<T> FilterOptOut<T>(this IEnumerable<T> enumerable) => enumerable.Where(i => i is not IOptInOut oio || oio.Opt.IsOptIn());
    }
}
