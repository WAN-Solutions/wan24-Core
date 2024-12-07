using System.Collections;

namespace wan24.Core
{
    // Synchronous writer
    public static partial class StreamHelper
    {
        /// <summary>
        /// Get a writer for an object type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="objType">Serialized object type (if <see cref="SerializedObjectTypes.None"/>, it'll be determined from the given object type <c>T</c>)</param>
        /// <param name="options">Options</param>
        /// <returns>Writer</returns>
        public static Action<Stream, T> GetWriter<T>(
            SerializedObjectTypes objType = SerializedObjectTypes.None,
            StreamExtensions.WritingOptions? options = null
            )
        {
            options = StreamExtensions.WritingOptions.DefaultOptions;
            Type type = typeof(T);
            if (objType == SerializedObjectTypes.None) objType = type.GetSerializedType(options.UseInterfaces ?? type.IsInterface);
            if ((objType == SerializedObjectTypes.Json && type.IsInterface) || type == typeof(object))
                return static (stream, item) => { };//TODO Write any object
            switch (objType)
            {
                case SerializedObjectTypes.Numeric:
                    return static (stream, value) => StreamExtensions.WriteNumeric(stream, value!);
                case SerializedObjectTypes.String:
                    {
                        StreamExtensions.StringWritingOptions stringOptions = options as StreamExtensions.StringWritingOptions ?? options.StringItemOptions;
                        return (stream, value) => stream.Write(value!.CastType<string>(), stringOptions);
                    }
                case SerializedObjectTypes.Boolean:
                    return static (stream, value) => stream.Write(value!.CastType<bool>());
                case SerializedObjectTypes.Type:
                    {
                        StreamExtensions.TypeWritingOptions typeOptions = options as StreamExtensions.TypeWritingOptions ?? options.TypeItemOptions;
                        return (stream, value) => stream.Write(value!.CastType<Type>(), typeOptions);
                    }
                case SerializedObjectTypes.Array:
                    {
                        StreamExtensions.ListWritingOptions itemOptions = options as StreamExtensions.ListWritingOptions ?? options.ListItemOptions;
                        return type.FindGenericType(typeof(IList<>)) is null
                            ? (stream, value) => stream.Write(value!.CastType<IList>(), itemOptions)
                            : (stream, value) => StreamExtensions.WriteList(stream, (dynamic)value!, itemOptions);
                    }
                case SerializedObjectTypes.Dictionary:
                    return type.FindGenericType(typeof(IDictionary<,>)) is null
                        ? static (stream, value) => { }
                        : static (stream, value) => { };//TODO Write dictionary value
                case SerializedObjectTypes.Stream:
                    return static (stream, value) => stream.Write(value!.CastType<Stream>());
                case SerializedObjectTypes.Serializable:
                    {
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeWritingOptions? typeOptions = isFinalType
                            ? null
                            : options as StreamExtensions.TypeWritingOptions ?? options.TypeItemOptions;
                        return isFinalType
                            ? static (stream, value) => ((ISerializeStream)value!).SerializeTo(stream)
                            : (stream, value) =>
                            {
                                stream.Write(value!.GetType(), typeOptions);
                                ((ISerializeStream)value).SerializeTo(stream);
                            };
                    }
                case SerializedObjectTypes.SerializeBinary:
                    if (!options.Buffer.HasValue) throw new ArgumentNullException(nameof(options), "Buffer required for writing binary serialized");
                    {
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeWritingOptions? typeOptions = isFinalType
                            ? null
                            : options as StreamExtensions.TypeWritingOptions ?? options.TypeItemOptions;
                        return isFinalType
                            ? type.GetIsFixedStructureSize()
                                ? (stream, value) =>
                                {
                                    // Final type with a fixed length
                                    Span<byte> bufferSpan = options.Buffer.Value.Span;
                                    ((ISerializeBinary)value!).GetBytes(bufferSpan);
                                    stream.Write(bufferSpan);
                                }
                                : (stream, value) =>
                                {
                                    // Final type with a dynamic length
                                    Span<byte> bufferSpan = options.Buffer.Value.Span;
                                    stream.WriteWithLengthInfo(bufferSpan[..((ISerializeBinary)value!).GetBytes(bufferSpan)]); ;
                                }
                            : type.GetIsFixedStructureSize() && type.CanConstruct()
                                ? (stream, value) =>
                                {
                                    // Constructable type with a fixed length
                                    Span<byte> bufferSpan = options.Buffer.Value.Span;
                                    stream.Write(value!.GetType(), typeOptions);
                                    ((ISerializeBinary)value).GetBytes(bufferSpan);
                                    stream.Write(bufferSpan);
                                }
                                : (stream, value) =>
                                {
                                    // Type with a dynamic length
                                    Span<byte> bufferSpan = options.Buffer.Value.Span;
                                    stream.Write(value!.GetType(), typeOptions);
                                    stream.WriteWithLengthInfo(bufferSpan[..((ISerializeBinary)value).GetBytes(bufferSpan)]);
                                };
                    }
                case SerializedObjectTypes.SerializeString:
                    {
                        StreamExtensions.StringWritingOptions stringOptions = options as StreamExtensions.StringWritingOptions ?? options.StringItemOptions;
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeWritingOptions? typeOptions = isFinalType
                            ? null
                            : options as StreamExtensions.TypeWritingOptions ?? options.TypeItemOptions;
                        return type.IsFinalType()
                            ? (stream, value) => stream.Write(value!.ToString() ?? string.Empty, stringOptions)
                            : (stream, value) =>
                            {
                                stream.Write(value!.GetType(), typeOptions);
                                stream.Write(value.ToString() ?? string.Empty, stringOptions);
                            };
                    }
                case SerializedObjectTypes.Enumerable:
                    return static (stream, item) => { };//TODO Write enumerable
                case SerializedObjectTypes.Enum:
                    {
                        Type numericType = type.GetEnumUnderlyingType();
                        return (stream, value) => StreamExtensions.WriteNumeric(stream, (dynamic)Convert.ChangeType(value!, numericType));
                    }
                case SerializedObjectTypes.Json:
                    {
                        StreamExtensions.JsonWritingOptions jsonOptions = options as StreamExtensions.JsonWritingOptions ?? options.JsonItemOptions;
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeWritingOptions? typeOptions = isFinalType
                            ? null
                            : options as StreamExtensions.TypeWritingOptions ?? options.TypeItemOptions;
                        return type.IsFinalType()
                            ? (stream, value) => stream.WriteJson(value, jsonOptions)
                            : (stream, value) =>
                            {
                                stream.Write(value!.GetType(), typeOptions);
                                stream.WriteJson(value, jsonOptions);
                            };
                    }
                default:
                    throw new InvalidProgramException($"Failed to determine a valid serialized object type for {typeof(T)} (got {objType})");
            }
        }

        /// <summary>
        /// Get a writer for a nullable object type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="objType">Serialized object type (if <see cref="SerializedObjectTypes.None"/>, it'll be determined from the given object type <c>T</c>)</param>
        /// <param name="options">Options</param>
        /// <returns>Writer</returns>
        public static Action<Stream, T?> GetNullableWriter<T>(
            SerializedObjectTypes objType = SerializedObjectTypes.None,
            StreamExtensions.WritingOptions? options = null
            )
        {
            options ??= StreamExtensions.WritingOptions.DefaultOptions;
            Type type = typeof(T);
            if (objType == SerializedObjectTypes.None) objType = type.GetSerializedType(options.UseInterfaces ?? type.IsInterface);
            if ((objType == SerializedObjectTypes.Json && type.IsInterface) || type == typeof(object))
                return static (stream, item) => { };//TODO Write any object
            switch (objType)
            {
                case SerializedObjectTypes.Numeric:
                    return static (stream, value) => StreamExtensions.WriteNumericNullable(stream, value!);
                case SerializedObjectTypes.String:
                    {
                        StreamExtensions.StringWritingOptions stringOptions = options as StreamExtensions.StringWritingOptions ?? options.StringItemOptions;
                        return (stream, value) => stream.Write(value is string v ? v : null, stringOptions);
                    }
                case SerializedObjectTypes.Boolean:
                    return static (stream, value) => stream.Write(value is bool v ? v : null);
                case SerializedObjectTypes.Type:
                    {
                        StreamExtensions.TypeWritingOptions typeOptions = options as StreamExtensions.TypeWritingOptions ?? options.TypeItemOptions;
                        return (stream, value) => stream.WriteNullable(value is Type v ? v : null, typeOptions);
                    }
                case SerializedObjectTypes.Array:
                    {
                        StreamExtensions.ListWritingOptions itemOptions = options as StreamExtensions.ListWritingOptions ?? options.ListItemOptions;
                        return type.FindGenericType(typeof(IList<>)) is null
                            ? (stream, value) => stream.WriteNullable(value is IList v ? v : null, itemOptions)
                            : (stream, value) => StreamExtensions.WriteListNullable(stream, (dynamic?)value, itemOptions);
                    }
                case SerializedObjectTypes.Dictionary:
                    return type.FindGenericType(typeof(IDictionary<,>)) is null
                        ? static (stream, value) => { }
                        : static (stream, value) => { };//TODO Write dictionary value
                case SerializedObjectTypes.Stream:
                    return static (stream, value) => stream.WriteNullable(value is Stream v ? v : null);
                case SerializedObjectTypes.Serializable:
                    {
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeWritingOptions? typeOptions = isFinalType
                            ? null
                            : options as StreamExtensions.TypeWritingOptions ?? options.TypeItemOptions;
                        return isFinalType
                            ? static (stream, value) =>
                            {
                                stream.Write(value is not null);
                                if (value is null) return;
                                ((ISerializeStream)value!).SerializeTo(stream);
                            }
                            : (stream, value) =>
                            {
                                stream.WriteNullable(value?.GetType(), typeOptions);
                                if (value is null) return;
                                ((ISerializeStream)value).SerializeTo(stream);
                            };
                    }
                case SerializedObjectTypes.SerializeBinary:
                    if (!options.Buffer.HasValue) throw new ArgumentNullException(nameof(options), "Buffer required for writing binary serialized");
                    {
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeWritingOptions? typeOptions = isFinalType
                            ? null
                            : options as StreamExtensions.TypeWritingOptions ?? options.TypeItemOptions;
                        return isFinalType
                            ? type.GetIsFixedStructureSize()
                                ? (stream, value) =>
                                {
                                    // Final type with a fixed length
                                    stream.Write(value is not null);
                                    if (value is null) return;
                                    Span<byte> bufferSpan = options.Buffer.Value.Span;
                                    ((ISerializeBinary)value).GetBytes(bufferSpan);
                                    stream.Write(bufferSpan);
                                }
                                : (stream, value) =>
                                {
                                    // Final type with a dynamic length
                                    if(value is null)
                                    {
                                        stream.WriteNullableWithLengthInfo(data: default(ReadOnlyMemory<byte>?));
                                        return;
                                    }
                                    Span<byte> bufferSpan = options.Buffer.Value.Span;
                                    stream.WriteWithLengthInfo(bufferSpan[..((ISerializeBinary)value).GetBytes(bufferSpan)]); ;
                                }
                            : type.GetIsFixedStructureSize() && type.CanConstruct()
                                ? (stream, value) =>
                                {
                                    // Constructable type with a fixed length
                                    stream.WriteNullable(value?.GetType(), typeOptions);
                                    if (value is null) return;
                                    Span<byte> bufferSpan = options.Buffer.Value.Span;
                                    ((ISerializeBinary)value).GetBytes(bufferSpan);
                                    stream.Write(bufferSpan);
                                }
                                : (stream, value) =>
                                {
                                    // Type with a dynamic length
                                    stream.WriteNullable(value?.GetType(), typeOptions);
                                    if (value is null) return;
                                    Span<byte> bufferSpan = options.Buffer.Value.Span;
                                    stream.WriteWithLengthInfo(bufferSpan[..((ISerializeBinary)value).GetBytes(bufferSpan)]);
                                };
                    }
                case SerializedObjectTypes.SerializeString:
                    {
                        StreamExtensions.StringWritingOptions stringOptions = options as StreamExtensions.StringWritingOptions ?? options.StringItemOptions;
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeWritingOptions? typeOptions = isFinalType
                            ? null
                            : options as StreamExtensions.TypeWritingOptions ?? options.TypeItemOptions;
                        return type.IsFinalType()
                            ? (stream, value) => stream.Write(value is null ? null : value.ToString() ?? string.Empty, stringOptions)
                            : (stream, value) =>
                            {
                                stream.WriteNullable(value?.GetType(), typeOptions);
                                if (value is null) return;
                                stream.Write(value.ToString() ?? string.Empty, stringOptions);
                            };
                    }
                case SerializedObjectTypes.Enumerable:
                    return static (stream, item) => { };//TODO Write enumerable
                case SerializedObjectTypes.Enum:
                    {
                        Type numericType = type.GetEnumUnderlyingType();
                        return (stream, value) => StreamExtensions.WriteNumericNullable(
                            stream, 
                            (dynamic?)(value is null ? default(int?) : Convert.ChangeType(value, numericType))
                            );
                    }
                case SerializedObjectTypes.Json:
                    {
                        StreamExtensions.JsonWritingOptions jsonOptions = options as StreamExtensions.JsonWritingOptions ?? options.JsonItemOptions;
                        bool isFinalType = type.IsFinalType();
                        StreamExtensions.TypeWritingOptions? typeOptions = isFinalType
                            ? null
                            : options as StreamExtensions.TypeWritingOptions ?? options.TypeItemOptions;
                        return type.IsFinalType()
                            ? (stream, value) =>
                            {
                                stream.Write(value is not null);
                                if (value is null) return;
                                stream.WriteJson(value, jsonOptions);
                            }
                            : (stream, value) =>
                            {
                                stream.WriteNullable(value?.GetType(), typeOptions);
                                stream.WriteJson(value, jsonOptions);
                            };
                    }
                default:
                    throw new InvalidProgramException($"Failed to determine a valid serialized object type for {typeof(T)} (got {objType})");
            }
        }
    }
}
