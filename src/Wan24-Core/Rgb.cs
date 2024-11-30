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
        public const int STUCTURE_SIZE = sizeof(int) * 3;
        /// <summary>
        /// Binary structure size in bytes (returned from <see cref="GetBytes()"/>)
        /// </summary>
        public const int BINARY_SIZE = 3;
        /// <summary>
        /// Red value byte offset
        /// </summary>
        public const int R_FIELD_OFFSET = 0;
        /// <summary>
        /// Green value byte offset
        /// </summary>
        public const int G_FIELD_OFFSET = R_FIELD_OFFSET + sizeof(int);
        /// <summary>
        /// Blue value byte offset
        /// </summary>
        public const int B_FIELD_OFFSET = G_FIELD_OFFSET + sizeof(int);

        /// <summary>
        /// Regular expression to match a CSS RGB string (single line)
        /// </summary>
        public static readonly Regex RX_CSS = new(
            @"^\s*rgba?\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(,\s*(\d+(\.\d+)?)\s*)?\)\s*$", 
            RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled, 
            TimeSpan.FromMilliseconds(3000)
            );
        /// <summary>
        /// Black (min. value)
        /// </summary>
        public static readonly Rgb Black = default;
        /// <summary>
        /// White (max. value)
        /// </summary>
        public static readonly Rgb White = new(byte.MaxValue, byte.MaxValue, byte.MaxValue);

        /// <summary>
        /// Red 8 bit unsigned integer value
        /// </summary>
        [FieldOffset(R_FIELD_OFFSET)]
        public readonly int R;
        /// <summary>
        /// Green 8 bit unsigned integer value
        /// </summary>
        [FieldOffset(G_FIELD_OFFSET)]
        public readonly int G;
        /// <summary>
        /// Blue 8 bit unsigned integer value
        /// </summary>
        [FieldOffset(B_FIELD_OFFSET)]
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
            if (!IsValidRgbUInt24(rgb)) throw new ArgumentOutOfRangeException(nameof(rgb));
            R = rgb & byte.MaxValue;
            G = (rgb >> 8) & byte.MaxValue;
            B = (rgb >> 16) & byte.MaxValue;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rgb">RGB bytes</param>
        public Rgb(in ReadOnlySpan<byte> rgb)
        {
            if (rgb.Length < BINARY_SIZE) throw new ArgumentOutOfRangeException(nameof(rgb));
            R = rgb[0];
            G = rgb[1];
            B = rgb[2];
        }

        /// <inheritdoc/>
        public static int? MaxStructureSize => BINARY_SIZE;

        /// <inheritdoc/>
        public static bool IsFixedStructureSize => true;

        /// <inheritdoc/>
        public static int? MaxStringSize => 13;

        /// <inheritdoc/>
        public static bool IsFixedStringSize => false;

        /// <inheritdoc/>
        public int? StructureSize => BINARY_SIZE;

        /// <inheritdoc/>
        public int? StringSize => null;

        /// <summary>
        /// Determine if a 24 bit RGB unsigned integer value is valid
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>If valid</returns>
        public static bool IsValidRgbUInt24(in int value) => value >= 0 && value <= MAX_24BIT_RGB;

        /// <summary>
        /// Sanitize a RGB 24 bit unsigned integer value
        /// </summary>
        /// <param name="rgb">RGB 24 bit unsigned integer value</param>
        /// <returns>sanitized RGB 24 bit unsigned integer value</returns>
        public static int Sanitize(in int rgb) => rgb < 0 ? 0 : rgb & MAX_24BIT_RGB;
    }
}
