using System.Collections;

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
        public static bool IsEmpty(this SerializedObjectTypes type) => (type & SerializedObjectTypes.Empty) == SerializedObjectTypes.Empty;

        /// <summary>
        /// If the value is nullable
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If nullable</returns>
        public static bool IsNullable(this SerializedObjectTypes type) => (type & SerializedObjectTypes.NullableValue) == SerializedObjectTypes.NullableValue;

        /// <summary>
        /// Determine if a type is valid
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If valid</returns>
        public static bool IsValid(this SerializedObjectTypes type)
            => (type & ~SerializedObjectTypes.FLAGS) switch
            {
                SerializedObjectTypes.Array or SerializedObjectTypes.Dictionary => !(type.IsNullable() && type.IsEmpty()),
                SerializedObjectTypes.Type or SerializedObjectTypes.Json or SerializedObjectTypes.Serializable => !type.IsEmpty() && !type.IsNullable(),
                SerializedObjectTypes.Enumerable => !type.IsEmpty(),
                _ => type == SerializedObjectTypes.None
            };

        /// <summary>
        /// Get the CLR type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>CLR type</returns>
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
                _ => null
            };

        /// <summary>
        /// Get the serialized type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Serialized type</returns>
        public static SerializedObjectTypes GetSerializedType(this Type type)
        {
            TypeInfoExt typeInfo = TypeInfoExt.From(type);
            if (type.IsNumeric()) return SerializedObjectTypes.Numeric;
            if (type == typeof(string)) return SerializedObjectTypes.String;
            if (type == typeof(bool)) return SerializedObjectTypes.Boolean;
            if (type == typeof(Type)) return SerializedObjectTypes.Type;
            if (typeof(ISerializeStream).IsAssignableFrom(type)) return SerializedObjectTypes.Serializable;
            if (typeof(Stream).IsAssignableFrom(type)) return SerializedObjectTypes.Stream;
            if (
                typeof(IList).IsAssignableFrom(type) ||
                typeInfo.GetInterfaces().Any(i => i.IsGenericType && TypeInfoExt.From(i).GetGenericTypeDefinition()?.Type == typeof(IList<>))
                )
                return SerializedObjectTypes.Array;
            if (
                typeof(IDictionary).IsAssignableFrom(type) ||
                typeInfo.GetInterfaces().Any(i => i.IsGenericType && TypeInfoExt.From(i).GetGenericTypeDefinition()?.Type == typeof(IDictionary<,>))
                )
                return SerializedObjectTypes.Dictionary;
            if (typeof(IEnumerable).IsAssignableFrom(type)) return SerializedObjectTypes.Enumerable;
            return SerializedObjectTypes.Json;
        }

        /// <summary>
        /// Get the value
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Value</returns>
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
        public static SerializedObjectTypes RemoveFlags(this SerializedObjectTypes type) => type & ~SerializedObjectTypes.FLAGS;

        /// <summary>
        /// Get the <see cref="SerializedObjectTypes"/> for a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="isNullable">If items are nullable (valid for <see cref="IList"/> and <see cref="IDictionary"/>)</param>
        /// <returns>Type</returns>
        public static SerializedObjectTypes GetSerializedType(this object value, bool isNullable)
            => value switch
            {
                string v => v.Length == 0 
                    ? SerializedObjectTypes.String | SerializedObjectTypes.Empty 
                    : SerializedObjectTypes.String,
                bool v => v 
                    ? SerializedObjectTypes.Boolean 
                    : SerializedObjectTypes.Boolean | SerializedObjectTypes.Empty,
                ISerializeStream => SerializedObjectTypes.Serializable,
                Type => SerializedObjectTypes.Type,
                Stream v => v.CanSeek && v.Length == 0
                    ? SerializedObjectTypes.Stream | SerializedObjectTypes.Empty
                    : SerializedObjectTypes.Stream | (isNullable ? SerializedObjectTypes.NullableValue : SerializedObjectTypes.None),
                IList v => v.Count == 0 
                    ? SerializedObjectTypes.Array | SerializedObjectTypes.Empty 
                    : SerializedObjectTypes.Array,
                IDictionary v => v.Count == 0 
                    ? SerializedObjectTypes.Dictionary | SerializedObjectTypes.Empty 
                    : SerializedObjectTypes.Dictionary | (isNullable ? SerializedObjectTypes.NullableValue : SerializedObjectTypes.None),
                _ when value.GetType().IsNumeric() => value.GetNumericType() switch
                {
                    NumericTypes.Zero => SerializedObjectTypes.Numeric | SerializedObjectTypes.Empty,
                    _ => SerializedObjectTypes.Numeric
                },
                IEnumerable v => SerializedObjectTypes.Enumerable,
                _ => SerializedObjectTypes.Json,
            };
    }
}
