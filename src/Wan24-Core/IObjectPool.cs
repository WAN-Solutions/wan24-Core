namespace wan24.Core
{
    /// <summary>
    /// Intrface for an object pool
    /// </summary>
    public interface IObjectPool : IPool
    {
        /// <summary>
        /// Rent an item
        /// </summary>
        /// <returns>Item (<see cref="IObjectPoolItem"/> will be reset before returning)</returns>
        object RentObject();
        /// <summary>
        /// Return an item
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="reset">Reset the <see cref="IObjectPoolItem"/> object?</param>
        void Return(in object item, in bool reset = false);
    }

    /// <summary>
    /// Interface for an object pool
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public interface IObjectPool<T> : IObjectPool
    {
        /// <summary>
        /// Rent an item
        /// </summary>
        /// <returns>Item (<see cref="IObjectPoolItem"/> will be reset before returning)</returns>
        T Rent();
        /// <summary>
        /// Return an item
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="reset">Reset the <see cref="IObjectPoolItem"/> object?</param>
        void Return(in T item, in bool reset = false);
    }
}
