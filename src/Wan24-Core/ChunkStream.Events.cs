namespace wan24.Core
{
    // Events
    public partial class ChunkStream<tStream, tFinal>
    {
        /// <summary>
        /// Delegate for an event handler
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="e">Arguments</param>
        public delegate void ChunkStreamEvent_Delegate(tFinal stream, ChunkStreamEventArgs e);
        /// <summary>
        /// Raised when a chunk was written
        /// </summary>
        public event ChunkStreamEvent_Delegate? OnChunkWritten;
        /// <summary>
        /// Raise the <see cref="OnChunkWritten"/> event
        /// </summary>
        /// <param name="chunk">Chunk index</param>
        /// <param name="type">Chunk type</param>
        /// <param name="len">Chunk length</param>
        protected virtual void RaiseOnChunkWritten(in long chunk, in ChunkTypes type, in int len)
            => OnChunkWritten?.Invoke((tFinal)this, new(chunk, type, len));
    }
}
