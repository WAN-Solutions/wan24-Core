namespace wan24.Core
{
    /// <summary>
    /// Interface for an object pool item
    /// </summary>
    public interface IObjectPoolItem
    {
        /// <summary>
        /// Reset the item for re-use
        /// </summary>
        void Reset();
    }
}
