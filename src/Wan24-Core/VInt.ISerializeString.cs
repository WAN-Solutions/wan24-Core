using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime;

namespace wan24.Core
{
    // ISerializeString
    public readonly partial struct VInt : ISerializeString<VInt>
    {
        /// <inheritdoc/>
        public static int? MaxStringSize => null;

        /// <inheritdoc/>
        public static bool IsFixedStringSize => false;

        /// <inheritdoc/>
        int? ISerializeString.StringSize => null;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override string ToString() => AsNumber.ToString()!;

        /// <inheritdoc/>
        public static VInt Parse(in ReadOnlySpan<char> str)
        {
            BigInteger bigInt = BigInteger.Parse(str);
            bool isUnsigned = bigInt.Sign >= 0 || bigInt == BigInteger.Zero;
            using RentedMemoryRef<byte> buffer = new(bigInt.GetByteCount(isUnsigned), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            if (!bigInt.TryWriteBytes(bufferSpan, out int bytes, isUnsigned)) throw new InvalidProgramException("Failed to get the big integer bytes");
            return FromBits(bufferSpan[..bytes], isUnsigned);
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object ParseObject(in ReadOnlySpan<char> str) => Parse(str);

        /// <inheritdoc/>
        public static bool TryParse(in ReadOnlySpan<char> str, [NotNullWhen(true)] out VInt result)
        {
            if (!BigInteger.TryParse(str, provider: null, out BigInteger bigInt))
            {
                result = default;
                return false;
            }
            bool isUnsigned = bigInt.Sign >= 0 || bigInt == BigInteger.Zero;
            using RentedMemoryRef<byte> buffer = new(bigInt.GetByteCount(isUnsigned), clean: false);
            Span<byte> bufferSpan = buffer.Span;
            if (!bigInt.TryWriteBytes(bufferSpan, out int bytes, isUnsigned))
            {
                result = default;
                return false;
            }
            result = FromBits(bufferSpan[..bytes], isUnsigned);
            return true;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryParseObject(in ReadOnlySpan<char> str, [NotNullWhen(true)] out object? result)
        {
            bool res = TryParse(str, out VInt obj);
            result = obj;
            return res;
        }
    }
}
