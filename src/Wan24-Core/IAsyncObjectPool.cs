namespace wan24.Core
{
    /// <summary>
    /// Interface for an object pool which supports asynchronous renting / returning
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public interface IAsyncObjectPool<T> : IObjectPool<T>
    {
        /// <summary>
        /// Rent an item
        /// </summary>
        /// <returns>Item (<see cref="IObjectPoolItem"/> will be reset before returning)</returns>
        Task<T> RentAsync();
        /// <summary>
        /// Return an item
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="reset">Reset the <see cref="IObjectPoolItem"/> object?</param>
        Task ReturnAsync(T item, bool reset = false);
    }
}
