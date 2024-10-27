using System.Collections.Immutable;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Array extensions
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Ensure valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="span">Span</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on offset/length error</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<T> EnsureValid<T>(this Span<T> span, in int offset, in int length)
        {
            long lastOffset = offset + length;
            if (offset < 0 || offset > span.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || lastOffset > int.MaxValue || lastOffset > span.Length) throw new ArgumentOutOfRangeException(nameof(length));
            return span;
        }

        /// <summary>
        /// Ensure valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="span">Span</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on offset/length error</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlySpan<T> EnsureValid<T>(this ReadOnlySpan<T> span, in int offset, in int length)
        {
            long lastOffset = offset + length;
            if (offset < 0 || offset > span.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || lastOffset > int.MaxValue || lastOffset > span.Length) throw new ArgumentOutOfRangeException(nameof(length));
            return span;
        }

        /// <summary>
        /// Determine if valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="span">Span</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Is valid?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsValid<T>(this Span<T> span, in int offset, in int length)
        {
            long lastOffset = offset + length;
            return !(offset < 0 || length < 0 || lastOffset > int.MaxValue || offset > span.Length || lastOffset > span.Length);
        }

        /// <summary>
        /// Determine if valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="span">Span</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Is valid?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsValid<T>(this ReadOnlySpan<T> span, in int offset, in int length)
        {
            long lastOffset = offset + length;
            return !(offset < 0 || length < 0 || lastOffset > int.MaxValue || offset > span.Length || lastOffset > span.Length);
        }

        /// <summary>
        /// Ensure valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="memory">Memory</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Memory</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on offset/length error</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<T> EnsureValid<T>(this Memory<T> memory, in int offset, in int length)
        {
            EnsureValid(memory.Span, offset, length);
            return memory;
        }

        /// <summary>
        /// Ensure valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="memory">Memory</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Memory</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on offset/length error</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlyMemory<T> EnsureValid<T>(this ReadOnlyMemory<T> memory, in int offset, in int length)
        {
            EnsureValid(memory.Span, offset, length);
            return memory;
        }

        /// <summary>
        /// Determine if valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="memory">Memory</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Is valid?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsValid<T>(this Memory<T> memory, in int offset, in int length) => IsValid(memory.Span, offset, length);

        /// <summary>
        /// Determine if valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="memory">Memory</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Is valid?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsValid<T>(this ReadOnlyMemory<T> memory, in int offset, in int length) => IsValid(memory.Span, offset, length);

        /// <summary>
        /// Get the index of a value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="value">Value</param>
        /// <returns>Index or <c>-1</c>, if not found</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int IndexOf<T>(this T[] arr, in T value) => Array.IndexOf(arr, value);

        /// <summary>
        /// Get the index of a value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="value">Value</param>
        /// <returns>Index or <c>-1</c>, if not found</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int IndexOf<T>(this Memory<T> arr, in T value) => IndexOf((ReadOnlySpan<T>)arr.Span, value);

        /// <summary>
        /// Get the index of a value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="value">Value</param>
        /// <returns>Index or <c>-1</c>, if not found</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int IndexOf<T>(this ReadOnlyMemory<T> arr, in T value) => IndexOf(arr.Span, value);

        /// <summary>
        /// Get the index of a value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="value">Value</param>
        /// <returns>Index or <c>-1</c>, if not found</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int IndexOf<T>(this ReadOnlySpan<T> arr, in T value)
        {
            if (value is null)
            {
                for (int i = 0, len = arr.Length; i < len; i++) if (arr[i] is null) return i;
            }
            else
            {
                for (int i = 0, len = arr.Length; i < len; i++) if (arr[i]?.Equals(value) ?? false) return i;
            }
            return -1;
        }

        /// <summary>
        /// Get as read-only
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="span">Span</param>
        /// <returns>Span</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlySpan<T> AsReadOnly<T>(this Span<T> span) => (ReadOnlySpan<T>)span;

        /// <summary>
        /// Get as read-only
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="mem">Memory</param>
        /// <returns>Memory</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlyMemory<T> AsReadOnly<T>(this Memory<T> mem) => (ReadOnlyMemory<T>)mem;

        /// <summary>
        /// Get as read-only
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Read-only collection</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArray<T> AsReadOnly<T>(this IEnumerable<T> enumerable) => [.. enumerable];

        /// <summary>
        /// Clone an array (items will be copied into a new array)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>New array</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T[] CloneArray<T>(this T[] arr)
        {
            if (arr.Length == 0) return [];
            T[] res = new T[arr.Length];
            Array.Copy(arr, res, arr.Length);
            return res;
        }
    }
}
