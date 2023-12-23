namespace wan24.Core
{
    /// <summary>
    /// Interface for a disposable object which will dispose foreign disposables when disposing
    /// </summary>
    public interface IWillDispose : IDisposableObject
    {
        /// <summary>
        /// Register an object for disposing when this object is being disposed
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="disposable">Disposable (will be disposed)</param>
        void RegisterForDispose<T>(in T disposable);
    }
}
