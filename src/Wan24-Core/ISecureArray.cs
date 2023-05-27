namespace wan24.Core
{
    /// <summary>
    /// Interface for a secure array
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    public interface ISecureArray<T> : IArray<T>, IDisposable where T : struct
    {
        /// <summary>
        /// Pointer
        /// </summary>
        IntPtr IntPtr { get; }
        /// <summary>
        /// Detach the secured byte array and dispose this instance
        /// </summary>
        /// <returns>Unsecure byte array</returns>
        T[] DetachAndDispose();
    }
}
