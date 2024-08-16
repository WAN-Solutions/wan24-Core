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
            if (minLen.HasValue && len < minLen.Value)
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
            if (options is not null)
            {
                if (options.StringValueConverterName is not null) return StreamSerializerTypes.StringValueConverter | StreamSerializerTypes.NamedSerializer;
                if (options.ObjectSerializerName is not null) return StreamSerializerTypes.ObjectSerializer | StreamSerializerTypes.NamedSerializer;
            }
            if (TypeSerializer.CanSerialize(type, checkDenied: false)) return StreamSerializerTypes.TypeSerializer;
            if (typeof(ISerializeBinary).IsAssignableFrom(type)) return StreamSerializerTypes.Binary;
            if (StringValueConverter.CanConvertToString(type)) return StreamSerializerTypes.StringValueConverter;
            return StreamSerializerTypes.ObjectSerializer;
        }

        /// <summary>
        /// Find the serializer type which would be used to deserialize the given type
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="options">Options</param>
        /// <returns>Serializer type (if <see cref="StreamSerializerTypes.None"/>, the type can't be deserialized)</returns>
        public static StreamSerializerTypes FindStreamDeserializerType(in Type type, in DeserializerOptions? options = null)
        {
            if (options?.StreamSerializer.HasValue ?? false) return options.StreamSerializer.Value;
            if (TypeSerializer.IsDeniedForSerialization(type)) return StreamSerializerTypes.None;
            if (options is not null)
            {
                if (options.StringValueConverterName is not null) return StreamSerializerTypes.StringValueConverter | StreamSerializerTypes.NamedSerializer;
                if (options.ObjectSerializerName is not null) return StreamSerializerTypes.ObjectSerializer | StreamSerializerTypes.NamedSerializer;
            }
            if (TypeSerializer.CanSerialize(type, checkDenied: false)) return StreamSerializerTypes.TypeSerializer;
            if (typeof(ISerializeBinary).IsAssignableFrom(type)) return StreamSerializerTypes.Binary;
            if (StringValueConverter.CanConvertToString(type)) return StreamSerializerTypes.StringValueConverter;
            return StreamSerializerTypes.ObjectSerializer;
        }

        /// <summary>
        /// Find a serializable type (see <see cref="TypeSerializer.CanSerialize(in Type, in bool)"/>), to which the given type may need to be converted using <see cref="TypeConverter"/>, 
        /// if any <see cref="ITypeConverter{T}"/> interface was implemented
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="options">Options</param>
        /// <returns>Serializable type</returns>
        public static Type? FindSerializableType(in Type type, SerializerOptions? options = null)
            => TypeSerializer.CanSerialize(type)
                ? type
                : (from tt in (from it in type.GetInterfaces()
                               where it.IsGenericType &&
                                it.GetGenericTypeDefinition() == typeof(ITypeConverter<>)
                               select it.GetGenericArgumentsCached()[0])
                   where FindStreamSerializerType(tt, options) != StreamSerializerTypes.None
                   orderby FindStreamSerializerType(tt, options)
                   select tt)
                    .FirstOrDefault();

        /// <summary>
        /// Find a serializable type (see <see cref="TypeSerializer.CanSerialize(in Type, in bool)"/>), from which the given type may need to be converted using <see cref="TypeConverter"/>, 
        /// if any <see cref="ITypeConverter{T}"/> interface was implemented
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="options">Options</param>
        /// <returns>Serializable type</returns>
        public static Type? FindDeserializableType(in Type type, DeserializerOptions? options = null)
            => TypeSerializer.CanSerialize(type)
                ? type
                : (from tt in (from it in type.GetInterfaces()
                               where it.IsGenericType &&
                                it.GetGenericTypeDefinition() == typeof(ITypeConverter<>)
                               select it.GetGenericArgumentsCached()[0])
                   where FindStreamDeserializerType(tt, options) != StreamSerializerTypes.None
                   orderby FindStreamDeserializerType(tt, options)
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
            return StreamSerializerTypes.None;
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
            return StreamSerializerTypes.None;
        }

        /// <summary>
        /// Serialize an object to a stream
        /// </summary>
        /// <param name="stream">Target stream</param>
        /// <param name="obj">Object</param>
        /// <param name="options">Options</param>
        public static void Serialize(in Stream stream, object obj, SerializerOptions? options = null)
        {
            // Find the serializer to use
            options ??= TypeSerializer.GetSerializerOptions(obj.GetType());
            StreamSerializerTypes serializer = GetStreamSerializerType(options);
            Type type = obj.GetType();
            if (serializer == StreamSerializerTypes.None)
            {
                serializer = FindStreamSerializerType(type, options);
                if (serializer == StreamSerializerTypes.None)
                    if (options.TryTypeConversion)
                    {
                        Type convertType = FindSerializableType(type, options) ?? throw new SerializerException($"Failed to find serializable type for {type}");
                        if (convertType != type)
                        {
                            Type originalType = type;
                            obj = TypeConverter.Convert(obj, convertType);
                            type = obj.GetType();
                            serializer = FindStreamSerializerType(type, options);
                            if (serializer == StreamSerializerTypes.None)
                                throw new SerializerException($"Failed to find serializer for {type} (converted from {originalType})");
                        }
                        else
                        {
                            throw new SerializerException($"Failed to find serializer for {type} (no type conversion)");
                        }
                    }
                    else
                    {
                        throw new SerializerException($"Failed to find serializer for {type}");
                    }
            }
            // Apply type conversion
            if (serializer.RequiresTypeConversion())
            {
                Type convertType = FindSerializableType(type, options) ?? throw new SerializerException($"Failed to find serializable type for {type}");
                if (convertType != type)
                {
                    Type originalType = type;
                    obj = TypeConverter.Convert(obj, convertType);
                    type = obj.GetType();
                    serializer = FindStreamSerializerType(type, options);
                    if (serializer == StreamSerializerTypes.None)
                        throw new SerializerException($"Failed to find serializer for {type} (converted from {originalType})");
                }
            }
            // Write serializer information
            if (options.IncludeSerializerInfo)
                TypeSerializer.Serialize(typeof(StreamSerializerTypes), serializer, stream, options);
            // Serialize
            switch (serializer & ~StreamSerializerTypes.FLAGS)
            {
                case StreamSerializerTypes.TypeSerializer:
                    TypeSerializer.Serialize(type, obj, stream, options);
                    break;
                case StreamSerializerTypes.StringValueConverter:
                    if (serializer.UsesNamedSerializer())
                    {
                        if (options.StringValueConverterName is null)
                            throw new SerializerException("Missing string value converter name", new InvalidProgramException());
                        if (StringValueConverter.NamedStringConversion(obj, options.StringValueConverterName, type) is not string str)
                            throw new SerializerException($"Named string value converter \"{options.StringValueConverterName}\" converted {type} to NULL");
                        TypeSerializer.Serialize(typeof(string), str, stream, options);
                    }
                    else
                    {
                        if (StringValueConverter.Convert(type, obj) is not string str)
                            throw new SerializerException($"String value converter converted {type} to NULL");
                        TypeSerializer.Serialize(typeof(string), str, stream, options);
                    }
                    break;
                case StreamSerializerTypes.ObjectSerializer:
                    if (serializer.UsesNamedSerializer())
                    {
                        if (options.ObjectSerializerName is null)
                            throw new SerializerException("Missing string value converter name", new InvalidProgramException());
                        ObjectSerializer.Serialize(options.ObjectSerializerName, obj, stream);
                    }
                    else
                    {
                        ObjectSerializer.Serialize(obj, stream);
                    }
                    break;
                default:
                    throw new SerializerException($"Invalid serializer type {serializer} for {type}", new InvalidProgramException());
            }
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
            // Find the serializer to use
            options ??= TypeSerializer.GetSerializerOptions(obj.GetType());
            StreamSerializerTypes serializer = GetStreamSerializerType(options);
            Type type = obj.GetType();
            if (serializer == StreamSerializerTypes.None)
            {
                serializer = FindStreamSerializerType(type, options);
                if (serializer == StreamSerializerTypes.None)
                    if (options.TryTypeConversion)
                    {
                        Type convertType = FindSerializableType(type, options) ?? throw new SerializerException($"Failed to find serializable type for {type}");
                        if (convertType != type)
                        {
                            Type originalType = type;
                            obj = TypeConverter.Convert(obj, convertType);
                            type = obj.GetType();
                            serializer = FindStreamSerializerType(type, options);
                            if (serializer == StreamSerializerTypes.None)
                                throw new SerializerException($"Failed to find serializer for {type} (converted from {originalType})");
                        }
                        else
                        {
                            throw new SerializerException($"Failed to find serializer for {type} (no type conversion)");
                        }
                    }
                    else
                    {
                        throw new SerializerException($"Failed to find serializer for {type}");
                    }
            }
            // Apply type conversion
            if (serializer.RequiresTypeConversion())
            {
                Type convertType = FindSerializableType(type, options) ?? throw new SerializerException($"Failed to find serializable type for {type}");
                if (convertType != type)
                {
                    Type originalType = type;
                    obj = TypeConverter.Convert(obj, convertType);
                    type = obj.GetType();
                    serializer = FindStreamSerializerType(type, options);
                    if (serializer == StreamSerializerTypes.None)
                        throw new SerializerException($"Failed to find serializer for {type} (converted from {originalType})");
                }
            }
            // Write serializer information
            if (options.IncludeSerializerInfo)
                await TypeSerializer.SerializeAsync(typeof(StreamSerializerTypes), serializer, stream, options, cancellationToken).DynamicContext();
            // Serialize
            switch (serializer & ~StreamSerializerTypes.FLAGS)
            {
                case StreamSerializerTypes.TypeSerializer:
                    await TypeSerializer.SerializeAsync(type, obj, stream, options, cancellationToken).DynamicContext();
                    break;
                case StreamSerializerTypes.StringValueConverter:
                    if (serializer.UsesNamedSerializer())
                    {
                        if (options.StringValueConverterName is null)
                            throw new SerializerException("Missing string value converter name", new InvalidProgramException());
                        if (StringValueConverter.NamedStringConversion(obj, options.StringValueConverterName, type) is not string str)
                            throw new SerializerException($"Named string value converter \"{options.StringValueConverterName}\" converted {type} to NULL");
                        await TypeSerializer.SerializeAsync(typeof(string), str, stream, options, cancellationToken).DynamicContext();
                    }
                    else
                    {
                        if (StringValueConverter.Convert(type, obj) is not string str)
                            throw new SerializerException($"String value converter converted {type} to NULL");
                        await TypeSerializer.SerializeAsync(typeof(string), str, stream, options, cancellationToken).DynamicContext();
                    }
                    break;
                case StreamSerializerTypes.ObjectSerializer:
                    if (serializer.UsesNamedSerializer())
                    {
                        if (options.ObjectSerializerName is null)
                            throw new SerializerException("Missing string value converter name", new InvalidProgramException());
                        await ObjectSerializer.SerializeAsync(options.ObjectSerializerName, obj, stream, cancellationToken).DynamicContext();
                    }
                    else
                    {
                        await ObjectSerializer.SerializeAsync(obj, stream, cancellationToken: cancellationToken).DynamicContext();
                    }
                    break;
                default:
                    throw new SerializerException($"Invalid serializer type {serializer} for {type}", new InvalidProgramException());
            }
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Object type</param>
        /// <param name="version">Serializer version</param>
        /// <param name="options">Options</param>
        /// <returns>Deserialized object</returns>
        public static object Deserialize(in Stream stream, Type type, int? version = null, DeserializerOptions? options = null)
        {
            version ??= SerializerSettings.Version;
            options ??= TypeSerializer.GetDeserializerOptions(type);
            // Read serializer information
            StreamSerializerTypes serializer = StreamSerializerTypes.None;
            if (options.SerializerInfoIncluded)
            {
                serializer = TypeSerializer.Deserialize<StreamSerializerTypes>(typeof(StreamSerializerTypes), stream, version, options);
                if (serializer == StreamSerializerTypes.None)
                    throw new SerializerException($"Invalid serializer {serializer}", new InvalidDataException());
            }
            // Find the serializer to use
            if (serializer == StreamSerializerTypes.None)
                serializer = GetStreamSerializerType(options);
            Type? convertType = null;
            if (serializer == StreamSerializerTypes.None)
            {
                serializer = FindStreamDeserializerType(type, options);
                if (serializer == StreamSerializerTypes.None)
                    if (options.TryTypeConversion)
                    {
                        convertType = FindDeserializableType(type, options) ?? throw new SerializerException($"Failed to find deserializable type for {type}");
                        if (convertType != type)
                        {
                            serializer = FindStreamDeserializerType(type, options);
                            if (serializer == StreamSerializerTypes.None)
                                throw new SerializerException($"Failed to find deserializer for {convertType} (used for {type})");
                        }
                        else
                        {
                            throw new SerializerException($"Failed to find deserializer for {type} (no type conversion)");
                        }
                    }
                    else
                    {
                        throw new SerializerException($"Failed to find deserializer for {type}");
                    }
            }
            // Deserialize
            object res;
            switch (serializer & ~StreamSerializerTypes.FLAGS)
            {
                case StreamSerializerTypes.TypeSerializer:
                    res = TypeSerializer.Deserialize(convertType ?? type, stream, version, options);
                    break;
                case StreamSerializerTypes.StringValueConverter:
                    string str = TypeSerializer.Deserialize<string>(typeof(string), stream, version, options);
                    if (serializer.UsesNamedSerializer())
                    {
                        if (options.StringValueConverterName is null)
                            throw new SerializerException("Missing string value converter name", new InvalidProgramException());
                        res = StringValueConverter.NamedObjectConversion(str, options.StringValueConverterName, convertType ?? type)
                            ?? throw new SerializerException($"Named string value converter \"{options.StringValueConverterName}\" converted {type} to NULL");
                    }
                    else
                    {
                        res = StringValueConverter.Convert(convertType ?? type, str)
                            ?? throw new SerializerException($"String value converter converted {type} to NULL");
                    }
                    break;
                case StreamSerializerTypes.ObjectSerializer:
                    if (serializer.UsesNamedSerializer())
                    {
                        if (options.ObjectSerializerName is null)
                            throw new SerializerException("Missing string value converter name", new InvalidProgramException());
                        res = ObjectSerializer.Deserialize(options.ObjectSerializerName, convertType ?? type, stream)
                            ?? throw new SerializerException($"Named object serializer \"{options.ObjectSerializerName}\" deserialized {type} to NULL", new InvalidDataException());
                    }
                    else
                    {
                        res = ObjectSerializer.Deserialize(convertType ?? type, stream)
                            ?? throw new SerializerException($"Object serializer deserialized {type} to NULL", new InvalidDataException());
                    }
                    break;
                default:
                    throw new SerializerException($"Invalid deserializer type {serializer} for {type}", new InvalidProgramException());
            }
            // Apply type conversion
            if (convertType is not null && convertType != type) res = TypeConverter.Convert(res, type);
            // Ensure having a valid result type
            if (res is null) throw new SerializerException($"{type} was deserialized to NULL", new InvalidDataException());
            if (!type.IsAssignableFrom(res.GetType()))
            {
                res.TryDispose();
                throw new SerializerException($"Serializer for {type} deserialized to {res.GetType()}", new InvalidDataException());
            }
            return res;
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
            version ??= SerializerSettings.Version;
            options ??= TypeSerializer.GetDeserializerOptions(type);
            // Read serializer information
            StreamSerializerTypes serializer = StreamSerializerTypes.None;
            if (options.SerializerInfoIncluded)
            {
                serializer = await TypeSerializer.DeserializeAsync<StreamSerializerTypes>(typeof(StreamSerializerTypes), stream, version, options, cancellationToken).DynamicContext();
                if (serializer == StreamSerializerTypes.None)
                    throw new SerializerException($"Invalid serializer {serializer}", new InvalidDataException());
            }
            // Find the serializer to use
            if (serializer == StreamSerializerTypes.None)
                serializer = GetStreamSerializerType(options);
            Type? convertType = null;
            if (serializer == StreamSerializerTypes.None)
            {
                serializer = FindStreamDeserializerType(type, options);
                if (serializer == StreamSerializerTypes.None)
                    if (options.TryTypeConversion)
                    {
                        convertType = FindDeserializableType(type, options) ?? throw new SerializerException($"Failed to find deserializable type for {type}");
                        if (convertType != type)
                        {
                            serializer = FindStreamDeserializerType(type, options);
                            if (serializer == StreamSerializerTypes.None)
                                throw new SerializerException($"Failed to find deserializer for {convertType} (used for {type})");
                        }
                        else
                        {
                            throw new SerializerException($"Failed to find deserializer for {type} (no type conversion)");
                        }
                    }
                    else
                    {
                        throw new SerializerException($"Failed to find deserializer for {type}");
                    }
            }
            // Deserialize
            object res;
            switch (serializer & ~StreamSerializerTypes.FLAGS)
            {
                case StreamSerializerTypes.TypeSerializer:
                    res = await TypeSerializer.DeserializeAsync(convertType ?? type, stream, version, options, cancellationToken).DynamicContext();
                    break;
                case StreamSerializerTypes.StringValueConverter:
                    string str = await TypeSerializer.DeserializeAsync<string>(typeof(string), stream, version, options, cancellationToken).DynamicContext();
                    if (serializer.UsesNamedSerializer())
                    {
                        if (options.StringValueConverterName is null)
                            throw new SerializerException("Missing string value converter name", new InvalidProgramException());
                        res = StringValueConverter.NamedObjectConversion(str, options.StringValueConverterName, convertType ?? type)
                            ?? throw new SerializerException($"Named string value converter \"{options.StringValueConverterName}\" converted {type} to NULL");
                    }
                    else
                    {
                        res = StringValueConverter.Convert(convertType ?? type, str)
                            ?? throw new SerializerException($"String value converter converted {type} to NULL");
                    }
                    break;
                case StreamSerializerTypes.ObjectSerializer:
                    if (serializer.UsesNamedSerializer())
                    {
                        if (options.ObjectSerializerName is null)
                            throw new SerializerException("Missing string value converter name", new InvalidProgramException());
                        res = await ObjectSerializer.DeserializeAsync(options.ObjectSerializerName, convertType ?? type, stream, cancellationToken).DynamicContext()
                            ?? throw new SerializerException($"Named object serializer \"{options.ObjectSerializerName}\" deserialized {type} to NULL", new InvalidDataException());
                    }
                    else
                    {
                        res = await ObjectSerializer.DeserializeAsync(convertType ?? type, stream, cancellationToken: cancellationToken).DynamicContext()
                            ?? throw new SerializerException($"Object serializer deserialized {type} to NULL", new InvalidDataException());
                    }
                    break;
                default:
                    throw new SerializerException($"Invalid deserializer type {serializer} for {type}", new InvalidProgramException());
            }
            // Apply type conversion
            if (convertType is not null && convertType != type) res = TypeConverter.Convert(res, type);
            // Ensure having a valid result type
            if (res is null) throw new SerializerException($"{type} was deserialized to NULL", new InvalidDataException());
            if (!type.IsAssignableFrom(res.GetType()))
            {
                await res.TryDisposeAsync().DynamicContext();
                throw new SerializerException($"Serializer for {type} deserialized to {res.GetType()}", new InvalidDataException());
            }
            return res;
        }
    }
}
