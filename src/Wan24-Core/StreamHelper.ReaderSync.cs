﻿using System.Collections;

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
        /// <param name="useInterfaces">If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="useItemInterfaces">If to use supported interfaces for items (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="buffer">Buffer (required, if <c>objType</c> is (going to be) <see cref="SerializedObjectTypes.SerializeBinary"/> or 
        /// <see cref="SerializedObjectTypes.Json"/>)</param>
        /// <param name="stringBuffer">String buffer (required, if <c>objType</c> is (going to be) <see cref="SerializedObjectTypes.String"/> or 
        /// <see cref="SerializedObjectTypes.SerializeString"/>)</param>
        /// <param name="minLen">Minimum length (used for string reading)</param>
        /// <param name="maxLen">Maximum length (required, if <c>objType</c> is (going to be) <see cref="SerializedObjectTypes.Array"/> or 
        /// <see cref="SerializedObjectTypes.Dictionary"/>)</param>
        /// <returns>Reader</returns>
        public static Func<Stream, T> GetReader<T>(
            int version,
            SerializedObjectTypes objType = SerializedObjectTypes.None,
            in bool? useInterfaces = null,
            bool? useItemInterfaces = null,
            RentedMemory<byte>? buffer = null,
            RentedMemory<char>? stringBuffer = null,
            int? minLen = null,
            int? maxLen = null
            )
        {
            Type type = typeof(T);
            if (objType == SerializedObjectTypes.None) objType = type.GetSerializedType(useInterfaces ?? type.IsInterface);
            if ((objType == SerializedObjectTypes.Json && type.IsInterface) || type == typeof(object))
            {
                if (!StreamExtensions.AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
                return static (stream) => default;//TODO Read any object
            }
            switch (objType)
            {
                case SerializedObjectTypes.Numeric:
                    return (stream) => stream.ReadNumeric(version, type).CastType<T>();
                case SerializedObjectTypes.String:
                    if (!stringBuffer.HasValue) throw new ArgumentNullException(nameof(stringBuffer));
                    return (stream) =>
                    {
                        Span<char> bufferSpan = stringBuffer.Value.Memory.Span;
                        return new string(bufferSpan[..stream.ReadString(version, bufferSpan, minLen ?? 0)]).CastType<T>();
                    };
                case SerializedObjectTypes.Boolean:
                    return (stream) => stream.ReadBoolean(version).CastType<T>();
                case SerializedObjectTypes.Type:
                    return (stream) => stream.ReadType(version).CastType<T>();
                case SerializedObjectTypes.Array:
                    if (!maxLen.HasValue) throw new ArgumentNullException(nameof(maxLen));
                    if (type.FindGenericType(typeof(IList<>)) is Type genericListType)
                    {
                        dynamic dummy = GenericHelper.GetDefault(TypeInfoExt.From(genericListType).FirstGenericArgument ?? throw new InvalidProgramException());
                        return (stream) => StreamExtensions.ReadGenericList(stream, version, maxLen.Value, dummy: dummy);
                    }
                    else
                    {
                        return (stream) => stream.ReadList(version, maxLen.Value).CastType<T>();
                    }
                case SerializedObjectTypes.Dictionary:
                    return type.FindGenericType(typeof(IDictionary<,>)) is null
                        ? (stream) => default
                        : (stream) => default;//TODO Read dictionary value
                case SerializedObjectTypes.Stream:
                    return (stream) => stream.ReadStream(version).CastType<T>();
                case SerializedObjectTypes.Serializable:
                    return type.IsFinalType()
                        ? (stream) => type.DeserializeFrom(stream, version).CastType<T>()
                        : (stream) => stream.ReadType(version).DeserializeFrom(stream, version).CastType<T>();
                case SerializedObjectTypes.SerializeBinary:
                    if (!buffer.HasValue) throw new ArgumentNullException(nameof(buffer));
                    {
                        int? fixedStructureSize = type.GetMaxStructureSize();
                        return type.IsFinalType()
                            ? fixedStructureSize.HasValue
                                ? (stream) =>
                                {
                                    // Final type with a fixed length
                                    Span<byte> bufferSpan = buffer.Value.Memory.Span[..fixedStructureSize.Value];
                                    stream.ReadExactly(bufferSpan);
                                    return type.DeserializeFrom(bufferSpan).CastType<T>();
                                }
                                : (stream) =>
                                {
                                    // Final type with a dynamic length
                                    Span<byte> bufferSpan = buffer.Value.Memory.Span;
                                    int len = stream.ReadDataWithLengthInfo(version, bufferSpan);
                                    return type.DeserializeFrom(bufferSpan[..len]).CastType<T>();
                                }
                            : fixedStructureSize.HasValue && type.CanConstruct()
                                ? (stream) =>
                                {
                                    // Constructable type with a fixed length
                                    Type valueType = stream.ReadType(version);
                                    Span<byte> bufferSpan = buffer.Value.Memory.Span[..fixedStructureSize.Value];
                                    stream.ReadExactly(bufferSpan);
                                    return valueType.DeserializeFrom(bufferSpan).CastType<T>();
                                }
                                : (stream) =>
                                {
                                    // Type with a dynamic length
                                    Type valueType = stream.ReadType(version);
                                    Span<byte> bufferSpan = buffer.Value.Memory.Span;
                                    int len = stream.ReadDataWithLengthInfo(version, bufferSpan);
                                    return valueType.DeserializeFrom(bufferSpan[..len]).CastType<T>();
                                };
                    }
                case SerializedObjectTypes.SerializeString:
                    if (!stringBuffer.HasValue) throw new ArgumentNullException(nameof(stringBuffer));
                    return type.IsFinalType()
                        ? (stream) =>
                        {
                            Span<char> bufferSpan = stringBuffer.Value.Memory.Span;
                            int len = stream.ReadString(version, bufferSpan, minLen ?? 0);
                            return type.ParseObject(bufferSpan).CastType<T>();
                        }
                        : (stream) =>
                        {
                            Type valueType = stream.ReadType(version);
                            Span<char> bufferSpan = stringBuffer.Value.Memory.Span;
                            int len = stream.ReadString(version, bufferSpan, minLen ?? 0);
                            return valueType.ParseObject(bufferSpan).CastType<T>();
                        };
                case SerializedObjectTypes.Enum:
                    {
                        IEnumInfo enumInfo = type.GetEnumInfo();
                        Type numericType = enumInfo.NumericType;
                        return (stream) =>
                        {
                            T res = stream.ReadNumeric(version, numericType).CastType<T>();
                            if (!enumInfo.IsValidValue(res)) throw new InvalidDataException($"Deserialized invalid {type} value of \"{res}\"");
                            return res;
                        };
                    }
                case SerializedObjectTypes.Enumerable:
                    return static (stream) => default;//TODO Read enumerable
                case SerializedObjectTypes.Json:
                    if (!buffer.HasValue) throw new ArgumentNullException(nameof(buffer));
                    return type.IsFinalType()
                        ? (stream) => stream.ReadJson<T>(version, buffer.Value.Memory.Span)
                        : (stream) => stream.ReadJson(version, stream.ReadType(version), buffer.Value.Memory.Span).CastType<T>();
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
        /// <param name="useInterfaces">If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="useItemInterfaces">If to use supported interfaces for items (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="buffer">Buffer (required, if <c>objType</c> is (going to be) <see cref="SerializedObjectTypes.SerializeBinary"/> or 
        /// <see cref="SerializedObjectTypes.Json"/>)</param>
        /// <param name="stringBuffer">String buffer (required, if <c>objType</c> is (going to be) <see cref="SerializedObjectTypes.String"/> or 
        /// <see cref="SerializedObjectTypes.SerializeString"/>)</param>
        /// <param name="minLen">Minimum length (used for string reading)</param>
        /// <param name="maxLen">Maximum length (required, if <c>objType</c> is (going to be) <see cref="SerializedObjectTypes.Array"/> or 
        /// <see cref="SerializedObjectTypes.Dictionary"/>)</param>
        /// <returns>Reader</returns>
        public static Func<Stream, T?> GetNullableReader<T>(
            int version,
            SerializedObjectTypes objType = SerializedObjectTypes.None,
            in bool? useInterfaces = null,
            bool? useItemInterfaces = null,
            RentedMemory<byte>? buffer = null,
            RentedMemory<char>? stringBuffer = null,
            int? minLen = null,
            int? maxLen = null
            )
        {
            Type type = typeof(T);
            if (objType == SerializedObjectTypes.None) objType = type.GetSerializedType(useInterfaces ?? type.IsInterface);
            if ((objType == SerializedObjectTypes.Json && type.IsInterface) || type == typeof(object))
            {
                if (!StreamExtensions.AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
                return static (stream) => default;//TODO Read any object
            }
            switch (objType)
            {
                case SerializedObjectTypes.Numeric:
                    return (stream) => stream.ReadNumericNullable(version, type) is T res ? res : default(T?);
                case SerializedObjectTypes.String:
                    if (!stringBuffer.HasValue) throw new ArgumentNullException(nameof(stringBuffer));
                    return (stream) =>
                    {
                        Span<char> bufferSpan = stringBuffer.Value.Memory.Span;
                        int len = stream.ReadStringNullable(version, bufferSpan, minLen ?? 0);
                        return len < 0 ? default(T?) : new string(bufferSpan[..len]).CastType<T>();
                    };
                case SerializedObjectTypes.Boolean:
                    return (stream) => stream.ReadBooleanNullable(version) is T res ? res : default(T?);
                case SerializedObjectTypes.Type:
                    return (stream) => stream.ReadTypeNullable(version) is T res ? res : default(T?);
                case SerializedObjectTypes.Array:
                    if (!maxLen.HasValue) throw new ArgumentNullException(nameof(maxLen));
                    if (type.FindGenericType(typeof(IList<>)) is Type genericListType)
                    {
                        dynamic dummy = GenericHelper.GetDefault(TypeInfoExt.From(genericListType).FirstGenericArgument ?? throw new InvalidProgramException());
                        return (stream) => StreamExtensions.ReadGenericListNullable(stream, version, maxLen.Value, dummy: dummy) is T res ? res : default(T?);
                    }
                    else
                    {
                        return (stream) => stream.ReadListNullable(version, maxLen.Value) is T res ? res : default(T?);
                    }
                case SerializedObjectTypes.Dictionary:
                    return type.FindGenericType(typeof(IDictionary<,>)) is null
                        ? (stream) => default
                        : (stream) => default;//TODO Read dictionary value
                case SerializedObjectTypes.Stream:
                    return (stream) => stream.ReadStreamNullable(version) is T res ? res : default(T?);
                case SerializedObjectTypes.Serializable:
                    return type.IsFinalType()
                        ? (stream) => stream.ReadBoolean(version) ? type.DeserializeFrom(stream, version).CastType<T>() : default(T?)
                        : (stream) => stream.ReadTypeNullable(version)?.DeserializeFrom(stream, version) is T res ? res : default(T?);
                case SerializedObjectTypes.SerializeBinary:
                    if (!buffer.HasValue) throw new ArgumentNullException(nameof(buffer));
                    {
                        int? fixedStructureSize = type.GetMaxStructureSize();
                        return type.IsFinalType()
                            ? fixedStructureSize.HasValue
                                ? (stream) =>
                                {
                                    // Final type with a fixed length
                                    if (!stream.ReadBoolean(version)) return default(T?);
                                    Span<byte> bufferSpan = buffer.Value.Memory.Span[..fixedStructureSize.Value];
                                    stream.ReadExactly(bufferSpan);
                                    return type.DeserializeFrom(bufferSpan).CastType<T>();
                                }
                                : (stream) =>
                                {
                                    // Final type with a dynamic length
                                    Span<byte> bufferSpan = buffer.Value.Memory.Span;
                                    int len = stream.ReadDataNullableWithLengthInfo(version, bufferSpan);
                                    return len < 0 ? default(T?) : type.DeserializeFrom(bufferSpan[..len]).CastType<T>();
                                }
                            : fixedStructureSize.HasValue && type.CanConstruct()
                                ? (stream) =>
                                {
                                    // Constructable type with a fixed length
                                    Type? valueType = stream.ReadTypeNullable(version);
                                    if (valueType is null) return default(T?);
                                    Span<byte> bufferSpan = buffer.Value.Memory.Span[..fixedStructureSize.Value];
                                    stream.ReadExactly(bufferSpan);
                                    return valueType.DeserializeFrom(bufferSpan).CastType<T>();
                                }
                                : (stream) =>
                                {
                                    // Type with a dynamic length
                                    Type? valueType = stream.ReadTypeNullable(version);
                                    if (valueType is null) return default(T?);
                                    Span<byte> bufferSpan = buffer.Value.Memory.Span;
                                    int len = stream.ReadDataWithLengthInfo(version, bufferSpan);
                                    return valueType.DeserializeFrom(bufferSpan[..len]).CastType<T>();
                                };
                    }
                case SerializedObjectTypes.SerializeString:
                    if (!stringBuffer.HasValue) throw new ArgumentNullException(nameof(stringBuffer));
                    return type.IsFinalType()
                        ? (stream) =>
                        {
                            Span<char> bufferSpan = stringBuffer.Value.Memory.Span;
                            int len = stream.ReadStringNullable(version, bufferSpan, minLen ?? 0);
                            return len < 0 ? default(T?) : type.ParseObject(bufferSpan).CastType<T>();
                        }
                        : (stream) =>
                        {
                            Type? valueType = stream.ReadTypeNullable(version);
                            if (valueType is null) return default(T?);
                            Span<char> bufferSpan = stringBuffer.Value.Memory.Span;
                            int len = stream.ReadString(version, bufferSpan, minLen ?? 0);
                            return valueType.ParseObject(bufferSpan).CastType<T>();
                    };
                case SerializedObjectTypes.Enum:
                    {
                        IEnumInfo enumInfo = type.GetEnumInfo();
                        Type numericType = enumInfo.NumericType;
                        return (stream) =>
                        {
                            dynamic? numericValue = stream.ReadNumericNullable(version, numericType);
                            if (numericValue is null) return default(T?);
                            T res = numericValue.CastType<T>();
                            if (!enumInfo.IsValidValue(res)) throw new InvalidDataException($"Deserialized invalid {type} value of \"{res}\"");
                            return res;
                        };
                    }
                case SerializedObjectTypes.Enumerable:
                    return static (stream) => default;//TODO Read enumerable
                case SerializedObjectTypes.Json:
                    if (!buffer.HasValue) throw new ArgumentNullException(nameof(buffer));
                    return type.IsFinalType()
                        ? (stream) => stream.ReadJsonNullable<T>(version, buffer.Value.Memory.Span)
                        : (stream) => stream.ReadTypeNullable(version) is Type valueType
                            ? stream.ReadJson(version, valueType, buffer.Value.Memory.Span).CastType<T>()
                            : default(T?);
                default:
                    throw new InvalidProgramException($"Failed to determine a valid serialized object type for {typeof(T)} (got {objType})");
            }
        }
    }
}
