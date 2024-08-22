using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// String parser context information
    /// </summary>
    public sealed class StringParserContext
    {
        /// <summary>
        /// Constructor
        /// </summary>
        internal StringParserContext() { }

        /// <summary>
        /// String
        /// </summary>
        public required string String { get; init; }

        /// <summary>
        /// Regular expression to parse a string (<c>$1</c> is the whole placeholder, <c>$2</c> the inner variable declaration)
        /// </summary>
        public required Regex Rx { get; set; }

        /// <summary>
        /// Regular expression content group (2 per default)
        /// </summary>
        public required int RxGroup { get; set; }

        /// <summary>
        /// Current value
        /// </summary>
        public string Value { get; internal set; } = null!;

        /// <summary>
        /// Matches
        /// </summary>
        public required MatchCollection Matches { get; init; } = null!;

        /// <summary>
        /// Current match
        /// </summary>
        public required Match M { get; init; } = null!;

        /// <summary>
        /// Parser data
        /// </summary>
        public required Dictionary<string, string> Data { get; init; } = null!;

        /// <summary>
        /// Parsed data (key is the placeholder)
        /// </summary>
        public required Dictionary<string, string> Parsed { get; init; } = null!;

        /// <summary>
        /// Current parser round (zero based)
        /// </summary>
        public required int Round { get; init; }

        /// <summary>
        /// Maximum number of parser rounds
        /// </summary>
        public required int MaxRounds { get; init; }

        /// <summary>
        /// Current match parts
        /// </summary>
        public required string[] Match { get; init; } = null!;

        /// <summary>
        /// Current function call match part index (zero based)
        /// </summary>
        public int Index { get; internal set; }

        /// <summary>
        /// Current function name
        /// </summary>
        public string Func { get; internal set; } = null!;

        /// <summary>
        /// Current function call parameters
        /// </summary>
        public string[] Param { get; internal set; } = null!;

        /// <summary>
        /// Error message
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Ensure a valid parameter count
        /// </summary>
        /// <param name="count">Allowed parameter counts</param>
        /// <returns>If the parameter count is valid</returns>
        public bool EnsureValidParameterCount(params int[] count)
        {
            if (Param.Length.In(count)) return true;
            if (Param.Length == 0)
            {
                Error = $"Missing parameters ({string.Join("/", count)} parameter(s) required)";
                return false;
            }
            Error = $"Invalid parameter count {Param.Length} ({string.Join("/", count)} parameter(s) allowed)";
            return false;
        }

        /// <summary>
        /// Try get parser data
        /// </summary>
        /// <param name="key">Key (including <c>$</c> prefix!)</param>
        /// <param name="value">Value</param>
        /// <returns>Succeed?</returns>
        public bool TryGetData(in string key, [NotNullWhen(returnValue: true)] out string? value)
        {
            if (!key.StartsWith('$'))
            {
                value = key;
                return true;
            }
            if (Data.TryGetValue(key[1..], out value)) return true;
            Error = $"Unknown parser data key {key[1..].ToQuotedLiteral()}";
            return false;
        }
    }
}
