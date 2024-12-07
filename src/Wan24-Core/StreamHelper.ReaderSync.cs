namespace wan24.Core
{
    // Synchronous reader
    public static partial class StreamHelper
    {
        /// <summary>
        /// Get a reader for an object type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="version">Data structure version</param>
        /// <param name="objType">Serialized object type (if <see cref="SerializedObjectTypes.None"/>, it'll be determined from the given object type <c>T</c>)</param>
        /// <param name="options">Options</param>
        /// <returns>Reader</returns>
        public static Func<Stream, T> GetReader<T>(
            int version,
            SerializedObjectTypes objType = SerializedObjectTypes.None,
            StreamExtensions.ReadingOptions? options = null
            )
        {
            options ??= StreamExtensions.ReadingOptions.DefaultOptions;
            Type type = typeof(T);
            if (objType == SerializedObjectTypes.None) objType = type.GetSerializedType(options.UseInterfaces ?? type.IsInterface);
            if ((objType == SerializedObjectTypes.Json && type.IsInterface) || type == typeof(object))
            {
                if (!StreamExtensions.AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
                return static (stream) => default;//TODO Read any object
            }
            switch (objType)
            {
                case SerializedObjectTypes.Numeric:
                    return (stream) => stream.ReadNumeric(version, type, options.Buffer).CastType<T>();
                case SerializedObjectTypes.String:
                    {
                        StreamExtensions.StringReadingOptions stringOptions = options as StreamExtensions.StringReadingOptions ?? options.StringItemOptions;
                        return (stream) => new string(stringOptions.StringBuffer!.Value.Span[..stream.ReadString(version, stringOptions.StringEncoding, stringOptions)])
                            .CastType<T>();
                    }
                case SerializedObjectTypes.Boolean:
                    return (stream) => stream.ReadBoolean(version).CastType<T>();
                case SerializedObjectTypes.Type:
                    {
                        StreamExtensions.TypeReadingOptions typeOptions = options as StreamExtensions.TypeReadingOptions ?? options.TypeItemOptions;
                        return (stream) => stream.ReadType(version, typeOptions).CastType<T>();
                    }
                case SerializedObjectTypes.Array:
                    if (!options.MaxItemCount.HasValue) throw new ArgumentNullException(nameof(options), "Max. item count required for reading array");
                    if (type.FindGenericType(typeof(IList<>)) is Type genericListType)
                    {
                        dynamic dummy = GenericHelper.GetDefault(TypeInfoExt.From(genericListType).FirstGenericArgument ?? throw new InvalidProgramException());
                        StreamExtensions.ListReadingOptions itemOptions = options.ListItemOptions;
                        return (stream) => StreamExtensions.ReadGenericList(stream, version, itemOptions, dummy: dummy);
                    }
                    else
                    {
                        return (stream) => stream.ReadList(version, options.ListItemOptions).CastType<T>();
                    }
                case SerializedObjectTypes.Dictionary:
                    return type.FindGenericType(typeof(IDictionary<,>)) is null
                        ? (stream) => default
                        : (stream) => default;//TODO Read dictionary value
                case SerializedObjectTypes.Stream:
                    return (stream) => stream.ReadStream(version).CastType<T>();
                case SerializedObjectTypes.Serializable:
                    {
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeReadingOptions? typeOptions = isFinalType
                            ? null
                            : options as StreamExtensions.TypeReadingOptions ?? options.TypeItemOptions;
                        return isFinalType
                            ? (stream) => type.DeserializeFrom(stream, version).CastType<T>()
                            : (stream) => stream.ReadType(version, typeOptions!).DeserializeFrom(stream, version).CastType<T>();
                    }
                case SerializedObjectTypes.SerializeBinary:
                    if (!options.Buffer.HasValue) throw new ArgumentNullException(nameof(options), "Buffer required for reading binary serialized");
                    {
                        int? fixedStructureSize = type.GetMaxStructureSize();
                        bool isFInalType = type.IsFinalType();
                        StreamExtensions.TypeReadingOptions? typeOptions = isFInalType
                            ? null
                            : options as StreamExtensions.TypeReadingOptions ?? options.TypeItemOptions;
                        return isFInalType
                            ? fixedStructureSize.HasValue
                                ? (stream) =>
                                {
                                    // Final type with a fixed length
                                    Span<byte> bufferSpan = options.Buffer.Value.Span[..fixedStructureSize.Value];
                                    stream.ReadExactly(bufferSpan);
                                    return type.DeserializeFrom(bufferSpan).CastType<T>();
                                }
                                : (stream) =>
                                {
                                    // Final type with a dynamic length
                                    Span<byte> bufferSpan = options.Buffer.Value.Span;
                                    int len = stream.ReadDataWithLengthInfo(version, bufferSpan);
                                    return type.DeserializeFrom(bufferSpan[..len]).CastType<T>();
                                }
                            : fixedStructureSize.HasValue && type.CanConstruct()
                                ? (stream) =>
                                {
                                    // Constructable type with a fixed length
                                    Type valueType = stream.ReadType(version, typeOptions!);
                                    Span<byte> bufferSpan = options.Buffer.Value.Span[..fixedStructureSize.Value];
                                    stream.ReadExactly(bufferSpan);
                                    return valueType.DeserializeFrom(bufferSpan).CastType<T>();
                                }
                                : (stream) =>
                                {
                                    // Type with a dynamic length
                                    Type valueType = stream.ReadType(version, typeOptions!);
                                    Span<byte> bufferSpan = options.Buffer.Value.Span;
                                    int len = stream.ReadDataWithLengthInfo(version, bufferSpan);
                                    return valueType.DeserializeFrom(bufferSpan[..len]).CastType<T>();
                                };
                    }
                case SerializedObjectTypes.SerializeString:
                    {
                        StreamExtensions.StringReadingOptions stringOptions = options as StreamExtensions.StringReadingOptions ?? options.StringItemOptions;
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeReadingOptions? typeOptions = isFinalType ? null : options as StreamExtensions.TypeReadingOptions ?? options.TypeItemOptions;
                        return isFinalType
                            ? (stream) =>
                            {
                                int len = stream.ReadString(version, stringOptions);
                                return type.ParseObject(stringOptions.StringBuffer!.Value.Span[..len]).CastType<T>();
                            }
                            : (stream) =>
                            {
                                Type valueType = stream.ReadType(version, typeOptions!);
                                int len = stream.ReadString(version, stringOptions);
                                return valueType.ParseObject(stringOptions.StringBuffer!.Value.Span[..len]).CastType<T>();
                            };
                    }
                case SerializedObjectTypes.Enum:
                    {
                        IEnumInfo enumInfo = type.GetEnumInfo();
                        Type numericType = enumInfo.NumericType;
                        return (stream) =>
                        {
                            T res = stream.ReadNumeric(version, numericType, options.Buffer).CastType<T>();
                            if (!enumInfo.IsValidValue(res)) throw new InvalidDataException($"Deserialized invalid {type} value of \"{res}\"");
                            return res;
                        };
                    }
                case SerializedObjectTypes.Enumerable:
                    return static (stream) => default;//TODO Read enumerable
                case SerializedObjectTypes.Json:
                    {
                        StreamExtensions.JsonReadingOptions jsonOptions = options as StreamExtensions.JsonReadingOptions ?? options.JsonItemOptions;
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeReadingOptions typeOptions = options as StreamExtensions.TypeReadingOptions ?? options.TypeItemOptions;
                        return type.IsFinalType()
                            ? (stream) => stream.ReadJson<T>(version, jsonOptions)
                            : (stream) => stream.ReadJson(version, stream.ReadType(version, typeOptions!), jsonOptions).CastType<T>();
                    }
                default:
                    throw new InvalidProgramException($"Failed to determine a valid serialized object type for {typeof(T)} (got {objType})");
            }
        }

        /// <summary>
        /// Get a reader for a nullable object type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="version">Data structure version</param>
        /// <param name="objType">Serialized object type (if <see cref="SerializedObjectTypes.None"/>, it'll be determined from the given object type <c>T</c>)</param>
        /// <param name="options">Options</param>
        /// <returns>Reader</returns>
        public static Func<Stream, T?> GetNullableReader<T>(
            int version,
            SerializedObjectTypes objType = SerializedObjectTypes.None,
            StreamExtensions.ReadingOptions? options = null
            )
        {
            options ??= StreamExtensions.ReadingOptions.DefaultOptions;
            Type type = typeof(T);
            if (objType == SerializedObjectTypes.None) objType = type.GetSerializedType(options.UseInterfaces ?? type.IsInterface);
            if ((objType == SerializedObjectTypes.Json && type.IsInterface) || type == typeof(object))
            {
                if (!StreamExtensions.AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
                return static (stream) => default;//TODO Read any object
            }
            switch (objType)
            {
                case SerializedObjectTypes.Numeric:
                    return (stream) => stream.ReadNumericNullable(version, type, options.Buffer) is T res ? res : default(T?);
                case SerializedObjectTypes.String:
                    {
                        StreamExtensions.StringReadingOptions stringOptions = options as StreamExtensions.StringReadingOptions ?? options.StringItemOptions;
                        return (stream) =>
                        {
                            int len = stream.ReadString(version, stringOptions.StringEncoding, stringOptions);
                            return len < 0
                                ? default(T?)
                                : len == 0
                                    ? string.Empty.CastType<T>()
                                    : new string(stringOptions.StringBuffer!.Value.Span[..len]).CastType<T>();
                        };
                    }
                case SerializedObjectTypes.Boolean:
                    return (stream) => stream.ReadBooleanNullable(version) is T res ? res : default(T?);
                case SerializedObjectTypes.Type:
                    {
                        StreamExtensions.TypeReadingOptions typeOptions = options as StreamExtensions.TypeReadingOptions ?? options.TypeItemOptions;
                        return (stream) => stream.ReadTypeNullable(version, typeOptions) is T res ? res : default(T?);
                    }
                case SerializedObjectTypes.Array:
                    if (!options.MaxItemCount.HasValue) throw new ArgumentNullException(nameof(options), "Max. item count required for reading array");
                    if (type.FindGenericType(typeof(IList<>)) is Type genericListType)
                    {
                        dynamic dummy = GenericHelper.GetDefault(TypeInfoExt.From(genericListType).FirstGenericArgument ?? throw new InvalidProgramException());
                        StreamExtensions.ListReadingOptions itemOptions = options.ListItemOptions;
                        return (stream) => StreamExtensions.ReadGenericListNullable(stream, version, itemOptions, dummy: dummy);
                    }
                    else
                    {
                        return (stream) => stream.ReadListNullable(version, options.ListItemOptions) is T res ? res : default(T?);
                    }
                case SerializedObjectTypes.Dictionary:
                    return type.FindGenericType(typeof(IDictionary<,>)) is null
                        ? (stream) => default
                        : (stream) => default;//TODO Read dictionary value
                case SerializedObjectTypes.Stream:
                    return (stream) => stream.ReadStreamNullable(version) is T res ? res : default(T?);
                case SerializedObjectTypes.Serializable:
                    {
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeReadingOptions? typeOptions = isFinalType
                            ? null
                            : options as StreamExtensions.TypeReadingOptions ?? options.TypeItemOptions;
                        return isFinalType
                            ? (stream) => stream.ReadBoolean(version) ? type.DeserializeFrom(stream, version).CastType<T>() : default(T?)
                            : (stream) => stream.ReadTypeNullable(version, typeOptions!) is Type valueType 
                                ? valueType.DeserializeFrom(stream, version).CastType<T>() 
                                : default(T?);
                    }
                case SerializedObjectTypes.SerializeBinary:
                    if (!options.Buffer.HasValue) throw new ArgumentNullException(nameof(options), "Buffer required for reading binary serialized");
                    {
                        int? fixedStructureSize = type.GetMaxStructureSize();
                        bool isFInalType = type.IsFinalType();
                        StreamExtensions.TypeReadingOptions? typeOptions = isFInalType
                            ? null
                            : options as StreamExtensions.TypeReadingOptions ?? options.TypeItemOptions;
                        return isFInalType
                            ? fixedStructureSize.HasValue
                                ? (stream) =>
                                {
                                    // Final type with a fixed length
                                    if (!stream.ReadBoolean(version)) return default(T?);
                                    Span<byte> bufferSpan = options.Buffer.Value.Span[..fixedStructureSize.Value];
                                    stream.ReadExactly(bufferSpan);
                                    return type.DeserializeFrom(bufferSpan).CastType<T>();
                                }
                                : (stream) =>
                                {
                                    // Final type with a dynamic length
                                    if (!stream.ReadBoolean(version)) return default(T?);
                                    Span<byte> bufferSpan = options.Buffer.Value.Span;
                                    int len = stream.ReadDataWithLengthInfo(version, bufferSpan);
                                    return type.DeserializeFrom(bufferSpan[..len]).CastType<T>();
                                }
                            : fixedStructureSize.HasValue && type.CanConstruct()
                                ? (stream) =>
                                {
                                    // Constructable type with a fixed length
                                    Type? valueType = stream.ReadTypeNullable(version, typeOptions!);
                                    if (valueType is null) return default(T?);
                                    Span<byte> bufferSpan = options.Buffer.Value.Span[..fixedStructureSize.Value];
                                    stream.ReadExactly(bufferSpan);
                                    return valueType.DeserializeFrom(bufferSpan).CastType<T>();
                                }
                                : (stream) =>
                                {
                                    // Type with a dynamic length
                                    Type? valueType = stream.ReadTypeNullable(version, typeOptions!);
                                    if (valueType is null) return default(T?);
                                    Span<byte> bufferSpan = options.Buffer.Value.Span;
                                    int len = stream.ReadDataWithLengthInfo(version, bufferSpan);
                                    return valueType.DeserializeFrom(bufferSpan[..len]).CastType<T>();
                                };
                    }
                case SerializedObjectTypes.SerializeString:
                    {
                        StreamExtensions.StringReadingOptions stringOptions = options as StreamExtensions.StringReadingOptions ?? options.StringItemOptions;
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeReadingOptions? typeOptions = isFinalType ? null : options as StreamExtensions.TypeReadingOptions ?? options.TypeItemOptions;
                        return isFinalType
                            ? (stream) =>
                            {
                                int len = stream.ReadStringNullable(version, stringOptions);
                                return len < 0 ? default(T?) : type.ParseObject(stringOptions.StringBuffer!.Value.Span[..len]).CastType<T>();
                            }
                        : (stream) =>
                        {
                            Type? valueType = stream.ReadTypeNullable(version, typeOptions!);
                            if(valueType is null) return default(T?);
                            int len = stream.ReadString(version, stringOptions);
                            return valueType.ParseObject(stringOptions.StringBuffer!.Value.Span[..len]).CastType<T>();
                        };
                    }
                case SerializedObjectTypes.Enum:
                    {
                        IEnumInfo enumInfo = type.GetEnumInfo();
                        Type numericType = enumInfo.NumericType;
                        return (stream) =>
                        {
                            T? res = stream.ReadNumericNullable(version, numericType, options.Buffer) is T value ? value : default(T?);
                            if (res is not null && !enumInfo.IsValidValue(res))
                                throw new InvalidDataException($"Deserialized invalid {type} value of \"{res}\"");
                            return res;
                        };
                    }
                case SerializedObjectTypes.Enumerable:
                    return static (stream) => default;//TODO Read enumerable
                case SerializedObjectTypes.Json:
                    {
                        StreamExtensions.JsonReadingOptions jsonOptions = options as StreamExtensions.JsonReadingOptions ?? options.JsonItemOptions;
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeReadingOptions typeOptions = options as StreamExtensions.TypeReadingOptions ?? options.TypeItemOptions;
                        return type.IsFinalType()
                            ? (stream) => stream.ReadBoolean(version) ? stream.ReadJson<T>(version, jsonOptions) : default(T?)
                            : (stream) => stream.ReadTypeNullable(version, typeOptions) is Type valueType
                                ? stream.ReadJson(version, valueType, jsonOptions).CastType<T>()
                                : default(T?);
                    }
                default:
                    throw new InvalidProgramException($"Failed to determine a valid serialized object type for {typeof(T)} (got {objType})");
            }
        }
    }
}
