using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="SerializedObjectTypes"/> extensions
    /// </summary>
    public static class SerializedObjectTypesExtensions
    {
        /// <summary>
        /// If the value is empty
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If empty</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsEmpty(this SerializedObjectTypes type) => (type & SerializedObjectTypes.Empty) == SerializedObjectTypes.Empty;

        /// <summary>
        /// If the value is nullable
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If nullable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsNullable(this SerializedObjectTypes type) => (type & SerializedObjectTypes.NullableValue) == SerializedObjectTypes.NullableValue;

        /// <summary>
        /// Determine if a type is valid
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If valid</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsValid(this SerializedObjectTypes type)
            => (type & ~SerializedObjectTypes.FLAGS) switch
            {
                SerializedObjectTypes.Array 
                    or SerializedObjectTypes.Dictionary
                    => !(type.IsNullable() && type.IsEmpty()),
                SerializedObjectTypes.Type 
                    or SerializedObjectTypes.Json 
                    or SerializedObjectTypes.Serializable
                    or SerializedObjectTypes.SerializeBinary
                    or SerializedObjectTypes.SerializeString
                    => !type.IsEmpty() && !type.IsNullable(),
                SerializedObjectTypes.Enumerable => !type.IsEmpty(),
                _ => type == SerializedObjectTypes.None
            };

        /// <summary>
        /// Get the CLR type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>CLR type</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type? GetClrType(this SerializedObjectTypes type)
            => (type & ~SerializedObjectTypes.FLAGS) switch
            {
                SerializedObjectTypes.Numeric => typeof(NumericTypes),
                SerializedObjectTypes.String => typeof(string),
                SerializedObjectTypes.Boolean => typeof(bool),
                SerializedObjectTypes.Type => typeof(Type),
                SerializedObjectTypes.Json => typeof(object),
                SerializedObjectTypes.Array => typeof(IList),
                SerializedObjectTypes.Dictionary => typeof(IDictionary),
                SerializedObjectTypes.Stream => typeof(Stream),
                SerializedObjectTypes.Serializable => typeof(ISerializeStream),
                SerializedObjectTypes.Enumerable => typeof(IEnumerable),
                SerializedObjectTypes.SerializeBinary => typeof(ISerializeBinary),
                SerializedObjectTypes.SerializeString => typeof(ISerializeString),
                _ => null
            };

        /// <summary>
        /// Get the serialized type
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="useInterfaces">If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <returns>Serialized type</returns>
        public static SerializedObjectTypes GetSerializedType(this Type type, in bool useInterfaces)
        {
            if (type.IsNumeric()) return SerializedObjectTypes.Numeric;
            if (type == typeof(string)) return SerializedObjectTypes.String;
            if (type == typeof(bool)) return SerializedObjectTypes.Boolean;
            if (type == typeof(Type)) return SerializedObjectTypes.Type;
            if (typeof(ISerializeStream).IsAssignableFrom(type)) return SerializedObjectTypes.Serializable;
            if (typeof(ISerializeBinary).IsAssignableFrom(type)) return SerializedObjectTypes.SerializeBinary;
            if (typeof(ISerializeString).IsAssignableFrom(type)) return SerializedObjectTypes.SerializeString;
            if (typeof(Stream).IsAssignableFrom(type)) return SerializedObjectTypes.Stream;
            if (useInterfaces)
            {
                if (type.IsList()) return SerializedObjectTypes.Array;
                if (type.IsDictionary()) return SerializedObjectTypes.Dictionary;
                if (typeof(IEnumerable).IsAssignableFrom(type)) return SerializedObjectTypes.Enumerable;
            }
            return SerializedObjectTypes.Json;
        }

        /// <summary>
        /// Get the value
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Value (don't forget to dispose the returned stream, if <c>type</c> was <c>SerializedObjectTypes.Stream | SerializedObjectTypes.Empty</c>!)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object? GetValue(this SerializedObjectTypes type)
            => type switch
            {
                SerializedObjectTypes.Numeric | SerializedObjectTypes.Empty => 0,
                SerializedObjectTypes.String | SerializedObjectTypes.Empty => string.Empty,
                SerializedObjectTypes.Boolean => true,
                SerializedObjectTypes.Boolean | SerializedObjectTypes.Empty => false,
                SerializedObjectTypes.Stream | SerializedObjectTypes.Empty => new MemoryStream(),
                _ => null
            };

        /// <summary>
        /// Remove all flags
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type without flags</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static SerializedObjectTypes RemoveFlags(this SerializedObjectTypes type) => type & ~SerializedObjectTypes.FLAGS;

        /// <summary>
        /// Get the <see cref="SerializedObjectTypes"/> for a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="isNullable">If items are nullable (valid for <see cref="IList"/> and <see cref="IDictionary"/>)</param>
        /// <param name="useInterfaces">If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <returns>Type</returns>
        public static SerializedObjectTypes GetSerializedType(this object value, in bool isNullable, in bool useInterfaces)
        {
            Type type = value.GetType();
            return value switch
            {
                _ when type.IsNumeric() => value.GetNumericType() switch
                {
                    NumericTypes.Zero => SerializedObjectTypes.Numeric | SerializedObjectTypes.Empty,
                    _ => SerializedObjectTypes.Numeric
                },
                string v => v.Length == 0
                    ? SerializedObjectTypes.String | SerializedObjectTypes.Empty
                    : SerializedObjectTypes.String,
                bool v => v
                    ? SerializedObjectTypes.Boolean
                    : SerializedObjectTypes.Boolean | SerializedObjectTypes.Empty,
                ISerializeStream => SerializedObjectTypes.Serializable,
                ISerializeBinary => SerializedObjectTypes.SerializeBinary,
                ISerializeString => SerializedObjectTypes.SerializeString,
                Type => SerializedObjectTypes.Type,
                Stream v => v.CanSeek && v.Length == 0
                    ? SerializedObjectTypes.Stream | SerializedObjectTypes.Empty
                    : SerializedObjectTypes.Stream,
                IList v when useInterfaces => v.Count == 0
                    ? SerializedObjectTypes.Array | SerializedObjectTypes.Empty | (isNullable ? SerializedObjectTypes.NullableValue : SerializedObjectTypes.None)
                    : SerializedObjectTypes.Array | (isNullable ? SerializedObjectTypes.NullableValue : SerializedObjectTypes.None),
                object ov when useInterfaces && type.IsGenericDictionary() => new GenericListWrapper(ov).Count == 0
                    ? SerializedObjectTypes.Array | SerializedObjectTypes.Empty | (isNullable ? SerializedObjectTypes.NullableValue : SerializedObjectTypes.None)
                    : SerializedObjectTypes.Array | (isNullable ? SerializedObjectTypes.NullableValue : SerializedObjectTypes.None),
                IDictionary v when useInterfaces => v.Count == 0
                    ? SerializedObjectTypes.Dictionary | SerializedObjectTypes.Empty | (isNullable ? SerializedObjectTypes.NullableValue : SerializedObjectTypes.None)
                    : SerializedObjectTypes.Dictionary | (isNullable ? SerializedObjectTypes.NullableValue : SerializedObjectTypes.None),
                object ov when useInterfaces && type.IsGenericDictionary() => new GenericDictionaryWrapper(ov).Count == 0
                    ? SerializedObjectTypes.Dictionary | SerializedObjectTypes.Empty | (isNullable ? SerializedObjectTypes.NullableValue : SerializedObjectTypes.None)
                    : SerializedObjectTypes.Dictionary | (isNullable ? SerializedObjectTypes.NullableValue : SerializedObjectTypes.None),
                IEnumerable when useInterfaces => SerializedObjectTypes.Enumerable,
                _ => SerializedObjectTypes.Json
            };
        }
    }
}
