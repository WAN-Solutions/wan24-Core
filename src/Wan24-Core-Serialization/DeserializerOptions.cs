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

        /// <summary>
        /// Object serializer name (see <see cref="ObjectSerializer"/>)
        /// </summary>
        public string? ObjectSerializerName { get; init; }

        /// <summary>
        /// String value converter name (see <see cref="StringValueConverter"/>)
        /// </summary>
        public string? StringValueConverterName { get; init; }

        /// <summary>
        /// If to use the <see cref="TypeCache"/>
        /// </summary>
        public bool UseTypeCache { get; init; }

        /// <summary>
        /// If to use the named <see cref="TypeCache"/> (has no effect, if <see cref="UseTypeCache"/> is <see langword="false"/>)
        /// </summary>
        public bool UseNamedTypeCache { get; init; } = true;

        /// <summary>
        /// If to try <see cref="TypeConverter"/> for converting an object to a serializable type
        /// </summary>
        public bool TryTypeConversion { get; init; }

        /// <summary>
        /// Stream serializer to use
        /// </summary>
        public StreamSerializerTypes? StreamSerializer { get; init; }
    }
}
