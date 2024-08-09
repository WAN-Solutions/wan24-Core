using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// RGB with alpha
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly record struct RgbA : ISerializeBinary<RgbA>, ISerializeString<RgbA>
    {
        /// <summary>
        /// Structure size in bytes
        /// </summary>
        public const int STRUCTURE_SIZE = Rgb.STUCTURE_SIZE + sizeof(float);
        /// <summary>
        /// Binary structure size in bytes (returned from <see cref="GetBytes()"/>)
        /// </summary>
        public const int BINARY_SIZE = Rgb.BINARY_SIZE + sizeof(float);
        /// <summary>
        /// RGB field byte offset
        /// </summary>
        public const int RGB_FIELD_OFFSET = 0;
        /// <summary>
        /// Alpha field byte offset
        /// </summary>
        public const int ALPHA_FIELD_OFFSET = RGB_FIELD_OFFSET + Rgb.STUCTURE_SIZE;

        /// <summary>
        /// Regular expression to match a CSS RGBA string (single line)
        /// </summary>
        public static readonly Regex RX_CSS = new(
            @"^\s*rgba\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+(\.\d+)?)\s*\)\s*$",
            RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(3000)
            );
        /// <summary>
        /// Black
        /// </summary>
        public static readonly RgbA Black = new(Rgb.Black);
        /// <summary>
        /// Black
        /// </summary>
        public static readonly RgbA White = new(Rgb.White);

        /// <summary>
        /// RGB
        /// </summary>
        [FieldOffset(RGB_FIELD_OFFSET)]
        public readonly Rgb RGB;
        /// <summary>
        /// Alpha (%)
        /// </summary>
        [FieldOffset(ALPHA_FIELD_OFFSET)]
        public readonly float Alpha;

        /// <summary>
        /// Constructor
        /// </summary>
        public RgbA()
        {
            RGB = default;
            Alpha = 1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rgb">RGB</param>
        /// <param name="alpha">Alpha (%)</param>
        public RgbA(in Rgb rgb, in float alpha = 1)
        {
            if (alpha < 0 || alpha > 1) throw new ArgumentOutOfRangeException(nameof(alpha));
            RGB = rgb;
            Alpha = alpha;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rgb">RGBA bytes</param>
        public RgbA(in ReadOnlySpan<byte> rgb)
        {
            if (rgb.Length < BINARY_SIZE) throw new ArgumentOutOfRangeException(nameof(rgb));
            RGB = rgb;
            Alpha = rgb[Rgb.BINARY_SIZE..].ToFloat();
            if (Alpha < 0 || Alpha > 1) throw new ArgumentException("Invalid alpha", nameof(rgb));
        }

        /// <inheritdoc/>
        public static int? MaxStructureSize => BINARY_SIZE;

        /// <inheritdoc/>
        public static int? MaxStringSize => byte.MaxValue;

        /// <inheritdoc/>
        public int? StructureSize => BINARY_SIZE;

        /// <inheritdoc/>
        public int? StringSize => null;

        /// <summary>
        /// Mix with another color
        /// </summary>
        /// <param name="other">Other color</param>
        /// <returns>Mixed color</returns>
        public RgbA Mix(in RgbA other) => new(RGB.Mix(other.RGB), (Alpha + other.Alpha) / 2);

        /// <summary>
        /// Mix with another color
        /// </summary>
        /// <param name="other">Other color</param>
        /// <returns>Mixed color</returns>
        public RgbA Mix(in Rgb other) => new(RGB.Mix(other), Alpha);

        /// <inheritdoc/>
        public byte[] GetBytes()
        {
            byte[] res = new byte[BINARY_SIZE];
            GetBytes(res);
            return res;
        }

        /// <inheritdoc/>
        public int GetBytes(in Span<byte> buffer)
        {
            if (buffer.Length < BINARY_SIZE) throw new ArgumentOutOfRangeException(nameof(buffer));
            RGB.GetBytes(buffer);
            Alpha.GetBytes(buffer[Rgb.BINARY_SIZE..]);
            return BINARY_SIZE;
        }

        /// <summary>
        /// To CSS RGBA color value
        /// </summary>
        /// <returns>CSS RGBA color value</returns>
        public string ToCssString() => $"rgba({this})";

        /// <inheritdoc/>
        public override string ToString() => $"{RGB}, {Alpha.ToString().Replace(',', '.')}";

        /// <summary>
        /// Cast as <see cref="string"/>
        /// </summary>
        /// <param name="rgb"><see cref="RgbA"/></param>
        public static implicit operator string(in RgbA rgb) => rgb.ToString();

        /// <summary>
        /// Cast as <see cref="Rgb"/>
        /// </summary>
        /// <param name="rgb"><see cref="Rgb"/></param>
        public static implicit operator Rgb(in RgbA rgb) => rgb.RGB;

        /// <summary>
        /// Cast as byte array
        /// </summary>
        /// <param name="rgb"><see cref="RgbA"/></param>
        public static implicit operator byte[](in RgbA rgb) => rgb.GetBytes();

        /// <summary>
        /// Cast from <see cref="string"/>
        /// </summary>
        /// <param name="rgb"><see cref="string"/></param>
        public static implicit operator RgbA(in string rgb) => Parse(rgb);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="rgb">RGB bytes</param>
        public static implicit operator RgbA(in byte[] rgb) => new(rgb);

        /// <summary>
        /// Cast from span
        /// </summary>
        /// <param name="rgb">RGB bytes</param>
        public static implicit operator RgbA(in Span<byte> rgb) => new(rgb);

        /// <summary>
        /// Cast from span
        /// </summary>
        /// <param name="rgb">RGB bytes</param>
        public static implicit operator RgbA(in ReadOnlySpan<byte> rgb) => new(rgb);

        /// <summary>
        /// Cast from memory
        /// </summary>
        /// <param name="rgb">RGB bytes</param>
        public static implicit operator RgbA(in Memory<byte> rgb) => new(rgb.Span);

        /// <summary>
        /// Cast from memory
        /// </summary>
        /// <param name="rgb">RGB bytes</param>
        public static implicit operator RgbA(in ReadOnlyMemory<byte> rgb) => new(rgb.Span);

        /// <summary>
        /// Parse a CSS RGBA color string (<c>rgba</c>)
        /// </summary>
        /// <param name="css">CSS RGBA color string</param>
        /// <returns><see cref="RgbA"/></returns>
        public static RgbA ParseCss(in string css)
        {
            if (!RX_CSS.IsMatch(css)) throw new ArgumentException("Invalid CSS RGBA string", nameof(css));
            string[] rgb = RX_CSS.Replace(css, "$1\t$2\t$3\t$4").Split('\t');
            return new(new Rgb(int.Parse(rgb[0]), int.Parse(rgb[1]), int.Parse(rgb[2])), float.Parse(rgb[3].Replace('.', ',')));
        }

        /// <summary>
        /// Parse a CSS RGBA color string (<c>rgba</c>)
        /// </summary>
        /// <param name="css">CSS RGBA color string</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryParseCss(in string css, out RgbA result)
        {
            if (!RX_CSS.IsMatch(css))
            {
                if (Logging.Debug) Logging.WriteDebug("CSS RGBA string parsing failed: Invalid CSS RGBA string format");
                result = default;
                return false;
            }
            string[] rgb = RX_CSS.Replace(css, "$1\t$2\t$3").Split('\t');
            if (!int.TryParse(rgb[0], out int r) || r > byte.MaxValue)
            {
                if (Logging.Debug) Logging.WriteDebug("CSS RGBA string parsing failed: Invalid CSS RGBA red value");
                result = default;
                return false;
            }
            if (!int.TryParse(rgb[1], out int g) || g > byte.MaxValue)
            {
                if (Logging.Debug) Logging.WriteDebug("CSS RGBA string parsing failed: Invalid CSS RGBA green value");
                result = default;
                return false;
            }
            if (!int.TryParse(rgb[2], out int b) || b > byte.MaxValue)
            {
                if (Logging.Debug) Logging.WriteDebug("CSS RGBA string parsing failed: Invalid CSS RGBA blue value");
                result = default;
                return false;
            }
            if (!float.TryParse(rgb[3].Replace('.', ','), out float a) || a > 1)
            {
                if (Logging.Debug) Logging.WriteDebug("CSS RGBA string parsing failed: Invalid CSS RGBA alpha value");
                result = default;
                return false;
            }
            result = new(new Rgb(r, g, b), a);
            return true;
        }

        /// <inheritdoc/>
        public static object DeserializeFrom(in ReadOnlySpan<byte> buffer) => new RgbA(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out object? result)
        {
            try
            {
                if (buffer.Length < BINARY_SIZE)
                {
                    result = null;
                    return false;
                }
                Rgb rgb = buffer;
                float alpha = buffer[Rgb.BINARY_SIZE..].ToFloat();
                if (alpha < 0 || alpha > 1)
                {
                    result = null;
                    return false;
                }
                result = new RgbA(rgb, alpha);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public static RgbA DeserializeTypeFrom(in ReadOnlySpan<byte> buffer) => new(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeTypeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out RgbA result)
        {
            try
            {
                if (buffer.Length < BINARY_SIZE)
                {
                    result = default;
                    return false;
                }
                Rgb rgb = buffer;
                float alpha = buffer[Rgb.BINARY_SIZE..].ToFloat();
                if (alpha < 0 || alpha > 1)
                {
                    result = default;
                    return false;
                }
                result = new RgbA(rgb, alpha);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <inheritdoc/>
        public static RgbA Parse(in ReadOnlySpan<char> str)
        {
            int index = str.LastIndexOf(',');
            if (index < 0) throw new InvalidDataException("Invalid string format");
            return new(Rgb.Parse(str[..index]), float.Parse(new string(str[(index + 1)..]).Replace('.', ',')));
        }

        /// <inheritdoc/>
        public static bool TryParse(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out RgbA result)
        {
            int index = str.LastIndexOf(',');
            if (index < 0)
            {
                if (Logging.Debug) Logging.WriteDebug("String parsing failed: Invalid string format");
                result = default;
                return false;
            }
            if (!Rgb.TryParse(str[..index], out Rgb rgb))
            {
                result = default;
                return false;
            }
            if (!float.TryParse(new string(str[(index + 1)..]).Replace('.', ','), out float a) || a > 1)
            {
                if (Logging.Debug) Logging.WriteDebug("String parsing failed: Invalid RGBA alpha value");
                result = default;
                return false;
            }
            result = new(rgb, a);
            return true;
        }

        /// <inheritdoc/>
        public static object ParseObject(in ReadOnlySpan<char> str) => Parse(str);

        /// <inheritdoc/>
        public static bool TryParseObject(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out object? result)
        {
            bool res;
            result = (res = TryParse(str, out RgbA rgba))
                ? rgba
                : default(RgbA?);
            return res;
        }
    }
}
