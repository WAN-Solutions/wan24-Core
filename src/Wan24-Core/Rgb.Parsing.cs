using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    // Parsing
    public readonly partial record struct Rgb
    {
        /// <inheritdoc/>
        public static Rgb Parse(in ReadOnlySpan<char> str)
        {
            int index1 = str.IndexOf(',');
            if (index1 < 0) throw new InvalidDataException("Invalid string format");
            int index2 = str[(index1 + 1)..].IndexOf(',');
            if (index2 < 0) throw new InvalidDataException("Invalid string format");
            return new(int.Parse(str[..index1]), int.Parse(str.Slice(index1 + 1, index2)), int.Parse(str[(index1 + index2 + 2)..]));
        }

        /// <inheritdoc/>
        public static bool TryParse(in ReadOnlySpan<char> str, out Rgb result)
        {
            int index1 = str.IndexOf(',');
            if (index1 < 0)
            {
                if (Logging.Debug) Logging.WriteDebug("String parsing failed: Invalid string format");
                result = default;
                return false;
            }
            int index2 = str[(index1 + 1)..].IndexOf(',');
            if (index2 < 0)
            {
                if (Logging.Debug) Logging.WriteDebug("String parsing failed: Invalid string format");
                result = default;
                return false;
            }
            if (!int.TryParse(str[..index1], out int r) || r > byte.MaxValue)
            {
                if (Logging.Debug) Logging.WriteDebug("String parsing failed: Invalid RGB red value");
                result = default;
                return false;
            }
            if (!int.TryParse(str.Slice(index1 + 1, index2), out int g) || g > byte.MaxValue)
            {
                if (Logging.Debug) Logging.WriteDebug("String parsing failed: Invalid RGB green value");
                result = default;
                return false;
            }
            if (!int.TryParse(str[(index1 + index2 + 2)..], out int b) || b > byte.MaxValue)
            {
                if (Logging.Debug) Logging.WriteDebug("String parsing failed: Invalid RGB blue value");
                result = default;
                return false;
            }
            result = new(r, g, b);
            return true;
        }

        /// <inheritdoc/>
        public static object ParseObject(in ReadOnlySpan<char> str) => Parse(str);

        /// <inheritdoc/>
        public static bool TryParseObject(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out object? result)
        {
            bool res;
            result = (res = TryParse(str, out Rgb rgb))
                ? rgb
                : default(Rgb?);
            return res;
        }

        /// <summary>
        /// Parse a HTML hex color string (with or without hash)
        /// </summary>
        /// <param name="html">HTML hex color string</param>
        /// <returns><see cref="Rgb"/></returns>
        public static Rgb ParseHtml(string html)
        {
            html = html.Trim().Replace("#", string.Empty);
            switch (html.Length)
            {
                case 3:
                    html = $"{html[0]}{html[0]}{html[1]}{html[1]}{html[2]}{html[2]}";
                    break;
                case 6:
                    break;
                default:
                    throw new ArgumentException("Invalid HTML hex color string", nameof(html));
            }
            return new(Convert.FromHexString(html));
        }

        /// <summary>
        /// Parse a HTML hex color string (with or without hash)
        /// </summary>
        /// <param name="html">HTML hex color string</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryParseHtml(string html, out Rgb result)
        {
            html = html.Trim().Replace("#", string.Empty);
            switch (html.Length)
            {
                case 3:
                    html = $"{html[0]}{html[0]}{html[1]}{html[1]}{html[2]}{html[2]}";
                    break;
                case 6:
                    break;
                default:
                    if (Logging.Debug) Logging.WriteDebug($"HTML hex string parsing failed: Invalid hex code length {html.Length}");
                    result = default;
                    return false;
            }
            if (!RegularExpressions.RX_HEX_STRING.IsMatch(html))
            {
                if (Logging.Debug) Logging.WriteDebug($"HTML hex string parsing failed: Invalid hex code");
                result = default;
                return false;
            }
            try
            {
                result = new(Convert.FromHexString(html));
                return true;
            }
            catch (Exception ex)
            {
                if (Logging.Debug) Logging.WriteDebug($"HTML hex string parsing failed: Invalid hex code: ({ex.GetType()}) {ex.Message}");
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Parse a CSS RGB color string (<c>rgb</c> or <c>rgba</c>)
        /// </summary>
        /// <param name="css">CSS RGB color string</param>
        /// <returns><see cref="Rgb"/></returns>
        public static Rgb ParseCss(in string css)
        {
            if (!RX_CSS.IsMatch(css)) throw new ArgumentException("Invalid CSS RGB string", nameof(css));
            string[] rgb = RX_CSS.Replace(css, "$1\t$2\t$3").Split('\t');
            return new(int.Parse(rgb[0]), int.Parse(rgb[1]), int.Parse(rgb[2]));
        }

        /// <summary>
        /// Parse a CSS RGB color string (<c>rgb</c> or <c>rgba</c>)
        /// </summary>
        /// <param name="css">CSS RGB color string</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryParseCss(in string css, out Rgb result)
        {
            if (!RX_CSS.IsMatch(css))
            {
                if (Logging.Debug) Logging.WriteDebug("CSS RGB string parsing failed: Invalid CSS RGB string format");
                result = default;
                return false;
            }
            string[] rgb = RX_CSS.Replace(css, "$1\t$2\t$3").Split('\t');
            if (!int.TryParse(rgb[0], out int r) || r > byte.MaxValue)
            {
                if (Logging.Debug) Logging.WriteDebug("CSS RGB string parsing failed: Invalid CSS RGB red value");
                result = default;
                return false;
            }
            if (!int.TryParse(rgb[1], out int g) || g > byte.MaxValue)
            {
                if (Logging.Debug) Logging.WriteDebug("CSS RGB string parsing failed: Invalid CSS RGB green value");
                result = default;
                return false;
            }
            if (!int.TryParse(rgb[2], out int b) || b > byte.MaxValue)
            {
                if (Logging.Debug) Logging.WriteDebug("CSS RGB string parsing failed: Invalid CSS RGB blue value");
                result = default;
                return false;
            }
            result = new(r, g, b);
            return true;
        }
    }
}
