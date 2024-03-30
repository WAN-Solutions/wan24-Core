using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// HSB
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly record struct Hsb
    {
        /// <summary>
        /// Structure size in bytes
        /// </summary>
        public const int STRUCTURE_SIZE = sizeof(float) << 2;
        /// <summary>
        /// Binary size in bytes
        /// </summary>
        public const int BINARY_SIZE = STRUCTURE_SIZE;
        /// <summary>
        /// Hue field byte offset
        /// </summary>
        public const int HUE_FIELD_BYTE_OFFSET = 0;
        /// <summary>
        /// Saturation field byte offset
        /// </summary>
        public const int SATURATION_FIELD_BYTE_OFFSET = HUE_FIELD_BYTE_OFFSET + sizeof(float);
        /// <summary>
        /// Brightness field byte offset
        /// </summary>
        public const int BRIGHTNESS_FIELD_BYTE_OFFSET = SATURATION_FIELD_BYTE_OFFSET + sizeof(float);
        /// <summary>
        /// Alpha field byte offset
        /// </summary>
        public const int ALPHA_FIELD_BYTE_OFFSET = BRIGHTNESS_FIELD_BYTE_OFFSET + sizeof(float);

        /// <summary>
        /// White
        /// </summary>
        public static readonly Hsb White = new(1);
        /// <summary>
        /// Black
        /// </summary>
        public static readonly Hsb Black = default;

        /// <summary>
        /// Hue (%)
        /// </summary>
        [FieldOffset(HUE_FIELD_BYTE_OFFSET)]
        public readonly float Hue;
        /// <summary>
        /// Saturation (%)
        /// </summary>
        [FieldOffset(SATURATION_FIELD_BYTE_OFFSET)]
        public readonly float Saturation;
        /// <summary>
        /// Brightness (%)
        /// </summary>
        [FieldOffset(BRIGHTNESS_FIELD_BYTE_OFFSET)]
        public readonly float Brightness;
        /// <summary>
        /// Alpha (%)
        /// </summary>
        [FieldOffset(ALPHA_FIELD_BYTE_OFFSET)]
        public readonly float Alpha;

        /// <summary>
        /// Constructor
        /// </summary>
        public Hsb()
        {
            Hue = default;
            Saturation = default;
            Brightness = default;
            Alpha = default;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hue">Hue (%)</param>
        /// <param name="saturation">Saturation (%)</param>
        /// <param name="brightness">Brightness (%)</param>
        /// <param name="alpha">Alpha (%)</param>
        public Hsb(in float hue, in float saturation = 1, in float brightness = 1, in float alpha = 1)
        {
            if (hue < 0 || hue > 1) throw new ArgumentOutOfRangeException(nameof(hue));
            if (saturation < 0 || saturation > 1) throw new ArgumentOutOfRangeException(nameof(saturation));
            if (brightness < 0 || brightness > 1) throw new ArgumentOutOfRangeException(nameof(brightness));
            if (alpha < 0 || alpha > 1) throw new ArgumentOutOfRangeException(nameof(alpha));
            Hue = hue;
            Saturation = saturation;
            Brightness = brightness;
            Alpha = alpha;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public Hsb(in ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < BINARY_SIZE) throw new ArgumentOutOfRangeException(nameof(buffer));
            Hue = buffer.ToFloat();
            Saturation = buffer[SATURATION_FIELD_BYTE_OFFSET..].ToFloat();
            Brightness = buffer[BRIGHTNESS_FIELD_BYTE_OFFSET..].ToFloat();
            Alpha = buffer[ALPHA_FIELD_BYTE_OFFSET..].ToFloat();
        }

        /// <summary>
        /// Hue degree value
        /// </summary>
        public float HueDegree => Hue * 360;

        /// <summary>
        /// Get this instance with another hue value
        /// </summary>
        /// <param name="hue">Hue (%)</param>
        /// <returns><see cref="Hsb"/></returns>
        public Hsb WithHue(in float hue)
        {
            if (hue < 0 || hue > 1) throw new ArgumentOutOfRangeException(nameof(hue));
            return new(hue, Saturation, Brightness, Alpha);
        }

        /// <summary>
        /// Get this instance with an added hue value
        /// </summary>
        /// <param name="hue">Hue (%)</param>
        /// <returns><see cref="Hsb"/></returns>
        public Hsb AddHue(in float hue)
        {
            if (hue < -1 || hue > 1) throw new ArgumentOutOfRangeException(nameof(hue));
            return new(MathF.Min(1, MathF.Max(0, Hue + hue)), Saturation, Brightness, Alpha);
        }

        /// <summary>
        /// Get this instance with another saturation value
        /// </summary>
        /// <param name="saturation">Saturation (%)</param>
        /// <returns><see cref="Hsb"/></returns>
        public Hsb WithSaturation(in float saturation)
        {
            if (saturation < 0 || saturation > 1) throw new ArgumentOutOfRangeException(nameof(saturation));
            return new(Hue, saturation, Brightness, Alpha);
        }

        /// <summary>
        /// Get this instance with an added saturation value
        /// </summary>
        /// <param name="saturation">Saturation (%)</param>
        /// <returns><see cref="Hsb"/></returns>
        public Hsb AddSaturation(in float saturation)
        {
            if (saturation < -1 || saturation > 1) throw new ArgumentOutOfRangeException(nameof(saturation));
            return new(Hue, MathF.Min(1, MathF.Max(0, Saturation + saturation)), Brightness, Alpha);
        }

        /// <summary>
        /// Get this instance with another brightness value
        /// </summary>
        /// <param name="brightness">Brightness (%)</param>
        /// <returns><see cref="Hsb"/></returns>
        public Hsb WithBrightness(in float brightness)
        {
            if (brightness < 0 || brightness > 1) throw new ArgumentOutOfRangeException(nameof(brightness));
            return new(Hue, Saturation, brightness, Alpha);
        }

        /// <summary>
        /// Get this instance with an added brightness value
        /// </summary>
        /// <param name="brightness">Brightness (%)</param>
        /// <returns><see cref="Hsb"/></returns>
        public Hsb AddBrightness(in float brightness)
        {
            if (brightness < -1 || brightness > 1) throw new ArgumentOutOfRangeException(nameof(brightness));
            return new(Hue, Saturation, MathF.Min(1, MathF.Max(0, Brightness + brightness)), Alpha);
        }

        /// <summary>
        /// Get this instance with another alpha value
        /// </summary>
        /// <param name="alpha">Alpha (%)</param>
        /// <returns><see cref="Hsb"/></returns>
        public Hsb WithAlpha(in float alpha)
        {
            if (alpha < 0 || alpha > 1) throw new ArgumentOutOfRangeException(nameof(alpha));
            return new(Hue, Saturation, Brightness, alpha);
        }

        /// <summary>
        /// Get this instance with an added alpha value
        /// </summary>
        /// <param name="alpha">Alpha (%)</param>
        /// <returns><see cref="Hsb"/></returns>
        public Hsb AddAlpha(in float alpha)
        {
            if (alpha < -1 || alpha > 1) throw new ArgumentOutOfRangeException(nameof(alpha));
            return new(Hue, Saturation, Brightness, MathF.Min(1, MathF.Max(0, Alpha + alpha)));
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <returns>Bytes</returns>
        public byte[] GetBytes()
        {
            byte[] res = new byte[BINARY_SIZE];
            GetBytes(res);
            return res;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Buffer</returns>
        public Span<byte> GetBytes(in Span<byte> buffer)
        {
            if (buffer.Length < BINARY_SIZE) throw new ArgumentOutOfRangeException(nameof(buffer));
            Hue.GetBytes(buffer);
            Saturation.GetBytes(buffer[SATURATION_FIELD_BYTE_OFFSET..]);
            Brightness.GetBytes(buffer[BRIGHTNESS_FIELD_BYTE_OFFSET..]);
            Alpha.GetBytes(buffer[ALPHA_FIELD_BYTE_OFFSET..]);
            return buffer;
        }

        /// <summary>
        /// Get as RGBA
        /// </summary>
        /// <returns><see cref="RgbA"/></returns>
        public RgbA ToRgbA()
        {
            float r = Brightness,
                g = Brightness,
                b = Brightness;
            if (Saturation != 0)
            {
                float max = Brightness,
                    diff = Brightness * Saturation,
                    min = Brightness - diff,
                    hue = HueDegree;
                if (hue < 60)
                {
                    r = max;
                    g = hue * diff / 60 + min;
                    b = min;
                }
                else if (hue < 120)
                {
                    r = -(hue - 120) * diff / 60 + min;
                    g = max;
                    b = min;
                }
                else if (hue < 180)
                {
                    r = min;
                    g = max;
                    b = (hue - 120) * diff / 60 + min;
                }
                else if (hue < 240)
                {
                    r = min;
                    g = -(hue - 240) * diff / 60 + min;
                    b = max;
                }
                else if (hue < 300)
                {
                    r = (hue - 240) * diff / 60 + min;
                    g = min;
                    b = max;
                }
                else if (hue <= 360)
                {
                    r = max;
                    g = min;
                    b = -(hue - 360) * diff / 60 + min;
                }
                else
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }
            return new(
                new Rgb(
                    (int)Math.Clamp(r, 0, byte.MaxValue),
                    (int)Math.Clamp(g, 0, byte.MaxValue),
                    (int)Math.Clamp(b, 0, byte.MaxValue)
                    ),
                Alpha
                );
        }

        /// <inheritdoc/>
        public override string ToString()
            => $"{Hue.ToString().Replace(',', '.')}, {Saturation.ToString().Replace(',', '.')}, {Brightness.ToString().Replace(',', '.')}, {Alpha.ToString().Replace(',', '.')}";

        /// <summary>
        /// Cast from <see cref="Rgb"/>
        /// </summary>
        /// <param name="rgb"><see cref="Rgb"/></param>
        public static implicit operator Hsb(in Rgb rgb) => FromRgbA(new(rgb));

        /// <summary>
        /// Cast from <see cref="RgbA"/>
        /// </summary>
        /// <param name="buffer"><see cref="RgbA"/></param>
        public static implicit operator Hsb(in RgbA buffer) => FromRgbA(buffer);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public static implicit operator Hsb(in byte[] buffer) => new(buffer);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public static implicit operator Hsb(in Span<byte> buffer) => new(buffer);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public static implicit operator Hsb(in ReadOnlySpan<byte> buffer) => new(buffer);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public static implicit operator Hsb(in Memory<byte> buffer) => new(buffer.Span);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public static implicit operator Hsb(in ReadOnlyMemory<byte> buffer) => new(buffer.Span);

        /// <summary>
        /// Cast as <see cref="Rgb"/>
        /// </summary>
        /// <param name="hsb"><see cref="Hsb"/></param>
        public static implicit operator Rgb(in Hsb hsb) => hsb.ToRgbA();

        /// <summary>
        /// Cast as <see cref="RgbA"/>
        /// </summary>
        /// <param name="hsb"><see cref="Hsb"/></param>
        public static implicit operator RgbA(in Hsb hsb) => hsb.ToRgbA();

        /// <summary>
        /// Cast as bytes
        /// </summary>
        /// <param name="hsb"><see cref="Hsb"/></param>
        public static implicit operator byte[](in Hsb hsb) => hsb.GetBytes();

        /// <summary>
        /// Create from RGBA
        /// </summary>
        /// <param name="rgb"><see cref="RgbA"/></param>
        /// <returns><see cref="Hsb"/></returns>
        public static Hsb FromRgbA(in RgbA rgb)
        {
            float r = rgb.RGB.R,
                g = rgb.RGB.G,
                b = rgb.RGB.B,
                max = MathF.Max(r, MathF.Max(g, b));
            if (max <= 0) return default;
            float min = MathF.Min(r, MathF.Min(g, b));
            if (max == min && min == byte.MaxValue) return new(360);
            float diff = max - min,
                hue;
            if (max > min)
            {
                if (g == max) hue = (b - r) / diff * 60 + 120;
                else if (b == max) hue = (r - g) / diff * 60 * 240;
                else if (b > g) hue = (g - b) / diff * 60 + 360;
                else hue = (g - b) / diff * 60;
                if (hue < 0) hue += 360;
            }
            else
            {
                hue = 0;
            }
            return new(hue * (1f / 360), diff / max, max, rgb.Alpha);
        }

        /// <summary>
        /// Create from RGB
        /// </summary>
        /// <param name="rgb"><see cref="Rgb"/></param>
        /// <returns><see cref="Hsb"/></returns>
        public static Hsb FromRgb(in Rgb rgb)
        {
            float r = rgb.R,
                g = rgb.G,
                b = rgb.B,
                max = MathF.Max(r, MathF.Max(g, b));
            if (max <= 0) return default;
            float min = MathF.Min(r, MathF.Min(g, b));
            if (max == min && min == byte.MaxValue) return new(360);
            float diff = max - min,
                hue;
            if (max > min)
            {
                if (g == max) hue = (b - r) / diff * 60 + 120;
                else if (b == max) hue = (r - g) / diff * 60 * 240;
                else if (b > g) hue = (g - b) / diff * 60 + 360;
                else hue = (g - b) / diff * 60;
                if (hue < 0) hue += 360;
            }
            else
            {
                hue = 0;
            }
            return new(hue * (1f / 360), diff / max, max);
        }

        /// <summary>
        /// Parse from a <see cref="string"/>
        /// </summary>
        /// <param name="str"><see cref="string"/></param>
        /// <returns><see cref="Hsb"/></returns>
        public static Hsb Parse(in string str)
        {
            string[] hsba = str.Split(',');
            if (hsba.Length < 3 || hsba.Length > 4) throw new ArgumentException("Invalid string format", nameof(str));
            return new(
                float.Parse(hsba[0].Replace('.', ',')),
                float.Parse(hsba[1].Replace('.', ',')),
                float.Parse(hsba[2].Replace('.', ',')),
                hsba.Length == 3
                    ? 1
                    : float.Parse(hsba[3].Replace('.', ','))
                );
        }

        /// <summary>
        /// Parse from a <see cref="string"/>
        /// </summary>
        /// <param name="str"><see cref="string"/></param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryParse(in string str, out Hsb result)
        {
            string[] hsba = str.Split(',');
            if (hsba.Length < 3 || hsba.Length > 4)
            {
                if (Logging.Debug) Logging.WriteDebug("Failed to parse HSB value: Invalid string format");
                result = default;
                return false;
            }
            if (!float.TryParse(hsba[0].Replace('.', ','), out float h))
            {
                if (Logging.Debug) Logging.WriteDebug("Failed to parse HSB value: Invalid hue value");
                result = default;
                return false;
            }
            if (!float.TryParse(hsba[1].Replace('.', ','), out float s))
            {
                if (Logging.Debug) Logging.WriteDebug("Failed to parse HSB value: Invalid saturation value");
                result = default;
                return false;
            }
            if (!float.TryParse(hsba[2].Replace('.', ','), out float b))
            {
                if (Logging.Debug) Logging.WriteDebug("Failed to parse HSB value: Invalid brightness value");
                result = default;
                return false;
            }
            float a;
            if (hsba.Length > 3)
            {
                if (!float.TryParse(hsba[3].Replace('.', ','), out a))
                {
                    if (Logging.Debug) Logging.WriteDebug("Failed to parse HSB value: Invalid alpha value");
                    result = default;
                    return false;
                }
            }
            else
            {
                a = 1;
            }
            result = new(h, s, b, a);
            return true;
        }
    }
}
