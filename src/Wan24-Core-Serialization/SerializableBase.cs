
namespace wan24.Core
{
    /// <summary>
    /// Base class for a serializable type
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public abstract class SerializableBase() : ISerializeStream
    {
        /// <summary>
        /// Minimum supported custom serializer version
        /// </summary>
        private int? _MinCustomSerializerVersion = null;
        /// <summary>
        /// Serializer version number used for deserialization (is <see langword="null"/>, if the instance wasn't deserialized)
        /// </summary>
        protected int? DeserializerVersion = null;
        /// <summary>
        /// Serializer custom version number used for deserialization (is <see langword="null"/>, if the instance wasn't deserialized)
        /// </summary>
        protected int? DeserializerCustomVersion = null;
        /// <summary>
        /// Deserialized object versions
        /// </summary>
        protected List<int>? DeserializedObjectVersion = null;

        /// <summary>
        /// Object version numbers which are being serialized (from lowest to highest type level; count must match the <see cref="MinSerializerObjectVersion"/> items count)
        /// </summary>
        protected static List<int>? SerializerObjectVersion { get; set; }

        /// <summary>
        /// Minimum object versions which can be deserialized (from lowest to highest type level; count must match the <see cref="SerializerObjectVersion"/> items count)
        /// </summary>
        protected static List<int> MinSerializerObjectVersion { get; set; } = [1];

        /// <summary>
        /// If to include the serializer version into the serialization stream
        /// </summary>
        protected static bool IncludeSerializerVersion { get; set; }

        /// <summary>
        /// Minimum supported custom serializer version
        /// </summary>
        protected int? MinCustomSerializerVersion
        {
            get => _MinCustomSerializerVersion;
            set
            {
                if (!value.HasValue && _MinCustomSerializerVersion.HasValue)
                    throw new InvalidOperationException($"Min. custom serializer version required by a lower level type of {GetType()}");
                if (value.HasValue && _MinCustomSerializerVersion.HasValue && value.Value < _MinCustomSerializerVersion.Value)
                    throw new InvalidOperationException($"Min. custom serializer version #{_MinCustomSerializerVersion} required by a lower level type of {GetType()} can't be lowered");
                _MinCustomSerializerVersion = value;
            }
        }

        /// <summary>
        /// Serialize to a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="options">Options</param>
        protected virtual void SerializeTo(Stream stream, SerializerOptions? options)
        {
            if (IncludeSerializerVersion)
            {
                if (MinCustomSerializerVersion.HasValue && MinCustomSerializerVersion.Value > SerializerSettings.CustomVersion)
                    throw new InvalidOperationException(
                        $"Min. custom serializer version #{MinCustomSerializerVersion} of {GetType()} doesn't support current custom serializer version #{SerializerSettings.CustomVersion}"
                        );
                //TODO Serialize serializer version
            }
            if (SerializerObjectVersion is not null)
            {
                if (SerializerObjectVersion.Count != MinSerializerObjectVersion.Count)
                    throw new InvalidProgramException($"Serializer object version count of {GetType()} doesn't match min. serializer object version count");
                //TODO Serialize object versions
            }
        }

        /// <summary>
        /// Serialize to a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task SerializeToAsync(Stream stream, SerializerOptions? options, CancellationToken cancellationToken)
        {

        }

        /// <summary>
        /// Deserialize from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Serializer version number</param>
        /// <param name="options">Options</param>
        protected virtual void DeserializeFrom(Stream stream, int version, DeserializerOptions? options)
        {
            if (IncludeSerializerVersion)
            {
                if (MinCustomSerializerVersion.HasValue && MinCustomSerializerVersion.Value > (version >> 8))
                    throw new InvalidDataException(
                        $"Custom serializer version #{version >> 8} used to deserialize {GetType()} is lower then the min. supported custom serializer version {MinCustomSerializerVersion}"
                        );
                //TODO Deserialize serializer version
                if (MinCustomSerializerVersion.HasValue && MinCustomSerializerVersion.Value > DeserializerCustomVersion.Value)
                    throw new InvalidDataException(
                        $"Custom serializer version #{version >> 8} used to serialize {GetType()} is lower then the min. supported custom serializer version {MinCustomSerializerVersion}"
                        );
            }
            else
            {
                DeserializerVersion = version & byte.MaxValue;
                DeserializerCustomVersion = version >> 8;
            }
            if (SerializerObjectVersion is not null)
            {
                if (SerializerObjectVersion.Count != MinSerializerObjectVersion.Count)
                    throw new InvalidDataException($"Serializer object version count of {GetType()} doesn't match min. serializer object version count");
                if (DeserializedObjectVersion is null)
                {
                    DeserializedObjectVersion = [];
                }
                else
                {
                    DeserializedObjectVersion.Clear();
                }
                //TODO Deserialize object versions
                if (DeserializedObjectVersion[0] > MinSerializerObjectVersion[0] || DeserializedObjectVersion[0] > SerializerObjectVersion[0])
                    throw new InvalidDataException(
                        $"Serialized {GetType()} object version #{DeserializedObjectVersion[0]} isn't supported (min. version #{MinSerializerObjectVersion[0]}, max. version #{SerializerObjectVersion[0]} required)"
                        );
            }
            else if(DeserializedObjectVersion is not null)
            {
                DeserializedObjectVersion = null;
            }
        }

        /// <summary>
        /// Deserialize from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Serializer version number</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task DeserializeFromAsync(Stream stream, int version, DeserializerOptions? options, CancellationToken cancellationToken)
        {

        }

        /// <inheritdoc/>
        void ISerializeStream.SerializeTo(Stream stream, SerializerOptions? options) => SerializeTo(stream, options);

        /// <inheritdoc/>
        Task ISerializeStream.SerializeToAsync(Stream stream, SerializerOptions? options, CancellationToken cancellationToken) => SerializeToAsync(stream, options, cancellationToken);

        /// <inheritdoc/>
        void ISerializeStream.DeserializeFrom(Stream stream, int version, DeserializerOptions? options) => DeserializeFrom(stream, version, options);

        /// <inheritdoc/>
        Task ISerializeStream.DeserializeFromAsync(Stream stream, int version, DeserializerOptions? options, CancellationToken cancellationToken)
            => DeserializeFromAsync(stream, version, options, cancellationToken);
    }
}
