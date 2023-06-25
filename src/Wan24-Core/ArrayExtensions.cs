using System.Runtime;

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
        public static Span<T> EnsureValid<T>(this Span<T> span, int offset, int length)
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
        public static ReadOnlySpan<T> EnsureValid<T>(this ReadOnlySpan<T> span, int offset, int length)
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
        public static bool IsValid<T>(this Span<T> span, int offset, int length)
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
        public static bool IsValid<T>(this ReadOnlySpan<T> span, int offset, int length)
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
        public static Memory<T> EnsureValid<T>(this Memory<T> memory, int offset, int length)
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
        public static ReadOnlyMemory<T> EnsureValid<T>(this ReadOnlyMemory<T> memory, int offset, int length)
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
        public static bool IsValid<T>(this Memory<T> memory, int offset, int length) => IsValid(memory.Span, offset, length);

        /// <summary>
        /// Determine if valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="memory">Memory</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Is valid?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsValid<T>(this ReadOnlyMemory<T> memory, int offset, int length) => IsValid(memory.Span, offset, length);

        /// <summary>
        /// Get the index of a value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="value">Value</param>
        /// <returns>Index or <c>-1</c>, if not found</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int IndexOf<T>(this T[] arr, T value) => Array.IndexOf(arr, value);

        /// <summary>
        /// Determine if all values are contained
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="values">Required values</param>
        /// <returns>All contained?</returns>
        public static bool ContainsAll<T>(this T[] arr, params T[] values)
        {
            int vlen = values.Length;
            if (vlen == 0) return true;
            int len = arr.Length;
            if (len < vlen) return false;
            bool[] found = new bool[vlen];
            for (int i = 0, index; i < len; i++)
            {
                index = values.IndexOf(arr[i]);
                if (index != -1) found[index] = true;
            }
            for (int i = 0; i < vlen; i++) if (!found[i]) return false;
            return true;
        }

        /// <summary>
        /// Determine if any of the values are contained
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="values">Values</param>
        /// <returns>Any contained?</returns>
        public static bool ContainsAny<T>(this T[] arr, params T[] values)
        {
            if (values.Length == 0) return false;
            int len = arr.Length;
            for (int i = 0; i < len; i++) if (values.IndexOf(arr[i]) != -1) return true;
            return false;
        }
    }
}
