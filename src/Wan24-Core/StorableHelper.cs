namespace wan24.Core
{
    /// <summary>
    /// <see cref="IStorable"/> helper
    /// </summary>
    public static class StorableHelper
    {
        /// <summary>
        /// Restore an object
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="key">Storage key</param>
        /// <param name="tag">Any tagged object</param>
        /// <param name="restoreType">Restore type (if different from <c>type</c>)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        public static async Task<object> RestoreAsync(Type type, string key, object? tag = null, Type? restoreType = null, CancellationToken cancellationToken = default)
            => await type.RestoreObjectAsync(restoreType ?? type, key, tag, cancellationToken).DynamicContext();

        /// <summary>
        /// Restore an object
        /// </summary>
        /// <typeparam name="T">Object type (will be used as restore type, too, if <c>restoreType</c> is <see langword="null"/>)</typeparam>
        /// <param name="key">Storage key</param>
        /// <param name="tag">Any tagged object</param>
        /// <param name="restoreType">Restore type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        public static async Task<T> RestoreAsync<T>(string key, object? tag = null, Type? restoreType = null, CancellationToken cancellationToken = default)
            => (T)await typeof(T).RestoreObjectAsync(restoreType ?? typeof(T), key, tag, cancellationToken).DynamicContext();
    }
}
