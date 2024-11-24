using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime;

namespace wan24.Core
{
    // Casting
    public readonly partial struct VInt
    {
        /// <summary>
        /// Cast as <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value (<see cref="VInt"/> format required - use <see cref="FromBits(in ReadOnlySpan{byte}, bool)"/> to convert)</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator VInt(in ReadOnlyMemory<byte> value) => new(value);

        /// <summary>
        /// Cast as <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value (<see cref="VInt"/> format required - use <see cref="FromBits(in ReadOnlySpan{byte}, bool)"/> to convert)</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator VInt(in Memory<byte> value) => new(value);

        /// <summary>
        /// Cast as <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value (will be copied; <see cref="VInt"/> format required - use <see cref="FromBits(in ReadOnlySpan{byte}, bool)"/> to convert)</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator VInt(in ReadOnlySpan<byte> value) => new(value);

        /// <summary>
        /// Cast as <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value (will be copied; <see cref="VInt"/> format required - use <see cref="FromBits(in ReadOnlySpan{byte}, bool)"/> to convert)</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator VInt(in Span<byte> value) => new(value);

        /// <summary>
        /// Cast as <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value (<see cref="VInt"/> format required - use <see cref="FromBits(in ReadOnlySpan{byte}, bool)"/> to convert)</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator VInt(in byte[] value) => new(value.AsMemory());

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ReadOnlyMemory<byte>(in VInt value) => value.Value;

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ReadOnlySpan<byte>(in VInt value) => value.Value.Span;

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator byte[](in VInt value) => value.Value.ToArray();

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Memory<byte>(in VInt value) => value.Value.ToArray();

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Span<byte>(in VInt value) => value.Value.ToArray();

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator BigInteger(in VInt value) => value.AsBigInteger;

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ulong(in VInt value) => value.AsULong;

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator long(in VInt value) => value.AsLong;

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator uint(in VInt value) => value.AsUInt;

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator int(in VInt value) => value.AsInt;

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ushort(in VInt value) => value.AsUShort;

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator short(in VInt value) => value.AsShort;

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator byte(in VInt value) => value.AsByte;

        /// <summary>
        /// Cast from <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator sbyte(in VInt value) => value.AsSByte;

        /// <summary>
        /// Cast to <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator VInt(in BigInteger value)
        {
            bool isUnsigned = value.Sign >= 0 || value == BigInteger.Zero;
            using RentedMemoryRef<byte> buffer = new(value.GetByteCount(isUnsigned), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            if (!value.TryWriteBytes(bufferSpan, out int bytes, isUnsigned)) throw new InvalidProgramException("Failed to get big integer bytes");
            return FromBits(bufferSpan[..bytes], isUnsigned);
        }

        /// <summary>
        /// Cast to <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator VInt(in ulong value)
        {
            using RentedMemoryRef<byte> buffer = new(sizeof(ulong), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            value.GetBytes(bufferSpan);
            return FromBits(bufferSpan, isUnsigned: true);
        }

        /// <summary>
        /// Cast to <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator VInt(in long value)
        {
            using RentedMemoryRef<byte> buffer = new(sizeof(long), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            value.GetBytes(bufferSpan);
            return FromBits(bufferSpan, isUnsigned: value >= 0);
        }

        /// <summary>
        /// Cast to <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator VInt(in uint value)
        {
            using RentedMemoryRef<byte> buffer = new(sizeof(uint), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            value.GetBytes(bufferSpan);
            return FromBits(bufferSpan, isUnsigned: true);
        }

        /// <summary>
        /// Cast to <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator VInt(in int value)
        {
            using RentedMemoryRef<byte> buffer = new(sizeof(int), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            value.GetBytes(bufferSpan);
            return FromBits(bufferSpan, isUnsigned: value >= 0);
        }

        /// <summary>
        /// Cast to <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator VInt(in ushort value)
        {
            using RentedMemoryRef<byte> buffer = new(sizeof(ushort), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            value.GetBytes(bufferSpan);
            return FromBits(bufferSpan, isUnsigned: true);
        }

        /// <summary>
        /// Cast to <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator VInt(in short value)
        {
            using RentedMemoryRef<byte> buffer = new(sizeof(short), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            value.GetBytes(bufferSpan);
            return FromBits(bufferSpan, isUnsigned: value >= 0);
        }

        /// <summary>
        /// Cast to <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator VInt(in byte value) => FromBits([value], isUnsigned: true);

        /// <summary>
        /// Cast to <see cref="VInt"/>
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator VInt(in sbyte value) => FromBits([(byte)value], isUnsigned: value >= 0);
    }
}
