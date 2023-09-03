using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Disposable extensions
    /// </summary>
    public static class DisposableExtensions
    {
        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <returns>Disposed disposables</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static IEnumerable<IDisposable> DisposeAll(this IEnumerable<IDisposable> disposables)
        {
            foreach (IDisposable disposable in disposables) disposable.Dispose();
            return disposables;
        }
    }
}
