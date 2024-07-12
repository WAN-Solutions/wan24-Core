using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// XYZ value
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly record struct XYZInt
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
        public const int X_FIELD_SIZE = sizeof(int);
        /// <summary>
        /// Y value field byte offset
        /// </summary>
        public const int Y_FIELD_OFFSET = X_FIELD_OFFSET + X_FIELD_SIZE;
        /// <summary>
        /// Y value field size in bytes
        /// </summary>
        public const int Y_FIELD_SIZE = sizeof(int);
        /// <summary>
        /// Y value field byte offset
        /// </summary>
        public const int Z_FIELD_OFFSET = Y_FIELD_OFFSET + Y_FIELD_SIZE;
        /// <summary>
        /// Y value field size in bytes
        /// </summary>
        public const int Z_FIELD_SIZE = sizeof(int);

        /// <summary>
        /// Zero X/Y/Z
        /// </summary>
        public static readonly XYZInt Zero = new();
        /// <summary>
        /// Minimum value
        /// </summary>
        public static readonly XYZInt MinValue = new(int.MinValue, int.MinValue, int.MinValue);
        /// <summary>
        /// Maximum value
        /// </summary>
        public static readonly XYZInt MaxValue = new(int.MaxValue, int.MaxValue, int.MaxValue);

        /// <summary>
        /// X value
        /// </summary>
        [FieldOffset(X_FIELD_OFFSET)]
        public readonly int X;
        /// <summary>
        /// Y value
        /// </summary>
        [FieldOffset(Y_FIELD_OFFSET)]
        public readonly int Y;
        /// <summary>
        /// Z value
        /// </summary>
        [FieldOffset(Z_FIELD_OFFSET)]
        public readonly int Z;

        /// <summary>
        /// Constructor
        /// </summary>
        public XYZInt()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        /// <param name="z">Z value</param>
        public XYZInt(in int x, in int y, in int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Serialized data (min. <see cref="STRUCTURE_SIZE"/> bytes length required)</param>
        public XYZInt(in ReadOnlySpan<byte> data)
        {
            if (data.Length < STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(data));
            X = data.ToInt();
            Y = data[Y_FIELD_OFFSET..].ToInt();
            Z = data[Z_FIELD_OFFSET..].ToInt();
        }

        /// <summary>
        /// Get the values as <see cref="XYInt"/>
        /// </summary>
        public XYInt AsXyInt
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => new(X, Y);
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
            Z.GetBytes(buffer[Z_FIELD_OFFSET..]);
            return buffer;
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
        public static implicit operator byte[](in XYZInt xyz) => xyz.GetBytes();

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZInt(in byte[] data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZInt(in Span<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZInt(in ReadOnlySpan<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZInt(in Memory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZInt(in ReadOnlyMemory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast as <see cref="string"/>
        /// </summary>
        /// <param name="xy"><see cref="XYZInt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator string(in XYZInt xy) => xy.ToString();

        /// <summary>
        /// Cast as <see cref="string"/>
        /// </summary>
        /// <param name="xy"><see cref="XYZInt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ReadOnlySpan<char>(in XYZInt xy) => xy.ToString();

        /// <summary>
        /// Cast from a <see cref="string"/>
        /// </summary>
        /// <param name="str">String</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZInt(in ReadOnlySpan<char> str) => Parse(str);

        /// <summary>
        /// Cast from a <see cref="string"/>
        /// </summary>
        /// <param name="str">String</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYZInt(in string str) => Parse(str);

        /// <summary>
        /// Cast as <see cref="double"/> array
        /// </summary>
        /// <param name="xyz"><see cref="XYZInt"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator double[](in XYZInt xyz) => [xyz.X, xyz.Y, xyz.Z];

        /// <summary>
        /// Cas as <see cref="XYInt"/>
        /// </summary>
        /// <param name="xyz"><see cref="XYZInt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYInt(in XYZInt xyz) => xyz.AsXyInt;

        /// <summary>
        /// Parse a string
        /// </summary>
        /// <param name="str">String</param>
        /// <returns><see cref="XYZInt"/></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static XYZInt Parse(in ReadOnlySpan<char> str)
        {
            int index1 = str.IndexOf(';');
            if (index1 < 1) throw new FormatException("No value separator found in input string");
            int index2 = str[(index1 + 1)..].IndexOf(';');
            if (index2 < 1) throw new FormatException("No second value separator found in input string");
            return new(int.Parse(str[..index1]), int.Parse(str.Slice(index1 + 1, index2)), int.Parse(str[(index1 + index2 + 2)..]));
        }

        /// <summary>
        /// Try parsing a string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="result"><see cref="XYZInt"/></param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryParse(in ReadOnlySpan<char> str, out XYZInt result)
        {
            result = default;
            int index1 = str.IndexOf(';');
            if (index1 < 1) return false;
            int index2 = str[(index1 + 1)..].IndexOf(';');
            if (index2 < 1) return false;
            if (!int.TryParse(str[..index1], out int x)) return false;
            if (!int.TryParse(str.Slice(index1 + 1, index2), out int y)) return false;
            if (!int.TryParse(str[(index1 + index2 + 2)..], out int z)) return false;
            result = new(x, y, z);
            return true;
        }
    }
}
