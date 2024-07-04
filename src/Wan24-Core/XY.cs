using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// XY value
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly record struct XY
    {
        /// <summary>
        /// Structure size in bytes
        /// </summary>
        public const int STRUCTURE_SIZE = Y_FIELD_OFFSET + Y_FIELD_SIZE;
        /// <summary>
        /// X value field byte offset
        /// </summary>
        public const int X_FIELD_OFFSET= 0;
        /// <summary>
        /// X value field size in bytes
        /// </summary>
        public const int X_FIELD_SIZE = sizeof(double);
        /// <summary>
        /// Y value field byte offset
        /// </summary>
        public const int Y_FIELD_OFFSET = X_FIELD_OFFSET + X_FIELD_SIZE;
        /// <summary>
        /// Y value field size in bytes
        /// </summary>
        public const int Y_FIELD_SIZE = sizeof(double);

        /// <summary>
        /// Zero X/Y
        /// </summary>
        public static readonly XY Zero = new();
        /// <summary>
        /// Minimum value
        /// </summary>
        public static readonly XY MinValue = new(double.MinValue, double.MinValue);
        /// <summary>
        /// Maximum value
        /// </summary>
        public static readonly XY MaxValue = new(double.MaxValue, double.MaxValue);

        /// <summary>
        /// X value (real number)
        /// </summary>
        [FieldOffset(X_FIELD_OFFSET)]
        public readonly double X;
        /// <summary>
        /// Y value (real number)
        /// </summary>
        [FieldOffset(Y_FIELD_OFFSET)]
        public readonly double Y;

        /// <summary>
        /// Constructor
        /// </summary>
        public XY()
        {
            X = 0;
            Y = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">X value (real number)</param>
        /// <param name="y">Y value (real number)</param>
        public XY(in double x, in double y)
        {
            if (double.IsNaN(x) || !double.IsFinite(x)) throw new ArgumentException("Invalid value", nameof(x));
            if (double.IsNaN(y) || !double.IsFinite(y)) throw new ArgumentException("Invalid value", nameof(y));
            X = x;
            Y = y;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Serialized data (min. <see cref="STRUCTURE_SIZE"/> bytes length required)</param>
        public XY(in ReadOnlySpan<byte> data)
        {
            if (data.Length < STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(data));
            X = data.ToDouble();
            if (double.IsNaN(X) || !double.IsFinite(X)) throw new InvalidDataException("Invalid X value");
            Y = data[Y_FIELD_OFFSET..].ToDouble();
            if (double.IsNaN(Y) || !double.IsFinite(Y)) throw new InvalidDataException("Invalid Y value");
        }

        /// <summary>
        /// Get this structure as serialized data
        /// </summary>
        /// <returns>Serialized data</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public byte[] GetBytes()
        {
            byte[] res = new byte[STRUCTURE_SIZE];
            GetBytes(res);
            return res;
        }

        /// <summary>
        /// Get this structure as serialized data
        /// </summary>
        /// <param name="buffer">Buffer (min. <see cref="STRUCTURE_SIZE"/> bytes length required)</param>
        /// <returns>Buffer</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Span<byte> GetBytes(in Span<byte> buffer)
        {
            if (buffer.Length < STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(buffer));
            X.GetBytes(buffer);
            Y.GetBytes(buffer[Y_FIELD_OFFSET..]);
            return buffer;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override string ToString() => $"{X};{Y}";

        /// <summary>
        /// Cast as serialized data
        /// </summary>
        /// <param name="xy">XY</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator byte[](in XY xy) => xy.GetBytes();

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XY(in byte[] data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XY(in Span<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XY(in ReadOnlySpan<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XY(in Memory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XY(in ReadOnlyMemory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast as <see cref="string"/>
        /// </summary>
        /// <param name="xy"><see cref="XY"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator string(in XY xy) => xy.ToString();

        /// <summary>
        /// Cast as <see cref="string"/>
        /// </summary>
        /// <param name="xy"><see cref="XY"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ReadOnlySpan<char>(in XY xy) => xy.ToString();

        /// <summary>
        /// Cast from a <see cref="string"/>
        /// </summary>
        /// <param name="str">String</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XY(in ReadOnlySpan<char> str) => Parse(str);

        /// <summary>
        /// Cast from a <see cref="string"/>
        /// </summary>
        /// <param name="str">String</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XY(in string str) => Parse(str);

        /// <summary>
        /// Cast as <see cref="double"/> array
        /// </summary>
        /// <param name="xy"><see cref="XY"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator double[](in XY xy) => [xy.X, xy.Y];

        /// <summary>
        /// Parse a string
        /// </summary>
        /// <param name="str">String</param>
        /// <returns><see cref="XY"/></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static XY Parse(in ReadOnlySpan<char> str)
        {
            int index = str.IndexOf(';');
            if (index < 1) throw new FormatException("No value separator found in input string");
            return new(double.Parse(str[..index]), double.Parse(str[(index + 1)..]));
        }

        /// <summary>
        /// Try parsing a string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="result"><see cref="XY"/></param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryParse(in ReadOnlySpan<char> str, out XY result)
        {
            result = default;
            int index = str.IndexOf(';');
            if (index < 1) return false;
            if (!double.TryParse(str[..index], out double x) || double.IsNaN(x) || !double.IsFinite(x)) return false;
            if (!double.TryParse(str[(index + 1)..], out double y) || double.IsNaN(y) || !double.IsFinite(y)) return false;
            result = new(x, y);
            return true;
        }
    }
}
