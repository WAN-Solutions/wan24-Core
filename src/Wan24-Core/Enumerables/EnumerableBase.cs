namespace wan24.Core.Enumerables
{
    /// <summary>
    /// Base class for an enumerable
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public abstract class EnumerableBase<T>()
    {
        /// <summary>
        /// First or default
        /// </summary>
        /// <returns>Item</returns>
        public virtual T? FirstOrDefault() => FirstOrDefault(default(T)!);

        /// <summary>
        /// First or default
        /// </summary>
        /// <param name="defaultValue">Default</param>
        /// <returns>Item</returns>
        public abstract T FirstOrDefault(T defaultValue);

        /// <summary>
        /// First or default
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Item</returns>
        public virtual T? FirstOrDefault(Func<T, bool> predicate) => FirstOrDefault(predicate, default!);

        /// <summary>
        /// First or default
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultValue">Default</param>
        /// <returns>Item</returns>
        public abstract T FirstOrDefault(Func<T, bool> predicate, T defaultValue);

        /// <summary>
        /// First or default
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Item</returns>
        public virtual async Task<T?> FirstOrDefaultAsync(Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
            => await FirstOrDefaultAsync(predicate, default!, cancellationToken).DynamicContext();

        /// <summary>
        /// First or default
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <param name="defaultValue">Default</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Item</returns>
        public abstract Task<T> FirstOrDefaultAsync(Func<T, CancellationToken, Task<bool>> predicate, T defaultValue, CancellationToken cancellationToken = default);
    }
}
