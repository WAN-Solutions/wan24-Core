using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Serialize extensions for <see cref="ISerializeBinary"/>/<see cref="ISerializeBinary{T}"/> and <see cref="ISerializeString"/>/<see cref="ISerializeString{T}"/>
    /// </summary>
    public static class SerializeExtensions
    {
        /// <summary>
        /// <see cref="Deserialize_Delegate"/> (key is the method hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, Deserialize_Delegate> DeserializeDelegates = [];
        /// <summary>
        /// <see cref="Deserialize_Delegate"/> (key is the method hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, TryDeserialize_Delegate> TryDeserializeDelegates = [];
        /// <summary>
        /// <see cref="Deserialize_Delegate"/> (key is the method hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, ParseObject_Delegate> ParseObjectDelegates = [];
        /// <summary>
        /// <see cref="Deserialize_Delegate"/> (key is the method hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, TryParseObject_Delegate> TryParseObjectDelegates = [];

        /// <summary>
        /// Determine if a type can be deserialized using <see cref="DeserializeFrom(Type, in ReadOnlySpan{byte})"/>
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If deserialization is possible</returns>
        public static bool CanDeserialize(this Type type)
            => typeof(ISerializeBinary).IsAssignableFrom(type) && type.GetMethodCached(nameof(ISerializeBinary.TryDeserializeFrom), BindingFlags.Public | BindingFlags.Static) is not null;

        /// <summary>
        /// Determine if a type can be parsed using <see cref="ParseObject(Type, in ReadOnlySpan{char})"/>
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If parsing is possible</returns>
        public static bool CanParse(this Type type)
            => typeof(ISerializeString).IsAssignableFrom(type)
                ? type.GetMethodCached(nameof(ISerializeString.TryParseObject), BindingFlags.Public | BindingFlags.Static) is not null
                : StringValueConverter.CanConvertFromString(type);

        /// <summary>
        /// Deserialize an instance from binary serialized data
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Instance</returns>
        public static object DeserializeFrom(this Type type, in ReadOnlySpan<byte> buffer)
        {
            if (!typeof(ISerializeBinary).IsAssignableFrom(type))
                throw new InvalidOperationException($"{type} isn't an {typeof(ISerializeBinary)}");
            MethodInfoExt mi = type.GetMethodCached(nameof(ISerializeBinary.DeserializeFrom), BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidProgramException($"{type} doesn't implement {typeof(ISerializeBinary)}.{nameof(ISerializeBinary.DeserializeFrom)}");
            if (!DeserializeDelegates.TryGetValue(mi.GetHashCode(), out Deserialize_Delegate? method))
            {
                method = mi.Method.CreateDelegate<Deserialize_Delegate>();
                DeserializeDelegates.TryAdd(mi.GetHashCode(), method);
            }
            return method(buffer) ?? throw new InvalidDataException("NULL deserialized");
        }

        /// <summary>
        /// Deserialize an instance from binary serialized data
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="result">Instance</param>
        /// <returns>If succeed</returns>
        public static bool TryDeserializeFrom(this Type type, in ReadOnlySpan<byte> buffer, [NotNullWhen(returnValue: true)] out object? result)
        {
            result = null;
            if (!typeof(ISerializeBinary).IsAssignableFrom(type))
                return false;
            MethodInfoExt? mi = type.GetMethodCached(nameof(ISerializeBinary.TryDeserializeFrom), BindingFlags.Public | BindingFlags.Static);
            if (mi is null)
                return false;
            try
            {
                if (!TryDeserializeDelegates.TryGetValue(mi.GetHashCode(), out TryDeserialize_Delegate? method))
                {
                    method = mi.Method.CreateDelegate<TryDeserialize_Delegate>();
                    TryDeserializeDelegates.TryAdd(mi.GetHashCode(), method);
                }
                return method(buffer, out result);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deserialize an instance from binary serialized data
        /// </summary>
        /// <typeparam name="T">Type to deserialize</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <returns>Instance</returns>
        public static T DeserializeTypeFrom<T>(this ReadOnlySpan<byte> buffer) where T : ISerializeBinary<T>
            => BinaryGenericHelper<T>.DeserializeTypeMethod(buffer);

        /// <summary>
        /// Deserialize an instance from binary serialized data
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <param name="result">Instance</param>
        /// <returns>If succeed</returns>
        public static bool TryDeserializeTypeFrom<T>(this ReadOnlySpan<byte> buffer, [NotNullWhen(returnValue: true)] out T? result) where T : ISerializeBinary<T>
            => BinaryGenericHelper<T>.TryDeserializeTypeMethod(buffer, out result);

        /// <summary>
        /// Deserialize an instance from binary serialized data
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="str">String</param>
        /// <returns>Instance</returns>
        public static object ParseObject(this Type type, in ReadOnlySpan<char> str)
        {
            if (!typeof(ISerializeString).IsAssignableFrom(type))
            {
                if (StringValueConverter.CanConvertFromString(type) && StringValueConverter.Convert(type, new string(str)) is string res)
                    return res;
                throw new InvalidOperationException($"{type} isn't an {typeof(ISerializeString)}");
            }
            MethodInfoExt mi = type.GetMethodCached(nameof(ISerializeString.ParseObject), BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidProgramException($"{type} doesn't implement {typeof(ISerializeString)}.{nameof(ISerializeString.ParseObject)}");
            if (!ParseObjectDelegates.TryGetValue(mi.GetHashCode(), out ParseObject_Delegate? method))
            {
                method = mi.Method.CreateDelegate<ParseObject_Delegate>();
                ParseObjectDelegates.TryAdd(mi.GetHashCode(), method);
            }
            return method(str) ?? throw new InvalidDataException("NULL deserialized");
        }

        /// <summary>
        /// Deserialize an instance from binary serialized data
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="str">String</param>
        /// <param name="result">Instance</param>
        /// <returns>If succeed</returns>
        public static bool TryParseObject(this Type type, in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out object? result)
        {
            result = null;
            if (!typeof(ISerializeString).IsAssignableFrom(type))
            {
                if (StringValueConverter.CanConvertFromString(type) && StringValueConverter.TryConvertObjectFromString(new string(str), type, out result))
                    return result is not null;
                return false;
            }
            MethodInfoExt? mi = type.GetMethodCached(nameof(ISerializeString.TryParseObject), BindingFlags.Public | BindingFlags.Static);
            if (mi is null)
                return false;
            try
            {
                if (!TryParseObjectDelegates.TryGetValue(mi.GetHashCode(), out TryParseObject_Delegate? method))
                {
                    method = mi.Method.CreateDelegate<TryParseObject_Delegate>();
                    TryParseObjectDelegates.TryAdd(mi.GetHashCode(), method);
                }
                return method(str, out result);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deserialize an instance from binary serialized data
        /// </summary>
        /// <typeparam name="T">Type to deserialize</typeparam>
        /// <param name="str">String</param>
        /// <returns>Instance</returns>
        public static T Parse<T>(this ReadOnlySpan<char> str) where T : ISerializeString<T> => StringGenericHelper<T>.ParseMethod(str);

        /// <summary>
        /// Deserialize an instance from binary serialized data
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="str">String</param>
        /// <param name="result">Instance</param>
        /// <returns>If succeed</returns>
        public static bool TryParse<T>(this ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out T? result) where T : ISerializeString<T>
            => StringGenericHelper<T>.TryParseMethod(str, out result);

        /// <summary>
        /// Deserialize from previously serialized data
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Instance</returns>
        private delegate object Deserialize_Delegate(in ReadOnlySpan<byte> buffer);
        /// <summary>
        /// Deserialize from previously serialized data
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="result">Instance</param>
        /// <returns>If succeed</returns>
        private delegate bool TryDeserialize_Delegate(in ReadOnlySpan<byte> buffer, [NotNullWhen(returnValue: true)] out object? result);
        /// <summary>
        /// Parse from previously serialized string
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Instance</returns>
        private delegate object ParseObject_Delegate(in ReadOnlySpan<char> str);
        /// <summary>
        /// Parse from previously serialized string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="result">Instance</param>
        /// <returns>If succeed</returns>
        private delegate bool TryParseObject_Delegate(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out object? result);

        /// <summary>
        /// Generic binary serialization helper
        /// </summary>
        /// <typeparam name="T">Serializable type</typeparam>
        private static class BinaryGenericHelper<T> where T : ISerializeBinary<T>
        {
            /// <summary>
            /// <see cref="ISerializeBinary{T}.DeserializeTypeFrom(in ReadOnlySpan{byte})"/>
            /// </summary>
            public static readonly DeserializeType_Delegate DeserializeTypeMethod;
            /// <summary>
            /// <see cref="ISerializeBinary{T}.TryDeserializeTypeFrom(in ReadOnlySpan{byte}, out T)"/>
            /// </summary>
            public static readonly TryDeserializeType_Delegate TryDeserializeTypeMethod;

            /// <summary>
            /// Constructor
            /// </summary>
            static BinaryGenericHelper()
            {
                DeserializeTypeMethod = (typeof(ISerializeBinary<T>).GetMethodCached(nameof(ISerializeBinary<T>.DeserializeTypeFrom))
                    ?? throw new InvalidProgramException($"Failed to get {typeof(ISerializeBinary<T>)}.{nameof(ISerializeBinary<T>.DeserializeTypeFrom)} method"))
                    .Method.CreateDelegate<DeserializeType_Delegate>();
                TryDeserializeTypeMethod = (typeof(ISerializeBinary<T>).GetMethodCached(nameof(ISerializeBinary<T>.TryDeserializeTypeFrom))
                    ?? throw new InvalidProgramException($"Failed to get {typeof(ISerializeBinary<T>)}.{nameof(ISerializeBinary<T>.TryDeserializeTypeFrom)} method"))
                    .Method.CreateDelegate<TryDeserializeType_Delegate>();
            }

            /// <summary>
            /// Deserialize from previously serialized data
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <returns>Instance</returns>
            public delegate T DeserializeType_Delegate(in ReadOnlySpan<byte> buffer);
            /// <summary>
            /// Deserialize from previously serialized data
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="result">Instance</param>
            /// <returns>If succeed</returns>
            public delegate bool TryDeserializeType_Delegate(in ReadOnlySpan<byte> buffer, [NotNullWhen(returnValue: true)] out T? result);
        }


        /// <summary>
        /// Generic string serialization helper
        /// </summary>
        /// <typeparam name="T">Serializable type</typeparam>
        private static class StringGenericHelper<T> where T : ISerializeString<T>
        {
            /// <summary>
            /// <see cref="ISerializeString{T}.Parse(in ReadOnlySpan{char})"/>
            /// </summary>
            public static readonly Parse_Delegate ParseMethod;
            /// <summary>
            /// <see cref="ISerializeString{T}.TryParse(in ReadOnlySpan{char}, out T)"/>
            /// </summary>
            public static readonly TryParse_Delegate TryParseMethod;

            /// <summary>
            /// Constructor
            /// </summary>
            static StringGenericHelper()
            {
                ParseMethod = (typeof(ISerializeString<T>).GetMethodCached(nameof(ISerializeString<T>.Parse))
                    ?? throw new InvalidProgramException($"Failed to get {typeof(ISerializeString<T>)}.{nameof(ISerializeString<T>.Parse)} method"))
                    .Method.CreateDelegate<Parse_Delegate>();
                TryParseMethod = (typeof(ISerializeString<T>).GetMethodCached(nameof(ISerializeString<T>.TryParse))
                    ?? throw new InvalidProgramException($"Failed to get {typeof(ISerializeString<T>)}.{nameof(ISerializeString<T>.TryParse)} method"))
                    .Method.CreateDelegate<TryParse_Delegate>();
            }

            /// <summary>
            /// Parse from previously serialized string
            /// </summary>
            /// <param name="str">String</param>
            /// <returns>Instance</returns>
            public delegate T Parse_Delegate(in ReadOnlySpan<char> str);
            /// <summary>
            /// Parse from previously serialized string
            /// </summary>
            /// <param name="str">String</param>
            /// <param name="result">Instance</param>
            /// <returns>If succeed</returns>
            public delegate bool TryParse_Delegate(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out T? result);
        }
    }
}
