using System.Buffers.Text;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace wan24.Core
{
    /// <summary>
    /// (Display) String to value conversion
    /// </summary>
    public static partial class StringValueConverter
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
        /// String value converter name
        /// </summary>
        public const string STRING_CONVERTER_NAME = "STRING";

        /// <summary>
        /// <see cref="Marshal.PtrToStructure{T}(nint)"/> method
        /// </summary>
        private static readonly MethodInfoExt MarshalStructureMethod;
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
            MarshalStructureMethod = typeof(Marshal).GetMethodsCached(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(Marshal.PtrToStructure) && m.Parameters.Length == 1);
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
            ValueConverter[typeof(UInt128)] = (t, s) => s is not null && UInt128.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(Int128)] = (t, s) => s is not null && Int128.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(Half)] = (t, s) => s is not null && Half.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(float)] = (t, s) => s is not null && float.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(double)] = (t, s) => s is not null && double.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(decimal)] = (t, s) => s is not null && decimal.TryParse(s, out var res) ? res : null;
            ValueConverter[typeof(BigInteger)] = (t, s) => s is not null && BigInteger.TryParse(s, out var res) ? res : null;
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
            ValueConverter[typeof(ISerializeString)] = (t, s) => s is null ? null : t.ParseObject(s);
            ValueConverter[typeof(ISerializeBinary)] = (t, s) =>
            {
                if (s is not string str) return null;
                ReadOnlySpan<char> chars = str;
                using RentedMemoryRef<byte> buffer = new(len: Encoding.UTF8.GetByteCount(chars), clean: false);
                Span<byte> bufferSpan = buffer.Span;
                return t.DeserializeFrom(bufferSpan[..chars.GetBase64Bytes(bufferSpan)]);
            };
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
            StringConverter[typeof(UInt128)] = (t, v) => v?.ToString();
            StringConverter[typeof(Int128)] = (t, v) => v?.ToString();
            StringConverter[typeof(Half)] = (t, v) => v?.ToString();
            StringConverter[typeof(float)] = (t, v) => v?.ToString();
            StringConverter[typeof(double)] = (t, v) => v?.ToString();
            StringConverter[typeof(decimal)] = (t, v) => v?.ToString();
            StringConverter[typeof(BigInteger)] = (t, v) => v?.ToString();
            StringConverter[typeof(DateTime)] = (t, v) => v?.ToString();
            StringConverter[typeof(DateOnly)] = (t, v) => v?.ToString();
            StringConverter[typeof(TimeOnly)] = (t, v) => v?.ToString();
            StringConverter[typeof(TimeSpan)] = (t, v) => v?.ToString();
            StringConverter[typeof(Regex)] = (t, v) => v is not Regex rx ? null : $"{(int)rx.Options} {rx}";
            StringConverter[typeof(Enum)] = (t, v) => v?.ToString();
            StringConverter[typeof(Uri)] = (t, v) => v?.ToString();
            StringConverter[typeof(Guid)] = (t, v) => v?.ToString();
            StringConverter[typeof(XmlDocument)] = (t, v) => (v as XmlDocument)?.OuterXml;
            StringConverter[typeof(XmlNode)] = (t, v) => (v as XmlNode)?.OuterXml;
            StringConverter[typeof(ISerializeString)] = (t, v) => v?.ToString();
            StringConverter[typeof(ISerializeBinary)] = (t, v) =>
            {
                if (v is null) return null;
                if (v is not ISerializeBinary serializeBinary) throw new InvalidCastException();
                if (serializeBinary.StructureSize is not int size) return serializeBinary.GetBytes().GetBase64String();
                using RentedMemoryRef<byte> buffer = new(len: Base64.GetMaxEncodedToUtf8Length(size), clean: false);
                Span<byte> bufferSpan = buffer.Span;
                return System.Convert.ToBase64String(bufferSpan[..serializeBinary.GetBytes(bufferSpan)]);
            };
            NamedValueConverter[JSON_CONVERTER_NAME] = (t, s) => s is null ? null : JsonHelper.DecodeObject(t, s);
            NamedValueConverter[XML_CONVERTER_NAME] = (t, s) =>
            {
                if (s is null) return null;
                using MemoryPoolStream ms = new();
                ms.Write(s.GetBytes());
                ms.Position = 0;
                XmlSerializer serializer = new(t);
                return serializer.Deserialize(ms);
            };
            NamedValueConverter[STRING_CONVERTER_NAME] = (t, s) => s is null ? null : ConvertObjectFromString(s, t);
            NamedStringConverter[JSON_CONVERTER_NAME] = (t, v) => JsonHelper.Encode(v);
            NamedStringConverter[XML_CONVERTER_NAME] = (t, v) =>
            {
                using MemoryPoolStream ms = new();
                XmlSerializer serializer = new(t);
                serializer.Serialize(ms, v);
                ms.Position = 0;
                using RentedMemoryRef<byte> buffer = new((int)ms.Length, clean: false);
                Span<byte> bufferSpan = buffer.Span;
                ms.ReadExactly(bufferSpan);
                return bufferSpan.ToUtf8String();
            };
            NamedStringConverter[STRING_CONVERTER_NAME] = (t, v) => v is null ? null : ConvertObjectToString(v);
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
