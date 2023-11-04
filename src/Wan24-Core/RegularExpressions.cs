using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// Regular expressions
    /// </summary>
    public static class RegularExpressions
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
        public static readonly Regex RX_MIME_TYPE = new(MIME_TYPE, RegexOptions.Compiled | RegexOptions.Singleline);
        /// <summary>
        /// New line (<c>$1</c> contains the new line control characters)
        /// </summary>
        public static readonly Regex RX_NEW_LINE = new(NEW_LINE, RegexOptions.Compiled | RegexOptions.Singleline);
        /// <summary>
        /// No new line
        /// </summary>
        public static readonly Regex RX_NO_NEW_LINE = new(NO_NEW_LINE, RegexOptions.Compiled | RegexOptions.Singleline);
        /// <summary>
        /// Locale using a dash or underscore (<c>$1</c> contains the first part, <c>$2</c> the second part)
        /// </summary>
        public static readonly Regex RX_LOCALE = new(LOCALE, RegexOptions.Compiled | RegexOptions.Singleline);
        /// <summary>
        /// Locale using a dash (<c>$1</c> contains the first part, <c>$2</c> the second part)
        /// </summary>
        public static readonly Regex RX_LOCALE_WITH_DASH = new(LOCALE_WITH_DASH, RegexOptions.Compiled | RegexOptions.Singleline);
        /// <summary>
        /// Locale using an underscore (<c>$1</c> contains the first part, <c>$2</c> the second part)
        /// </summary>
        public static readonly Regex RX_LOCALE_WITH_UNDERSCORE = new(LOCALE_WITH_UNDERSCORE, RegexOptions.Compiled | RegexOptions.Singleline);
        /// <summary>
        /// GUID
        /// </summary>
        public static readonly Regex RX_GUID = new(GUID, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Named expression names
        /// </summary>
        public static string[] NamedExpressionNames => _NamedExpressions.Keys.ToArray();

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
    }
}
