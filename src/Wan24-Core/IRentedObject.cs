namespace wan24.Core
{
    /// <summary>
    /// Interface for a rented object
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public interface IRentedObject<T> : IDisposable
    {
        /// <summary>
        /// Object pool
        /// </summary>
        IObjectPool<T> Pool { get; }
        /// <summary>
        /// Rented object
        /// </summary>
        T Object { get; }
        /// <summary>
        /// Reset the <see cref="IObjectPoolItem"/> object when returning?
        /// </summary>
        bool Reset { get; set; }
    }
}
