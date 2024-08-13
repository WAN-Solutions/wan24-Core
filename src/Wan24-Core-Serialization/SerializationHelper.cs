namespace wan24.Core
{
    /// <summary>
    /// Serialization helper
    /// </summary>
    public static class SerializationHelper
    {
        /// <summary>
        /// Ensure a valid length
        /// </summary>
        /// <param name="len">Positive length</param>
        /// <param name="minLen">Positive minimum length (including)</param>
        /// <param name="maxLen">Maximum length (including)</param>
        /// <param name="throwOnError">If to throw an exception on invalid length</param>
        /// <returns>The valid length or <c>-1</c>, if invalid</returns>
        /// <exception cref="ArgumentOutOfRangeException">Invalid min./max. length</exception>
        /// <exception cref="InvalidDataException">Invalid length</exception>
        public static int EnsureValidLength(in int len, in int? minLen = null, in int? maxLen = null, in bool throwOnError = true)
        {
            if (minLen.HasValue && minLen.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(minLen));
            if (minLen.HasValue && maxLen.HasValue && minLen.Value > maxLen.Value)
                throw new ArgumentOutOfRangeException(nameof(maxLen), "Max. length is greater than the min. length");
            if (len < 0)
            {
                if (!throwOnError) return -1;
                throw new InvalidDataException($"Invalid negative length {len}");
            }
            if(minLen.HasValue && len < minLen.Value)
            {
                if (!throwOnError) return -1;
                throw new InvalidDataException($"Length of {len} doesn't fit the min. length of {minLen}");
            }
            if (maxLen.HasValue && len > maxLen.Value)
            {
                if (!throwOnError) return -1;
                throw new InvalidDataException($"Length of {len} doesn't fit the max. length of {maxLen}");
            }
            return len;
        }

        /// <summary>
        /// Find the serializer type which would be used to serialize the given type
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="options">Options</param>
        /// <returns>Serializer type (if <see cref="StreamSerializerTypes.None"/>, the type can't be serialized)</returns>
        public static StreamSerializerTypes FindStreamSerializerType(in Type type, in SerializerOptions? options = null)
        {
            if (options?.StreamSerializer.HasValue ?? false) return options.StreamSerializer.Value;
            if (TypeSerializer.IsDeniedForSerialization(type)) return StreamSerializerTypes.None;
            if(options is not null)
            {
                if (options.StringValueConverterName is not null) return StreamSerializerTypes.StringValueConverter | StreamSerializerTypes.NamedSerializer;
                if (options.ObjectSerializerName is not null) return StreamSerializerTypes.ObjectSerializer | StreamSerializerTypes.NamedSerializer;
            }
            if (TypeSerializer.CanSerialize(type)) return StreamSerializerTypes.TypeSerializer;
            if (typeof(ISerializeBinary).IsAssignableFrom(type)) return StreamSerializerTypes.Binary;
            if (StringValueConverter.CanConvertToString(type)) return StreamSerializerTypes.StringValueConverter;
            return StreamSerializerTypes.ObjectSerializer;
        }

        /// <summary>
        /// Find a serializable type (see <see cref="TypeSerializer.CanSerialize(in Type, in bool)"/>), to which the given type may need to be converted using <see cref="TypeConverter"/>, 
        /// if any <see cref="ITypeConverter{T}"/> interface was implemented
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Serializable type</returns>
        public static Type? FindSerializableType(in Type type)
            => TypeSerializer.CanSerialize(type)
                ? type
                : (from tt in (from it in type.GetInterfaces()
                               where it.IsGenericType &&
                                it.GetGenericTypeDefinition() == typeof(ITypeConverter<>)
                               select it.GetGenericArgumentsCached()[0])
                   where FindStreamSerializerType(tt) != StreamSerializerTypes.None
                   orderby FindStreamSerializerType(tt)
                   select tt)
                    .FirstOrDefault();

        /// <summary>
        /// Get the stream serializer type to use
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>Stream serializer type</returns>
        public static StreamSerializerTypes GetStreamSerializerType(in SerializerOptions options)
        {
            if (options.StreamSerializer.HasValue) return options.StreamSerializer.Value;
            if (options.StringValueConverterName is not null) return StreamSerializerTypes.StringValueConverter;
            if (options.ObjectSerializerName is not null) return StreamSerializerTypes.ObjectSerializer;
            return StreamSerializerTypes.TypeSerializer;
        }

        /// <summary>
        /// Get the stream serializer type to use
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>Stream serializer type</returns>
        public static StreamSerializerTypes GetStreamSerializerType(in DeserializerOptions options)
        {
            if (options.StreamSerializer.HasValue) return options.StreamSerializer.Value;
            if (options.StringValueConverterName is not null) return StreamSerializerTypes.StringValueConverter;
            if (options.ObjectSerializerName is not null) return StreamSerializerTypes.ObjectSerializer;
            return StreamSerializerTypes.TypeSerializer;
        }

        /// <summary>
        /// Serialize an object to a stream
        /// </summary>
        /// <param name="stream">Target stream</param>
        /// <param name="obj">Object</param>
        /// <param name="options">Options</param>
        public static void Serialize(in Stream stream, in object obj, in SerializerOptions? options = null)
        {
            //TODO Serialize the object
        }

        /// <summary>
        /// Serialize an object to a stream
        /// </summary>
        /// <param name="stream">Target stream</param>
        /// <param name="obj">Object</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task SerializeAsync(Stream stream, object obj, SerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            //TODO Serialize the object
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Object type</param>
        /// <param name="version">Serializer version</param>
        /// <param name="options">Options</param>
        /// <returns>Deserialized object</returns>
        public static object Deserialize(in Stream stream, in Type type, int? version = null, in DeserializerOptions? options = null)
        {
            //TODO Deserialize object
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Object type</param>
        /// <param name="version">Serializer version</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Deserialized object</returns>
        public static async Task<object> DeserializeAsync(Stream stream, Type type, int? version = null, DeserializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            //TODO Deserialize object
        }
    }
}
