using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Static
    public readonly partial struct VInt
    {
        /// <summary>
        /// Sign flag
        /// </summary>
        public const int SIGN_FLAG = 64;
        /// <summary>
        /// More bits flag
        /// </summary>
        public const int MORE_BITS_FLAG = 128;
        /// <summary>
        /// 6 bit mask
        /// </summary>
        public const int MASK_6BIT = 63;
        /// <summary>
        /// 7 bit mask
        /// </summary>
        public const int MASK_7BIT = 127;

        /// <summary>
        /// Zero
        /// </summary>
        public static readonly VInt Zero = new(new byte[] { 0 }.AsMemory());

        /// <summary>
        /// Determine if an integer can be stored as <see cref="VInt"/>
        /// </summary>
        /// <param name="bitsLength">Number of integer bytes</param>
        /// <returns>If the integer can be stored as <see cref="VInt"/></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool CanStore(in int bitsLength) => bitsLength + (long)Math.Ceiling((bitsLength + 1) / 8f) <= int.MaxValue;

        /// <summary>
        /// Get the number of bytes which are required to store the number of integer bytes as a <see cref="VInt"/>
        /// </summary>
        /// <param name="bitsLength">Number of integer bytes</param>
        /// <returns>Number of <see cref="VInt"/> buffer bytes</returns>
        /// <exception cref="OverflowException">The Int32 maximum was exceeded</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetRequiredBufferLength(in int bitsLength)
        {
            long res = bitsLength + (long)Math.Ceiling((bitsLength + 1) / 8f);
            if (res > int.MaxValue)
                throw new OverflowException($"The required number of {res} buffer bytes exceeds the maximum Int32 value of {int.MaxValue} by {res - int.MaxValue} bytes");
            return (int)res;
        }

        /// <summary>
        /// Create from bits (little endian)
        /// </summary>
        /// <param name="bits">Bits (little endian)</param>
        /// <param name="isUnsigned">If is unsigned</param>
        /// <returns><see cref="VInt"/></returns>
        public static VInt FromBits(in ReadOnlySpan<byte> bits, bool isUnsigned)
        {
            if (bits.Length < 1) throw new ArgumentOutOfRangeException(nameof(bits));
            if (!isUnsigned) isUnsigned = (bits[^1] & 128) == 0;
            byte b = bits[0];
            int sourceOffset = 0,
                targetOffset = 1,
                sourceBit = 6,
                targetBit = 0,
                len = bits.Length,
                last = len - 1,
                v = 0,
                copyBits;
            byte[] buffer = new byte[GetRequiredBufferLength(len)];
            buffer[0] = isUnsigned ? (byte)(b & MASK_6BIT) : (byte)(b & MASK_6BIT | SIGN_FLAG);
            while (true)
            {
                copyBits = Math.Min(7, 8 - sourceBit);
                v |= (b >> sourceBit) << targetBit;
                if (targetBit + copyBits >= 7)
                {
                    buffer[targetOffset] = sourceOffset == last ? (byte)(v & MASK_7BIT) : (byte)((v & MASK_7BIT) | MORE_BITS_FLAG);
                    targetOffset++;
                    targetBit = 0;
                    v = 0;
                }
                else
                {
                    targetBit += copyBits;
                }
                if (sourceBit + copyBits >= 8)
                {
                    if (++sourceOffset == len)
                    {
                        buffer[^1] = (byte)v;
                        break;
                    }
                    sourceBit = 0;
                    b = bits[sourceOffset];
                }
                else
                {
                    sourceBit += copyBits;
                }
            }
            return new(buffer.AsMemory());
        }
    }
}
