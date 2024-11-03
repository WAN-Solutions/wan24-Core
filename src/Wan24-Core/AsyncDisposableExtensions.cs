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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ConfiguredAsyncDisposable FixedContext(this IAsyncDisposable disposable) => disposable.ConfigureAwait(continueOnCapturedContext: true);

        /// <summary>
        /// Return in any thread context
        /// </summary>
        /// <param name="disposable">Disposable</param>
        /// <returns>Disposable</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ConfiguredAsyncDisposable DynamicContext(this IAsyncDisposable disposable) => disposable.ConfigureAwait(continueOnCapturedContext: false);

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <param name="parallel">Dispose parallel?</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
        /// <typeparam name="T">List type</typeparam>
        /// <param name="disposables">Disposables</param>
        /// <param name="parallel">Dispose parallel?</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task DisposeAllAsync<T>(this T disposables, bool parallel = false) where T : IList<IAsyncDisposable>
        {
            if (parallel)
            {
                int len = disposables.Count;
                if (len < 1) return;
                using RentedArrayStructSimple<Task> tasks = new(len);
                Task[] tasksArray = tasks.Array;
                for (int i = 0; i < len; tasksArray[i] = disposables[i].DisposeAsync().AsTask(), i++) ;
                await Task.WhenAll(tasks.Array.Enumerate(offset: 0, len)).DynamicContext();
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task DisposeAllAsync(this ImmutableArray<IAsyncDisposable> disposables, bool parallel = false)
        {
            if (parallel)
            {
                int len = disposables.Length;
                if (len < 1) return;
                using RentedArrayStructSimple<Task> tasks = new(len);
                Task[] tasksArray = tasks.Array;
                for (int i = 0; i < len; tasksArray[i] = disposables[i].DisposeAsync().AsTask(), i++) ;
                await Task.WhenAll(tasks.Array.Enumerate(offset: 0, len)).DynamicContext();
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
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task DisposeAllAsync(this FrozenSet<IAsyncDisposable> disposables, bool parallel = false) => DisposeAllAsync(disposables.Items, parallel);

        /// <summary>
        /// Try dispose, if disposable
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Object</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<T> TryDisposeAsync<T>(this T obj)
        {
            await Task.Yield();
            ArgumentNullException.ThrowIfNull(obj);
            switch (obj)
            {
                case IBasicDisposableObject basicDisposable and IAsyncDisposable disposable:
                    if (!basicDisposable.IsDisposing) await disposable.DisposeAsync().DynamicContext();
                    break;
                case IBasicDisposableObject basicDisposable and IDisposable disposable:
                    if (!basicDisposable.IsDisposing) disposable.Dispose();
                    break;
                case IAsyncDisposable disposable:
                    await disposable.DisposeAsync().DynamicContext();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
            return obj;
        }

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <param name="objects">Objects</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task TryDisposeAllAsync(this IEnumerable<object> objects)
            => Task.WhenAll(objects.Select(o => o.TryDisposeAsync()));

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="objects">Objects</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task TryDisposeAllAsync<T>(this T objects) where T : IList<object>
        {
            int len = objects.Count;
            if (len < 1) return;
            using RentedArrayStructSimple<Task> tasks = new(len);
            Task[] tasksArray = tasks.Array;
            for (int i = 0; i < len; tasksArray[i] = TryDisposeAsync(objects[i]), i++) ;
            await Task.WhenAll(tasksArray.Enumerate(offset: 0, len)).DynamicContext();
        }

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <param name="objects">Objects</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task TryDisposeAllAsync(this ImmutableArray<object> objects)
        {
            int len = objects.Length;
            if (len < 1) return;
            using RentedArrayStructSimple<Task> tasks = new(len);
            Task[] tasksArray = tasks.Array;
            for (int i = 0; i < len; tasksArray[i] = TryDisposeAsync(objects[i]), i++) ;
            await Task.WhenAll(tasksArray.Enumerate(offset: 0, len)).DynamicContext();
        }

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <param name="objects">Objects</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task TryDisposeAllAsync(this FrozenSet<object> objects) => TryDisposeAllAsync(objects.Items);
    }
}
