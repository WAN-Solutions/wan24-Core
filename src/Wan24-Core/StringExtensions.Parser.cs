using System.Collections.Concurrent;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using static wan24.Core.Logging;

namespace wan24.Core
{
    // Parser
    public static partial class StringExtensions
    {
        /// <summary>
        /// String parser function handlers (key is the lower case function name)
        /// </summary>
        public static readonly ConcurrentDictionary<string, Parser_Delegate> ParserFunctionHandlers;
        /// <summary>
        /// String parser environment variables
        /// </summary>
        public static readonly ConcurrentDictionary<string, string> ParserEnvironment = new();

        /// <summary>
        /// Regular expression to parse a string (<c>$1</c> is the whole placeholder, <c>$2</c> the inner variable declaration)
        /// </summary>
        public static Regex RxParser { get; set; } = RxParser_Generated();

        /// <summary>
        /// Regular expression content group (2 per default)
        /// </summary>
        public static int RxParserGroup { get; set; } = 2;

        /// <summary>
        /// Default parsing rounds limit
        /// </summary>
        public static int ParserMaxRounds { get; set; } = 3;

        /// <summary>
        /// Parse a string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="data">Parser data (accessable with the zero based index)</param>
        /// <returns>Parsed string</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static string Parse(this string str, params string[] data)
        {
            int len = data.Length;
            Dictionary<string, string> dict = new(len);
            for (int i = 0; i < len; dict[i.ToString()] = data[i], i++) ;
            return Parse(str, dict);
        }

        /// <summary>
        /// Parse a string (the current round is available as <c>_round</c>)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="data">Parser data</param>
        /// <param name="options">Options</param>
        /// <returns>Parsed string</returns>
        public static string Parse(this string str, in Dictionary<string, string> data, in StringParserOptions? options = null)
        {
            Regex rx = options?.Regex ?? RxParser;
            int rxGroup = options?.RegexGroup ?? RxParserGroup;
            int maxRounds = options?.MaxParserRounds ?? ParserMaxRounds;
            foreach (var kvp in ParserEnvironment) if (!data.ContainsKey(kvp.Key)) data[kvp.Key] = kvp.Value;
            int round = 0,// Current parser round
                index,// Current function call index
                charIndex;
            List<string> parts = [];// Current match parts (during parsing)
            string[] match,// Current match parts (after parsing)
                param;// Current function call parameters
            string func,// Current function name
                key,// Current parser data key
                value;// Current parsed value
            Dictionary<string, string> parsed = new(data.Count);// Parsed values
            MatchCollection matches;// Placeholder matches
            bool needClosingBracket;// Does the function parser wait for a closing bracket?
            StringBuilder sb = new();
#pragma warning disable IDE0018 // Can be declared inline
            Parser_Delegate? handler;// Function handler
#pragma warning restore IDE0018 // Can be declared inline
            StringParserContext context;
            for (matches = rx.Matches(str); matches.Count > 0 && round < maxRounds; matches = RxParser.Matches(str), round++)
            {
                data["_round"] = round.ToString();
                // Parse placeholders
                foreach (Match m in matches.Cast<Match>())
                {
                    if (parsed.ContainsKey(m.Groups[1].Value)) continue;
                    parts.Clear();
                    key = m.Groups[rxGroup].Value.Trim();
                    parts.Add(key);
                    // Parse function calls
                    if (key.Contains(':'))
                    {
                        sb.Clear();
                        needClosingBracket = false;
                        foreach (char c in key)
                            if (needClosingBracket)
                            {
                                // Waiting for the closing bracket of a function call
                                sb.Append(c);
                                if (c != ')') continue;
                                needClosingBracket = false;
                                parts.Add(sb.ToString());
                                sb.Clear();
                            }
                            else if (c == ':')
                            {
                                // New function call
                                if (sb.Length != 0 || parts.Count == 1) parts.Add(sb.ToString());
                                sb.Clear();
                            }
                            else if (c == '(' && parts.Count != 1)
                            {
                                // Function call parameter list opening bracket
                                if (sb.Length == 0)
                                    if (options?.ThrowOnError ?? true)
                                    {
                                        throw new InvalidDataException($"Unexpected opening bracket in \"{m.Groups[rxGroup].Value}\" in round {round}");
                                    }
                                    else
                                    {
                                        if (Warning)
                                            Logging.WriteWarning($"Ignoring unexpected opening bracket in \"{m.Groups[rxGroup].Value}\" in round {round}");
                                        continue;
                                    }
                                sb.Append(c);
                                needClosingBracket = true;
                            }
                            else
                            {
                                // Variable or function name
                                sb.Append(c);
                            }
                        if (needClosingBracket)
                            if (options?.ThrowOnError ?? true)
                            {
                                throw new InvalidDataException($"Missing closing bracket for function call in \"{parts[0]}\" near \"{sb}\" in round {round}");
                            }
                            else
                            {
                                if (Warning)
                                    Logging.WriteWarning($"Missing closing bracket for function call in \"{parts[0]}\" near \"{sb}\" in round {round}");
                            }
                        if (sb.Length != 0) parts.Add(sb.ToString());
                        parts.RemoveAt(0);
                        key = parts[0].Trim();
                    }
                    match = [.. parts];
                    // Get the variable value
                    if (data.TryGetValue(key, out string? v))
                    {
                        value = v;
                    }
                    else
                    {
                        if (key.Length != 0 || match.Length == 1)
                            if (options?.ThrowOnError ?? true)
                            {
                                throw new InvalidDataException($"Missing parser data \"{key}\" in round {round}");
                            }
                            else
                            {
                                if (Warning) Logging.WriteWarning($"Missing parser data \"{key}\" in round {round}");
                            }
                        value = key;
                    }
                    // Execute function calls
                    if (match.Length != 1)
                    {
                        // Prepare the context
                        context = new()
                        {
                            String = str,
                            Rx = rx,
                            RxGroup = rxGroup,
                            Matches = matches,
                            M = m,
                            Data = data,
                            Parsed = parsed,
                            Round = round,
                            MaxRounds = maxRounds,
                            Match = match
                        };
                        // Call function handlers
                        for (index = 1; index < match.Length; index++)
                        {
                            // Parse function name and parameters
                            charIndex = match[index].IndexOf('(');
                            if (charIndex != -1)
                            {
                                func = match[index][..charIndex].Trim();
                                param = (from p in match[index][(charIndex + 1)..^1].Split(',')
                                         select p.Trim()).ToArray();
                            }
                            else
                            {
                                func = match[index].Trim();
                                param = [];
                            }
                            // Try to find the handler
                            if (!ParserFunctionHandlers.TryGetValue(func, out handler))
                            {
                                if (options?.ThrowOnError ?? true)
                                {
                                    throw new InvalidDataException($"Unknown parser function \"{func}\" in \"{m.Groups[rxGroup].Value}\" in round {round}");
                                }
                                else
                                {
                                    if (Error)
                                        Logging.WriteError($"Unknown parser function \"{func}\" in \"{m.Groups[rxGroup].Value}\" in round {round}");
                                }
                                continue;
                            }
                            // Finalize the context
                            context.Value = value;
                            context.Index = index;
                            context.Func = func;
                            context.Param = param;
                            context.Error = null;
                            // Call the function handler
                            try
                            {
                                value = handler(context);
                                if (context.Error is not null)
                                    if (options?.ThrowOnError ?? true)
                                    {
                                        throw new InvalidDataException($"Failed to execute function \"{match[index]}\" in \"{m.Groups[rxGroup].Value}\" in round {round}: {context.Error}");
                                    }
                                    else
                                    {
                                        if (Error)
                                            Logging.WriteError($"Failed to execute function \"{match[index]}\" in \"{m.Groups[rxGroup].Value}\" in round {round}: {context.Error}");
                                    }
                            }
                            catch (InvalidDataException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                if (options?.ThrowOnError ?? true)
                                {
                                    throw new InvalidDataException($"Failed to handle function call \"{match[index]}\" in \"{m.Groups[rxGroup].Value}\" in round {round}: {ex.Message}", ex);
                                }
                                else
                                {
                                    if (Error)
                                        Logging.WriteError($"Failed to handle function call \"{match[index]}\" in \"{m.Groups[rxGroup].Value}\" in round {round}: {ex.Message}");
                                }
                            }
                            // Apply new settings
                            rx = context.Rx;
                            rxGroup = context.RxGroup;
                        }
                    }
                    // Store the parsers resulting value
                    parsed[m.Groups[1].Value] = value;
                }
                // Finalize the string for the current round
                if (parsed.Count == 0) break;
                StringBuilder stringBuilder = new(str);
                foreach (string k in parsed.Keys) stringBuilder.Replace(k, parsed[k]);
                str = stringBuilder.ToString();
                parsed.Clear();
            }
            // Handle parser problem
            if (round == maxRounds && matches.Count != 0)
                if (options?.ThrowOnError ?? true)
                {
                    throw new InvalidDataException($"String not fully parsed (after {round} rounds there are still {matches.Count} placeholders left)");
                }
                else
                {
                    if (Warning)
                        Logging.WriteWarning($"String not fully parsed (after {round} rounds there are still {matches.Count} placeholders left)");
                }
            return str;
        }

        /// <summary>
        /// Delegate for a string parser function handler
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Function return value (will be the final value or the value for the next function call)</returns>
        public delegate string Parser_Delegate(StringParserContext context);

        /// <summary>
        /// Regular expression to parse a string (<c>$1</c> is the whole placeholder, <c>$2</c> the inner variable declaration)
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"(\%\{([^\}]+)\})", RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex RxParser_Generated();
    }
}
