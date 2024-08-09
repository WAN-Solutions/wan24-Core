using System.Buffers;

namespace wan24.Core
{
    /// <summary>
    /// Deserializer options
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public record class DeserializerOptions()
    {
        /// <summary>
        /// Service provider
        /// </summary>
        private IServiceProvider? _ServiceProvider = DefaultServiceProvider;

        /// <summary>
        /// Default
        /// </summary>
        public static DeserializerOptions Default { get; set; } = new();

        /// <summary>
        /// Default buffer pool
        /// </summary>
        public static ArrayPool<byte> DefaultBufferPool { get; set; } = ArrayPool<byte>.Shared;

        /// <summary>
        /// Default service provider
        /// </summary>
        public static IServiceProvider? DefaultServiceProvider { get; set; } = DiHelper.ServiceProvider;

        /// <summary>
        /// Buffer pool
        /// </summary>
        public ArrayPool<byte> BufferPool { get; init; } = DefaultBufferPool;

        /// <summary>
        /// Service provider
        /// </summary>
        public IServiceProvider? ServiceProvider
        {
            get => _ServiceProvider ?? DefaultServiceProvider ?? DiHelper.ServiceProvider;
            init => _ServiceProvider = value;
        }

        /// <summary>
        /// Minimum length
        /// </summary>
        public int? MinLength { get; init; }

        /// <summary>
        /// Maximum length
        /// </summary>
        public int? MaxLength { get; init; }
    }
}
