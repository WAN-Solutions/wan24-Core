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
        public static Span<T> EnsureValid<T>(this Span<T> span, int offset, int length)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            long lastOffset = offset + length;
            if (lastOffset > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(length));
            if (offset > span.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (lastOffset > span.Length) throw new ArgumentOutOfRangeException(nameof(length));
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
        public static bool IsValid<T>(this Span<T> span, int offset, int length)
        {
            if (offset < 0) return false;
            if (length < 0) return false;
            long lastOffset = offset + length;
            if (lastOffset > int.MaxValue) return false;
            if (offset > span.Length) return false;
            if (lastOffset > span.Length) return false;
            return true;
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
        public static Memory<T> EnsureValid<T>(this Memory<T> memory, int offset, int length)
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
        public static bool IsValid<T>(this Memory<T> memory, int offset, int length) => IsValid(memory.Span, offset, length);
    }
}
