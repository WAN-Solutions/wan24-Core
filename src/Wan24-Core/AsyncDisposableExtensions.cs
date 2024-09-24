using System.Collections.Frozen;
using System.Collections.Immutable;
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

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <param name="parallel">Dispose parallel?</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task DisposeAllAsync(this ReadOnlyMemory<IAsyncDisposable> disposables, bool parallel = false)
        {
            if (parallel)
            {
                int len = disposables.Length;
                List<Task> tasks = new(len);
                for (int i = 0; i < len; tasks.Add(disposables.Span[i].DisposeAsync().AsTask()), i++) ;
                await tasks.WaitAll().DynamicContext();
            }
            else
            {
                for (int i = 0, len = disposables.Length; i < len; await disposables.Span[i].DisposeAsync().DynamicContext(), i++) ;
            }
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <param name="parallel">Dispose parallel?</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static Task DisposeAllAsync(this IAsyncDisposable[] disposables, bool parallel = false) => DisposeAllAsync((ReadOnlyMemory<IAsyncDisposable>)disposables, parallel);

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <param name="parallel">Dispose parallel?</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static Task DisposeAllAsync(this Memory<IAsyncDisposable> disposables, bool parallel = false) => DisposeAllAsync((ReadOnlyMemory<IAsyncDisposable>)disposables, parallel);

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <param name="parallel">Dispose parallel?</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task DisposeAllAsync(this IList<IAsyncDisposable> disposables, bool parallel = false)
        {
            if (parallel)
            {
                int len = disposables.Count;
                List<Task> tasks = new(len);
                for (int i = 0; i < len; tasks.Add(disposables[i].DisposeAsync().AsTask()), i++) ;
                await tasks.WaitAll().DynamicContext();
            }
            else
            {
                for (int i = 0, len = disposables.Count; i < len; await disposables[i].DisposeAsync().DynamicContext(), i++) ;
            }
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <param name="parallel">Dispose parallel?</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task DisposeAllAsync(this List<IAsyncDisposable> disposables, bool parallel = false)
        {
            if (parallel)
            {
                int len = disposables.Count;
                List<Task> tasks = new(len);
                for (int i = 0; i < len; tasks.Add(disposables[i].DisposeAsync().AsTask()), i++) ;
                await tasks.WaitAll().DynamicContext();
            }
            else
            {
                for (int i = 0, len = disposables.Count; i < len; await disposables[i].DisposeAsync().DynamicContext(), i++) ;
            }
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <param name="parallel">Dispose parallel?</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task DisposeAllAsync(this ImmutableArray<IAsyncDisposable> disposables, bool parallel = false)
        {
            if (parallel)
            {
                int len = disposables.Length;
                List<Task> tasks = new(len);
                for (int i = 0; i < len; tasks.Add(disposables[i].DisposeAsync().AsTask()), i++) ;
                await tasks.WaitAll().DynamicContext();
            }
            else
            {
                for (int i = 0, len = disposables.Length; i < len; await disposables[i].DisposeAsync().DynamicContext(), i++) ;
            }
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <param name="parallel">Dispose parallel?</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task DisposeAllAsync(this FrozenSet<IAsyncDisposable> disposables, bool parallel = false)
        {
            if (parallel)
            {
                int len = disposables.Count;
                List<Task> tasks = new(len);
                for (int i = 0; i < len; tasks.Add(disposables.Items[i].DisposeAsync().AsTask()), i++) ;
                await tasks.WaitAll().DynamicContext();
            }
            else
            {
                for (int i = 0, len = disposables.Count; i < len; await disposables.Items[i].DisposeAsync().DynamicContext(), i++) ;
            }
        }
    }
}
