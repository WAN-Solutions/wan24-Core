using System.Reflection;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// (Display) String to value conversion
    /// </summary>
    public static class StringValueConverter
    {
        /// <summary>
        /// Display string to value converter
        /// </summary>
        public static readonly Dictionary<Type, ValueConverter_Delegate> ValueConverter = [];
        /// <summary>
        /// Value to display string converter
        /// </summary>
        public static readonly Dictionary<Type, StringConverter_Delegate> StringConverter = [];

        /// <summary>
        /// Constructor
        /// </summary>
        static StringValueConverter()
        {
            ValueConverter[typeof(string)] = (t, s) => s;
            ValueConverter[typeof(bool)] = (t, s) => bool.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(byte)] = (t, s) => byte.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(sbyte)] = (t, s) => sbyte.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(ushort)] = (t, s) => ushort.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(short)] = (t, s) => short.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(uint)] = (t, s) => uint.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(int)] = (t, s) => int.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(ulong)] = (t, s) => ulong.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(long)] = (t, s) => long.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(Half)] = (t, s) => Half.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(float)] = (t, s) => float.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(double)] = (t, s) => double.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(decimal)] = (t, s) => decimal.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(DateTime)] = (t, s) => DateTime.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(DateOnly)] = (t, s) => DateOnly.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(TimeOnly)] = (t, s) => TimeOnly.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(Regex)] = (t, s) =>
            {
                if (s is null) return null;
                string[] info = s.Split(' ', 2);
                if (info.Length != 2) throw new ArgumentException("Invalid regular expression string representation", nameof(s));
                return new Regex(info[1], (RegexOptions)int.Parse(info[0]));
            };
            ValueConverter[typeof(Enum)] = (t, s) => Enum.TryParse(t, s, out var res) ? res : null;
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
            StringConverter[typeof(Regex)] = (t, v) => v is not Regex rx ? null : $"{(int)rx.Options} {rx}";
            StringConverter[typeof(Enum)] = (t, v) => v?.ToString();
        }

        /// <summary>
        /// Convert a display string to a value
        /// </summary>
        /// <param name="type">Value type (may be abstract)</param>
        /// <param name="str">String</param>
        /// <returns>Value</returns>
        public static object? Convert(Type type, in string? str)
        {
            if (!ValueConverter.TryGetValue(type, out ValueConverter_Delegate? converter))
            {
                converter = ValueConverter.FirstOrDefault(kvp => kvp.Key.IsAssignableFrom(type)).Value;
                if (converter == default)
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
            if (!StringConverter.TryGetValue(type, out StringConverter_Delegate? converter))
            {
                converter = StringConverter.FirstOrDefault(kvp => kvp.Key.IsAssignableFrom(type)).Value;
                if (converter == default)
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
