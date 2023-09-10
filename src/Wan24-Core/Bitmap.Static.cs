using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Static
    public partial class Bitmap
    {
        /// <summary>
        /// Cast as byte array
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        public static implicit operator byte[](in Bitmap bitmap) => bitmap._Map;

        /// <summary>
        /// Cast from byte array
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Bitmap(in byte[] bitmap) => new(bitmap);

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        public static implicit operator Span<byte>(in Bitmap bitmap) => bitmap._Map.AsSpan();

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        public static implicit operator ReadOnlySpan<byte>(in Bitmap bitmap) => bitmap._Map.AsSpan();

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        public static implicit operator Memory<byte>(in Bitmap bitmap) => bitmap._Map.AsMemory();

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        public static implicit operator ReadOnlyMemory<byte>(in Bitmap bitmap) => bitmap._Map.AsMemory();

        /// <summary>
        /// Cast as bit count
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator long(in Bitmap bitmap) => bitmap.BitCount;

        /// <summary>
        /// Cast as bit array
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator bool[](in Bitmap bitmap) => ((IEnumerable<bool>)bitmap).ToArray();

        /// <summary>
        /// Cast from bit array
        /// </summary>
        /// <param name="bits">Bits</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Bitmap(in bool[] bits)
        {
            Bitmap res = new(initialSize: GetByteCount(bits.LongLength));
            res.SetBits(offset: 0, bits);
            return res;
        }

        /// <summary>
        /// Cast from byte
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Bitmap(in byte value) => new(new byte[] { value });

        /// <summary>
        /// Cast from signed byte
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Bitmap(in sbyte value) => new(new byte[] { (byte)value });

        /// <summary>
        /// Cast from unsigned short
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Bitmap(in ushort value) => new(value.GetBytes());

        /// <summary>
        /// Cast from short
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Bitmap(in short value) => new(value.GetBytes());

        /// <summary>
        /// Cast from unsigned int
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Bitmap(in uint value) => new(value.GetBytes());

        /// <summary>
        /// Cast from int
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Bitmap(in int value) => new(value.GetBytes());

        /// <summary>
        /// Cast from unsigned long
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Bitmap(in ulong value) => new(value.GetBytes());

        /// <summary>
        /// Cast from long
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Bitmap(in long value) => new(value.GetBytes());

        /// <summary>
        /// Cast from enumeration
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static implicit operator Bitmap(in Enum value)
            => Convert.ChangeType(value, value.GetType().GetEnumUnderlyingType()) switch
            {
                byte b => (Bitmap)b,
                sbyte sb => (Bitmap)sb,
                ushort us => (Bitmap)us,
                short s => (Bitmap)s,
                uint ui => (Bitmap)ui,
                int i => (Bitmap)i,
                ulong ul => (Bitmap)ul,
                long l => (Bitmap)l,
                _ => throw new InvalidProgramException($"Unsupported enumeration {value.GetType()} underlying numeric type {value.GetType().GetEnumUnderlyingType()}")
            };

        /// <summary>
        /// Get the number of bytes required for covering a number of bits
        /// </summary>
        /// <param name="bitCount">Bit count</param>
        /// <returns>Byte count</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static long GetByteCount(in long bitCount) => (bitCount & 7) == 0 ? bitCount >> 3 : (bitCount >> 3) + 1;
    }
}
