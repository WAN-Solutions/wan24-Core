namespace wan24.Core
{
    /// <summary>
    /// Chunk type enumeration
    /// </summary>
    [Flags]
    public enum ChunkTypes : byte
    {
        /// <summary>
        /// None
        /// </summary>
        [DisplayText("None")]
        None = 0,
        /// <summary>
        /// Full chunk
        /// </summary>
        [DisplayText("Full chunk")]
        Full = 1,
        /// <summary>
        /// Partial final chunk (only valid in combination with the <see cref="Final"/> flag)
        /// </summary>
        [DisplayText("Partial final chunk")]
        Partial = 2,
        /// <summary>
        /// Final chunk flag
        /// </summary>
        [DisplayText("Final chunk")]
        Final = 128,
        /// <summary>
        /// All flags
        /// </summary>
        [DisplayText("All flags")]
        FLAGS = Final
    }
}
