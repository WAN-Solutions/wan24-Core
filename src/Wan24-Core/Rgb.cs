using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// RGB color value
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial record struct Rgb
    {
        /// <summary>
        /// Max. unsigned 24 bit integer RGB value
        /// </summary>
        public const int MAX_24BIT_RGB = 16_777_215;
        /// <summary>
        /// Structure size in bytes
        /// </summary>
        public const int STUCTURE_SIZE = 3;
        /// <summary>
        /// Red value byte offset
        /// </summary>
        public const int R_FIELD_OFFSET = 2;
        /// <summary>
        /// Green value byte offset
        /// </summary>
        public const int G_FIELD_OFFSET = 1;
        /// <summary>
        /// Blue value byte offset
        /// </summary>
        public const int B_FIELD_OFFSET = 0;

        /// <summary>
        /// Regular expression to match a CSS RGB string (single line)
        /// </summary>
        private static readonly Regex RX_CSS = new(@"^\s*rgba+\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*\d+\s*\)\s+$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(3000));
        /// <summary>
        /// Black (min. value)
        /// </summary>
        public static readonly Rgb Black = new();
        /// <summary>
        /// White (max. value)
        /// </summary>
        public static readonly Rgb White = new(byte.MaxValue, byte.MaxValue, byte.MaxValue);

        /// <summary>
        /// Red 8 bit unsigned integer value
        /// </summary>
        [FieldOffset(0)]
        public readonly int R;
        /// <summary>
        /// Green 8 bit unsigned integer value
        /// </summary>
        [FieldOffset(1)]
        public readonly int G;
        /// <summary>
        /// Blue 8 bit unsigned integer value
        /// </summary>
        [FieldOffset(2)]
        public readonly int B;

        /// <summary>
        /// Constructor
        /// </summary>
        public Rgb()
        {
            R = default;
            G = default;
            B = default;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="r">Red 8 bit unsigned integer value</param>
        /// <param name="g">Green 8 bit unsigned integer value</param>
        /// <param name="b">Blue 8 bit unsigned integer value</param>
        public Rgb(in int r, in int g, in int b)
        {
            if (r < 0 || r > byte.MaxValue) throw new ArgumentOutOfRangeException(nameof(r));
            if (g < 0 || g > byte.MaxValue) throw new ArgumentOutOfRangeException(nameof(g));
            if (b < 0 || b > byte.MaxValue) throw new ArgumentOutOfRangeException(nameof(b));
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rgb">RGB 24 bit unsigned integer value</param>
        public Rgb(in int rgb)
        {
            if (!IsValidRgbInt24(rgb)) throw new ArgumentOutOfRangeException(nameof(rgb));
            R = (rgb >> 16) & byte.MaxValue;
            G = (rgb >> 8) & byte.MaxValue;
            B = rgb & byte.MaxValue;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rgb">RGB bytes</param>
        public Rgb(in ReadOnlySpan<byte> rgb)
        {
            if (rgb.Length < 3) throw new ArgumentOutOfRangeException(nameof(rgb));
            R = rgb[2];
            G = rgb[1];
            B = rgb[0];
        }

        /// <summary>
        /// Get the bytes of this RGB value
        /// </summary>
        /// <returns>RGB bytes</returns>
        public byte[] GetBytes() => [(byte)R, (byte)G, (byte)B];

        /// <summary>
        /// Get the bytes of this RGB value
        /// </summary>
        /// <returns>RGB bytes</returns>
        public Span<byte> GetBytes(in Span<byte> buffer)
        {
            if (buffer.Length < 3) throw new ArgumentOutOfRangeException(nameof(buffer));
            buffer[2] = (byte)R;
            buffer[1] = (byte)G;
            buffer[0] = (byte)B;
            return buffer;
        }

        /// <summary>
        /// To hex string
        /// </summary>
        /// <returns>Hex string (upper case)</returns>
        public string ToHexString()
        {
            using RentedArrayRefStruct<byte> buffer = new(len: 3, clean: false);
            return Convert.ToHexString(GetBytes(buffer.Span));
        }

        /// <summary>
        /// To HTML hex string
        /// </summary>
        /// <returns>HTML hex string (lower case)</returns>
        public string ToHtmlString() => $"#{ToHexString().ToLower()}";

        /// <summary>
        /// To CSS RGB color value
        /// </summary>
        /// <returns>CSS RGB color value</returns>
        public string ToCssString() => $"rgb({this})";

        /// <summary>
        /// To CSS RGB color value with alpha
        /// </summary>
        /// <param name="alpha">Alpha value (0..100%)</param>
        /// <returns>CSS RGB color value with alpha</returns>
        public string ToCssString(in int alpha)
        {
            if (alpha < 0 || alpha > 100) throw new ArgumentOutOfRangeException(nameof(alpha));
            return $"rgba({this}, {(decimal)alpha / 100})";
        }

        /// <summary>
        /// To integer
        /// </summary>
        /// <returns>RGB 24 bit unsigned integer value</returns>
        public int ToInt24() => (R << 16) | (G << 8) | B;

        /// <inheritdoc/>
        public override string ToString() => $"{R}, {G}, {B}";

        /// <summary>
        /// Determine if a 24 bit RGB unsigned integer value is valid
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>If valid</returns>
        public static bool IsValidRgbInt24(in int value) => value >= 0 && value <= MAX_24BIT_RGB;

        /// <summary>
        /// Sanitize a RGB 24 bit unsigned integer value
        /// </summary>
        /// <param name="rgb">RGB 24 bit unsigned integer value</param>
        /// <returns>sanitized RGB 24 bit unsigned integer value</returns>
        public static int Sanitize(in int rgb) => rgb < 0 ? 0 : rgb & MAX_24BIT_RGB;
    }
}
