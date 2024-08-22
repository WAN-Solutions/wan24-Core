using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;

namespace wan24.Core
{
    // String parser functions
    public static partial class StringExtensions
    {
        /// <summary>
        /// Sub-string function <c>sub</c> (Syntax: <c>%{input:sub(3)}</c> (like <c>%{input:left(3)}</c>) or <c>%{input:sub(3,2)}</c> (2 characters from start index 3); parameters may be 
        /// variable names with a <c>$</c> prefix)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_SubString(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(1, 2)) return context.Value;
            int offset,
                len;
            string? value;
            switch (context.Param.Length)
            {
                case 1:
                    value = context.Param[0];
                    if (!context.TryGetData(value, out value)) return context.Value;
                    offset = int.Parse(value);
                    len = Math.Max(0, context.Value.Length - offset);
                    break;
                default:
                    value = context.Param[0];
                    if (!context.TryGetData(value, out value)) return context.Value;
                    offset = int.Parse(value);
                    value = context.Param[1];
                    if (!context.TryGetData(value, out value)) return context.Value;
                    len = int.Parse(value);
                    break;
            }
            if (offset >= context.Value.Length) return string.Empty;
            if (offset + len > context.Value.Length) len = context.Value.Length - offset;
            return context.Value.Substring(offset, len);
        }

        /// <summary>
        /// Left-sub-string function <c>left</c> (Syntax: <c>%{input:left(3)}</c>; the length parameter may be a variable name with a <c>$</c> prefix)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_Left(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(1) || !context.TryGetData(context.Param[0], out string? value)) return context.Value;
            int len = int.Parse(value);
            return len >= context.Value.Length ? context.Value : context.Value[..len];
        }

        /// <summary>
        /// Right-sub-string function <c>right</c> (Syntax: <c>%{input:right(3)}</c>; the length parameter may be a variable name with a <c>$</c> prefix)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_Right(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(1) || !context.TryGetData(context.Param[0], out string? value)) return context.Value;
            int len = int.Parse(value);
            return len >= context.Value.Length ? context.Value : context.Value[^len..];
        }

        /// <summary>
        /// Trim value function <c>trim</c> (Syntax: <c>%{input:trim}</c>)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_Trim(StringParserContext context) => context.EnsureValidParameterCount(0) ? context.Value.Trim() : context.Value;

        /// <summary>
        /// Discard output function <c>discard</c> (Syntax: <c>%{input:discard}</c>)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Empty string</returns>
        public static string Parser_Discard(StringParserContext context)
            => context.EnsureValidParameterCount(0) ? string.Empty : context.Value;

        /// <summary>
        /// HTML escaping function <c>escape_html</c> (Syntax: <c>%{input:escape_html}</c>)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_Escape_Html(StringParserContext context)
            => context.EnsureValidParameterCount(0) ? HttpUtility.HtmlEncode(context.Value).Replace("\r", string.Empty).Replace("\n", "<br>\n") : context.Value;

        /// <summary>
        /// JSON escaping function <c>escape_json</c> (will trim double quotes!) (Syntax: <c>%{input:escape_json}</c>)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_Escape_Json(StringParserContext context)
            => context.EnsureValidParameterCount(0) ? JsonHelper.Encode(context.Value)[1..^1] : context.Value;

        /// <summary>
        /// URI escaping function <c>escape_uri</c> (Syntax: <c>%{input:escape_uri}</c>)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_Escape_URI(StringParserContext context)
            => context.EnsureValidParameterCount(0) ? HttpUtility.UrlEncode(context.Value) : context.Value;

        /// <summary>
        /// Parser data setter function <c>set</c> (Syntax: <c>%{input:set(targetName)}</c>; the variable name parameter may be a variable name with a <c>$</c> prefix)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_Set(StringParserContext context)
            => context.EnsureValidParameterCount(1) && context.TryGetData(context.Param[0], out string? value) ? context.Data[value] = context.Value : context.Value;

        /// <summary>
        /// Parser data getter function <c>var</c> (Syntax: <c>%{:var(name)}</c>; the variable name parameter may be a variable name with a <c>$</c> prefix)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_Var(StringParserContext context)
            => context.EnsureValidParameterCount(1) && context.TryGetData(context.Param[0], out string? value) && context.TryGetData(value, out value) ? value : context.Value;

        /// <summary>
        /// Parser item selector function <c>item</c> (Syntax: <c>%{input:item(0,item0,item1,...)}</c> (returns item #0) or <c>%{input:item($name,item0,item1,...)}</c> or 
        /// <c>%{input:item(0,$name)}</c> (to split parser data <c>name</c> by pipe (<c>|</c>) as items))
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_Item(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(Enumerable.Range(2, 10).ToArray())) return context.Value;
            int index;
            string[] items = context.Param.AsSpan(1).ToArray();
            if (context.Param[0].StartsWith('$'))
            {
                if (!context.TryGetData(context.Param[0], out string? value)) return context.Value;
                index = int.Parse(value);
            }
            else
            {
                index = int.Parse(context.Param[0]);
            }
            if (index < 0)
            {
                context.Error = $"Item index #{index} is out of range (0..{items.Length - 1})";
                return context.Value;
            }
            if (items[0].StartsWith('$'))
            {
                if (!context.TryGetData(items[0], out string? value)) return context.Value;
                items = [.. value.Split('|'), .. items.Skip(1)];
            }
            if (index >= items.Length)
            {
                context.Error = $"Item index #{index} is out of range (0..{items.Length - 1})";
                return context.Value;
            }
            return items[index];
        }

        /// <summary>
        /// Value prepending function <c>prepend</c> (Syntax: <c>%{input:prepend(string)}</c> (to prepend <c>string</c>) or <c>%{input:prepend($name)}</c> (to prepend 
        /// variable <c>name</c>))
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_Prepend(StringParserContext context)
            => !context.EnsureValidParameterCount(1) || !context.TryGetData(context.Param[0], out string? str) ? context.Value : str + context.Value;

        /// <summary>
        /// Value appending function <c>append</c> (Syntax: <c>%{input:append(string)}</c> (to append <c>string</c>) or <c>%{input:append($name)}</c> (to append 
        /// variable <c>name</c>))
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_Append(StringParserContext context)
            => !context.EnsureValidParameterCount(1) || !context.TryGetData(context.Param[0], out string? str) ? context.Value : context.Value + str;

        /// <summary>
        /// Value inserting function <c>insert</c> (Syntax: <c>%{input:insert(1,string)}</c> (to insert <c>string</c> at index #1) or <c>%{name:input(1,$name)}</c> (to insert 
        /// variable <c>name</c> at index #1); the offset parameter may be a variable name with a <c>$</c> prefix)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_Insert(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(2) || !context.TryGetData(context.Param[0], out string? value)) return context.Value;
            int index = int.Parse(value);
            string? str = context.Param[1];
            if (!context.TryGetData(str, out str)) return context.Value;
            return index >= str.Length ? context.Value + str : context.Value[0..index] + str + context.Value[index..];
        }

        /// <summary>
        /// Value removing function <c>remove</c> (Syntax: <c>%{input:remove(1)}</c> (to remove the first character) or <c>%{v:remove(1,2)}</c> (remove two characters from index #1); 
        /// the parameters may be a variable name with a <c>$</c> prefix)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_Remove(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(1, 2)) return context.Value;
            int offset = 0,
                len;
            string? value;
            switch (context.Param.Length)
            {
                case 1:
                    value = context.Param[0];
                    if (!context.TryGetData(value, out value)) return context.Value;
                    len = int.Parse(value);
                    break;
                default:
                    value = context.Param[0];
                    if (!context.TryGetData(value, out value)) return context.Value;
                    offset = int.Parse(value);
                    value = context.Param[1];
                    if (!context.TryGetData(value, out value)) return context.Value;
                    len = int.Parse(value);
                    break;
            }
            if (offset >= context.Value.Length) return context.Value;
            return offset + len >= context.Value.Length ? context.Value[..offset] : context.Value[..offset] + context.Value[(offset + len)..];
        }

        /// <summary>
        /// Value concatenation function <c>concat</c> (Syntax: <c>%{:concat(string,$name,...)}</c>)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_Concat(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(Enumerable.Range(2, 10).ToArray())) return context.Value;
            string[] values = [.. context.Param];
            string? str;
            for (int i = 0, len = values.Length; i < len; i++)
            {
                str = values[i];
                if (!context.TryGetData(str, out str)) return context.Value;
                values[i] = str;
            }
            return string.Join(string.Empty, values);
        }

        /// <summary>
        /// Value joining function <c>join</c> (Syntax: <c>%{:join(separator,string,$name,...)}</c>; the separator parameter may be a variable name, too)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_Join(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(Enumerable.Range(3, 10).ToArray())) return context.Value;
            string? value = context.Param[0];
            if (value.Length != 1 && !context.TryGetData(value, out value)) return context.Value;
            string[] values = [.. context.Param[1..]];
            string? str;
            for (int i = 0, len = values.Length; i < len; i++)
            {
                str = values[i];
                if (!context.TryGetData(str, out str)) return context.Value;
                values[i] = str;
            }
            return string.Join(value, values);
        }

        /// <summary>
        /// Math function <c>math</c> (Syntax: <c>%{:math([operator],value1,$name2,...)}</c>; all parameters may be variable names with a <c>$</c> prefix; <see cref="decimal"/> is 
        /// being used as numeric format; numbers can be written in invariant culture <see cref="float"/> style (so the result will be written)) - supported operators are:
        /// <list type="bullet">
        /// <item><c>+</c> summarize</item>
        /// <item><c>-</c> substract</item>
        /// <item><c>*</c> multiply</item>
        /// <item><c>/</c> divide</item>
        /// <item><c>%</c> modulo</item>
        /// <item><c>a</c> average</item>
        /// <item><c>i</c> minimum</item>
        /// <item><c>x</c> maximum</item>
        /// <item><c>r</c> round (second value is the number of decimals, other values are ignored)</item>
        /// <item><c>f</c> floor (second values will be ignored)</item>
        /// <item><c>c</c> ceiling (second values will be ignored)</item>
        /// <item><c>p</c> Y power of X (second value is Y, other values are ignored; conversion to <see cref="double"/> is required)</item>
        /// <item><c>=</c> equality (result is <c>0</c> for <see langword="false"/> or <c>1</c> for <see langword="true"/>)</item>
        /// <item><c>&lt;</c> lower than (result is <c>0</c> for <see langword="false"/> or <c>1</c> for <see langword="true"/>)</item>
        /// <item><c>&gt;</c> greater than (result is <c>0</c> for <see langword="false"/> or <c>1</c> for <see langword="true"/>)</item>
        /// <item><c>s</c> to change the sign (second values will be ignored)</item>
        /// </list>
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_Math(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(Enumerable.Range(3, 10).ToArray())) return context.Value;
            string? value = context.Param[0];
            if (value.Length != 1 && !context.TryGetData(value, out value)) return context.Value;
            if (value.Length != 1)
            {
                context.Error = $"Invalid math operator \"{value}\"";
                return context.Value;
            }
            decimal[] values = new decimal[context.Param.Length - 1];
            for (int i = 0, len = values.Length; i < len; i++)
            {
                value = context.Param[i + 1];
                if (!context.TryGetData(value, out value)) return context.Value;
                values[i] = decimal.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat);
            }
            switch (context.Param[0][0])
            {
                case '+': return values.Sum().ToString(CultureInfo.InvariantCulture.NumberFormat);
                case '-': return (values[0] + values.Skip(1).Sum(v => -v)).ToString(CultureInfo.InvariantCulture.NumberFormat);
                case '*': return values.Skip(1).Aggregate(values[0], (result, current) => result * current).ToString(CultureInfo.InvariantCulture.NumberFormat);
                case '/': return values.Skip(1).Aggregate(values[0], (result, current) => result / current).ToString(CultureInfo.InvariantCulture.NumberFormat);
                case '%': return values.Skip(1).Aggregate(values[0], (result, current) => result % current).ToString(CultureInfo.InvariantCulture.NumberFormat);
                case 'a': return values.Average().ToString(CultureInfo.InvariantCulture.NumberFormat);
                case 'i': return values.Min().ToString(CultureInfo.InvariantCulture.NumberFormat);
                case 'x': return values.Max().ToString(CultureInfo.InvariantCulture.NumberFormat);
                case 'r': return Math.Round(values[0], (int)values[1]).ToString(CultureInfo.InvariantCulture.NumberFormat);
                case 'f': return Math.Floor(values[0]).ToString(CultureInfo.InvariantCulture.NumberFormat);
                case 'c': return Math.Ceiling(values[0]).ToString(CultureInfo.InvariantCulture.NumberFormat);
                case 'p': return ((decimal)Math.Pow((double)values[0], (double)values[1])).ToString(CultureInfo.InvariantCulture.NumberFormat);
                case '=':
                    {
                        bool eq = values[0] == values[1];
                        for (int i = 2, len = values.Length; eq && i < len; eq &= values[i - 1] == values[i], i++) ;
                        return eq ? "1" : "0";
                    }
                case '<':
                    {
                        bool lt = values[0] < values[1];
                        for (int i = 2, len = values.Length; lt && i < len; lt &= values[i - 1] < values[i], i++) ;
                        return lt ? "1" : "0";
                    }
                case '>':
                    {
                        bool gt = values[0] > values[1];
                        for (int i = 2, len = values.Length; gt && i < len; gt &= values[i - 1] > values[i], i++) ;
                        return gt ? "1" : "0";
                    }
                case 's': return (-values[0]).ToString(CultureInfo.InvariantCulture.NumberFormat);
                default:
                    context.Error = $"Invalid math operator {context.Param[0].ToQuotedLiteral()}";
                    return context.Value;
            }
        }

        /// <summary>
        /// Parser regular expression setting function <c>rx</c> (Syntax: <c>%{:rx(2,$name)}</c>; the regular expression content group index may be a variable name, too)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_Rx(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(2) || !context.TryGetData(context.Param[0], out string? value)) return context.Value;
            int group = int.Parse(value);
            value = context.Param[1];
            if (!context.TryGetData(value, out value)) return context.Value;
            context.Rx = new(value, RegexOptions.Compiled | RegexOptions.Singleline);
            context.RxGroup = group;
            return context.Value;
        }

        /// <summary>
        /// Numeric value formatter function <c>format</c> (Syntax: <c>%{input:format(format)}</c>; the format may be a variable name; <see cref="decimal"/> is 
        /// being used as numeric format; numbers can be written in invariant culture <see cref="float"/> style (so the result will be written))
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_Format(StringParserContext context)
            => context.EnsureValidParameterCount(1) && context.TryGetData(context.Param[0], out string? value)
                ? decimal.Parse(context.Value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat).ToString(value)
                : context.Value;

        /// <summary>
        /// String value formatter function <c>str_format</c> (Syntax: <c>%{input:str_format(value1,value2,...)}</c>; the values may be variable names)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_StrFormat(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(Enumerable.Range(0, 10).ToArray())) return context.Value;
            string? value;
            string[] values = [.. context.Param];
            for (int i = 0, len = values.Length; i < len; i++)
            {
                value = values[i];
                if (!context.TryGetData(value, out value)) return context.Value;
                values[i] = value;
            }
            return string.Format(context.Value, values);
        }

        /// <summary>
        /// String length function <c>len</c> (Syntax: <c>%{input:len}</c>)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_Len(StringParserContext context) => context.EnsureValidParameterCount(0) ? context.Value.Length.ToString() : context.Value;

        /// <summary>
        /// Item count function <c>count</c> (Syntax: <c>%{input:count}</c>; items will be splitted by pipe <c>|</c>)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_Count(StringParserContext context) => context.EnsureValidParameterCount(0) ? context.Value.Split('|').Length.ToString() : context.Value;

        /// <summary>
        /// Insert item function <c>insert_item</c> (Syntax: <c>%{input:insert_item(1,$items)}</c>; items will be splitted by pipe <c>|</c>)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_InsertItem(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(2)) return context.Value;
            string? value = context.Param[0];
            if (!context.TryGetData(value, out value)) return context.Value;
            int index = int.Parse(value);
            if (index < 0)
            {
                context.Error = $"Invalid index #{index}";
                return context.Value;
            }
            if (!context.Param[1].StartsWith('$'))
            {
                context.Error = "Parameter 2 must be a parser data variable name";
                return context.Value;
            }
            string key = context.Param[1][1..];
            value = context.Param[1];
            if (!context.TryGetData(value, out value)) return context.Value;
            string[] values = value.Split('|');
            if (index == 0)
            {
                values = [context.Value, .. values];
            }
            else if (index >= values.Length)
            {
                values = [.. values, value];
            }
            else
            {
                values = values.Take(index).Append(value).Concat(values.Skip(index)).ToArray();
            }
            context.Data[key] = string.Join('|', values);
            return context.Value;
        }

        /// <summary>
        /// Remove item function <c>remove_item</c> (Syntax: <c>%{input:remove_item(1)}</c>; items will be splitted by pipe <c>|</c>)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_RemoveItem(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(1)) return context.Value;
            string? value = context.Param[0];
            if (!context.TryGetData(value, out value)) return context.Value;
            int index = int.Parse(value);
            if (index < 0)
            {
                context.Error = $"Invalid index #{index}";
                return context.Value;
            }
            string[] values = context.Value.Split('|');
            if (index == 0)
            {
                values = values.Skip(1).ToArray();
            }
            else if (index < values.Length)
            {
                values = values.Take(index).Concat(values.Skip(index + 1)).ToArray();
            }
            return string.Join('|', values);
        }

        /// <summary>
        /// Sort items function <c>sort</c> (Syntax: <c>%{input:sort}</c> (to sort ascending) or <c>%{name:sort(desc)}</c> (to sort descending); items will be splitted by 
        /// pipe <c>|</c>)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_Sort(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(0, 1)) return context.Value;
            List<string> values = new(context.Value.Split('|'));
            values.Sort();
            if (context.Param.Length != 0 && context.Param[0] == "desc") values.Reverse();
            return string.Join('|', values);
        }

        /// <summary>
        /// For-each loop function <c>foreach</c> (Syntax: <c>%{input:foreach($name)}</c>; current item will be stored in <c>_item</c>; items will be splitted by pipe <c>|</c>)
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_ForEach(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(1)) return context.Value;
            string? value = context.Param[0];
            if (!value.StartsWith('$'))
            {
                context.Error = "Parameter must be a parser data variable name";
                return context.Value;
            }
            if (!context.TryGetData(value, out value)) return context.Value;
            string[] values = context.Value.Split('|');
            List<string> res = new(values.Length);
            foreach (string v in values)
            {
                context.Data["_item"] = v;
                res.Add(value.Parse(
                    context.Data,
                    new()
                    {
                        MaxParserRounds = context.MaxRounds - context.Round,
                        Regex = context.Rx,
                        RegexGroup = context.RxGroup
                    }
                    ));
            }
            return string.Join(string.Empty, res);
        }

        /// <summary>
        /// Conditional function <c>if</c> (Syntax: <c>%{input:if($name)}</c> (to parse <c>name</c>, if <c>input</c> is <c>1</c>) or <c>%{input:if($name,$name2)}</c> 
        /// (to parse <c>name</c>, if <c>input</c> is <c>1</c>, else parse <c>name2</c>))
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Parsed value</returns>
        public static string Parser_If(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(1, 2)) return context.Value;
            string? value = context.Param[0];
            if (!context.TryGetData(value, out value)) return context.Value;
            string trueValue = value;
            string? falseValue = null;
            if (context.Param.Length == 2)
            {
                value = context.Param[1];
                if (!context.TryGetData(value, out value)) return context.Value;
                falseValue = value;
            }
            if (context.Value == "1")
            {
                return trueValue.Parse(
                    context.Data,
                    new()
                    {
                        MaxParserRounds = context.MaxRounds - context.Round,
                        Regex = context.Rx,
                        RegexGroup = context.RxGroup
                    }
                    );
            }
            else if (falseValue is not null)
            {
                return falseValue.Parse(
                    context.Data,
                    new()
                    {
                        MaxParserRounds = context.MaxRounds - context.Round,
                        Regex = context.Rx,
                        RegexGroup = context.RxGroup
                    }
                    );
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Split function <c>split</c> (Syntax: <c>%{input:split(prefix)}</c> (to split the value by pipe (<c>|</c>) and set it as parser data using <c>prefix</c> as key appended 
        /// with the item index))
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_Split(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(1)) return context.Value;
            if (context.Param[0].StartsWith('$'))
            {
                context.Error = "No variable name allowed";
                return context.Value;
            }
            string[] values = context.Value.Split('|');
            for (int i = 0, len = values.Length; i < len; context.Data[$"{context.Param[0]}{i}"] = values[i], i++) ;
            return context.Value;
        }

        /// <summary>
        /// Numeric range function <c>range</c> (Syntax: <c>%{:range(0,10)}</c> (to create a numeric range from <c>0</c>, having <c>10</c> items))
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_Range(StringParserContext context)
        {
            if (!context.EnsureValidParameterCount(2)) return context.Value;
            string? value = context.Param[0];
            if (!context.TryGetData(value, out value)) return context.Value;
            int start = int.Parse(value);
            value = context.Param[1];
            if (!context.TryGetData(value, out value)) return context.Value;
            int count = int.Parse(value);
            return string.Join('|', Enumerable.Range(start, count));
        }

        /// <summary>
        /// Dummy function <c>dummy</c> (Syntax: <c>%{:dummy(...)}</c> (does nothing))
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Value</returns>
        public static string Parser_Dummy(StringParserContext context) => context.Value;
    }
}
