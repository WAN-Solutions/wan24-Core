using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime;
using System.Runtime.CompilerServices;

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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<IDisposable> DisposeAll(this IEnumerable<IDisposable> disposables)
        {
            foreach (IDisposable disposable in disposables) disposable.Dispose();
            return disposables;
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="disposables">Disposables</param>
        /// <returns>Disposed disposables</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T DisposeAll<T>(this T disposables) where T : IList<IDisposable>
        {
            for (int i = 0, len = disposables.Count; i < len; disposables[i].Dispose(), i++) ;
            return disposables;
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <returns>Disposed disposables</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlySpan<IDisposable> DisposeAll(this ReadOnlySpan<IDisposable> disposables)
        {
            for (int i = 0, len = disposables.Length; i < len; disposables[i].Dispose(), i++) ;
            return disposables;
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <returns>Disposed disposables</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IDisposable[] DisposeAll(this IDisposable[] disposables)
        {
            DisposeAll((ReadOnlySpan<IDisposable>)disposables);
            return disposables;
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <returns>Disposed disposables</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<IDisposable> DisposeAll(this Span<IDisposable> disposables)
        {
            DisposeAll((ReadOnlySpan<IDisposable>)disposables);
            return disposables;
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <returns>Disposed disposables</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<IDisposable> DisposeAll(this Memory<IDisposable> disposables)
        {
            DisposeAll((ReadOnlySpan<IDisposable>)disposables.Span);
            return disposables;
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <returns>Disposed disposables</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlyMemory<IDisposable> DisposeAll(this ReadOnlyMemory<IDisposable> disposables)
        {
            DisposeAll(disposables.Span);
            return disposables;
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <returns>Disposed disposables</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArray<IDisposable> DisposeAll(this ImmutableArray<IDisposable> disposables)
        {
            for (int i = 0, len = disposables.Length; i < len; disposables[i].Dispose(), i++) ;
            return disposables;
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <returns>Disposed disposables</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static FrozenSet<IDisposable> DisposeAll(this FrozenSet<IDisposable> disposables)
        {
            DisposeAll(disposables.Items);
            return disposables;
        }

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
        public static T TryDispose<T>(this T obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
            switch (obj)
            {
                case IBasicDisposableObject basicDisposable and IDisposable disposable:
                    if (!basicDisposable.IsDisposing) disposable.Dispose();
                    break;
                case IBasicDisposableObject basicDisposable and IAsyncDisposable disposable:
                    if (!basicDisposable.IsDisposing) disposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
                case IAsyncDisposable disposable:
                    disposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
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
        public static void TryDisposeAll(this IEnumerable<object> objects)
        {
            foreach (object obj in objects) obj.TryDispose();
        }

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <param name="objects">Objects</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void TryDisposeAll(this ReadOnlySpan<object> objects)
        {
            for (int i = 0, len = objects.Length; i < len; objects[i].TryDispose(), i++) ;
        }

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <param name="objects">Objects</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void TryDisposeAll(this Span<object> objects) => TryDisposeAll((ReadOnlySpan<object>)objects);

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <param name="objects">Objects</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void TryDisposeAll(this Memory<object> objects) => TryDisposeAll((ReadOnlySpan<object>)objects.Span);

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <param name="objects">Objects</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void TryDisposeAll(this ReadOnlyMemory<object> objects) => TryDisposeAll(objects.Span);

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <param name="objects">Objects</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void TryDisposeAll(this object[] objects) => TryDisposeAll((ReadOnlySpan<object>)objects);

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <param name="objects">Objects</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void TryDisposeAll<T>(this T objects) where T : IList<object>
        {
            for (int i = 0, len = objects.Count; i < len; objects[i].TryDispose(), i++) ;
        }

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <param name="objects">Objects</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void TryDisposeAll(this ImmutableArray<object> objects)
        {
            for (int i = 0, len = objects.Length; i < len; objects[i].TryDispose(), i++) ;
        }

        /// <summary>
        /// Try to dispose disposable objects
        /// </summary>
        /// <param name="objects">Objects</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void TryDisposeAll(this FrozenSet<object> objects) => TryDisposeAll(objects.Items);
    }
}
