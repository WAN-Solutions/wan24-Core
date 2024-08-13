using System.Collections;
using System.Numerics;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="SerializerObjectTypes"/> extensions
    /// </summary>
    public static class SerializerObjectTypesExtensions
    {
        /// <summary>
        /// Get <see cref="SerializerObjectTypes"/> from an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns><see cref="SerializerObjectTypes"/></returns>
        public static SerializerObjectTypes ToSerializerObjectTypes<T>(this T obj)
        {
            SerializerObjectTypes res = SerializerObjectTypes.Null;
            if (obj is null) return res;
            // Get the (array element) type
            TypeInfoExt type = obj.GetType();
            if (Nullable.GetUnderlyingType(type) is Type underlyingType) type = underlyingType;
            if (type.Type.IsArray)
            {
                type = type.Type.GetElementType() ?? throw new InvalidProgramException($"Failed to get element type of {type}");
                res |= SerializerObjectTypes.Array;
                if ((obj as Array)!.Length == 0) res |= SerializerObjectTypes.Empty;
                if (type.Type.IsArray) return res;// Multi dimensional array requires another SerializerObjectTypes until the final element type was resolved
            }
            // Try to map a basic type
            if (obj is ISerializeStream) return res | SerializerObjectTypes.Serializable;
            if (type.IsGenericType)
            {
                // Generic types
                if (type.GenericArgumentCount == 1 && typeof(List<>).MakeGenericType(type.GenericArguments) == type.Type)
                {
                    if ((obj as IList)!.Count == 0) res |= SerializerObjectTypes.Empty;
                    return res | SerializerObjectTypes.List;
                }
                if (type.GenericArgumentCount == 2 && typeof(Dictionary<,>).MakeGenericType(type.GenericArguments) == type.Type)
                {
                    if ((obj as IDictionary)!.Count == 0) res |= SerializerObjectTypes.Empty;
                    return res | SerializerObjectTypes.Dictionary;
                }
            }
            else if (type.Type.IsValueType)
            {
                // Value types
                if (type.Type.IsEnum)
                {
                    if (type.ParameterlessConstructor?.Invoker is not null && obj.Equals(type.ParameterlessConstructor.Invoker([]))) res |= SerializerObjectTypes.Empty;
                    return res | SerializerObjectTypes.Enum;
                }
                if (obj is bool boolVal) return res | SerializerObjectTypes.Boolean | (boolVal ? SerializerObjectTypes.Null : SerializerObjectTypes.Empty);
                if (obj is string strVal) return res | SerializerObjectTypes.String | (strVal.Length == 0 ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null);
                if (obj.TryGetSerializerNumericTypes(out SerializerNumericTypes numericType))
                    return res | SerializerObjectTypes.Number | (obj switch
                    {
                        sbyte val => val == 0 ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null,
                        byte val => val == 0 ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null,
                        short val => val == 0 ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null,
                        ushort val => val == 0 ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null,
                        Half val => val == Half.Zero ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null,
                        int val => val == 0 ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null,
                        uint val => val == 0 ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null,
                        float val => val == 0 ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null,
                        long val => val == 0 ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null,
                        ulong val => val == 0 ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null,
                        double val => val == 0 ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null,
                        decimal val => val == decimal.Zero ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null,
                        BigInteger val => val == BigInteger.Zero ? SerializerObjectTypes.Empty : SerializerObjectTypes.Null,
                        _ => throw new InvalidProgramException($"Unsupported number type {type.Type} (storable as {numericType})")
                    });
            }
            return SerializerObjectTypes.Object;
        }

        /// <summary>
        /// Get <see cref="SerializerObjectTypes"/> from a CLR type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns><see cref="SerializerObjectTypes"/></returns>
        public static SerializerObjectTypes ToSerializerObjectTypes(this TypeInfoExt type)
        {
            SerializerObjectTypes res = SerializerObjectTypes.Null;
            // Get the (array element) type
            if (Nullable.GetUnderlyingType(type) is Type underlyingType) type = underlyingType;
            if (type.Type.IsArray)
            {
                type = type.Type.GetElementType() ?? throw new InvalidProgramException($"Failed to get element type of {type}");
                res |= SerializerObjectTypes.Array;
                if (type.Type.IsArray) return res;// Multi dimensional array requires another SerializerObjectTypes until the final element type was resolved
            }
            // Try to map a basic type
            if (typeof(ISerializeStream).IsAssignableFrom(type)) return res | SerializerObjectTypes.Serializable;
            if (type.IsGenericType)
            {
                // Generic types
                if (type.GenericArgumentCount == 1 && typeof(List<>).MakeGenericType(type.GenericArguments) == type.Type) return res | SerializerObjectTypes.List;
                if (type.GenericArgumentCount == 2 && typeof(Dictionary<,>).MakeGenericType(type.GenericArguments) == type.Type) return res | SerializerObjectTypes.Dictionary;
            }
            else if (type.Type.IsValueType)
            {
                // Value types
                if (type.Type.IsEnum) return res | SerializerObjectTypes.Enum;
                if (type.Type == typeof(bool)) return res | SerializerObjectTypes.Boolean;
                if (type.Type == typeof(string)) return res | SerializerObjectTypes.String;
                SerializerNumericTypes snt = type.Type.ToSerializerNumericTypes();
                if (snt != SerializerNumericTypes.None) return SerializerObjectTypes.Number;
            }
            return SerializerObjectTypes.Object;
        }

        /// <summary>
        /// Determine if a <see cref="SerializerObjectTypes"/> contains a complete type information
        /// </summary>
        /// <param name="types">Types</param>
        /// <returns>If complete type information are included</returns>
        public static bool ContainsClrType(this SerializerObjectTypes types)
            => types switch
            {
                SerializerObjectTypes.Boolean or
                    SerializerObjectTypes.Boolean | SerializerObjectTypes.Empty or
                    SerializerObjectTypes.String or
                    SerializerObjectTypes.String | SerializerObjectTypes.Empty or
                    SerializerObjectTypes.Boolean | SerializerObjectTypes.Array or
                    SerializerObjectTypes.Boolean | SerializerObjectTypes.Empty | SerializerObjectTypes.Array or
                    SerializerObjectTypes.String | SerializerObjectTypes.Array or
                    SerializerObjectTypes.String | SerializerObjectTypes.Array | SerializerObjectTypes.Empty => true,
                _ => false
            };

        /// <summary>
        /// Determine if a <see cref="SerializerObjectTypes"/> contains a value
        /// </summary>
        /// <param name="types">Types</param>
        /// <returns>If a value is contained</returns>
        public static bool ContainsClrValue(this SerializerObjectTypes types)
            => types switch
            {
                SerializerObjectTypes.Null or
                    SerializerObjectTypes.Boolean or
                    SerializerObjectTypes.Boolean | SerializerObjectTypes.Empty or
                    SerializerObjectTypes.String | SerializerObjectTypes.Empty or
                    SerializerObjectTypes.Boolean | SerializerObjectTypes.Empty | SerializerObjectTypes.Array or
                    SerializerObjectTypes.String | SerializerObjectTypes.Array | SerializerObjectTypes.Empty => true,
                _ => false
            };

        /// <summary>
        /// Get the CLR type
        /// </summary>
        /// <param name="types">Types</param>
        /// <returns>CLR type</returns>
        public static Type? ToClrType(this SerializerObjectTypes types)
            => types switch
            {
                SerializerObjectTypes.Boolean or
                    SerializerObjectTypes.Boolean | SerializerObjectTypes.Empty => typeof(bool),
                SerializerObjectTypes.String or
                    SerializerObjectTypes.String | SerializerObjectTypes.Empty => typeof(string),
                SerializerObjectTypes.Boolean | SerializerObjectTypes.Array or
                    SerializerObjectTypes.Boolean | SerializerObjectTypes.Array | SerializerObjectTypes.Empty => typeof(bool[]),
                SerializerObjectTypes.String | SerializerObjectTypes.String or
                    SerializerObjectTypes.String | SerializerObjectTypes.Array | SerializerObjectTypes.Empty => typeof(string[]),
                _ => null
            };

        /// <summary>
        /// Get the CLR value
        /// </summary>
        /// <param name="types">Types</param>
        /// <returns>CLR value</returns>
        public static object? ToClrValue(this SerializerObjectTypes types)
            => types switch
            {
                SerializerObjectTypes.Boolean => true,
                SerializerObjectTypes.Boolean | SerializerObjectTypes.Empty => false,
                SerializerObjectTypes.String | SerializerObjectTypes.Empty => string.Empty,
                SerializerObjectTypes.Boolean | SerializerObjectTypes.Array | SerializerObjectTypes.Empty => Array.Empty<bool>(),
                SerializerObjectTypes.String | SerializerObjectTypes.Array | SerializerObjectTypes.Empty => Array.Empty<string>(),
                _ => null
            };

        /// <summary>
        /// Get the CLR value
        /// </summary>
        /// <param name="types">Types</param>
        /// <param name="clrTypes">CLR types</param>
        /// <returns>CLR value</returns>
        public static object? ToClrValue(this SerializerObjectTypes types, params Type[] clrTypes)
        {
            if (types == SerializerObjectTypes.Null) return null;
            if (ToClrValue(types) is object res) return res;
            switch (types)
            {
                case SerializerObjectTypes.Object:
                    if (clrTypes.Length != 1) throw new ArgumentOutOfRangeException(nameof(clrTypes));
                    {
                        if (clrTypes[0].GetParameterlessConstructor() is not ConstructorInfoExt ci)
                            throw new ArgumentException($"No suitable constructor found for {clrTypes[0]}", nameof(clrTypes));
                        return ci.Constructor.InvokeAuto();
                    }
                case SerializerObjectTypes.Object | SerializerObjectTypes.Array | SerializerObjectTypes.Empty:
                case SerializerObjectTypes.Object | SerializerObjectTypes.Enumerable | SerializerObjectTypes.Empty:
                case SerializerObjectTypes.Serializable | SerializerObjectTypes.Array | SerializerObjectTypes.Empty:
                case SerializerObjectTypes.Serializable | SerializerObjectTypes.Enumerable | SerializerObjectTypes.Empty:
                    if (clrTypes.Length != 1) throw new ArgumentOutOfRangeException(nameof(clrTypes));
                    return Array.CreateInstance(clrTypes[0], length: 0);
                case SerializerObjectTypes.Number | SerializerObjectTypes.Empty:
                    if (clrTypes.Length != 1) throw new ArgumentOutOfRangeException(nameof(clrTypes));
                    if (!clrTypes[0].IsValueType || !typeof(IConvertible).IsAssignableFrom(clrTypes[0])) throw new ArgumentException("Not a number type", nameof(clrTypes));
                    return clrTypes[0].GetParameterlessConstructor()!.Invoker!([]);
                case SerializerObjectTypes.Enum | SerializerObjectTypes.Empty:
                    if (clrTypes.Length != 1) throw new ArgumentOutOfRangeException(nameof(clrTypes));
                    if (!clrTypes[0].IsEnum) throw new ArgumentException("Not an enum type", nameof(clrTypes));
                    return clrTypes[0].GetParameterlessConstructor()!.Invoker!([]);
                case SerializerObjectTypes.List | SerializerObjectTypes.Empty:
                    if (clrTypes.Length != 1) throw new ArgumentOutOfRangeException(nameof(clrTypes));
                    return typeof(List<>).MakeGenericType(clrTypes).GetParameterlessConstructor()!.Invoker!([]);
                case SerializerObjectTypes.Dictionary | SerializerObjectTypes.Empty:
                    if (clrTypes.Length != 2) throw new ArgumentOutOfRangeException(nameof(clrTypes));
                    return typeof(Dictionary<,>).MakeGenericType(clrTypes).GetParameterlessConstructor()!.Invoker!([]);
                case SerializerObjectTypes.Serializable:
                    if (clrTypes.Length != 1) throw new ArgumentOutOfRangeException(nameof(clrTypes));
                    {
                        if (
                            (
                                clrTypes[0].GetConstructorsCached().FirstOrDefault(c => c.GetCustomAttributeCached<SerializerConstructorAttribute>() is not null) ??
                                clrTypes[0].GetParameterlessConstructor()
                            ) is not ConstructorInfoExt ci
                            )
                            throw new ArgumentException($"No suitable constructor found for {clrTypes[0]}", nameof(clrTypes));
                        return SerializerSettings.ServiceProvider is not null
                            ? ci.Constructor.InvokeAuto(SerializerSettings.ServiceProvider)
                            : ci.Constructor.InvokeAuto();
                    }
                default:
                    throw new ArgumentException("Incomplete CLR value information", nameof(types));
            }
        }
    }
}
