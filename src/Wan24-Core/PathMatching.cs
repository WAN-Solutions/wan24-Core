using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using static wan24.Core.Logger;
using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Path matching helper (path separator will be normalized to <c>/</c>)
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
        protected readonly FrozenSet<string>? Names = null;
        /// <summary>
        /// Partials
        /// </summary>
        protected readonly FrozenSet<string>? Partials = null;
        /// <summary>
        /// Paths
        /// </summary>
        protected readonly FrozenSet<string>? Paths = null;
        /// <summary>
        /// Regular expression
        /// </summary>
        protected readonly Regex? Expression = null;
        /// <summary>
        /// If there are any matchings
        /// </summary>
        protected readonly bool AnyMatchings;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="patterns">Patterns (absolute or partial path or file-/foldername only (\"*\" (any or none) and \"+\" (one or many) may be used as wildcard); 
        /// case insensitive)</param>
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
                    if (expression.ContainsAny('*','+'))
                    {
                        if (Trace) WriteTrace($"Adding \"{expression}\" to the regular expression");
                        rx.Add(FsHelper.NormalizeLinuxDisplayPath(expression));
                    }
                    else if (expression.StartsWith('/') || RegularExpressions.RX_WINDOWS_PATH.IsMatch(expression))
                    {
                        if (Trace) WriteTrace($"Adding \"{expression}\" to the paths");
                        paths.Add(FsHelper.NormalizeLinuxDisplayPath($"{Path.GetFullPath(expression)}/"));
                    }
                    else if (expression.ContainsAny('/', '\\'))
                    {
                        if (Trace) WriteTrace($"Adding \"{expression}\" to the partials");
                        partials.Add(FsHelper.NormalizeLinuxDisplayPath(expression));
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
                    string pattern = $"^{string.Join('|', rx.Select(Regex.Escape))}$";
                    while (RxOneOrMany.IsMatch(pattern)) pattern = RxOneOrMany.Replace(pattern, "$1.$2");
                    Expression = new(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                else if(Trace)
                {
                    WriteTrace("No regular expression");
                }
                AnyMatchings = true;
            }
            else
            {
                if (Trace) WriteTrace("No paths will be matched");
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
        {
            if (!AnyMatchings) return false;
            path = FsHelper.NormalizeLinuxDisplayPath(path);
            return (MatchFileName && (Names?.Contains(Path.GetFileName(path), this) ?? false)) ||
                (Paths?.Any(p => path.StartsWith(path, StringComparison.OrdinalIgnoreCase)) ?? false) ||
                (MatchPartialPaths && (Partials?.Any(p => path.Contains(p, StringComparison.OrdinalIgnoreCase)) ?? false)) ||
                (AllowWildcards && (Expression?.IsMatch(path) ?? false));
        }

        /// <summary>
        /// Compare two strings
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>If the strings are equal (ignoring the case)</returns>
        protected virtual bool Equals(string? x, string? y)
            => (x is null && y is null) ||
                (
                    x is not null &&
                    y is not null &&
                    x.Length == y.Length &&
                    x.Equals(y, StringComparison.OrdinalIgnoreCase)
                );

        /// <summary>
        /// Get the hash code for a string (case ignoring)
        /// </summary>
        /// <param name="obj">String</param>
        /// <returns>Hash code</returns>
        protected virtual int GetHashCode([DisallowNull] string obj) => obj.ToLower().GetHashCode();

        /// <inheritdoc/>
        bool IEqualityComparer<string>.Equals(string? x, string? y) => Equals(x, y);

        /// <inheritdoc/>
        int IEqualityComparer<string>.GetHashCode(string obj) => GetHashCode(obj);

        /// <summary>
        /// Regular expression to match a one or many wildcard (<c>$1</c> is the prefix, <c>$2</c> the postfix)
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"^(.*[^\\])?\\([\*\+].*)$", RegexOptions.Compiled)]
        private static partial Regex RxOneOrMany_Generator();
    }
}
