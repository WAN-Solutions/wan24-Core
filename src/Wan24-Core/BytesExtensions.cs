using System.Buffers.Binary;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace wan24.Core
{
    /// <summary>
    /// Bytes extensions
    /// </summary>
    public static partial class BytesExtensions
    {
        /// <summary>
        /// Clear bytes handler
        /// </summary>
        public static Clear_Delegate? ClearHandler { get; set; }

        /// <summary>
        /// Convert the endian to be little endian for serializing, and big endian, if the system uses it
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Converted bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] ConvertEndian(this ReadOnlySpan<byte> bytes)
        {
            byte[] res = bytes.ToArray();
            if (!BitConverter.IsLittleEndian) Array.Reverse(res);
            return res;
        }

        /// <summary>
        /// Convert the endian to be little endian for serializing, and big endian, if the system uses it
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Converted bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] ConvertEndian(this ReadOnlyMemory<byte> bytes) => bytes.Span.ConvertEndian();

        /// <summary>
        /// Convert the endian to be little endian for serializing, and big endian, if the system uses it
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Converted bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<byte> ConvertEndian(this Span<byte> bytes)
        {
            if (!BitConverter.IsLittleEndian) bytes.Reverse();
            return bytes;
        }

        /// <summary>
        /// Convert the endian to be little endian for serializing, and big endian, if the system uses it
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Converted bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<byte> ConvertEndian(this Memory<byte> bytes)
        {
            bytes.Span.ConvertEndian();
            return bytes;
        }

        /// <summary>
        /// Convert the endian to be little endian for serializing, and big endian, if the system uses it
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Converted bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] ConvertEndian(this byte[] bytes)
        {
            bytes.AsSpan().ConvertEndian();
            return bytes;
        }

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SlowCompare(this ReadOnlySpan<byte> a, in ReadOnlySpan<byte> b)
        {
            int diff = a.Length ^ b.Length;
            for (int i = 0; i < a.Length && i < b.Length; diff |= a[i] ^ b[i], i++) ;
            return diff == 0;
        }

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SlowCompare(this Span<byte> a, in ReadOnlySpan<byte> b) => SlowCompare((ReadOnlySpan<byte>)a, b);

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SlowCompare(this byte[] a, in ReadOnlySpan<byte> b) => SlowCompare((ReadOnlySpan<byte>)a, b);

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SlowCompare(this ReadOnlyMemory<byte> a, in ReadOnlySpan<byte> b) => SlowCompare(a.Span, b);

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SlowCompare(this Memory<byte> a, in ReadOnlySpan<byte> b) => SlowCompare((ReadOnlySpan<byte>)a.Span, b);

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SlowCompare(this ReadOnlyMemory<byte> a, in ReadOnlyMemory<byte> b) => SlowCompare(a.Span, b.Span);

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SlowCompare(this Memory<byte> a, in Memory<byte> b) => SlowCompare((ReadOnlySpan<byte>)a.Span, b.Span);

        /// <summary>
        /// Get an Int16
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static short ToShort(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadInt16LittleEndian(bits);

        /// <summary>
        /// Get an Int16
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static short ToShort(this Span<byte> bits) => ToShort((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get an UInt16
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ushort ToUShort(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadUInt16LittleEndian(bits);

        /// <summary>
        /// Get an UInt16
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ushort ToUShort(this Span<byte> bits) => ToUShort((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get an Int32
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int ToInt(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadInt32LittleEndian(bits);

        /// <summary>
        /// Get an Int32
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int ToInt(this Span<byte> bits) => ToInt((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get an UInt32
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static uint ToUInt(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadUInt32LittleEndian(bits);

        /// <summary>
        /// Get an UInt32
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static uint ToUInt(this Span<byte> bits) => ToUInt((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get an Int64
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static long ToLong(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadInt64LittleEndian(bits);

        /// <summary>
        /// Get an Int64
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static long ToLong(this Span<byte> bits) => ToLong((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get an UInt64
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ulong ToULong(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadUInt64LittleEndian(bits);

        /// <summary>
        /// Get an UInt64
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ulong ToULong(this Span<byte> bits) => ToULong((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get a float
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Half ToHalf(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadHalfLittleEndian(bits);

        /// <summary>
        /// Get a float
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Half ToHalf(this Span<byte> bits) => ToHalf((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get a float
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static float ToFloat(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadSingleLittleEndian(bits);

        /// <summary>
        /// Get a float
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static float ToFloat(this Span<byte> bits) => ToFloat((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Get a double
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static double ToDouble(this ReadOnlySpan<byte> bits) => BinaryPrimitives.ReadDoubleLittleEndian(bits);

        /// <summary>
        /// Get a double
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static double ToDouble(this Span<byte> bits) => ToDouble((ReadOnlySpan<byte>)bits);

#if NO_UNSAFE
        /// <summary>
        /// Get a decimal
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static decimal ToDecimal(this ReadOnlySpan<byte> bits)
        {
            if (bits.Length < sizeof(int) << 2) throw new ArgumentOutOfRangeException(nameof(bits));
            RentedMemoryRef<int> intBits = new(len: 4, clean: false);
            Span<int> intBitsSpan = intBits.Span;
            for (int i = 0; i < 4; intBitsSpan[i] = bits.Slice(i << 2, sizeof(int)).ToInt(), i++) ;
            return new decimal(intBitsSpan);
        }
#else
        /// <summary>
        /// Get a decimal
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        [SkipLocalsInit]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static decimal ToDecimal(this ReadOnlySpan<byte> bits)
        {
            if (bits.Length < sizeof(int) << 2) throw new ArgumentOutOfRangeException(nameof(bits));
            Span<int> intBits = stackalloc int[4];
            for (int i = 0; i < 4; intBits[i] = bits.Slice(i << 2, sizeof(int)).ToInt(), i++) ;
            return new decimal(intBits);
        }
#endif

        /// <summary>
        /// Get a decimal
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static decimal ToDecimal(this Span<byte> bits) => ToDecimal((ReadOnlySpan<byte>)bits);

        /// <summary>
        /// Clear the array
        /// </summary>
        /// <param name="bytes">Bytes</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Clear(this byte[] bytes) => Clean(bytes.AsSpan());

        /// <summary>
        /// Clear the array
        /// </summary>
        /// <param name="bytes">Bytes</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Clean(this Span<byte> bytes)
        {
            if (bytes.Length == 0) return;
            if (ClearHandler is null)
            {
                RandomNumberGenerator.Fill(bytes);
                bytes.Clear();
            }
            else
            {
                ClearHandler(bytes);
            }
        }

        /// <summary>
        /// Delegate for a byte clearing handler
        /// </summary>
        /// <param name="bytes">Bytes to clear (must be zeroed)</param>
        public delegate void Clear_Delegate(Span<byte> bytes);
    }
}
