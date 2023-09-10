using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Asynchronous disposable extensions
    /// </summary>
    public static class AsyncDisposableExtensions
    {
        /// <summary>
        /// Return to the awaiting context
        /// </summary>
        /// <param name="disposable">Disposable</param>
        /// <returns>Disposable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static ConfiguredAsyncDisposable FixedContext(this IAsyncDisposable disposable) => disposable.ConfigureAwait(continueOnCapturedContext: true);

        /// <summary>
        /// Return in any thread context
        /// </summary>
        /// <param name="disposable">Disposable</param>
        /// <returns>Disposable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static ConfiguredAsyncDisposable DynamicContext(this IAsyncDisposable disposable) => disposable.ConfigureAwait(continueOnCapturedContext: false);

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <param name="parallel">Dispose parallel?</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task DisposeAllAsync(this IEnumerable<IAsyncDisposable> disposables, bool parallel = false)
        {
            if (parallel)
            {
                await (from disposable in disposables
                       select disposable.DisposeAsync())
                       .WaitAll()
                       .DynamicContext();
            }
            else
            {
                foreach (IAsyncDisposable disposable in disposables) await disposable.DisposeAsync().DynamicContext();
            }
        }
    }
}
