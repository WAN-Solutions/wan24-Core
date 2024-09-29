using System.Collections.Immutable;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace wan24.Core
{
    // Format
    public partial class CliArguments
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new();
            ReadOnlySpan<char> key;
            bool first;
            int i,
                len;
            foreach (KeyValuePair<string, ImmutableArray<string>> kvp in _Arguments)
                if (kvp.Value.Length == 0)
                {
                    sb.Append('-');
                    sb.Append(SanitizeValue(kvp.Key));
                    sb.Append(' ');
                }
                else
                {
                    key = SanitizeValue(kvp.Key);
                    sb.Append('-', 2);
                    sb.Append(key);
                    first = true;
                    for (i = 0, len = kvp.Value.Length; i < len; i++)
                    {
                        sb.Append(' ');
                        if (!first && kvp.Value[i].Length != 0 && kvp.Value[i][0] == '-')
                        {
                            sb.Append('-', 2);
                            sb.Append(key);
                            sb.Append(' ');
                        }
                        sb.Append(SanitizeValue(kvp.Value[i]));
                        first = false;
                    }
                    sb.Append(' ');
                }
            if (sb.Length == 0) return string.Empty;
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        /// <summary>
        /// Initialize the instance
        /// </summary>
        /// <param name="args">Arguments</param>
        protected void Initialize(in ReadOnlySpan<string> args)
        {
            this.EnsureValidState(KeyLessArguments == default, "Initialized already");
            Dictionary<string, List<string>> a = [];
            List<string> keyLess = [];
            string? lastKey = null;
            bool requireValue = false;
            int i = 0;
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            void HandleDashFlag()
            {
                if (a.TryGetValue("-", out List<string>? v))
                {
                    // Ensure the "dash-flag" isn't a value list
                    if (v.Count != 0) throw new FormatException($"Argument \"-\" was a value list first - can't convert to boolean (at #{i})");
                }
                else
                {
                    // Add the "dash-flag"
                    a["-"] = [];
                }
                a.Remove(string.Empty);// Remove the wrong entry
                requireValue = false;
            }
            for (int len; i < args.Length; i++)
            {
                len = args[i].Length;
                if (requireValue)
                {
                    // Value for a previously defined key
                    if (lastKey!.Length != 0 || len == 0 || args[i][0] != '-' || a[string.Empty].Count != 0)
                    {
                        // Store the value
                        a[lastKey!].Add(args[i]);
                        requireValue = false;
                    }
                    else
                    {
                        // Handle a "dash-flag"
                        HandleDashFlag();
                    }
                }
                else if (len >= 1 && args[i][0] == '-')
                {
                    // New key
                    if (len >= 2 && args[i][1] == '-')
                    {
                        // New key/value(s)
                        lastKey = args[i][2..];
                        if (a.TryGetValue(lastKey, out List<string>? v))
                        {
                            if (v.Count == 0) throw new FormatException($"Argument \"--{lastKey}\" was boolean first - can't convert to value list (at #{i})");
                        }
                        else
                        {
                            a[lastKey] = [];
                        }
                        requireValue = true;
                    }
                    else
                    {
                        // New flag
                        lastKey = args[i][1..];
                        if (a.TryGetValue(lastKey, out List<string>? v))
                        {
                            if (v.Count != 0) throw new FormatException($"Argument \"-{lastKey}\" was a value list first - can't convert to boolean (at #{i})");
                        }
                        else
                        {
                            a[lastKey] = [];
                        }
                    }
                }
                else if (lastKey is not null)
                {
                    if (a[lastKey].Count == 0)
                    {
                        // Keyless value
                        keyLess.Add(args[i]);
                    }
                    else
                    {
                        // Additional value
                        a[lastKey].Add(args[i]);
                    }
                }
                else
                {
                    // Keyless value
                    keyLess.Add(args[i]);
                }
            }
            if (requireValue)
                if (lastKey!.Length != 0 || a[string.Empty].Count != 0)
                {
                    // Throw on missing last value
                    if (requireValue) throw new FormatException($"Missing last argument (\"--{lastKey}\") value");
                }
                else
                {
                    // Handle a "dash-flag"
                    HandleDashFlag();
                }
            if (lastKey is not null)
                _Arguments.AddRange(from kvp in a
                                    select new KeyValuePair<string, ImmutableArray<string>>(kvp.Key, [.. kvp.Value]));
            KeyLessArguments = [.. keyLess];
        }

        /// <summary>
        /// Parse arguments from a string (use <c>'</c> or <c>"</c> for a quoted value, <c>\</c> for escaping)
        /// </summary>
        /// <param name="str">String  (use <c>'</c> or <c>"</c> for a quoted value, <c>\</c> for escaping within a quoted value, double escape <c>\</c>)</param>
        /// <returns>Arguments</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static CliArguments Parse(in ReadOnlySpan<char> str) => new(Split(str));

        /// <summary>
        /// Split a CLI argument string into arguments (use <c>'</c> or <c>"</c> for a quoted value, <c>\</c> for escaping)
        /// </summary>
        /// <param name="str">String  (use <c>'</c> or <c>"</c> for a quoted value, <c>\</c> for escaping within a quoted value, double escape <c>\</c>)</param>
        /// <returns>Arguments</returns>
        public static string[] Split(in ReadOnlySpan<char> str)
        {
            // Return early, if empty
            if (str.Length == 0) return [];
            // Prepare parsing
            using RentedMemoryRef<string> argsBuffer = new(Math.Max(1, str.Length >> 1));// Arguments
            Span<string> argsBufferSpan = argsBuffer.Span;
            using RentedMemoryRef<char> valueBuffer = new(str.Length);// Current value
            Span<char> valueBufferSpan = valueBuffer.Span;
            bool inValue = false,// Is within a value at present?
                isQuoted = false,// Is within a quoted value at present?
                isEscaped = false,// Was the previous character a backslash?
                isWhiteSpace;// Is a whitespace character?
            char c,// Current character
                quote = '\0';// Start quote character
            int argsOffset = 0,// Result buffer offset
                valueOffset = 0,// Value buffer offset
                i = 0;// Current char index
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            void AddValue(in Span<string> argsBufferSpan, in Span<char> valueBufferSpan)
            {
                // Add the current value to the result
                if (valueOffset == 0)
                {
                    // Empty value
                    argsBufferSpan[argsOffset] = string.Empty;
                }
                else
                {
                    // Raw or JSON encoded value
                    argsBufferSpan[argsOffset] = isQuoted
                        ? JsonHelper.Decode<string>($"\"{new string(valueBufferSpan[..valueOffset])}\"")
                            ?? throw new InvalidDataException($"Failed to JSON decode quoted value ending at #{i} (\"{new string(valueBufferSpan[..valueOffset])}\")")
                        : new string(valueBufferSpan[..valueOffset]);
                    valueOffset = 0;
                }
                argsOffset++;
                inValue = false;
                isQuoted = false;
            }
            for (; i < str.Length; i++)
            {
                c = str[i];
                if (inValue)
                {
                    // Handle an in-value character
                    isWhiteSpace = char.IsWhiteSpace(c);
                    if (!isWhiteSpace && char.IsControl(c))
                        throw new FormatException($"Illegal control character at #{i} (current value \"{valueBufferSpan[..valueOffset]}\")");
                    switch (c)
                    {
                        case '\'':
                        case '"':
                            // Possible value quote end
                            if (isQuoted && isEscaped)
                            {
                                // Escaped quote character
                                if (c == '"')
                                {
                                    // Double quote (needs to stay escaped for JSON decoding)
                                    valueBufferSpan[valueOffset] = c;
                                    valueOffset++;
                                }
                                else
                                {
                                    // Single quote doesn't need escaping for JSON decoding
                                    valueBufferSpan[valueOffset - 1] = c;
                                }
                                isEscaped = false;
                            }
                            else if (isQuoted && c == quote && (i == str.Length - 1 || char.IsWhiteSpace(str[i + 1])))
                            {
                                // Value closing quote
                                AddValue(argsBufferSpan, valueBufferSpan);
                            }
                            else if (isQuoted)
                            {
                                // Add an unescaped quote character to the current value
                                if (c == '"') throw new FormatException($"Double quote must be escaped always at #{i} (current value \"{valueBufferSpan[..valueOffset]}\")");
                                valueBufferSpan[valueOffset] = c;
                                valueOffset++;
                            }
                            else
                            {
                                throw new FormatException($"Illegal quote character \"{c}\" in unquoted value at #{i} (current value \"{valueBufferSpan[..valueOffset]}\")");
                            }
                            break;
                        case '\\':
                            // Escaping
                            if (isEscaped)
                            {
                                // Escaped backslash
                                isEscaped = false;
                            }
                            else
                            {
                                // Start escaping
                                if (!isQuoted)
                                    throw new FormatException($"Illegal escape character in unquoted value at #{i} (current value \"{valueBufferSpan[..valueOffset]}\")");
                                isEscaped = true;
                                valueBufferSpan[valueOffset] = c;
                                valueOffset++;
                            }
                            break;
                        default:
                            // Add a character to the current value
                            if (!isQuoted && !isEscaped && isWhiteSpace)
                            {
                                // End of the value found
                                AddValue(argsBufferSpan, valueBufferSpan);
                            }
                            else
                            {
                                // Extend the current value
                                valueBufferSpan[valueOffset] = c;
                                valueOffset++;
                                isEscaped = false;
                            }
                            break;
                    }
                }
                else if (!char.IsWhiteSpace(c))
                {
                    // Start a new value
                    if (char.IsControl(c)) throw new FormatException($"Illegal control character at #{i}");
                    switch (c)
                    {
                        case '\'':
                        case '"':
                            // Quoted
                            isQuoted = true;
                            quote = c;
                            break;
                        case '\\':
                            throw new FormatException($"Illegal escape character at the beginning of the unquoted value at #{i}");
                    }
                    inValue = true;
                    if (isQuoted) continue;
                    valueBufferSpan[0] = c;
                    valueOffset++;
                }
            }
            // Be sure to add the last value
            if (inValue)
            {
                if (isEscaped) throw new FormatException($"Last escape character is unused (current value \"{valueBufferSpan[..valueOffset]}\")");
                if (isQuoted) throw new FormatException($"Last quoted argument is missing the closing quote \"{quote}\" (current value \"{valueBufferSpan[..valueOffset]}\")");
                AddValue(argsBufferSpan, valueBufferSpan);
            }
            return argsOffset == 0 ? [] : argsBufferSpan[..argsOffset].ToArray();
        }

        /// <summary>
        /// Sanitize a string for use as a CLI command argument
        /// </summary>
        /// <param name="str">Raw string</param>
        /// <param name="quote">Quote character</param>
        /// <returns>Raw (if encoding isn't required) or encoded string (will be quoted, JSON encoded and properly escaped for use as a CLI command argument and with 
        /// <see cref="CliArguments"/>)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static string SanitizeValue(in string str, in char quote = '\'')
            => NeedsEncoding(str) ? $"{quote}{JsonHelper.Encode(str)[1..^1].Replace(BACKSLASH, ESCAPED_BACKSLASH)}{quote}" : str;

        /// <summary>
        /// Determine if a value needs encoding for use as a CLI command argument
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Needs encoding for use as a CLI command argument?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool NeedsEncoding(in ReadOnlySpan<char> str)
        {
            for (int i = 0, len = str.Length; i < len; i++)
                if (
                    str[i] == '\'' ||
                    str[i] == '"' ||
                    str[i] == '\\' ||
                    str[i] == ' ' ||
                    char.IsWhiteSpace(str[i]) ||
                    char.IsControl(str[i]) ||
                    char.IsLowSurrogate(str[i]) ||
                    char.IsHighSurrogate(str[i])
                    )
                    return true;
            return false;
        }
    }
}
