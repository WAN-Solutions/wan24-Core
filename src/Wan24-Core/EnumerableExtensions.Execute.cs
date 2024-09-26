using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime;
using System.Runtime.CompilerServices;

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
        {
            ExecuteResult<tResult> result;
            for (int i = offset, len = length ?? enumerable.Length; i < len; i++)
            {
                result = action(enumerable[i]);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
            }
        }

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
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this List<tItem> enumerable, Func<tItem, ExecuteResult<tResult>> action, int offset = 0, int? length = null)
        {
            ExecuteResult<tResult> result;
            for (int i = offset, len = length ?? enumerable.Count; i < len; i++)
            {
                result = action(enumerable[i]);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
            }
        }

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
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(this IList<tItem> enumerable, Func<tItem, ExecuteResult<tResult>> action, int offset = 0, int? length = null)
        {
            ExecuteResult<tResult> result;
            for (int i = offset, len = length ?? enumerable.Count; i < len; i++)
            {
                result = action(enumerable[i]);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
            }
        }

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
        {
            ExecuteResult<tResult> result;
            for (int i = offset, len = length ?? enumerable.Count; i < len; i++)
            {
                result = action(enumerable.Items[i]);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
            }
        }

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
        public static IEnumerable<tResult> ExecuteForAll<tItem, tResult>(
            this ImmutableArray<tItem> enumerable, 
            Func<tItem, ExecuteResult<tResult>> action, 
            int offset = 0, 
            int? length = null
            )
        {
            ExecuteResult<tResult> result;
            for (int i = offset, len = length ?? enumerable.Length; i < len; i++)
            {
                result = action(enumerable[i]);
                if (!result.Next) yield break;
                if (result) yield return result.Result;
            }
        }
    }
}
