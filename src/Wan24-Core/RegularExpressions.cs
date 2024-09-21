using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// Regular expressions
    /// </summary>
    public static partial class RegularExpressions
    {
        /// <summary>
        /// MIME type (<c>$1</c> contains the part before the slash, <c>$2</c> the part after the slash)
        /// </summary>
        public const string MIME_TYPE = @"^(\w+)\/([-.\w]+(?:\+[-.\w]+)?)$";
        /// <summary>
        /// New line (<c>$1</c> contains the new line control characters)
        /// </summary>
        public const string NEW_LINE = @"(\r?\n)";
        /// <summary>
        /// No new line
        /// </summary>
        public const string NO_NEW_LINE = @"[^\r\n]";
        /// <summary>
        /// Locale using a dash or underscore (<c>$1</c> contains the first part, <c>$2</c> the second part)
        /// </summary>
        public const string LOCALE = @"^([a-z]{2})[-_]([A-Z]{2})$";
        /// <summary>
        /// Locale using a dash (<c>$1</c> contains the first part, <c>$2</c> the second part)
        /// </summary>
        public const string LOCALE_WITH_DASH = @"^([a-z]{2})-([A-Z]{2})$";
        /// <summary>
        /// Locale using an underscore (<c>$1</c> contains the first part, <c>$2</c> the second part)
        /// </summary>
        public const string LOCALE_WITH_UNDERSCORE = @"^([a-z]{2})_([A-Z]{2})$";
        /// <summary>
        /// GUID
        /// </summary>
        public const string GUID = @"^[0-9|a-f]{8}-([0-9|a-f]{4}-){3}[0-9|a-f]{12}$";
        /// <summary>
        /// Regular expression to match a possible JSON value
        /// </summary>
        public const string JSON = @"^\s*(null|true|false|\d+(\.\d+)?|\""(\\.|[^\\\""])*[^\\]\""|\[.*\]|\{.*\})\s*$";
        /// <summary>
        /// Regular expression to match a possible Windows path (won't validate!)
        /// </summary>
        public const string WINDOWS_PATH = @"^([a-z][\/\\]\:?|[\/\\]?[a-z][\/\\])[\/\\]";
        /// <summary>
        /// Regular expression to match a possible Linux path (won't validate!)
        /// </summary>
        public const string LINUX_PATH = @"^\/?[^\/]+(\/[^\/]+){1,}\/?$";
        /// <summary>
        /// Regular expression to match a string literal
        /// </summary>
        public const string STRING_LITERAL = @"^\""(\\.|[^\\\""])*[^\\]\""$";
        /// <summary>
        /// Regular expression to match a string which may contain a string parser variable
        /// </summary>
        public const string PARSER_VAR = @"\%\{[^\r\n]+\}";
        /// <summary>
        /// Regular expression to match a hex string
        /// </summary>
        public const string HEX_STRING = @"^[a-f|0-9]+$";
        /// <summary>
        /// Regular expression to match a whitespace
        /// </summary>
        public const string WHITESPACE = @"\s";

        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        private static readonly object SyncObject = new();
        /// <summary>
        /// Named regular expressions
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> _NamedExpressions = new();
        /// <summary>
        /// Named <see cref="Regex"/>
        /// </summary>
        private static readonly ConcurrentDictionary<string, Regex> _NamedRegex = new();
        /// <summary>
        /// MIME type (<c>$1</c> contains the part before the slash, <c>$2</c> the part after the slash)
        /// </summary>
        public static readonly Regex RX_MIME_TYPE = RX_MIME_TYPE_Generated();
        /// <summary>
        /// New line (<c>$1</c> contains the new line control characters)
        /// </summary>
        public static readonly Regex RX_NEW_LINE = RX_NEW_LINE_Generated();
        /// <summary>
        /// No new line
        /// </summary>
        public static readonly Regex RX_NO_NEW_LINE = RX_NO_NEW_LINE_Generated();
        /// <summary>
        /// Locale using a dash or underscore (<c>$1</c> contains the first part, <c>$2</c> the second part)
        /// </summary>
        public static readonly Regex RX_LOCALE = RX_LOCALE_Generated();
        /// <summary>
        /// Locale using a dash (<c>$1</c> contains the first part, <c>$2</c> the second part)
        /// </summary>
        public static readonly Regex RX_LOCALE_WITH_DASH = RX_LOCALE_WITH_DASH_Generated();
        /// <summary>
        /// Locale using an underscore (<c>$1</c> contains the first part, <c>$2</c> the second part)
        /// </summary>
        public static readonly Regex RX_LOCALE_WITH_UNDERSCORE = RX_LOCALE_WITH_UNDERSCORE_Generated();
        /// <summary>
        /// GUID
        /// </summary>
        public static readonly Regex RX_GUID = RX_GUID_Generated();
        /// <summary>
        /// Regular expression to match a possible JSON value
        /// </summary>
        public static readonly Regex RX_JSON = RX_JSON_Generated();
        /// <summary>
        /// Regular expression to match a possible Windows path (won't validate!)
        /// </summary>
        public static readonly Regex RX_WINDOWS_PATH = RX_WINDOWS_PATH_Generated();
        /// <summary>
        /// Regular expression to match a possible Linux path (won't validate!)
        /// </summary>
        public static readonly Regex RX_LINUX_PATH = RX_LINUX_PATH_Generated();
        /// <summary>
        /// Regular expression to match a string literal
        /// </summary>
        public static readonly Regex RX_STRING_LITERAL = RX_STRING_LITERAL_Generated();
        /// <summary>
        /// Regular expression to match a string which may contain a string parser variable
        /// </summary>
        public static readonly Regex RX_PARSER_VAR = RX_PARSER_VAR_Generated();
        /// <summary>
        /// Regular expression to match a hex string
        /// </summary>
        public static readonly Regex RX_HEX_STRING = RX_HEX_STRING_Generated();
        /// <summary>
        /// Regular expression to match a whitespace
        /// </summary>
        public static readonly Regex RX_WHITESPACE = RX_WHITESPACE_Generated();

        /// <summary>
        /// Named expression names
        /// </summary>
        public static string[] NamedExpressionNames => [.. _NamedExpressions.Keys];

        /// <summary>
        /// Number of named expressions
        /// </summary>
        public static int NamedExpressionCount => _NamedExpressions.Count;

        /// <summary>
        /// Named expressions as dictionary
        /// </summary>
        public static Dictionary<string, string> NamedExpressions => new(_NamedExpressions);

        /// <summary>
        /// Named <see cref="Regex"/> as dictionary
        /// </summary>
        public static Dictionary<string, Regex> NamedRegex => new(_NamedRegex);

        /// <summary>
        /// Add an expression
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="expression">Expression</param>
        /// <param name="options">Options</param>
        /// <returns><see cref="Regex"/></returns>
        public static Regex Add(string name, string expression, RegexOptions options = RegexOptions.Compiled)
        {
            lock (SyncObject)
            {
                _NamedExpressions[name] = expression;
                return _NamedRegex[name] = new(expression, options);
            }
        }

        /// <summary>
        /// Remove an expression
        /// </summary>
        /// <param name="name">Name</param>
        public static void Remove(string name)
        {
            lock (SyncObject)
            {
                _NamedRegex.TryRemove(name, out _);
                _NamedExpressions.TryRemove(name, out _);
            }
        }

        /// <summary>
        /// Get an expression pattern
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Pattern</returns>
        public static string? GetPattern(string name) => _NamedExpressions.TryGetValue(name, out string? res) ? res : null;

        /// <summary>
        /// Get a regular expression
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns><see cref="Regex"/></returns>
        public static Regex? GetRegex(string name) => _NamedRegex.TryGetValue(name, out Regex? res) ? res : null;

        /// <summary>
        /// Determine if a named expression exists
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>If the expression exists</returns>
        public static bool Contains(string name) => _NamedRegex.ContainsKey(name);

        /// <summary>
        /// MIME type (<c>$1</c> contains the part before the slash, <c>$2</c> the part after the slash)
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(MIME_TYPE, RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RX_MIME_TYPE_Generated();

        /// <summary>
        /// New line (<c>$1</c> contains the new line control characters)
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(NEW_LINE, RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RX_NEW_LINE_Generated();

        /// <summary>
        /// No new line
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(NO_NEW_LINE, RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RX_NO_NEW_LINE_Generated();

        /// <summary>
        /// Locale using a dash or underscore (<c>$1</c> contains the first part, <c>$2</c> the second part)
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(LOCALE, RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RX_LOCALE_Generated();

        /// <summary>
        /// Locale using a dash (<c>$1</c> contains the first part, <c>$2</c> the second part)
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(LOCALE_WITH_DASH, RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RX_LOCALE_WITH_DASH_Generated();

        /// <summary>
        /// Locale using an underscore (<c>$1</c> contains the first part, <c>$2</c> the second part)
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(LOCALE_WITH_UNDERSCORE, RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RX_LOCALE_WITH_UNDERSCORE_Generated();

        /// <summary>
        /// GUID
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(GUID, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RX_GUID_Generated();

        /// <summary>
        /// Regular expression to match a possible JSON value
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(JSON, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled, 3000)]
        private static partial Regex RX_JSON_Generated();

        /// <summary>
        /// Regular expression to match a possible Windows path (won't validate!)
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(WINDOWS_PATH, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled, 3000)]
        private static partial Regex RX_WINDOWS_PATH_Generated();

        /// <summary>
        /// Regular expression to match a possible Linux path (won't validate!)
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(LINUX_PATH, RegexOptions.Singleline | RegexOptions.Compiled, 3000)]
        private static partial Regex RX_LINUX_PATH_Generated();

        /// <summary>
        /// Regular expression to match a string literal
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(STRING_LITERAL, RegexOptions.Singleline | RegexOptions.Compiled, 3000)]
        private static partial Regex RX_STRING_LITERAL_Generated();

        /// <summary>
        /// Regular expression to match a string which may contain a string parser variable
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(PARSER_VAR, RegexOptions.Singleline | RegexOptions.Compiled, 3000)]
        private static partial Regex RX_PARSER_VAR_Generated();

        /// <summary>
        /// Regular expression to match a hex string
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(HEX_STRING, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled, 3000)]
        private static partial Regex RX_HEX_STRING_Generated();

        /// <summary>
        /// Regular expression to match a whitespace
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(WHITESPACE, RegexOptions.Singleline | RegexOptions.Compiled, 3000)]
        private static partial Regex RX_WHITESPACE_Generated();
    }
}
