using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace wan24.Core
{
    // Methods
    public static partial class StringValueConverter
    {
        /// <summary>
        /// Determine if a type has a to string converter
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If the type can be converted to string</returns>
        public static bool CanConvertToString(Type type)
        {
            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
                type = underlyingType;
            return StringConverter.Keys.GetClosestType(type) is not null;
        }

        /// <summary>
        /// Determine if a type has a from string converter
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If the type can be converted from a string</returns>
        public static bool CanConvertFromString(Type type)
        {
            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
                type = underlyingType;
            return ValueConverter.Keys.GetClosestType(type) is not null;
        }

        /// <summary>
        /// Convert a display string to a value
        /// </summary>
        /// <param name="type">Value type (may be abstract)</param>
        /// <param name="str">String</param>
        /// <returns>Value</returns>
        public static object? Convert(Type type, in string? str)
        {
            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
                type = underlyingType;
            if (type == typeof(string)) return str;
            if (!ValueConverter.TryGetValue(type, out ValueConverter_Delegate? converter))
            {
                converter = ValueConverter.Keys.GetClosestType(type) is Type converterType
                    ? ValueConverter.TryGetValue(converterType, out converter)
                        ? converter
                        : null
                    : null;
                if (converter is null)
                    if (typeof(IStringValueConverter).IsAssignableFrom(type))
                    {
                        object?[] param = [str, null];
                        type.GetMethodCached(nameof(IStringValueConverter.TryParse), BindingFlags.Public | BindingFlags.Static)!.Method.Invoke(obj: null, param);
                        return param[1];
                    }
                    else
                    {
                        throw new ArgumentException("Unknown type", nameof(type));
                    }
            }
            return converter(type, str);
        }

        /// <summary>
        /// Convert a display string to a value
        /// </summary>
        /// <param name="type">Value type (may be abstract)</param>
        /// <param name="str">String</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryConvert(Type type, in string? str, out object? result)
        {
            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
                type = underlyingType;
            if (type == typeof(string))
            {
                result = str;
                return true;
            }
            if (!ValueConverter.TryGetValue(type, out ValueConverter_Delegate? converter))
            {
                converter = ValueConverter.Keys.GetClosestType(type) is Type converterType
                    ? ValueConverter.TryGetValue(converterType, out converter)
                        ? converter
                        : null
                    : null;
                if (converter is null)
                    if (typeof(IStringValueConverter).IsAssignableFrom(type))
                    {
                        object?[] param = [str, null];
                        type.GetMethodCached(nameof(IStringValueConverter.TryParse), BindingFlags.Public | BindingFlags.Static)!.Method.Invoke(obj: null, param);
                        result = param[1];
                        return true;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
            }
            result = converter(type, str);
            return true;
        }

        /// <summary>
        /// Convert a value to a display string
        /// </summary>
        /// <typeparam name="T">Value type (may be abstract)</typeparam>
        /// <param name="value">Value</param>
        /// <returns>String</returns>
        public static string? Convert<T>(in T? value) => value is IStringValueConverter svc ? svc.DisplayString : Convert(typeof(T), value);

        /// <summary>
        /// Convert a value to a display string
        /// </summary>
        /// <typeparam name="T">Value type (may be abstract)</typeparam>
        /// <param name="value">Value</param>
        /// <param name="result">Result</param>
        /// <returns>String</returns>
        public static bool TryConvert<T>(in T? value, out string? result)
        {
            if (value is IStringValueConverter svc)
            {
                result = svc.DisplayString;
                return true;
            }
            return TryConvert(typeof(T), value, out result);
        }

        /// <summary>
        /// Convert a value to a display string
        /// </summary>
        /// <param name="type">Value type (may be abstract)</param>
        /// <param name="value">Value</param>
        /// <returns>String</returns>
        public static string? Convert(Type type, in object? value)
        {
            if (value is string str) return str;
            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
                type = underlyingType;
            if (value?.GetType() is Type valueType && !type.IsAssignableFromExt(valueType))
                throw new ArgumentException("Incompatible type", nameof(type));
            if (!StringConverter.TryGetValue(type, out StringConverter_Delegate? converter))
            {
                converter = StringConverter.Keys.GetClosestType(type) is Type converterType
                    ? StringConverter.TryGetValue(converterType, out converter)
                        ? converter
                        : null
                    : null;
                if (converter is null)
                    if (typeof(IStringValueConverter).IsAssignableFrom(type))
                    {
                        return (value as IStringValueConverter)?.DisplayString;
                    }
                    else
                    {
                        throw new ArgumentException("Unknown type", nameof(type));
                    }
            }
            return converter(type, value);
        }

        /// <summary>
        /// Convert a value to a display string
        /// </summary>
        /// <param name="type">Value type (may be abstract)</param>
        /// <param name="value">Value</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryConvert(Type type, in object? value, out string? result)
        {
            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
                type = underlyingType;
            if (value is string str)
            {
                result = str;
                return true;
            }
            if (value?.GetType() is Type valueType && !type.IsAssignableFromExt(valueType))
            {
                result = null;
                return false;
            }
            if (!StringConverter.TryGetValue(type, out StringConverter_Delegate? converter))
            {
                converter = StringConverter.Keys.GetClosestType(type) is Type converterType
                    ? StringConverter.TryGetValue(converterType, out converter)
                        ? converter
                        : null
                    : null;
                if (converter is null)
                    if (typeof(IStringValueConverter).IsAssignableFrom(type))
                    {
                        result = (value as IStringValueConverter)?.DisplayString;
                        return true;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
            }
            result = converter(type, value);
            return true;
        }

        /// <summary>
        /// Convert an object to a string
        /// </summary>
        /// <typeparam name="T">Object type (may be abstract)</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>String</returns>
        public static string? ConvertToString<T>(this T obj) => Convert(typeof(T), obj);

        /// <summary>
        /// Convert an object to a string
        /// </summary>
        /// <typeparam name="T">Object type (may be abstract)</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryConvertToString<T>(this T obj, out string? result) => TryConvert(typeof(T), obj, out result);

        /// <summary>
        /// Convert an object to a string
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>String</returns>
        public static string? ConvertObjectToString(this object obj) => Convert(obj.GetType(), obj);

        /// <summary>
        /// Convert an object to a string
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool ConvertObjectToString(this object obj, out string? result) => TryConvert(obj.GetType(), obj, out result);

        /// <summary>
        /// Convert a string to an object
        /// </summary>
        /// <typeparam name="T">Object type (may be abstract)</typeparam>
        /// <param name="str">String</param>
        /// <returns>Object</returns>
        public static T? ConvertFromString<T>(this string str) => (T?)Convert(typeof(T), str);

        /// <summary>
        /// Convert a string to an object
        /// </summary>
        /// <typeparam name="T">Object type (may be abstract)</typeparam>
        /// <param name="str">String</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryConvertFromString<T>(this string str, out T? result)
        {
            if (!TryConvert(typeof(T), str, out object? res))
            {
                result = default(T?);
                return false;
            }
            if (res is null)
            {
                result = default(T?);
                return true;
            }
            if (typeof(T).IsAssignableFrom(res.GetType()))
            {
                result = (T)res;
                return true;
            }
            result = default(T?);
            return false;
        }

        /// <summary>
        /// Convert a string to an object
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="type">Object type (may be abstract)</param>
        /// <returns>Object</returns>
        public static object? ConvertObjectFromString(this string str, Type type) => Convert(type, str);

        /// <summary>
        /// Convert a string to an object
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="type">Object type (may be abstract)</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryConvertObjectFromString(this string str, Type type, out object? result) => TryConvert(type, str, out result);

        /// <summary>
        /// Named string to value converter
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="name">Conversion name</param>
        /// <param name="type">Object type (may be abstract)</param>
        /// <returns>Object</returns>
        public static object? NamedObjectConversion(this string? str, in string name, Type type)
        {
            if (!NamedValueConverter.TryGetValue(name, out ValueConverter_Delegate? converter))
                throw new ArgumentException("Unknown conversion", nameof(name));
            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
                type = underlyingType;
            object? res = converter.Invoke(type, str);
            if (res is not null && !type.IsAssignableFromExt(res.GetType()))
            {
                res.TryDispose();
                throw new InvalidDataException($"Converted value type {res.GetType()} doesn't match the expected type {type}");
            }
            return res;
        }

        /// <summary>
        /// Named string to value converter
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="name">Conversion name</param>
        /// <param name="type">Object type (may be abstract)</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryNamedObjectConversion(this string? str, in string name, Type type, out object? result)
        {
            if (!NamedValueConverter.TryGetValue(name, out ValueConverter_Delegate? converter))
            {
                result = null;
                return false;
            }
            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
                type = underlyingType;
            result = converter.Invoke(type, str);
            if (result is not null && !type.IsAssignableFrom(result.GetType()))
            {
                result.TryDispose();
                result = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Named value to string converter
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="name">Conversion name</param>
        /// <param name="type">Object type (may be abstract)</param>
        /// <returns>Object</returns>
        public static string? NamedStringConversion(this object? obj, in string name, Type type)
        {
            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
                type = underlyingType;
            if (!NamedStringConverter.TryGetValue(name, out StringConverter_Delegate? converter))
                throw new ArgumentException("Unknown conversion", nameof(name));
            return converter.Invoke(type, obj);
        }

        /// <summary>
        /// Named value to string converter
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="name">Conversion name</param>
        /// <param name="type">Object type (may be abstract)</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryNamedStringConversion(this object? obj, in string name, Type type, out string? result)
        {
            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
                type = underlyingType;
            bool res;
            result = (res = NamedStringConverter.TryGetValue(name, out StringConverter_Delegate? converter))
                    ? converter!.Invoke(type, obj)
                    : null;
            return res;
        }

        /// <summary>
        /// Convert a structure to a string representation (base64)
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <param name="obj">Instance</param>
        /// <returns>String</returns>
        public static string? ConvertStruct<T>(this T? obj) where T : struct
            => TryConvertStruct(obj, out string? result) ? result : throw new InvalidOperationException("Structure to string conversion failed");

        /// <summary>
        /// Convert a structure to a string representation (base64)
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <param name="obj">Instance</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryConvertStruct<T>(this T? obj, [MaybeNullWhen(returnValue: false)] out string? result) where T : struct
        {
            if(obj is null)
            {
                result = null;
                return true;
            }
            try
            {
                if (CanConvertToString(typeof(T))) return TryConvert(obj, out result);
                using RentedArrayRefStruct<byte> buffer = new(Marshal.SizeOf(obj), clean: false);
                obj.Value.GetMarshalBytes(buffer.Array);
                result = System.Convert.ToBase64String(buffer.Span);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Convert a structure to a string representation (base64)
        /// </summary>
        /// <param name="obj">Instance</param>
        /// <returns>String</returns>
        public static string? ConvertStructInstance(this object? obj)
            => TryConvertStructInstance(obj, out string? result) ? result : throw new InvalidOperationException("Structure to string conversion failed");

        /// <summary>
        /// Convert a structure to a string representation (base64)
        /// </summary>
        /// <param name="obj">Instance</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryConvertStructInstance(this object? obj, [MaybeNullWhen(returnValue: true)] out string? result)
        {
            if (obj is null)
            {
                result = null;
                return true;
            }
            Type type = obj.GetType();
            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
                type = underlyingType;
            if (!type.IsValueType)
            {
                result = null;
                return false;
            }
            try
            {
                if (CanConvertToString(type)) return TryConvert(type, obj, out result);
                using RentedArrayRefStruct<byte> buffer = new(Marshal.SizeOf(obj), clean: false);
                obj.GetMarshalBytes(buffer.Array);
                result = System.Convert.ToBase64String(buffer.Span);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Convert a string (base64) to a structure
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <param name="str">String</param>
        /// <returns>Structure</returns>
        public static T? ConvertStruct<T>(this string? str) where T : struct
            => TryConvertStruct(str, out T? res) ? res : throw new InvalidOperationException("String to structure conversion failed");

        /// <summary>
        /// Convert a structure to a string representation (base64)
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <param name="str">String</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryConvertStruct<T>(this string? str, [MaybeNullWhen(returnValue: true)] out T? result) where T : struct
        {
            if (str is null)
            {
                result = null;
                return true;
            }
            try
            {
                if (CanConvertFromString(typeof(T)))
                {
                    bool res;
                    result = (res = TryConvertFromString<T>(str, out T obj)) ? obj : null;
                    return res;
                }
                int len = Encoding.UTF8.GetByteCount(str);
                if (len < Marshal.SizeOf<T>()) throw new InvalidDataException("Not enough data");
                using RentedArrayRefStruct<byte> buffer = new(len, clean: true);
                str.GetBase64Bytes(buffer.Span);
                result = buffer.Array.UnmarshalStructure<T>();
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Convert a structure to a string representation (base64)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="type">Structure type</param>
        /// <returns>Instance</returns>
        public static object? ConvertStructInstance(this string? str, in Type type)
            => TryConvertStructInstance(str, type, out object? result) ? result : throw new InvalidOperationException("String to structure conversion failed");

        /// <summary>
        /// Convert a structure to a string representation (base64)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="type">Structure type</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryConvertStructInstance(this string? str, Type type, [MaybeNullWhen(returnValue: true)] out object? result)
        {
            if (Nullable.GetUnderlyingType(type) is Type underlyingType)
                type = underlyingType;
            if (!type.IsValueType)
            {
                result = null;
                return false;
            }
            if (str is null)
            {
                result = null;
                return true;
            }
            try
            {
                if (CanConvertFromString(type)) return TryConvertObjectFromString(str, type, out result);
                int len = Encoding.UTF8.GetByteCount(str);
                if (len < type.GetMarshaledSize()) throw new InvalidDataException("Not enough data");
                using RentedArrayRefStruct<byte> buffer = new(len, clean: false);
                str.GetBase64Bytes(buffer.Span);
                result = buffer.Array.UnmarshalStructure(type);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
