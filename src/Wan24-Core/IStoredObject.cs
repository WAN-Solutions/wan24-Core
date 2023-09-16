namespace wan24.Core
{
    /// <summary>
    /// Interface for a stored object
    /// </summary>
    /// <typeparam name="T">Object key type</typeparam>
    public interface IStoredObject<T> where T : notnull
    {
        /// <summary>
        /// Object key
        /// </summary>
        T ObjectKey { get; }
    }
}
