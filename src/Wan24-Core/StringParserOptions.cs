using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// String parser options
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public sealed record class StringParserOptions()
    {
        /// <summary>
        /// Maximum recursive parser rounds
        /// </summary>
        public int? MaxParserRounds { get; set; }

        /// <summary>
        /// Regular expression to parse a string (<c>$1</c> is the whole placeholder, <c>$2</c> the inner variable declaration)
        /// </summary>
        public Regex? Regex { get; set; }

        /// <summary>
        /// Regular expression content group (2 per default)
        /// </summary>
        public int? RegexGroup { get; set; }

        /// <summary>
        /// Throw an exception on error?
        /// </summary>
        public bool? ThrowOnError { get; set; } = true;
    }
}
