using System.Buffers;

namespace wan24.Core
{
    /// <summary>
    /// Serializer options
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public record class SerializerOptions()
    {
        /// <summary>
        /// Default buffer pool
        /// </summary>
        public static ArrayPool<byte> DefaultBufferPool { get; set; } = ArrayPool<byte>.Shared;

        /// <summary>
        /// Default
        /// </summary>
        public static SerializerOptions Default { get; set; } = new();

        /// <summary>
        /// Buffer pool
        /// </summary>
        public ArrayPool<byte> BufferPool { get; init; } = DefaultBufferPool;
    }
}
