using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Extension methods for <see cref="IOptInOut"/> and <see cref="OptInOut"/>
    /// </summary>
    public static class OptInOutExtensions
    {
        /// <summary>
        /// Determine if the opt direction is opt in
        /// </summary>
        /// <param name="optInOut">Opt direction</param>
        /// <returns>If opt in</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsOptIn(this OptInOut optInOut) => optInOut == OptInOut.OptIn;

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptIn<T>(this IEnumerable<T> enumerable) where T : IOptInOut => enumerable.Where(i => i.Opt.IsOptIn());

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptIn<T>(this Memory<T> enumerable) where T : IOptInOut => FilterOptIn((ReadOnlyMemory<T>)enumerable);

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptIn<T>(this ReadOnlyMemory<T> enumerable) where T : IOptInOut
        {
            T item;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                item = enumerable.Span[i];
                if (item.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptIn<T>(this T[] enumerable) where T : IOptInOut => FilterOptIn((ReadOnlyMemory<T>)enumerable);

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptIn<T>(this IList<T> enumerable) where T : IOptInOut
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable[i];
                if (item.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptIn<T>(this List<T> enumerable) where T : IOptInOut
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable[i];
                if (item.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptIn<T>(this ImmutableArray<T> enumerable) where T : IOptInOut
        {
            T item;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                item = enumerable[i];
                if (item.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptIn<T>(this FrozenSet<T> enumerable) where T : IOptInOut
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable.Items[i];
                if (item.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptInOnly<T>(this IEnumerable<T> enumerable) => enumerable.Where(i => i is IOptInOut oio && oio.Opt.IsOptIn());

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptInOnly<T>(this Memory<T> enumerable) => FilterOptInOnly((ReadOnlyMemory<T>)enumerable);

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptInOnly<T>(this T[] enumerable) => FilterOptInOnly((ReadOnlyMemory<T>)enumerable);

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptInOnly<T>(this ReadOnlyMemory<T> enumerable)
        {
            T item;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                item = enumerable.Span[i];
                if (item is IOptInOut oio && oio.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptInOnly<T>(this IList<T> enumerable)
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable[i];
                if (item is IOptInOut oio && oio.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptInOnly<T>(this List<T> enumerable)
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable[i];
                if (item is IOptInOut oio && oio.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptInOnly<T>(this ImmutableArray<T> enumerable)
        {
            T item;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                item = enumerable[i];
                if (item is IOptInOut oio && oio.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt in items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptInOnly<T>(this FrozenSet<T> enumerable)
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable.Items[i];
                if (item is IOptInOut oio && oio.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt out items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items (or items with no opt direction)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptOut<T>(this IEnumerable<T> enumerable) => enumerable.Where(i => i is not IOptInOut oio || !oio.Opt.IsOptIn());

        /// <summary>
        /// Filter all opt out items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items (or items with no opt direction)</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptOut<T>(this Memory<T> enumerable) => FilterOptOut((ReadOnlyMemory<T>)enumerable);

        /// <summary>
        /// Filter all opt out items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items (or items with no opt direction)</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptOut<T>(this T[] enumerable) => FilterOptOut((ReadOnlyMemory<T>)enumerable);

        /// <summary>
        /// Filter all opt out items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items (or items with no opt direction)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptOut<T>(this ReadOnlyMemory<T> enumerable)
        {
            T item;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                item = enumerable.Span[i];
                if (item is not IOptInOut oio || !oio.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt out items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items (or items with no opt direction)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptOut<T>(this IList<T> enumerable)
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable[i];
                if (item is not IOptInOut oio || !oio.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt out items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items (or items with no opt direction)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptOut<T>(this List<T> enumerable)
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable[i];
                if (item is not IOptInOut oio || !oio.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt out items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items (or items with no opt direction)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptOut<T>(this ImmutableArray<T> enumerable)
        {
            T item;
            for (int i = 0, len = enumerable.Length; i < len; i++)
            {
                item = enumerable[i];
                if (item is not IOptInOut oio || !oio.Opt.IsOptIn()) yield return item;
            }
        }

        /// <summary>
        /// Filter all opt out items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Opt in items (or items with no opt direction)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<T> FilterOptOut<T>(this FrozenSet<T> enumerable)
        {
            T item;
            for (int i = 0, len = enumerable.Count; i < len; i++)
            {
                item = enumerable.Items[i];
                if (item is not IOptInOut oio || !oio.Opt.IsOptIn()) yield return item;
            }
        }
    }
}
