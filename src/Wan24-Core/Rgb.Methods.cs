namespace wan24.Core
{
    // Methods
    public readonly partial record struct Rgb
    {
        /// <summary>
        /// Shade the color
        /// </summary>
        /// <param name="factor">Factor in percent (negative value darkens)</param>
        /// <returns>Shaded color</returns>
        public Rgb Shade(float factor)
        {
            if (factor < -1 || factor > 1) throw new ArgumentOutOfRangeException(nameof(factor));
            float r = R,
                g = G,
                b = B;
            if (factor < 0)
            {
                factor++;
                r *= factor;
                g *= factor;
                b *= factor;
            }
            else
            {
                r += (byte.MaxValue - r) * factor;
                g += (byte.MaxValue - g) * factor;
                b += (byte.MaxValue - b) * factor;
            }
            return new(
                Math.Min(byte.MaxValue, (int)r),
                Math.Min(byte.MaxValue, (int)g),
                Math.Min(byte.MaxValue, (int)b)
                );
        }

        /// <summary>
        /// Tint the color
        /// </summary>
        /// <param name="factor">Factor in percent (negative value darkens)</param>
        /// <returns>Tinted color</returns>
        public Rgb Tint(float factor)
        {
            if (factor < -1 || factor > 1) throw new ArgumentOutOfRangeException(nameof(factor));
            factor = factor < 0 ? factor + 1 : 1 - factor;
            return new(
                Math.Min(byte.MaxValue, (int)(R * factor)),
                Math.Min(byte.MaxValue, (int)(G * factor)),
                Math.Min(byte.MaxValue, (int)(B * factor))
                );
        }

        /// <summary>
        /// Invert the color
        /// </summary>
        /// <returns>Inverted color</returns>
        public Rgb Invert() => new(byte.MaxValue - R, byte.MaxValue - G, byte.MaxValue - B);

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
            if (buffer.Length < BINARY_SIZE) throw new ArgumentOutOfRangeException(nameof(buffer));
            buffer[0] = (byte)R;
            buffer[1] = (byte)G;
            buffer[2] = (byte)B;
            return buffer;
        }

        /// <summary>
        /// To hex string
        /// </summary>
        /// <returns>Hex string (upper case)</returns>
        public string ToHexString()
        {
            using RentedArrayRefStruct<byte> buffer = new(len: BINARY_SIZE, clean: false);
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
        /// <param name="alpha">Alpha value (%)</param>
        /// <returns>CSS RGB color value with alpha</returns>
        public string ToCssString(in float alpha)
        {
            if (alpha < 0 || alpha > 100) throw new ArgumentOutOfRangeException(nameof(alpha));
            return $"rgba({this}, {alpha.ToString().Replace(',', '.')})";
        }

        /// <summary>
        /// To integer
        /// </summary>
        /// <returns>RGB 24 bit unsigned integer value</returns>
        public int ToUInt24() => (B << 16) | (G << 8) | R;

        /// <inheritdoc/>
        public override string ToString() => $"{R}, {G}, {B}";
    }
}
