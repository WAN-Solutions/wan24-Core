using System.Collections.Frozen;
using System.Collections.Immutable;
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

        /// <summary>
        /// Dispose all
        /// </summary>
        /// <param name="disposables">Disposables</param>
        /// <returns>Disposed disposables</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static IList<IDisposable> DisposeAll(this IList<IDisposable> disposables)
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
        public static List<IDisposable> DisposeAll(this List<IDisposable> disposables)
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
        public static FrozenSet<IDisposable> DisposeAll(this FrozenSet<IDisposable> disposables)
        {
            for (int i = 0, len = disposables.Count; i < len; disposables.Items[i].Dispose(), i++) ;
            return disposables;
        }
    }
}
