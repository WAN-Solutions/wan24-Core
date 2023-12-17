namespace wan24.Core
{
    // Enumerations
    public static partial class ByteEncoding
    {
        /// <summary>
        /// AVX command sets
        /// </summary>
        [Flags]
        public enum AvxCmd
        {
            /// <summary>
            /// AVX2
            /// </summary>
            Avx2,
            /// <summary>
            /// AVX-512
            /// </summary>
            Avx512
        }
    }
}
