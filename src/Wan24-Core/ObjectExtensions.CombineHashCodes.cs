namespace wan24.Core
{
    // Combine hash codes
    public static partial class ObjectExtensions
    {
        /// <summary>
        /// Combine hash codes
        /// </summary>
        /// <param name="objs">Objects</param>
        /// <returns>Hash code</returns>
        public static int CombineHashCodes<T>(this ReadOnlySpan<T> objs)
        {
            int len = objs.Length;
            switch (len)
            {
                case 0:
                    return 0;
                case 1:
                    return objs[0]?.GetHashCode() ?? 0;
            }
            int hashCode = CombineHashCodesInt(objs[..Math.Min(8, len)]);
            if (len < 9) return hashCode;
            objs = objs[8..];
            len -= 8;
            using RentedMemoryRef<object?> buffer = new(len: Math.Min(8, len + 1), clean: false);
            Span<object?> bufferSpan = buffer.Span;
            int i,
                j,
                len2;
            while (true)
            {
                bufferSpan[0] = hashCode;
                for (i = 0, j = 0, len2 = Math.Min(7, len); i < len2; bufferSpan[++i] = objs[j], j = i) ;
                hashCode = CombineHashCodesInt<object?>(bufferSpan[..Math.Min(8, len + 1)]);
                if (len <= 7) return hashCode;
                objs = objs[7..];
                len -= 7;
            }
        }

        /// <summary>
        /// Combine hash codes
        /// </summary>
        /// <param name="objs">Objects</param>
        /// <returns>Hash code</returns>
        public static int CombineHashCodes<T>(this Span<T> objs) => CombineHashCodes((ReadOnlySpan<T>)objs);

        /// <summary>
        /// Combine hash codes
        /// </summary>
        /// <param name="objs">Objects</param>
        /// <returns>Hash code</returns>
        public static int CombineHashCodes<T>(this Memory<T> objs) => CombineHashCodes((ReadOnlySpan<T>)objs.Span);

        /// <summary>
        /// Combine hash codes
        /// </summary>
        /// <param name="objs">Objects</param>
        /// <returns>Hash code</returns>
        public static int CombineHashCodes<T>(this ReadOnlyMemory<T> objs) => CombineHashCodes(objs.Span);

        /// <summary>
        /// Combine hash codes
        /// </summary>
        /// <param name="objs">Objects</param>
        /// <returns>Hash code</returns>
        public static int CombineHashCodes<T>(this T[] objs) => CombineHashCodes((ReadOnlySpan<T>)objs);

        /// <summary>
        /// Combine hash codes
        /// </summary>
        /// <param name="objs">Objects</param>
        /// <returns>Hash code</returns>
        public static int CombineHashCodes<T>(this IEnumerable<T> objs) => CombineHashCodes((ReadOnlySpan<T>)objs.ToArray());

        /// <summary>
        /// Combine hash codes of up to 8 objects
        /// </summary>
        /// <param name="objs">Objects (min. 2 required; only the first 8 objects will be used)</param>
        /// <returns>Hash code</returns>
        private static int CombineHashCodesInt<T>(in ReadOnlySpan<T> objs)
            => objs.Length switch
            {
                0 or 1 => throw new InvalidProgramException(),
                2 => HashCode.Combine(objs[0], objs[1]),
                3 => HashCode.Combine(objs[0], objs[1], objs[2]),
                4 => HashCode.Combine(objs[0], objs[1], objs[2], objs[3]),
                5 => HashCode.Combine(objs[0], objs[1], objs[2], objs[3], objs[4]),
                6 => HashCode.Combine(objs[0], objs[1], objs[2], objs[3], objs[4], objs[5]),
                7 => HashCode.Combine(objs[0], objs[1], objs[2], objs[3], objs[4], objs[5], objs[6]),
                _ => HashCode.Combine(objs[0], objs[1], objs[2], objs[3], objs[4], objs[5], objs[6], objs[7])
            };
    }
}
