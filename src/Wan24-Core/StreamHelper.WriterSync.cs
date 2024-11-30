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
        /// <param name="useInterfaces">If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="useItemInterfaces">If to use supported interfaces for items (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="buffer">Buffer (required, if <c>objType</c> is (going to be) <see cref="SerializedObjectTypes.SerializeBinary"/>)</param>
        /// <returns>Writer</returns>
        public static Action<Stream, T> GetWriter<T>(
            SerializedObjectTypes objType = SerializedObjectTypes.None,
            in bool? useInterfaces = null,
            bool? useItemInterfaces = null,
            RentedMemory<byte>? buffer = null
            )
        {
            Type type = typeof(T);
            if (objType == SerializedObjectTypes.None) objType = type.GetSerializedType(useInterfaces ?? type.IsInterface);
            if ((objType == SerializedObjectTypes.Json && type.IsInterface) || type == typeof(object))
                return static (stream, item) => { };//TODO Write any object
            switch (objType)
            {
                case SerializedObjectTypes.Numeric:
                    return static (stream, value) => StreamExtensions.WriteNumeric(stream, (dynamic)value!);
                case SerializedObjectTypes.String:
                    return static (stream, value) => stream.Write(value!.CastType<string>());
                case SerializedObjectTypes.Boolean:
                    return static (stream, value) => stream.Write(value!.CastType<bool>());
                case SerializedObjectTypes.Type:
                    return static (stream, value) => stream.Write(value!.CastType<Type>());
                case SerializedObjectTypes.Array:
                    return type.FindGenericType(typeof(IList<>)) is null
                        ? (stream, value) => stream.Write(value!.CastType<IList>(), useItemInterfaces)
                        : (stream, value) => StreamExtensions.WriteList(stream, (dynamic)value!, useItemInterfaces);
                case SerializedObjectTypes.Dictionary:
                    return type.FindGenericType(typeof(IDictionary<,>)) is null
                        ? static (stream, value) => { }
                    : static (stream, value) => { };//TODO Write dictionary value
                case SerializedObjectTypes.Stream:
                    return static (stream, value) => stream.Write(value!.CastType<Stream>());
                case SerializedObjectTypes.Serializable:
                    return type.IsFinalType()
                        ? static (stream, value) => ((ISerializeStream)value!).SerializeTo(stream)
                        : static (stream, value) =>
                        {
                            stream.Write(value!.GetType());
                            ((ISerializeStream)value).SerializeTo(stream);
                        };
                case SerializedObjectTypes.SerializeBinary:
                    if (!buffer.HasValue) throw new ArgumentNullException(nameof(buffer));
                    return type.IsFinalType()
                        ? type.GetIsFixedStructureSize()
                            ? (stream, value) =>
                            {
                                // Final type with a fixed length
                                Span<byte> bufferSpan = buffer.Value.Memory.Span;
                                ((ISerializeBinary)value!).GetBytes(bufferSpan);
                                stream.Write(bufferSpan);
                            }
                    : (stream, value) =>
                    {
                        // Final type with a dynamic length
                        Span<byte> bufferSpan = buffer.Value.Memory.Span;
                        stream.WriteWithLengthInfo(bufferSpan[..((ISerializeBinary)value!).GetBytes(bufferSpan)]); ;
                    }
                    : type.GetIsFixedStructureSize() && type.CanConstruct()
                            ? (stream, value) =>
                            {
                                // Constructable type with a fixed length
                                Span<byte> bufferSpan = buffer.Value.Memory.Span;
                                stream.Write(value!.GetType());
                                ((ISerializeBinary)value).GetBytes(bufferSpan);
                                stream.Write(bufferSpan);
                            }
                    : (stream, value) =>
                    {
                        // Type with a dynamic length
                        Span<byte> bufferSpan = buffer.Value.Memory.Span;
                        stream.Write(value!.GetType());
                        stream.WriteWithLengthInfo(bufferSpan[..((ISerializeBinary)value).GetBytes(bufferSpan)]);
                    };
                case SerializedObjectTypes.SerializeString:
                    return type.IsFinalType()
                        ? static (stream, value) => stream.Write(value!.ToString() ?? string.Empty)
                        : static (stream, value) =>
                        {
                            stream.Write(value!.GetType());
                            stream.Write(value.ToString() ?? string.Empty);
                        };
                case SerializedObjectTypes.Enumerable:
                    {
                        Type numericType = type.GetEnumUnderlyingType();
                        return (stream, value) => StreamExtensions.WriteNumeric(stream, (dynamic)Convert.ChangeType(value!, numericType));
                    }
                case SerializedObjectTypes.Json:
                    return type.IsFinalType()
                        ? static (stream, value) => stream.WriteJson(value)
                        : static (stream, value) =>
                        {
                            stream.Write(value!.GetType());
                            stream.WriteJson(value);
                        };
                default:
                    throw new InvalidProgramException($"Failed to determine a valid serialized object type for {typeof(T)} (got {objType})");
            }
        }

        /// <summary>
        /// Get a writer for a nullable object type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="objType">Serialized object type (if <see cref="SerializedObjectTypes.None"/>, it'll be determined from the given object type <c>T</c>)</param>
        /// <param name="useInterfaces">If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="useItemInterfaces">If to use supported interfaces for items (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="buffer">Buffer (required, if <c>objType</c> is (going to be) <see cref="SerializedObjectTypes.SerializeBinary"/>)</param>
        /// <returns>Writer</returns>
        public static Action<Stream, T?> GetNullableWriter<T>(
            SerializedObjectTypes objType = SerializedObjectTypes.None,
            in bool? useInterfaces = null,
            bool? useItemInterfaces = null,
            RentedMemory<byte>? buffer = null
            )
        {
            Type type = typeof(T);
            if (objType == SerializedObjectTypes.None) objType = type.GetSerializedType(useInterfaces ?? type.IsInterface);
            if ((objType == SerializedObjectTypes.Json && type.IsInterface) || type == typeof(object))
                return static (stream, item) => { };//TODO Write any object
            switch (objType)
            {
                case SerializedObjectTypes.Numeric:
                    return static (stream, value) => StreamExtensions.WriteNumericNullable(stream, (dynamic)value!);
                case SerializedObjectTypes.String:
                    return static (stream, value) => stream.Write(value?.CastType<string>());
                case SerializedObjectTypes.Boolean:
                    return static (stream, value) => stream.Write(value?.CastType<bool>());
                case SerializedObjectTypes.Type:
                    return static (stream, value) => stream.WriteNullable(value?.CastType<Type>());
                case SerializedObjectTypes.Array:
                    return type.FindGenericType(typeof(IList<>)) is null
                        ? (stream, value) => stream.WriteNullable(value?.CastType<IList>(), useItemInterfaces)
                        : (stream, value) => StreamExtensions.WriteListNullable(stream, (dynamic?)value, useItemInterfaces);
                case SerializedObjectTypes.Dictionary:
                    return type.FindGenericType(typeof(IDictionary<,>)) is null
                        ? static (stream, value) => { }
                    : static (stream, value) => { };//TODO Write dictionary value
                case SerializedObjectTypes.Stream:
                    return static (stream, value) => stream.WriteNullable(value?.CastType<Stream>());
                case SerializedObjectTypes.Serializable:
                    return type.IsFinalType()
                        ? static (stream, value) =>
                        {
                            stream.Write(value is not null);
                            ((ISerializeStream?)value)?.SerializeTo(stream);
                        }
                    : static (stream, value) =>
                    {
                        stream.WriteNullable(value?.GetType());
                        ((ISerializeStream?)value)?.SerializeTo(stream);
                    };
                case SerializedObjectTypes.SerializeBinary:
                    if (!buffer.HasValue) throw new ArgumentNullException(nameof(buffer));
                    return type.IsFinalType()
                        ? type.GetIsFixedStructureSize()
                            ? (stream, value) =>
                            {
                                // Final type with a fixed length
                                stream.Write(value is not null);
                                if (value is null) return;
                                Span<byte> bufferSpan = buffer.Value.Memory.Span;
                                ((ISerializeBinary)value!).GetBytes(bufferSpan);
                                stream.Write(bufferSpan);
                            }
                    : (stream, value) =>
                    {
                        // Final type with a dynamic length
                        Memory<byte> bufferMem = buffer.Value.Memory;
                        stream.WriteNullableWithLengthInfo(value is null ? null : bufferMem[..((ISerializeBinary)value).GetBytes(bufferMem.Span)]);
                    }
                    : type.GetIsFixedStructureSize() && type.CanConstruct()
                            ? (stream, value) =>
                            {
                                // Constructable type with a fixed length
                                stream.WriteNullable(value?.GetType());
                                if (value is null) return;
                                Span<byte> bufferSpan = buffer.Value.Memory.Span;
                                ((ISerializeBinary)value).GetBytes(bufferSpan);
                                stream.Write(bufferSpan);
                            }
                    : (stream, value) =>
                    {
                        // Type with a dynamic length
                        stream.WriteNullable(value?.GetType());
                        if (value is null) return;
                        Span<byte> bufferSpan = buffer.Value.Memory.Span;
                        stream.WriteWithLengthInfo(bufferSpan[..((ISerializeBinary)value).GetBytes(bufferSpan)]);
                    };
                case SerializedObjectTypes.SerializeString:
                    return type.IsFinalType()
                        ? static (stream, value) => stream.Write(value?.ToString())
                        : static (stream, value) =>
                        {
                            stream.WriteNullable(value?.GetType());
                            if (value is null) return;
                            stream.Write(value.ToString() ?? string.Empty);
                        };
                case SerializedObjectTypes.Enumerable:
                    {
                        Type numericType = type.GetEnumUnderlyingType();
                        return (stream, value) =>
                        {
                            if (value is null)
                            {
                                stream.Write((byte)NumericTypes.None);
                            }
                            else
                            {
                                StreamExtensions.WriteNumericNullable(stream, (dynamic)Convert.ChangeType(value, numericType));
                            }
                        };
                    }
                case SerializedObjectTypes.Json:
                    return type.IsFinalType()
                        ? static (stream, value) => stream.WriteJsonNullable(value)
                        : static (stream, value) =>
                        {
                            stream.WriteNullable(value?.GetType());
                            if (value is null) return;
                            stream.WriteJson(value);
                        };
                default:
                    throw new InvalidProgramException($"Failed to determine a valid serialized object type for {typeof(T)} (got {objType})");
            }
        }
    }
}
