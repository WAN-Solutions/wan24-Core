namespace wan24.Core
{
    /// <summary>
    /// Interface for a storable object
    /// </summary>
    public interface IStorable
    {
        /// <summary>
        /// Store the object
        /// </summary>
        /// <param name="key">Storage key</param>
        /// <param name="tag">Any tagged object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task StoreAsync(string key, object? tag = null, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for a storable object
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public interface IStorable<T> : IStorable
    {
        /// <summary>
        /// Restore the object
        /// </summary>
        /// <param name="key">Storage key</param>
        /// <param name="tag">Any tagged object</param>
        /// <param name="cancellation">Cancellation token</param>
        /// <returns>Restored object</returns>
        public static abstract Task<T> RestoreAsync(string key, object? tag = null, CancellationToken cancellation = default);
    }
}
