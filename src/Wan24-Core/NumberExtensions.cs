using System.Buffers.Binary;
using System.Runtime;

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
        public static bool IsUnsigned<T>(this T? value) where T : struct, IConvertible => (value?.GetType() ?? typeof(T)).IsUnsigned();

        /// <summary>
        /// Determine if a numeric value is unsigned (works for enumerations, too)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is unsigned?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsUnsigned<T>(this T value) where T : struct, IConvertible => value.GetType().IsUnsigned();

        /// <summary>
        /// Determine if a numeric value is unsigned (works for enumerations, too)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <returns>Is unsigned?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsUnsigned<T>() where T : struct, IConvertible => typeof(T).IsUnsigned();

        /// <summary>
        /// Determine if a numeric type is unsigned (works for enumerations, too)
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is unsigned?</returns>
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
        public static Span<byte> GetBytes(this short value, Span<byte> target)
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
        public static Memory<byte> GetBytes(this short value, Memory<byte> target)
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
        public static Span<byte> GetBytes(this ushort value, Span<byte> target)
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
        public static Memory<byte> GetBytes(this ushort value, Memory<byte> target)
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
        public static Span<byte> GetBytes(this int value, Span<byte> target)
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
        public static Memory<byte> GetBytes(this int value, Memory<byte> target)
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
        public static Span<byte> GetBytes(this uint value, Span<byte> target)
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
        public static Memory<byte> GetBytes(this uint value, Memory<byte> target)
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
        public static Span<byte> GetBytes(this long value, Span<byte> target)
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
        public static Memory<byte> GetBytes(this long value, Memory<byte> target)
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
        public static Span<byte> GetBytes(this ulong value, Span<byte> target)
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
        public static Memory<byte> GetBytes(this ulong value, Memory<byte> target)
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
        public static Span<byte> GetBytes(this float value, Span<byte> target)
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
        public static Memory<byte> GetBytes(this float value, Memory<byte> target)
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
        public static Span<byte> GetBytes(this double value, Span<byte> target)
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
        public static Memory<byte> GetBytes(this double value, Memory<byte> target)
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
        public static byte[] GetBytes(this decimal value)
        {
            byte[] res = new byte[sizeof(int) << 2];
            int[] bits = decimal.GetBits(value);
            for (int i = 0; i < bits.Length; Array.Copy(bits[i].GetBytes(), 0, res, i << 2, sizeof(int)), i++) ;
            return res;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target span</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static Span<byte> GetBytes(this decimal value, Span<byte> target)
        {
            if (target.Length < sizeof(int) << 2) throw new ArgumentOutOfRangeException(nameof(target));
            int[] bits = decimal.GetBits(value);
            for (int i = 0; i < bits.Length; bits[i].GetBytes().CopyTo(target.Slice(i << 2, sizeof(int))), i++) ;
            return target;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="target">Target memory</param>
        /// <returns>Bytes (endian converted)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static Memory<byte> GetBytes(this decimal value, Memory<byte> target)
        {
            if (target.Length < sizeof(int) << 2) throw new ArgumentOutOfRangeException(nameof(target));
            int[] bits = decimal.GetBits(value);
            for (int i = 0; i < bits.Length; bits[i].GetBytes().CopyTo(target.Slice(i << 2, sizeof(int))), i++) ;
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
        public static bool IsBetween<T>(this T value, T lowerBorder, T higherBorder) where T : IComparable => value.CompareTo(lowerBorder) >= 0 && value.CompareTo(higherBorder) <= 0;
    }
}
