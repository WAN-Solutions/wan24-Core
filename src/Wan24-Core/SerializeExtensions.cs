using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Serialize extensions for <see cref="ISerializeBinary"/>/<see cref="ISerializeBinary{T}"/>, <see cref="ISerializeString"/>/<see cref="ISerializeString{T}"/> and 
    /// <see cref="ISerializeStream"/>/<see cref="ISerializeStream{T}"/>
    /// </summary>
    public static class SerializeExtensions
    {
        /// <summary>
        /// <see cref="Deserialize_Delegate"/> (key is the method)
        /// </summary>
        private static readonly ConcurrentDictionary<MethodInfo, Deserialize_Delegate> DeserializeDelegates = [];
        /// <summary>
        /// <see cref="Deserialize_Delegate"/> (key is the method)
        /// </summary>
        private static readonly ConcurrentDictionary<MethodInfo, TryDeserialize_Delegate> TryDeserializeDelegates = [];
        /// <summary>
        /// <see cref="Deserialize_Delegate"/> (key is the method)
        /// </summary>
        private static readonly ConcurrentDictionary<MethodInfo, ParseObject_Delegate> ParseObjectDelegates = [];
        /// <summary>
        /// <see cref="Deserialize_Delegate"/> (key is the method)
        /// </summary>
        private static readonly ConcurrentDictionary<MethodInfo, TryParseObject_Delegate> TryParseObjectDelegates = [];
        /// <summary>
        /// <see cref="DeserializeStream_Delegate"/> (key is the method)
        /// </summary>
        private static readonly ConcurrentDictionary<MethodInfo, DeserializeStream_Delegate> DeserializeStreamDelegates = [];
        /// <summary>
        /// <see cref="DeserializeStreamAsync_Delegate"/> (key is the method)
        /// </summary>
        private static readonly ConcurrentDictionary<MethodInfo, DeserializeStreamAsync_Delegate> DeserializeStreamAsyncDelegates = [];
        /// <summary>
        /// <see cref="DeserializeBuffer_Delegate"/> (key is the method)
        /// </summary>
        private static readonly ConcurrentDictionary<MethodInfo, DeserializeBuffer_Delegate> DeserializeBufferDelegates = [];

        /// <summary>
        /// Deserialize an instance from binary serialized data (see <see cref="ISerializeBinary"/>)
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
            if (!DeserializeDelegates.TryGetValue(mi, out Deserialize_Delegate? method))
            {
                method = mi.Method.CreateDelegate<Deserialize_Delegate>();
                DeserializeDelegates.TryAdd(mi, method);
            }
            return method(buffer) ?? throw new InvalidDataException("NULL deserialized");
        }

        /// <summary>
        /// Deserialize an instance from binary serialized data (see <see cref="ISerializeBinary"/>)
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
                if (!TryDeserializeDelegates.TryGetValue(mi, out TryDeserialize_Delegate? method))
                {
                    method = mi.Method.CreateDelegate<TryDeserialize_Delegate>();
                    TryDeserializeDelegates.TryAdd(mi, method);
                }
                return method(buffer, out result);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deserialize an instance from binary serialized data (see <see cref="ISerializeBinary{T}"/>)
        /// </summary>
        /// <typeparam name="T">Type to deserialize</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T DeserializeTypeFrom<T>(this ReadOnlySpan<byte> buffer) where T : ISerializeBinary<T>
            => ISerializeBinary<T>.DeserializeTypeFrom<T>(buffer);

        /// <summary>
        /// Deserialize an instance from binary serialized data (see <see cref="ISerializeBinary{T}"/>)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <param name="result">Instance</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryDeserializeTypeFrom<T>(this ReadOnlySpan<byte> buffer, [NotNullWhen(returnValue: true)] out T? result) where T : ISerializeBinary<T>
            => ISerializeBinary<T>.TryDeserializeTypeFrom<T>(buffer, out result);

        /// <summary>
        /// Deserialize an instance from string serialized data (see <see cref="ISerializeString"/>)
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
            if (!ParseObjectDelegates.TryGetValue(mi, out ParseObject_Delegate? method))
            {
                method = mi.Method.CreateDelegate<ParseObject_Delegate>();
                ParseObjectDelegates.TryAdd(mi, method);
            }
            return method(str) ?? throw new InvalidDataException("NULL deserialized");
        }

        /// <summary>
        /// Deserialize an instance from string serialized data (see <see cref="ISerializeString"/>)
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
                if (!TryParseObjectDelegates.TryGetValue(mi, out TryParseObject_Delegate? method))
                {
                    method = mi.Method.CreateDelegate<TryParseObject_Delegate>();
                    TryParseObjectDelegates.TryAdd(mi, method);
                }
                return method(str, out result);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deserialize an instance from string serialized data (see <see cref="ISerializeString{T}"/>)
        /// </summary>
        /// <typeparam name="T">Type to deserialize</typeparam>
        /// <param name="str">String</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T Parse<T>(this ReadOnlySpan<char> str) where T : ISerializeString<T> => ISerializeString<T>.Parse<T>(str);

        /// <summary>
        /// Deserialize an instance from string serialized data (see <see cref="ISerializeString{T}"/>)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="str">String</param>
        /// <param name="result">Instance</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryParse<T>(this ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out T? result) where T : ISerializeString<T>
            => ISerializeString<T>.TryParse<T>(str, out result);

        /// <summary>
        /// Deserialize an instance from a stream (see <see cref="ISerializeStream"/>)
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Instance</returns>
        public static object DeserializeFrom(this Type type, in Stream stream, in int version)
        {
            if (!typeof(ISerializeStream).IsAssignableFrom(type))
                throw new InvalidOperationException($"{type} isn't an {typeof(ISerializeStream)}");
            MethodInfoExt mi = type.GetMethodCached(nameof(ISerializeStream.DeserializeFrom), BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidProgramException($"{type} doesn't implement {typeof(ISerializeStream)}.{nameof(ISerializeStream.DeserializeFrom)}");
            if (!DeserializeStreamDelegates.TryGetValue(mi, out DeserializeStream_Delegate? method))
            {
                method = mi.Method.CreateDelegate<DeserializeStream_Delegate>();
                DeserializeStreamDelegates.TryAdd(mi, method);
            }
            return method(stream, version);
        }

        /// <summary>
        /// Deserialize an instance from a stream (see <see cref="ISerializeStream"/>)
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instance</returns>
        public static async Task<object> DeserializeFromAsync(this Type type, Stream stream, int version, CancellationToken cancellationToken = default)
        {
            if (!typeof(ISerializeStream).IsAssignableFrom(type))
                throw new InvalidOperationException($"{type} isn't an {typeof(ISerializeStream)}");
            MethodInfoExt mi = type.GetMethodCached(nameof(ISerializeStream.DeserializeFromAsync), BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidProgramException($"{type} doesn't implement {typeof(ISerializeStream)}.{nameof(ISerializeStream.DeserializeFromAsync)}");
            if (!DeserializeStreamAsyncDelegates.TryGetValue(mi, out DeserializeStreamAsync_Delegate? method))
            {
                method = mi.Method.CreateDelegate<DeserializeStreamAsync_Delegate>();
                DeserializeStreamAsyncDelegates.TryAdd(mi, method);
            }
            return await method(stream, version, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Deserialize an instance from a buffer (see <see cref="ISerializeStream"/>)
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Instance</returns>
        public static object DeserializeFrom(this Type type, in ReadOnlyMemory<byte> buffer, in int version)
        {
            if (!typeof(ISerializeStream).IsAssignableFrom(type))
                throw new InvalidOperationException($"{type} isn't an {typeof(ISerializeStream)}");
            MethodInfoExt mi = type.GetMethodCached(nameof(ISerializeStream.ObjectFromBytes), BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidProgramException($"{type} doesn't implement {typeof(ISerializeStream)}.{nameof(ISerializeStream.ObjectFromBytes)}");
            if (!DeserializeBufferDelegates.TryGetValue(mi, out DeserializeBuffer_Delegate? method))
            {
                method = mi.Method.CreateDelegate<DeserializeBuffer_Delegate>();
                DeserializeBufferDelegates.TryAdd(mi, method);
            }
            return method(buffer, version);
        }

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
        /// Deserialize from previously serialized data
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Instance</returns>
        private delegate object DeserializeStream_Delegate(in Stream stream, in int version);
        /// <summary>
        /// Deserialize from previously serialized data
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instance</returns>
        private delegate Task<object> DeserializeStreamAsync_Delegate(Stream stream, int version, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deserialize from previously serialized data
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="version">Data structure version</param>
        /// <returns>Instance</returns>
        private delegate object DeserializeBuffer_Delegate(in ReadOnlyMemory<byte> buffer, in int version);
    }
}
