using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// String parser options
    /// </summary>
    public sealed class StringParserOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public StringParserOptions() { }

        /// <summary>
        /// Maximum recursive parser rounds
        /// </summary>
        public int? MaxParserRounds { get; set; }

        /// <summary>
        /// Placeholder regular expression
        /// </summary>
        public Regex? Regex { get; set; }

        /// <summary>
        /// Regular expression group to use
        /// </summary>
        public int? RegexGroup { get; set; }

        /// <summary>
        /// Throw an exception on error?
        /// </summary>
        public bool? ThrowOnError { get; set; } = true;
    }
}
