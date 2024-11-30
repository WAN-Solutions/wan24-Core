using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// XYZ value
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly record struct XYZ : ISerializeBinary<XYZ>, ISerializeString<XYZ>
    {
        /// <summary>
        /// Structure size in bytes
        /// </summary>
        public const int STRUCTURE_SIZE = Z_FIELD_OFFSET + Z_FIELD_SIZE;
        /// <summary>
        /// X value field byte offset
        /// </summary>
        public const int X_FIELD_OFFSET = 0;
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
        /// Z value field byte offset
        /// </summary>
        public const int Z_FIELD_OFFSET = Y_FIELD_OFFSET + Y_FIELD_SIZE;
        /// <summary>
        /// Z value field size in bytes
        /// </summary>
        public const int Z_FIELD_SIZE = sizeof(double);

        /// <summary>
        /// Zero X/Y/Z
        /// </summary>
        public static readonly XYZ Zero = new();
        /// <summary>
        /// Minimum value
        /// </summary>
        public static readonly XYZ MinValue = new(double.MinValue, double.MinValue, double.MinValue);
        /// <summary>
        /// Maximum value
        /// </summary>
        public static readonly XYZ MaxValue = new(double.MaxValue, double.MaxValue, double.MaxValue);

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
        /// Z value (real number)
        /// </summary>
        [FieldOffset(Z_FIELD_OFFSET)]
        public readonly double Z;

        /// <summary>
        /// Constructor
        /// </summary>
        public XYZ()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">X value (real number)</param>
        /// <param name="y">Y value (real number)</param>
        /// <param name="z">Z value (real number)</param>
        public XYZ(in double x, in double y, in double z)
        {
            if (double.IsNaN(x) || !double.IsFinite(x)) throw new ArgumentException("Invalid value", nameof(x));
            if (double.IsNaN(y) || !double.IsFinite(y)) throw new ArgumentException("Invalid value", nameof(y));
            if (double.IsNaN(z) || !double.IsFinite(z)) throw new ArgumentException("Invalid value", nameof(z));
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Serialized data (min. <see cref="STRUCTURE_SIZE"/> bytes length required)</param>
        public XYZ(in ReadOnlySpan<byte> data)
        {
            if (data.Length < STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(data));
            X = data.ToDouble();
            if (double.IsNaN(X) || !double.IsFinite(X)) throw new InvalidDataException("Invalid X value");
            Y = data[Y_FIELD_OFFSET..].ToDouble();
            if (double.IsNaN(Y) || !double.IsFinite(Y)) throw new InvalidDataException("Invalid Y value");
            Z = data[Z_FIELD_OFFSET..].ToDouble();
            if (double.IsNaN(Z) || !double.IsFinite(Z)) throw new InvalidDataException("Invalid Z value");
        }

        /// <inheritdoc/>
        public static int? MaxStructureSize => STRUCTURE_SIZE;

        /// <inheritdoc/>
        public static bool IsFixedStructureSize => true;

        /// <inheritdoc/>
        public static int? MaxStringSize => byte.MaxValue;

        /// <inheritdoc/>
        public static bool IsFixedStringSize => false;

        /// <inheritdoc/>
        public int? StructureSize => STRUCTURE_SIZE;

        /// <inheritdoc/>
        public int? StringSize => null;

        /// <summary>
        /// Get the values as <see cref="XY"/>
        /// </summary>
        public XY AsXy
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => new(X, Y);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int GetBytes(in Span<byte> buffer)
        {
            if (buffer.Length < STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(buffer));
            X.GetBytes(buffer);
            Y.GetBytes(buffer[Y_FIELD_OFFSET..]);
            Z.GetBytes(buffer[Z_FIELD_OFFSET..]);
            return STRUCTURE_SIZE;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override string ToString() => $"{X};{Y};{Z}";

        /// <summary>
        /// Cast as serialized data
        /// </summary>
        /// <param name="xyz">XYZ</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator byte[](in XYZ xyz) => xyz.GetBytes();

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZ(in byte[] data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZ(in Span<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZ(in ReadOnlySpan<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZ(in Memory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZ(in ReadOnlyMemory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast as <see cref="string"/>
        /// </summary>
        /// <param name="xy"><see cref="XY"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator string(in XYZ xy) => xy.ToString();

        /// <summary>
        /// Cast as <see cref="string"/>
        /// </summary>
        /// <param name="xy"><see cref="XY"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ReadOnlySpan<char>(in XYZ xy) => xy.ToString();

        /// <summary>
        /// Cast from a <see cref="string"/>
        /// </summary>
        /// <param name="str">String</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZ(in ReadOnlySpan<char> str) => Parse(str);

        /// <summary>
        /// Cast from a <see cref="string"/>
        /// </summary>
        /// <param name="str">String</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZ(in string str) => Parse(str);

        /// <summary>
        /// Cast as <see cref="double"/> array
        /// </summary>
        /// <param name="xyz"><see cref="XYZ"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator double[](in XYZ xyz) => [xyz.X, xyz.Y, xyz.Z];

        /// <summary>
        /// Cas as <see cref="XY"/>
        /// </summary>
        /// <param name="xyz"><see cref="XYZ"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XY(in XYZ xyz) => xyz.AsXy;

        /// <inheritdoc/>
        public static object DeserializeFrom(in ReadOnlySpan<byte> buffer) => new XYZ(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out object? result)
        {
            try
            {
                if (buffer.Length < STRUCTURE_SIZE)
                {
                    result = null;
                    return false;
                }
                double x = buffer.ToDouble();
                if (double.IsNaN(x) || !double.IsFinite(x))
                {
                    result = null;
                    return false;
                }
                double y = buffer[Y_FIELD_OFFSET..].ToDouble();
                if (double.IsNaN(y) || !double.IsFinite(y))
                {
                    result = null;
                    return false;
                }
                double z = buffer[Z_FIELD_OFFSET..].ToDouble();
                if (double.IsNaN(z) || !double.IsFinite(z))
                {
                    result = null;
                    return false;
                }
                result = new XYZ(x, y, z);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public static XYZ DeserializeTypeFrom(in ReadOnlySpan<byte> buffer) => new(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeTypeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out XYZ result)
        {
            try
            {
                if (buffer.Length < STRUCTURE_SIZE)
                {
                    result = default;
                    return false;
                }
                double x = buffer.ToDouble();
                if (double.IsNaN(x) || !double.IsFinite(x))
                {
                    result = default;
                    return false;
                }
                double y = buffer[Y_FIELD_OFFSET..].ToDouble();
                if (double.IsNaN(y) || !double.IsFinite(y))
                {
                    result = default;
                    return false;
                }
                double z = buffer[Z_FIELD_OFFSET..].ToDouble();
                if (double.IsNaN(z) || !double.IsFinite(z))
                {
                    result = default;
                    return false;
                }
                result = new XYZ(x, y, z);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static XYZ Parse(in ReadOnlySpan<char> str)
        {
            int index1 = str.IndexOf(';');
            if (index1 < 1) throw new FormatException("No value separator found in input string");
            int index2 = str[(index1 + 1)..].IndexOf(';');
            if (index2 < 1) throw new FormatException("No second value separator found in input string");
            return new(double.Parse(str[..index1]), double.Parse(str.Slice(index1 + 1, index2)), double.Parse(str[(index1 + index2 + 2)..]));
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryParse(in ReadOnlySpan<char> str, out XYZ result)
        {
            result = default;
            int index1 = str.IndexOf(';');
            if (index1 < 1) return false;
            int index2 = str[(index1 + 1)..].IndexOf(';');
            if (index2 < 1) return false;
            if (!double.TryParse(str[..index1], out double x) || double.IsNaN(x) || !double.IsFinite(x)) return false;
            if (!double.TryParse(str.Slice(index1 + 1, index2), out double y) || double.IsNaN(y) || !double.IsFinite(y)) return false;
            if (!double.TryParse(str[(index1 + index2 + 2)..], out double z) || double.IsNaN(z) || !double.IsFinite(z)) return false;
            result = new(x, y, z);
            return true;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object ParseObject(in ReadOnlySpan<char> str) => Parse(str);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryParseObject(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out object? result)
        {
            bool res;
            result = (res = TryParse(str, out XYZ xy))
                ? xy
                : default(XYZ?);
            return res;
        }
    }
}
