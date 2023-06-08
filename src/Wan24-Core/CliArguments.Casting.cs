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
        public static implicit operator bool(CliArguments args) => args.Count > 0;

        /// <summary>
        /// Cast as arguments count
        /// </summary>
        /// <param name="args">Arguments</param>
        public static implicit operator int(CliArguments args) => args.Count;

        /// <summary>
        /// Cast as escaped arguments string
        /// </summary>
        /// <param name="args">Arguments</param>
        public static implicit operator string(CliArguments args) => args.ToString();

        /// <summary>
        /// Cast from string
        /// </summary>
        /// <param name="str">String</param>
        public static explicit operator CliArguments(Span<char> str) => Parse(str);

        /// <summary>
        /// Cast from string
        /// </summary>
        /// <param name="str">String</param>
        public static explicit operator CliArguments(ReadOnlySpan<char> str) => Parse(str);

        /// <summary>
        /// Cast from string
        /// </summary>
        /// <param name="str">String</param>
        public static explicit operator CliArguments(Memory<char> str) => Parse(str.Span);

        /// <summary>
        /// Cast from string
        /// </summary>
        /// <param name="str">String</param>
        public static explicit operator CliArguments(ReadOnlyMemory<char> str) => Parse(str.Span);

        /// <summary>
        /// Cast from string
        /// </summary>
        /// <param name="str">String</param>
        public static explicit operator CliArguments(string str) => Parse(str);

        /// <summary>
        /// Cast from strings
        /// </summary>
        /// <param name="str">Strings</param>
        public static explicit operator CliArguments(Span<string> str) => new(str);

        /// <summary>
        /// Cast from strings
        /// </summary>
        /// <param name="str">Strings</param>
        public static explicit operator CliArguments(ReadOnlySpan<string> str) => new(str);

        /// <summary>
        /// Cast from strings
        /// </summary>
        /// <param name="str">Strings</param>
        public static explicit operator CliArguments(Memory<string> str) => new(str.Span);

        /// <summary>
        /// Cast from strings
        /// </summary>
        /// <param name="str">Strings</param>
        public static explicit operator CliArguments(ReadOnlyMemory<string> str) => new(str.Span);

        /// <summary>
        /// Cast from strings
        /// </summary>
        /// <param name="str">Strings</param>
        public static explicit operator CliArguments(string[] str) => new(str);

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(Dictionary<string, ReadOnlyCollection<string>> dict) => new(dict);

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(Dictionary<string, IEnumerable<string>> dict)
            => new(from kvp in dict
                   select new KeyValuePair<string, ReadOnlyCollection<string>>(kvp.Key, kvp.Value.ToList().AsReadOnly()));

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(Dictionary<string, string[]> dict)
            => new(from kvp in dict
                   select new KeyValuePair<string, ReadOnlyCollection<string>>(kvp.Key, kvp.Value.ToList().AsReadOnly()));

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(Dictionary<string, List<string>> dict)
            => new(from kvp in dict
                   select new KeyValuePair<string, ReadOnlyCollection<string>>(kvp.Key, kvp.Value.AsReadOnly()));

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(OrderedDictionary<string, ReadOnlyCollection<string>> dict) => new(dict);

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(OrderedDictionary<string, IEnumerable<string>> dict)
            => new(from kvp in dict
                   select new KeyValuePair<string, ReadOnlyCollection<string>>(kvp.Key, kvp.Value.ToList().AsReadOnly()));

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(OrderedDictionary<string, string[]> dict)
            => new(from kvp in dict
                   select new KeyValuePair<string, ReadOnlyCollection<string>>(kvp.Key, kvp.Value.ToList().AsReadOnly()));

        /// <summary>
        /// Cast from dictionary
        /// </summary>
        /// <param name="dict">Dictionary</param>
        public static explicit operator CliArguments(OrderedDictionary<string, List<string>> dict)
            => new(from kvp in dict
                   select new KeyValuePair<string, ReadOnlyCollection<string>>(kvp.Key, kvp.Value.AsReadOnly()));
    }
}
