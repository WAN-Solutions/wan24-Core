using System.Buffers;

namespace wan24.Core
{
    /// <summary>
    /// Serialization helper
    /// </summary>
    public static class SerializationHelper
    {
        /// <summary>
        /// Get the buffer pool
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>Buffer pool</returns>
        public static ArrayPool<byte> GetBufferPool(SerializerOptions? options) => options?.BufferPool ?? SerializerOptions.DefaultBufferPool;

        /// <summary>
        /// Get the buffer pool
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>Buffer pool</returns>
        public static ArrayPool<byte> GetBufferPool(DeserializerOptions? options) => options?.BufferPool ?? DeserializerOptions.DefaultBufferPool;

        /// <summary>
        /// Get the service provider
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>Service provider</returns>
        public static IServiceProvider? GetServiceProvider(DeserializerOptions? options) => options?.ServiceProvider ?? DiHelper.ServiceProvider;
    }
}
