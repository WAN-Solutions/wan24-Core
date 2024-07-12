using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace wan24.Core
{
    /// <summary>
    /// (Display) String to value conversion
    /// </summary>
    public static class StringValueConverter
    {
        /// <summary>
        /// JSON value converter name
        /// </summary>
        public const string JSON_CONVERTER_NAME = "JSON";
        /// <summary>
        /// XML value converter name
        /// </summary>
        public const string XML_CONVERTER_NAME = "XML";

        /// <summary>
        /// Display string to value converter
        /// </summary>
        public static readonly Dictionary<Type, ValueConverter_Delegate> ValueConverter = [];
        /// <summary>
        /// Value to display string converter
        /// </summary>
        public static readonly Dictionary<Type, StringConverter_Delegate> StringConverter = [];
        /// <summary>
        /// Named display string to value converter
        /// </summary>
        public static readonly Dictionary<string, ValueConverter_Delegate> NamedValueConverter = [];
        /// <summary>
        /// Named value to display string converter
        /// </summary>
        public static readonly Dictionary<string, StringConverter_Delegate> NamedStringConverter = [];

        /// <summary>
        /// Constructor
        /// </summary>
        static StringValueConverter()
        {
            ValueConverter[typeof(string)] = (t, s) => s;
            ValueConverter[typeof(bool)] = (t, s) => s is not null && bool.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(byte)] = (t, s) => s is not null && byte.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(sbyte)] = (t, s) => s is not null && sbyte.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(ushort)] = (t, s) => s is not null && ushort.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(short)] = (t, s) => s is not null && short.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(uint)] = (t, s) => s is not null && uint.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(int)] = (t, s) => s is not null && int.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(ulong)] = (t, s) => s is not null && ulong.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(long)] = (t, s) => s is not null && long.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(Half)] = (t, s) => s is not null && Half.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(float)] = (t, s) => s is not null && float.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(double)] = (t, s) => s is not null && double.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(decimal)] = (t, s) => s is not null && decimal.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(DateTime)] = (t, s) => s is not null && DateTime.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(DateOnly)] = (t, s) => s is not null && DateOnly.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(TimeOnly)] = (t, s) => s is not null && TimeOnly.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(TimeSpan)] = (t, s) => s is not null && TimeSpan.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(Regex)] = (t, s) =>
            {
                if (s is null) return null;
                string[] info = s.Split(' ', 2);
                if (info.Length != 2) return null;
                try
                {
                    return new Regex(info[1], (RegexOptions)int.Parse(info[0]));
                }
                catch
                {
                    return null;
                }
            };
            ValueConverter[typeof(Enum)] = (t, s) => s is not null && Enum.TryParse(t, s, out var res) ? res : null;
            ValueConverter[typeof(IpSubNet)] = (t, s) => s is not null && IpSubNet.TryParse(s, out var res) ? (object)res : null;
            ValueConverter[typeof(HostEndPoint)] = (t, s) => s is not null && HostEndPoint.TryParse(s, out var res) ? (object)res : null;
            ValueConverter[typeof(UnixTime)] = (t, s) => s is not null && UnixTime.TryParse(s, out var res) ? (object)res : null;
            ValueConverter[typeof(Uid)] = (t, s) => s is not null && Uid.TryParse(s, out var res) ? (object)res : null;
            ValueConverter[typeof(UidExt)] = (t, s) => s is not null && UidExt.TryParse(s, out var res) ? (object)res : null;
            ValueConverter[typeof(Rgb)] = (t, s) => s is not null && Rgb.TryParse(s, out var res) ? (object)res : null;
            ValueConverter[typeof(RgbA)] = (t, s) => s is not null && RgbA.TryParse(s, out var res) ? (object)res : null;
            ValueConverter[typeof(Hsb)] = (t, s) => s is not null && Hsb.TryParse(s, out var res) ? (object)res : null;
            ValueConverter[typeof(IntRange)] = (t, s) => s is not null && IntRange.TryParse(s, out var res) ? (object)res : null;
            ValueConverter[typeof(Uri)] = (t, s) => s is not null && Uri.TryCreate(s, new(), out var res) ? (object)res : null;
            ValueConverter[typeof(Guid)] = (t, s) => s is not null && Guid.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(XmlDocument)] = (t, s) =>
            {
                if (s is null) return null;
                XmlDocument res = new();
                try
                {
                    res.LoadXml(s);
                }
                catch
                {
                    return null;
                }
                return res;
            };
            ValueConverter[typeof(XmlNode)] = (t, s) =>
            {
                if (s is null) return null;
                XmlDocument res = new();
                try
                {
                    res.LoadXml(s);
                }
                catch
                {
                    return null;
                }
                return res.FirstChild;
            };
            ValueConverter[typeof(XY)] = (t, s) => s is not null && XY.TryParse(s, out var res) ? (object)res : null;
            ValueConverter[typeof(XYZ)] = (t, s) => s is not null && XYZ.TryParse(s, out var res) ? (object)res : null;
            ValueConverter[typeof(XYInt)] = (t, s) => s is not null && XYInt.TryParse(s, out var res) ? (object)res : null;
            ValueConverter[typeof(XYZInt)] = (t, s) => s is not null && XYZInt.TryParse(s, out var res) ? (object)res : null;
            StringConverter[typeof(string)] = (t, v) => v as string;
            StringConverter[typeof(bool)] = (t, v) => v?.ToString();
            StringConverter[typeof(byte)] = (t, v) => v?.ToString();
            StringConverter[typeof(sbyte)] = (t, v) => v?.ToString();
            StringConverter[typeof(ushort)] = (t, v) => v?.ToString();
            StringConverter[typeof(short)] = (t, v) => v?.ToString();
            StringConverter[typeof(uint)] = (t, v) => v?.ToString();
            StringConverter[typeof(int)] = (t, v) => v?.ToString();
            StringConverter[typeof(ulong)] = (t, v) => v?.ToString();
            StringConverter[typeof(long)] = (t, v) => v?.ToString();
            StringConverter[typeof(Half)] = (t, v) => v?.ToString();
            StringConverter[typeof(float)] = (t, v) => v?.ToString();
            StringConverter[typeof(double)] = (t, v) => v?.ToString();
            StringConverter[typeof(decimal)] = (t, v) => v?.ToString();
            StringConverter[typeof(DateTime)] = (t, v) => v?.ToString();
            StringConverter[typeof(DateOnly)] = (t, v) => v?.ToString();
            StringConverter[typeof(TimeOnly)] = (t, v) => v?.ToString();
            StringConverter[typeof(TimeSpan)] = (t, v) => v?.ToString();
            StringConverter[typeof(Regex)] = (t, v) => v is not Regex rx ? null : $"{(int)rx.Options} {rx}";
            StringConverter[typeof(Enum)] = (t, v) => v?.ToString();
            StringConverter[typeof(IpSubNet)] = (t, v) => v?.ToString();
            StringConverter[typeof(HostEndPoint)] = (t, v) => v?.ToString();
            StringConverter[typeof(UnixTime)] = (t, v) => v?.ToString();
            StringConverter[typeof(Uid)] = (t, v) => v?.ToString();
            StringConverter[typeof(UidExt)] = (t, v) => v?.ToString();
            StringConverter[typeof(Rgb)] = (t, v) => v?.ToString();
            StringConverter[typeof(RgbA)] = (t, v) => v?.ToString();
            StringConverter[typeof(Hsb)] = (t, v) => v?.ToString();
            StringConverter[typeof(IntRange)] = (t, v) => v?.ToString();
            StringConverter[typeof(Uri)] = (t, v) => v?.ToString();
            StringConverter[typeof(Guid)] = (t, v) => v?.ToString();
            StringConverter[typeof(XmlDocument)] = (t, v) => (v as XmlDocument)?.OuterXml;
            StringConverter[typeof(XmlNode)] = (t, v) => (v as XmlNode)?.OuterXml;
            StringConverter[typeof(XY)] = (t, v) => v?.ToString();
            StringConverter[typeof(XYZ)] = (t, v) => v?.ToString();
            StringConverter[typeof(XYInt)] = (t, v) => v?.ToString();
            StringConverter[typeof(XYZInt)] = (t, v) => v?.ToString();
            NamedValueConverter[JSON_CONVERTER_NAME] = (t, s) =>
            {
                if (s is null) return null;
                try
                {
                    return JsonHelper.DecodeObject(t, s);
                }
                catch
                {
                    return null;
                }
            };
            NamedValueConverter[XML_CONVERTER_NAME] = (t, s) =>
            {
                if (s is null) return null;
                using MemoryPoolStream ms = new();
                try
                {
                    ms.Write(s.GetBytes());
                    ms.Position = 0;
                    XmlSerializer serializer = new(t);
                    return serializer.Deserialize(ms);
                }
                catch
                {
                    return null;
                }
            };
            NamedStringConverter[JSON_CONVERTER_NAME] = (t, v) => JsonHelper.Encode(v);
            NamedStringConverter[XML_CONVERTER_NAME] = (t, v) =>
            {
                using MemoryPoolStream ms = new();
                XmlSerializer serializer = new(t);
                try
                {
                    serializer.Serialize(ms, v);
                    ms.Position = 0;
                    using RentedArrayRefStruct<byte> buffer = new((int)ms.Length);
                    ms.ReadExactly(buffer.Span);
                    return buffer.Span.ToUtf8String();
                }
                catch
                {
                    return null;
                }
            };
        }

        /// <summary>
        /// Convert a display string to a value
        /// </summary>
        /// <param name="type">Value type (may be abstract)</param>
        /// <param name="str">String</param>
        /// <returns>Value</returns>
        public static object? Convert(Type type, in string? str)
        {
            if (type == typeof(string)) return str;
            if (!ValueConverter.TryGetValue(type, out ValueConverter_Delegate? converter))
            {
                converter = ValueConverter.FirstOrDefault(kvp => kvp.Key.IsAssignableFrom(type)).Value;
                if (converter is null)
                    if (typeof(IStringValueConverter).IsAssignableFrom(type))
                    {
                        object?[] param = [str, null];
                        type.GetMethodCached(nameof(IStringValueConverter.TryParse), BindingFlags.Public | BindingFlags.Static)!.Invoke(obj: null, param);
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
        /// Convert a value to a display string
        /// </summary>
        /// <typeparam name="T">Value type (may be abstract)</typeparam>
        /// <param name="value">Value</param>
        /// <returns>String</returns>
        public static string? Convert<T>(in T? value) => value is IStringValueConverter svc ? svc.DisplayString : Convert(typeof(T), value);

        /// <summary>
        /// Convert a value to a display string
        /// </summary>
        /// <param name="type">Value type (may be abstract)</param>
        /// <param name="value">Value</param>
        /// <returns>String</returns>
        public static string? Convert(Type type, in object? value)
        {
            if (value is string str) return str;
            if (!StringConverter.TryGetValue(type, out StringConverter_Delegate? converter))
            {
                converter = StringConverter.FirstOrDefault(kvp => kvp.Key.IsAssignableFrom(type)).Value;
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
        /// Convert an object to a string
        /// </summary>
        /// <typeparam name="T">Object type (may be abstract)</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>String</returns>
        public static string? ConvertToString<T>(this T obj) => Convert(typeof(T), obj);

        /// <summary>
        /// Convert an object to a string
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>String</returns>
        public static string? ConvertObjectToString(this object obj) => Convert(obj.GetType(), obj);

        /// <summary>
        /// Comvert a string to an object
        /// </summary>
        /// <typeparam name="T">Object type (may be abstract)</typeparam>
        /// <param name="str">String</param>
        /// <returns>Object</returns>
        public static T? ConvertFromString<T>(this string str) => (T?)Convert(typeof(T), str);

        /// <summary>
        /// Convert a string to an object
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="type">Object type (may be abstract)</param>
        /// <returns>Objct</returns>
        public static object? ConvertObjectFromString(this string str, Type type) => Convert(type, str);

        /// <summary>
        /// Named string to value converter
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="name">Conversion name</param>
        /// <param name="type">Object type (may be abstract)</param>
        /// <returns>Object</returns>
        public static object? NamedObjectConversion(this string? str, in string name, in Type type)
        {
            if (!NamedValueConverter.TryGetValue(name, out ValueConverter_Delegate? converter))
                throw new ArgumentException("Unknown conversion", nameof(name));
            object? res = converter.Invoke(type, str);
            if (res is not null && !type.IsAssignableFrom(res.GetType()))
            {
                res.TryDispose();
                throw new InvalidDataException($"Converted value type {res.GetType()} doesn't match the expected type {type}");
            }
            return res;
        }

        /// <summary>
        /// Named value to string converter
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="name">Conversion name</param>
        /// <param name="type">Object type (may be abstract)</param>
        /// <returns>Object</returns>
        public static string? NamedStringConversion(this object? obj, in string name, in Type type)
        {
            if (!NamedStringConverter.TryGetValue(name, out StringConverter_Delegate? converter))
                throw new ArgumentException("Unknown conversion", nameof(name));
            return converter.Invoke(type, obj);
        }

        /// <summary>
        /// Display string to value converter delegate
        /// </summary>
        /// <param name="type">Value type</param>
        /// <param name="str">String</param>
        /// <returns>Value</returns>
        public delegate object? ValueConverter_Delegate(Type type, string? str);
        /// <summary>
        /// Value to display string converter
        /// </summary>
        /// <param name="type">Value type</param>
        /// <param name="value">Value</param>
        /// <returns>String</returns>
        public delegate string? StringConverter_Delegate(Type type, object? value);
    }
}
