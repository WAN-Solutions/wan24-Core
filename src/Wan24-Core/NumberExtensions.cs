using System.Buffers.Binary;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Number extensions
    /// </summary>
    public static class NumberExtensions
    {
        /// <summary>
        /// Determine if a numeric value is unsigned (works for enumerations, too)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is unsigned?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsUnsigned<T>(this T? value) where T : struct, IConvertible, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
            => (value?.GetType() ?? typeof(T)).IsUnsigned();

        /// <summary>
        /// Determine if a numeric value is unsigned (works for enumerations, too)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is unsigned?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsUnsigned<T>(this T value) where T : struct, IConvertible, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
            => value.GetType().IsUnsigned();

        /// <summary>
        /// Determine if a numeric value is unsigned (works for enumerations, too)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <returns>Is unsigned?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsUnsigned<T>() where T : struct, IConvertible, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
            => typeof(T).IsUnsigned();

        /// <summary>
        /// Determine if a numeric type is unsigned (works for enumerations, too)
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is unsigned?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsUnsigned(this Type type)
            => Activator.CreateInstance(type.IsEnum ? type.GetEnumUnderlyingType() : type) switch
            {
                sbyte => false,
                byte => true,
                short => false,
                ushort => true,
                int => false,
                uint => true,
                long => false,
                ulong => true,
                Half => false,
                float => false,
                double => false,
                decimal => false,
                _ => throw new ArgumentException($"Not a supported numeric type {type}")
            };

        /// <summary>
        /// Determine if a type is numeric
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is a numeric type?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNumeric(this Type type)
        {
            try
            {
                return type.IsValueType && typeof(IConvertible).IsAssignableFrom(type) && Activator.CreateInstance(type) switch
                {
                    sbyte => true,
                    byte => true,
                    short => true,
                    ushort => true,
                    int => true,
                    uint => true,
                    long => true,
                    ulong => true,
                    Half => true,
                    float => true,
                    double => true,
                    decimal => true,
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determine if a type is numeric and unsigned
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is an unsigned numeric type?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNumericAndUnsigned(this Type type)
        {
            try
            {
                object? value;
                return type.IsValueType && typeof(IConvertible).IsAssignableFrom(type) && (value = Activator.CreateInstance(type)) switch
                {
                    sbyte => true,
                    byte => true,
                    short => true,
                    ushort => true,
                    int => true,
                    uint => true,
                    long => true,
                    ulong => true,
                    Half => true,
                    float => true,
                    double => true,
                    decimal => true,
                    _ => false
                } && value switch
                {
                    sbyte => false,
                    byte => true,
                    short => false,
                    ushort => true,
                    int => false,
                    uint => true,
                    long => false,
                    ulong => true,
                    Half => false,
                    float => false,
                    double => false,
                    decimal => false,
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this short value)
        {
            byte[] res = new byte[sizeof(short)];
            BinaryPrimitives.WriteInt16LittleEndian(res, value);
            return res;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this short value, in byte[] target)
        {
            GetBytes(value, target.AsSpan());
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<byte> GetBytes(this short value, in Span<byte> target)
        {
            BinaryPrimitives.WriteInt16LittleEndian(target, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target memory</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<byte> GetBytes(this short value, in Memory<byte> target)
        {
            BinaryPrimitives.WriteInt16LittleEndian(target.Span, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this ushort value)
        {
            byte[] res = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16LittleEndian(res, value);
            return res;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this ushort value, in byte[] target)
        {
            GetBytes(value, target.AsSpan());
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<byte> GetBytes(this ushort value, in Span<byte> target)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(target, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target memory</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<byte> GetBytes(this ushort value, in Memory<byte> target)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(target.Span, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this int value)
        {
            byte[] res = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(res, value);
            return res;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this int value, in byte[] target)
        {
            GetBytes(value, target.AsSpan());
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<byte> GetBytes(this int value, in Span<byte> target)
        {
            BinaryPrimitives.WriteInt32LittleEndian(target, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target memory</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<byte> GetBytes(this int value, in Memory<byte> target)
        {
            BinaryPrimitives.WriteInt32LittleEndian(target.Span, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this uint value)
        {
            byte[] res = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32LittleEndian(res, value);
            return res;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this uint value, in byte[] target)
        {
            GetBytes(value, target.AsSpan());
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<byte> GetBytes(this uint value, in Span<byte> target)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(target, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target memory</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<byte> GetBytes(this uint value, in Memory<byte> target)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(target.Span, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this long value)
        {
            byte[] res = new byte[sizeof(long)];
            BinaryPrimitives.WriteInt64LittleEndian(res, value);
            return res;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this long value, in byte[] target)
        {
            GetBytes(value, target.AsSpan());
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<byte> GetBytes(this long value, in Span<byte> target)
        {
            BinaryPrimitives.WriteInt64LittleEndian(target, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target memory</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<byte> GetBytes(this long value, in Memory<byte> target)
        {
            BinaryPrimitives.WriteInt64LittleEndian(target.Span, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this ulong value)
        {
            byte[] res = new byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64LittleEndian(res, value);
            return res;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this ulong value, in byte[] target)
        {
            GetBytes(value, target.AsSpan());
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<byte> GetBytes(this ulong value, in Span<byte> target)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(target, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target memory</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<byte> GetBytes(this ulong value, in Memory<byte> target)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(target.Span, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this Half value)
        {
            byte[] res = new byte[2];
            BinaryPrimitives.WriteHalfLittleEndian(res, value);
            return res;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this Half value, in byte[] target)
        {
            GetBytes(value, target.AsSpan());
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<byte> GetBytes(this Half value, in Span<byte> target)
        {
            BinaryPrimitives.WriteHalfLittleEndian(target, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target memory</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<byte> GetBytes(this Half value, in Memory<byte> target)
        {
            BinaryPrimitives.WriteHalfLittleEndian(target.Span, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this float value)
        {
            byte[] res = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleLittleEndian(res, value);
            return res;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this float value, in byte[] target)
        {
            GetBytes(value, target.AsSpan());
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<byte> GetBytes(this float value, in Span<byte> target)
        {
            BinaryPrimitives.WriteSingleLittleEndian(target, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target memory</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<byte> GetBytes(this float value, in Memory<byte> target)
        {
            BinaryPrimitives.WriteSingleLittleEndian(target.Span, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this double value)
        {
            byte[] res = new byte[sizeof(double)];
            BinaryPrimitives.WriteDoubleLittleEndian(res, value);
            return res;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this double value, in byte[] target)
        {
            GetBytes(value, target.AsSpan());
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<byte> GetBytes(this double value, in Span<byte> target)
        {
            BinaryPrimitives.WriteDoubleLittleEndian(target, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target memory</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<byte> GetBytes(this double value, in Memory<byte> target)
        {
            BinaryPrimitives.WriteDoubleLittleEndian(target.Span, value);
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this decimal value)
        {
            byte[] res = new byte[sizeof(int) << 2];
            int[] bits = decimal.GetBits(value);
            for (int i = 0; i < 4; Array.Copy(bits[i].GetBytes(), 0, res, i << 2, sizeof(int)), i++) ;
            return res;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this decimal value, in byte[] target)
        {
            GetBytes(value, target.AsSpan());
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<byte> GetBytes(this decimal value, in Span<byte> target)
        {
            if (target.Length < sizeof(int) << 2) throw new ArgumentOutOfRangeException(nameof(target));
            int[] bits = decimal.GetBits(value);
            for (int i = 0; i < 4; bits[i].GetBytes().CopyTo(target.Slice(i << 2, sizeof(int))), i++) ;
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target memory</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<byte> GetBytes(this decimal value, in Memory<byte> target)
        {
            if (target.Length < sizeof(int) << 2) throw new ArgumentOutOfRangeException(nameof(target));
            int[] bits = decimal.GetBits(value);
            for (int i = 0; i < 4; bits[i].GetBytes().CopyTo(target.Slice(i << 2, sizeof(int))), i++) ;
            return target;
        }

        /// <summary>
        /// Determine if a value is within a range
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="lowerBorder">Range begin (including)</param>
        /// <param name="higherBorder">Range end (including)</param>
        /// <returns>Is within the range?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsBetween<T>(this T value, in T lowerBorder, in T higherBorder) where T : struct, IConvertible, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
            => value.CompareTo(lowerBorder) >= 0 && value.CompareTo(higherBorder) <= 0;
    }
}
