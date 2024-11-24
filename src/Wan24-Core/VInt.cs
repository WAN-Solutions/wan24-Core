using System.Numerics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Variable length signable integer
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct VInt
    {
        /// <summary>
        /// Value
        /// </summary>
        [FieldOffset(0)]
        public readonly ReadOnlyMemory<byte> Value;

        /// <summary>
        /// Constructor
        /// </summary>
        public VInt() => Value = Zero.Value;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value (<see cref="VInt"/> format required - use <see cref="FromBits(in ReadOnlySpan{byte}, bool)"/> to convert)</param>
        public VInt(in ReadOnlyMemory<byte> value)
        {
            if (value.Length < 1) throw new ArgumentOutOfRangeException(nameof(value));
            Value = value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value (will be copied; <see cref="VInt"/> format required - use <see cref="FromBits(in ReadOnlySpan{byte}, bool)"/> to convert)</param>
        public VInt(in ReadOnlySpan<byte> value)
        {
            if (value.Length < 1) throw new ArgumentOutOfRangeException(nameof(value));
            Value = value.ToArray();
        }

        /// <summary>
        /// Number of bits used (minimum 6 bit)
        /// </summary>
        public long NumberOfBits
        {
            get
            {
                long bits = 6;
                ReadOnlySpan<byte> value = Value.Span;
                for (int i = 1, len = value.Length, last = len - 1; i < len; i++)
                    if (i != last || (value[i] & (1 << 6)) == 1) bits += 7;
                    else if ((value[i] & (1 << 5)) == 1) bits += 6;
                    else if ((value[i] & (1 << 4)) == 1) bits += 5;
                    else if ((value[i] & (1 << 3)) == 1) bits += 4;
                    else if ((value[i] & (1 << 2)) == 1) bits += 3;
                    else if ((value[i] & (1 << 1)) == 1) bits += 2;
                    else bits++;
                return bits;
            }
        }

        /// <summary>
        /// If the value is unsigned (positive; <see langword="false"/> MAY be negative)
        /// </summary>
        public bool IsUnsigned
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (Value.Span[0] & SIGN_FLAG) == 0;
        }

        /// <summary>
        /// Numeric CLR type which fits the value
        /// </summary>
        public Type NumericClrType
        {
            get
            {
                int bits = 6;
                ReadOnlySpan<byte> value = Value.Span;
                for (int i = 1, len = value.Length, last = len - 1; i < len && bits < 65; i++)
                    if (i != last || (value[i] & (1 << 6)) == 1) bits += 7;
                    else if ((value[i] & (1 << 5)) == 1) bits += 6;
                    else if ((value[i] & (1 << 4)) == 1) bits += 5;
                    else if ((value[i] & (1 << 3)) == 1) bits += 4;
                    else if ((value[i] & (1 << 2)) == 1) bits += 3;
                    else if ((value[i] & (1 << 1)) == 1) bits += 2;
                    else bits++;
                bool isUnsigned = (value[0] & SIGN_FLAG) == 0;
                if (bits < 9) return isUnsigned ? typeof(byte) : typeof(sbyte);
                if (bits < 17) return isUnsigned ? typeof(ushort) : typeof(short);
                if (bits < 33) return isUnsigned ? typeof(uint) : typeof(int);
                if (bits < 65) return isUnsigned ? typeof(ulong) : typeof(long);
                return typeof(BigInteger);
            }
        }

        /// <summary>
        /// Get the number of bytes required for a buffer that can hold the CLR numeric types bytes
        /// </summary>
        public int NumericClrTypeLength
        {
            get
            {
                Type type = NumericClrType;
                if (type == typeof(byte) || type == typeof(sbyte)) return sizeof(byte);
                if (type == typeof(ushort) || type == typeof(short)) return sizeof(ushort);
                if (type == typeof(int) || type == typeof(sbyte)) return sizeof(uint);
                if (type == typeof(long) || type == typeof(sbyte)) return sizeof(ulong);
                return Value.Length - (int)Math.Floor((1 + Value.Length) / 8f);
            }
        }

        /// <summary>
        /// Get as .NET CLR numeric type
        /// </summary>
        public object AsNumber
        {
            get
            {
                ReadOnlySpan<byte> value = Value.Span;
                int len = value.Length;
                bool isUnsigned = (value[0] & SIGN_FLAG) == 0;
                byte b = (byte)(value[0] & ~(SIGN_FLAG | MORE_BITS_FLAG));
                int offset = 0,
                    v;
                using RentedMemoryRef<byte> buffer = new(len - (int)Math.Floor((1 + len) / 8f), clean: false);
                Span<byte> bufferSpan = buffer.Span;
                for (int i = 1, bit = 6; i < len; i++)
                {
                    v = value[i] & ~MORE_BITS_FLAG;
                    b |= (byte)(v << bit);
                    if (bit < 1)
                    {
                        bit = 7;
                        continue;
                    }
                    bufferSpan[offset] = b;
                    offset++;
                    if (bit > 1)
                    {
                        b = (byte)(v >> (8 - bit));
                        bit = 7 - bit;
                    }
                    else
                    {
                        b = 0;
                        bit = 0;
                    }
                }
                bufferSpan[offset] = b;
                if (offset < 1)
                {
                    offset = 1;
                }
                else if (offset < 3)
                {
                    offset = 3;
                }
                else if (offset < 7)
                {
                    offset = 7;
                }
                if (!isUnsigned) bufferSpan[offset] |= 128;
                return offset switch
                {
                    1 => isUnsigned ? bufferSpan[..sizeof(ushort)].ToUShort() : bufferSpan[..sizeof(short)].ToShort(),
                    3 => isUnsigned ? bufferSpan[..sizeof(uint)].ToUInt() : bufferSpan[..sizeof(int)].ToInt(),
                    7 => isUnsigned ? bufferSpan[..sizeof(ulong)].ToULong() : bufferSpan[..sizeof(long)].ToLong(),
                    _ => new BigInteger(bufferSpan[..(offset + 1)], isUnsigned)
                };
            }
        }

        /// <summary>
        /// Get as <see cref="BigInteger"/>
        /// </summary>
        public BigInteger AsBigInteger
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => AsNumber.CastType<BigInteger>();
        }

        /// <summary>
        /// Get as <see cref="ulong"/>
        /// </summary>
        public ulong AsULong
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => AsNumber.CastType<ulong>();
        }

        /// <summary>
        /// Get as <see cref="long"/>
        /// </summary>
        public long AsLong
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => AsNumber.CastType<long>();
        }

        /// <summary>
        /// Get as <see cref="uint"/>
        /// </summary>
        public uint AsUInt
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => AsNumber.CastType<uint>();
        }

        /// <summary>
        /// Get as <see cref="int"/>
        /// </summary>
        public int AsInt
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => AsNumber.CastType<int>();
        }

        /// <summary>
        /// Get as <see cref="ushort"/>
        /// </summary>
        public ushort AsUShort
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => AsNumber.CastType<ushort>();
        }

        /// <summary>
        /// Get as <see cref="short"/>
        /// </summary>
        public short AsShort
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => AsNumber.CastType<short>();
        }

        /// <summary>
        /// Get as <see cref="byte"/>
        /// </summary>
        public byte AsByte
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => AsNumber.CastType<byte>();
        }

        /// <summary>
        /// Get as <see cref="sbyte"/>
        /// </summary>
        public sbyte AsSByte
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => AsNumber.CastType<sbyte>();
        }
    }
}
