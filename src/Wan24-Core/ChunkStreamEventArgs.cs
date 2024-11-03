namespace wan24.Core
{
    /// <summary>
    /// Chunk stream event arguments
    /// </summary>
    /// <param name="chunk">Chunk index</param>
    /// <param name="type">Chunk type</param>
    /// <param name="len">Chunk length</param>
    public class ChunkStreamEventArgs(in long chunk, in ChunkTypes type, in int len) : EventArgs()
    {
        /// <summary>
        /// Chunk index
        /// </summary>
        public long Chunk { get; } = chunk;

        /// <summary>
        /// Chunk type
        /// </summary>
        public ChunkTypes Type { get; } = type;

        /// <summary>
        /// Chunk length in bytes
        /// </summary>
        public int Length { get; } = len;
    }
}
