using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime;
using System.Runtime.CompilerServices;
using wan24.Core.Enumerables;

namespace wan24.Core
{
    // Execute
    public static partial class EnumerableExtensions
    {
        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this tItem[] enumerable, Func<tItem, ExecuteResult<tResult>> action, int offset = 0, int? length = null)
            => new ArrayEnumerable<tItem>(enumerable, offset, length).ExecuteForAll(action);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this ImmutableArray<tItem> enumerable, Func<tItem, ExecuteResult<tResult>> action, int offset = 0, int? length = null)
            => new ImmutableArrayEnumerable<tItem>(enumerable, offset, length).ExecuteForAll(action);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<tResult> ExecuteForAll<tList, tItem, tResult>(this tList enumerable, Func<tItem, ExecuteResult<tResult>> action, int offset = 0, int? length = null)
            where tList : IList<tItem>
            => new ListEnumerable<tList, tItem>(enumerable, offset, length).ExecuteForAll(action);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this FrozenSet<tItem> enumerable, Func<tItem, ExecuteResult<tResult>> action, int offset = 0, int? length = null)
            => new ImmutableArrayEnumerable<tItem>(enumerable.Items, offset, length).ExecuteForAll(action);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ExecuteForAll<T>(this T[] enumerable, Action<T> action, int offset = 0, int? length = null)
            => new ArrayEnumerable<T>(enumerable, offset, length).ExecuteForAll(action);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ExecuteForAll<T>(this ImmutableArray<T> enumerable, Action<T> action, int offset = 0, int? length = null)
            => new ImmutableArrayEnumerable<T>(enumerable, offset, length).ExecuteForAll(action);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ExecuteForAll<tList, tItem>(this tList enumerable, Action<tItem> action, int offset = 0, int? length = null) where tList : IList<tItem>
            => new ListEnumerable<tList, tItem>(enumerable, offset, length).ExecuteForAll(action);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ExecuteForAll<T>(this FrozenSet<T> enumerable, Action<T> action, int offset = 0, int? length = null)
            => new ImmutableArrayEnumerable<T>(enumerable.Items, offset, length).ExecuteForAll(action);

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tResult">Result type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this IEnumerable<tItem> enumerable, Func<tItem, ExecuteResult<tResult>> action)
        {
            ExecuteResult<tResult> result;
            foreach (tItem item in enumerable)
            {
                result = action(item);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
            }
        }

        /// <summary>
        /// Execute an action for all items
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ExecuteForAll<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable) action(item);
        }
    }
}
