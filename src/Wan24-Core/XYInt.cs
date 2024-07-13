﻿using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// XY value
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly record struct XYInt
    {
        /// <summary>
        /// Structure size in bytes
        /// </summary>
        public const int STRUCTURE_SIZE = Y_FIELD_OFFSET + Y_FIELD_SIZE;
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
        /// Zero X/Y
        /// </summary>
        public static readonly XYInt Zero = new();
        /// <summary>
        /// Minimum value
        /// </summary>
        public static readonly XYInt MinValue = new(int.MinValue, int.MinValue);
        /// <summary>
        /// Maximum value
        /// </summary>
        public static readonly XYInt MaxValue = new(int.MaxValue, int.MaxValue);

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
        /// Constructor
        /// </summary>
        public XYInt()
        {
            X = 0;
            Y = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        public XYInt(in int x, in int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Serialized data (min. <see cref="STRUCTURE_SIZE"/> bytes length required)</param>
        public XYInt(in ReadOnlySpan<byte> data)
        {
            if (data.Length < STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(data));
            X = data.ToInt();
            Y = data[Y_FIELD_OFFSET..].ToInt();
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
        public static implicit operator byte[](in XYInt xy) => xy.GetBytes();

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYInt(in byte[] data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYInt(in Span<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYInt(in ReadOnlySpan<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYInt(in Memory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYInt(in ReadOnlyMemory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast as <see cref="string"/>
        /// </summary>
        /// <param name="xy"><see cref="XYInt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator string(in XYInt xy) => xy.ToString();

        /// <summary>
        /// Cast as <see cref="string"/>
        /// </summary>
        /// <param name="xy"><see cref="XYInt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ReadOnlySpan<char>(in XYInt xy) => xy.ToString();

        /// <summary>
        /// Cast from a <see cref="string"/>
        /// </summary>
        /// <param name="str">String</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYInt(in ReadOnlySpan<char> str) => Parse(str);

        /// <summary>
        /// Cast from a <see cref="string"/>
        /// </summary>
        /// <param name="str">String</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator XYInt(in string str) => Parse(str);

        /// <summary>
        /// Cast as <see cref="double"/> array
        /// </summary>
        /// <param name="xy"><see cref="XYInt"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator double[](in XYInt xy) => [xy.X, xy.Y];

        /// <summary>
        /// Parse a string
        /// </summary>
        /// <param name="str">String</param>
        /// <returns><see cref="XYInt"/></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static XYInt Parse(in ReadOnlySpan<char> str)
        {
            int index = str.IndexOf(';');
            if (index < 1) throw new FormatException("No value separator found in input string");
            return new(int.Parse(str[..index]), int.Parse(str[(index + 1)..]));
        }

        /// <summary>
        /// Try parsing a string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="result"><see cref="XYInt"/></param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryParse(in ReadOnlySpan<char> str, out XYInt result)
        {
            result = default;
            int index = str.IndexOf(';');
            if (index < 1) return false;
            if (!int.TryParse(str[..index], out int x)) return false;
            if (!int.TryParse(str[(index + 1)..], out int y)) return false;
            result = new(x, y);
            return true;
        }
    }
}