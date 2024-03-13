using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using static wan24.Core.Logger;
using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Path matching helper
    /// </summary>
    public partial record class PathMatching : IEqualityComparer<string>
    {
        /// <summary>
        /// Regular expression to match a one or many wildcard (<c>$1</c> is the prefix, <c>$2</c> the postfix)
        /// </summary>
        protected static readonly Regex RxOneOrMany = RxOneOrMany_Generator();

        /// <summary>
        /// Names
        /// </summary>
        protected readonly FrozenSet<string>? Names;
        /// <summary>
        /// Partials
        /// </summary>
        protected readonly FrozenSet<string>? Partials;
        /// <summary>
        /// Paths
        /// </summary>
        protected readonly FrozenSet<string>? Paths;
        /// <summary>
        /// Regular expression
        /// </summary>
        protected readonly Regex? Expression;
        /// <summary>
        /// If there are any matchings
        /// </summary>
        protected readonly bool AnyMatchings;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="patterns">Patterns (absolute or partial path or file-/foldername only (\"*\" (any or none) and \"+\" (one or many) may be used as wildcard); case insensitive)</param>
        public PathMatching(in string[] patterns)
        {
            if (patterns.Length > 0)
            {
                if (Trace) WriteTrace($"Handling {patterns.Length} patterns");
                HashSet<string> rx = [],
                    names = [],
                    partials = [],
                    paths = [];
                foreach (string expression in patterns)
                    if (expression.Contains('*'))
                    {
                        if (Trace) WriteTrace($"Adding \"{expression}\" to the regular expression");
                        rx.Add(expression);
                    }
                    else if (expression.StartsWith('/') || RegularExpressions.RX_WINDOWS_PATH.IsMatch(expression))
                    {
                        if (Trace) WriteTrace($"Adding \"{expression}\" to the paths");
                        paths.Add($"{Path.GetFullPath(expression)}{(ENV.IsWindows ? "\\" : "/")}");
                    }
                    else if (expression.ContainsAny('/', '\\'))
                    {
                        if (Trace) WriteTrace($"Adding \"{expression}\" to the partials");
                        partials.Add(expression);
                    }
                    else
                    {
                        if (Trace) WriteTrace($"Adding \"{expression}\" to the names");
                        names.Add(expression);
                    }
                if (names.Count > 0) Names = names.ToFrozenSet();
                if (partials.Count > 0) Partials = partials.ToFrozenSet();
                if (paths.Count > 0) Paths = paths.ToFrozenSet();
                if (rx.Count > 0)
                {
                    if (Trace) WriteTrace($"Building regular expression from {rx.Count} patterns");
                    string regex = Regex.Escape(string.Join('|', rx)).Replace("\\*", ".*").Replace("\\|", "|");
                    regex = RxOneOrMany.Replace(regex, "$1.$2");
                    Expression = new(regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                else
                {
                    if (Trace) WriteTrace("No regular expression");
                    Expression = null;
                }
                AnyMatchings = true;
            }
            else
            {
                if (Trace) WriteTrace("No paths will be matched");
                Names = null;
                Partials = null;
                Paths = null;
                Expression = null;
                AnyMatchings = false;
            }
        }

        /// <summary>
        /// Match a filename in any folder?
        /// </summary>
        public bool MatchFileName { get; set; } = true;

        /// <summary>
        /// Match partial paths?
        /// </summary>
        public bool MatchPartialPaths { get; set; } = true;

        /// <summary>
        /// Allow wildcards usage?
        /// </summary>
        public bool AllowWildcards { get; set; } = true;

        /// <summary>
        /// Determine if a path is matched
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>If matched</returns>
        public virtual bool IsMatch(string path)
            => AnyMatchings &&
                (
                    (MatchFileName && (Names?.Contains(Path.GetFileName(path), this) ?? false)) ||
                    (Paths?.Any(p => path.StartsWith(path, StringComparison.OrdinalIgnoreCase)) ?? false) ||
                    (MatchPartialPaths && (Partials?.Any(p => path.Contains(p, StringComparison.OrdinalIgnoreCase)) ?? false)) ||
                    (AllowWildcards && (Expression?.IsMatch(path) ?? false))
                );

        /// <inheritdoc/>
        public virtual bool Equals(string? x, string? y)
            => (x is null && y is null) ||
                (
                    x is not null &&
                    y is not null &&
                    x.Length == y.Length &&
                    x.Equals(y, StringComparison.OrdinalIgnoreCase)
                );

        /// <inheritdoc/>
        public virtual int GetHashCode([DisallowNull] string obj) => obj.ToLower().GetHashCode();

        /// <summary>
        /// Regular expression to match a one or many wildcard (<c>$1</c> is the prefix, <c>$2</c> the postfix)
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"^(.*[^\\])?\\([\*\+].*)$", RegexOptions.Compiled)]
        private static partial Regex RxOneOrMany_Generator();
    }
}
