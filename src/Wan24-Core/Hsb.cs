using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// HSB
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly record struct Hsb : ISerializeBinary<Hsb>, ISerializeString<Hsb>
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
            if (Hue < 0 || Hue > 1) throw new InvalidDataException("Invalid hue value");
            if (Saturation < 0 || Saturation > 1) throw new InvalidDataException("Invalid saturation value");
            if (Brightness < 0 || Brightness > 1) throw new InvalidDataException("Invalid brightness value");
            if (Alpha < 0 || Alpha > 1) throw new InvalidDataException("Invalid alpha value");
        }

        /// <inheritdoc/>
        public static int? MaxStructureSize => BINARY_SIZE;

        /// <inheritdoc/>
        public static bool IsFixedStructureSize => true;

        /// <inheritdoc/>
        public static int? MaxStringSize => null;

        /// <inheritdoc/>
        public static bool IsFixedStringSize => false;

        /// <inheritdoc/>
        public int? StructureSize => BINARY_SIZE;

        /// <inheritdoc/>
        public int? StringSize => null;

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
            Hue.GetBytes(buffer);
            Saturation.GetBytes(buffer[SATURATION_FIELD_BYTE_OFFSET..]);
            Brightness.GetBytes(buffer[BRIGHTNESS_FIELD_BYTE_OFFSET..]);
            Alpha.GetBytes(buffer[ALPHA_FIELD_BYTE_OFFSET..]);
            return BINARY_SIZE;
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

        /// <inheritdoc/>
        public static object DeserializeFrom(in ReadOnlySpan<byte> buffer) => new Hsb(buffer);

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
                float hue = buffer.ToFloat(),
                    saturation = buffer[SATURATION_FIELD_BYTE_OFFSET..].ToFloat(),
                    brightness = buffer[BRIGHTNESS_FIELD_BYTE_OFFSET..].ToFloat(),
                    alpha = buffer[ALPHA_FIELD_BYTE_OFFSET..].ToFloat();
                if (hue < 0 || hue > 1 || saturation < 0 || saturation > 1 || brightness < 0 || brightness > 1 || alpha < 0 || alpha > 1)
                {
                    result = null;
                    return false;
                }
                result = new Hsb(hue, saturation, brightness, alpha);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public static Hsb DeserializeTypeFrom(in ReadOnlySpan<byte> buffer) => new(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeTypeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out Hsb result)
        {
            try
            {
                if (buffer.Length < BINARY_SIZE)
                {
                    result = default;
                    return false;
                }
                float hue = buffer.ToFloat(),
                    saturation = buffer[SATURATION_FIELD_BYTE_OFFSET..].ToFloat(),
                    brightness = buffer[BRIGHTNESS_FIELD_BYTE_OFFSET..].ToFloat(),
                    alpha = buffer[ALPHA_FIELD_BYTE_OFFSET..].ToFloat();
                if (hue < 0 || hue > 1 || saturation < 0 || saturation > 1 || brightness < 0 || brightness > 1 || alpha < 0 || alpha > 1)
                {
                    result = default;
                    return false;
                }
                result = new Hsb(hue, saturation, brightness, alpha);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <inheritdoc/>
        public static Hsb Parse(in ReadOnlySpan<char> str)
        {
            int index1 = str.IndexOf(',');
            if (index1 < 0) throw new ArgumentException("Invalid string format", nameof(str));
            int index2 = str[(index1 + 1)..].IndexOf(',');
            if (index2 < 0) throw new ArgumentException("Invalid string format", nameof(str));
            int index3 = str[(index1 + index2 + 2)..].IndexOf(',');
            return new(
                float.Parse(new string(str[..index1]).Replace('.', ',')),
                float.Parse(new string(str.Slice(index1 + 1, index2)).Replace('.', ',')),
                float.Parse(new string(index3 < 0 ? str[(index1 + index2 + 2)..] : str.Slice(index1 + index2 + 2, index3)).Replace('.', ',')),
                index3 < 0
                    ? 1
                    : float.Parse(new string(str[(index1 + index2 + index3 + 3)..]).Replace('.', ','))
                );
        }

        /// <inheritdoc/>
        public static bool TryParse(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out Hsb result)
        {
            int index1 = str.IndexOf(',');
            if (index1 < 0)
            {
                if (Logging.Debug) Logging.WriteDebug("Failed to parse HSB value: Invalid string format");
                result = default;
                return false;
            }
            int index2 = str[(index1 + 1)..].IndexOf(',');
            if (index2 < 0)
            {
                if (Logging.Debug) Logging.WriteDebug("Failed to parse HSB value: Invalid string format");
                result = default;
                return false;
            }
            int index3 = str[(index1 + index2 + 2)..].IndexOf(',');
            if (!float.TryParse(new string(str[..index1]).Replace('.', ','), out float h))
            {
                if (Logging.Debug) Logging.WriteDebug("Failed to parse HSB value: Invalid hue value");
                result = default;
                return false;
            }
            if (!float.TryParse(new string(str.Slice(index1 + 1, index2)).Replace('.', ','), out float s))
            {
                if (Logging.Debug) Logging.WriteDebug("Failed to parse HSB value: Invalid saturation value");
                result = default;
                return false;
            }
            if (!float.TryParse(new string(str.Slice(index1 + index2 + 2, index2)).Replace('.', ','), out float b))
            {
                if (Logging.Debug) Logging.WriteDebug("Failed to parse HSB value: Invalid brightness value");
                result = default;
                return false;
            }
            float a;
            if (index3 >= 0)
            {
                if (!float.TryParse(new string(str[(index1 + index2 + index3 + 3)..]).Replace('.', ','), out a))
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

        /// <inheritdoc/>
        public static object ParseObject(in ReadOnlySpan<char> str) => Parse(str);

        /// <inheritdoc/>
        public static bool TryParseObject(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out object? result)
        {
            bool res;
            result = (res = TryParse(str, out Hsb hsb))
                ? hsb
                : default(Hsb?);
            return res;
        }
    }
}
