using System.Collections.ObjectModel;

namespace wan24.Core
{
    // Casting
    public partial class CliArguments
    {
        /// <summary>
        /// Cast as arguments-flag
        /// </summary>
        /// <param name="args">Arguments</param>
        public static implicit operator bool(in CliArguments args) => args.Count > 0;

        /// <summary>
        /// Cast as arguments count
        /// </summary>
        /// <param name="args">Arguments</param>
        public static implicit operator int(in CliArguments args) => args.Count;

        /// <summary>
        /// Cast as escaped arguments string
        /// </summary>
        /// <param name="args">Arguments</param>
        public static implicit operator string(in CliArguments args) => args.ToString();

        /// <summary>
        /// Cast from string
        /// </summary>
        /// <param name="str">String</param>
        public static explicit operator CliArguments(in Span<char> str) => Parse(str);

        /// <summary>
        /// Cast from string
        /// </summary>
        /// <param name="str">String</param>
        public static explicit operator CliArguments(in ReadOnlySpan<char> str) => Parse(str);

        /// <summary>
        /// Cast from string
        /// </summary>
        /// <param name="str">String</param>
        public static explicit operator CliArguments(in Memory<char> str) => Parse(str.Span);

        /// <summary>
        /// Cast from string
        /// </summary>
        /// <param name="str">String</param>
        public static explicit operator CliArguments(in ReadOnlyMemory<char> str) => Parse(str.Span);

        /// <summary>
        /// Cast from string
        /// </summary>
        /// <param name="str">String</param>
        public static explicit operator CliArguments(in string str) => Parse(str);

        /// <summary>
        /// Cast from strings
        /// </summary>
        /// <param name="str">Strings</param>
        public static explicit operator CliArguments(in Span<string> str) => new(str);

        /// <summary>
        /// Cast from strings
        /// </summary>
        /// <param name="str">Strings</param>
        public static explicit operator CliArguments(in ReadOnlySpan<string> str) => new(str);

        /// <summary>
        /// Cast from strings
        /// </summary>
        /// <param name="str">Strings</param>
        public static explicit operator CliArguments(in Memory<string> str) => new(str.Span);

        /// <summary>
        /// Cast from strings
        /// </summary>
        /// <param name="str">Strings</param>
        public static explicit operator CliArguments(in ReadOnlyMemory<string> str) => new(str.Span);

        /// <summary>
        /// Cast from strings
        /// </summary>
        /// <param name="str">Strings</param>
        public static explicit operator CliArguments(in string[] str) => new(str);

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(in Dictionary<string, ReadOnlyCollection<string>> dict) => new(dict);

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(in Dictionary<string, IEnumerable<string>> dict)
            => new(from kvp in dict
                   select new KeyValuePair<string, ReadOnlyCollection<string>>(kvp.Key, kvp.Value.AsReadOnly()));

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(in Dictionary<string, string[]> dict)
            => new(from kvp in dict
                   select new KeyValuePair<string, ReadOnlyCollection<string>>(kvp.Key, kvp.Value.AsReadOnly()));

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(in Dictionary<string, List<string>> dict)
            => new(from kvp in dict
                   select new KeyValuePair<string, ReadOnlyCollection<string>>(kvp.Key, kvp.Value.AsReadOnly()));

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(in OrderedDictionary<string, ReadOnlyCollection<string>> dict) => new(dict);

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(in OrderedDictionary<string, IEnumerable<string>> dict)
            => new(from kvp in dict
                   select new KeyValuePair<string, ReadOnlyCollection<string>>(kvp.Key, kvp.Value.AsReadOnly()));

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(in OrderedDictionary<string, string[]> dict)
            => new(from kvp in dict
                   select new KeyValuePair<string, ReadOnlyCollection<string>>(kvp.Key, kvp.Value.AsReadOnly()));

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(in OrderedDictionary<string, List<string>> dict)
            => new(from kvp in dict
                   select new KeyValuePair<string, ReadOnlyCollection<string>>(kvp.Key, kvp.Value.AsReadOnly()));
    }
}
