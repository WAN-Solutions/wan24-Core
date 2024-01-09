using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace wan24.Core
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly record struct Uid
    {
        /// <summary>
        /// Structure size in byte
        /// </summary>
        public const int STRUCTURE_SIZE = sizeof(long) /* Time */ + sizeof(long) /* Random */;
        /// <summary>
        /// Time byte offset
        /// </summary>
        public const int TIME_OFFSET = 0;
        /// <summary>
        /// Time length in byte
        /// </summary>
        public const int TIME_LEN = sizeof(long);
        /// <summary>
        /// Random byte offset
        /// </summary>
        public const int RANDOM_OFFSET = TIME_LEN;
        /// <summary>
        /// Random length in byte
        /// </summary>
        public const int RANDOM_LEN = sizeof(long);

        /// <summary>
        /// String length in characters
        /// </summary>
        public static readonly int STRING_LEN = ByteEncoding.GetEncodedLength(STRUCTURE_SIZE);

        /// <summary>
        /// Time (UTC)
        /// </summary>
        [FieldOffset(TIME_OFFSET)]
        public readonly DateTime Time;
        /// <summary>
        /// Random
        /// </summary>
        [FieldOffset(RANDOM_OFFSET)]
        public readonly long Random;

        /// <summary>
        /// Constructor
        /// </summary>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public Uid()
        {
            Time = DateTime.UtcNow;
#if !NO_UNSAFE
            if (RANDOM_LEN <= Settings.StackAllocBorder)
            {
                Span<byte> buffer = stackalloc byte[RANDOM_LEN];
                RandomNumberGenerator.Fill(buffer);
                Random = buffer.ToLong();
            }
            else
            {
#endif
                using RentedArrayRefStruct<byte> buffer = new(len: RANDOM_LEN);
                RandomNumberGenerator.Fill(buffer.Span);
                Random = buffer.Span.ToLong();
#if !NO_UNSAFE
            }
#endif
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="time">Time</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public Uid(in DateTime time)
        {
            Time = time.Kind == DateTimeKind.Utc ? time : time.ToUniversalTime();
#if !NO_UNSAFE
            if (RANDOM_LEN <= Settings.StackAllocBorder)
            {
                Span<byte> buffer = stackalloc byte[RANDOM_LEN];
                RandomNumberGenerator.Fill(buffer);
                Random = buffer.ToLong();
            }
            else
            {
#endif
                using RentedArrayRefStruct<byte> buffer = new(len: RANDOM_LEN);
                RandomNumberGenerator.Fill(buffer.Span);
                Random = buffer.Span.ToLong();
#if !NO_UNSAFE
            }
#endif
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="random">Random</param>
        public Uid(in DateTime time, in long random)
        {
            Time = time.Kind == DateTimeKind.Utc ? time : time.ToUniversalTime();
            Random = random;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        public Uid(in ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(buffer));
            Time = new(buffer.ToLong(), DateTimeKind.Utc);
            Random = buffer[RANDOM_OFFSET..].ToLong();
        }

        /// <summary>
        /// Get bytes
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
        /// Get bytes
        /// </summary>
        /// <param name="buffer">Serialized data buffer (length needs to fit <see cref="STRUCTURE_SIZE"/>)</param>
        /// <returns>Serialized data buffer</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public Span<byte> GetBytes(in Span<byte> buffer)
        {
            if (buffer.Length < STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(buffer));
            Time.ToUniversalTime().Ticks.GetBytes(buffer);
            Random.GetBytes(buffer[RANDOM_OFFSET..]);
            return buffer;
        }

        /// <inheritdoc/>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override string ToString()
        {
#if !NO_UNSAFE
            if (STRUCTURE_SIZE + STRING_LEN <= Settings.StackAllocBorder)
            {
                Span<byte> buffer = stackalloc byte[STRUCTURE_SIZE];
                GetBytes(buffer);
                Span<char> strBuffer = stackalloc char[STRING_LEN];
                buffer.AsReadOnly().Encode(res: strBuffer);
                return new(strBuffer);
            }
            else
            {
#endif
                using RentedArrayRefStruct<byte> buffer = new(len: STRUCTURE_SIZE, clean: false);
                GetBytes(buffer.Span);
                using RentedArrayRefStruct<char> strBuffer = new(len: STRING_LEN, clean: false);
                buffer.Span.Encode(res: strBuffer.Array);
                return new(strBuffer.Span);
#if !NO_UNSAFE
            }
#endif
        }

        /// <summary>
        /// Cast as serialized data
        /// </summary>
        /// <param name="uid">UID</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator byte[](in Uid uid) => uid.GetBytes();

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Uid(in byte[] buffer) => new(buffer);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Uid(in Span<byte> buffer) => new(buffer);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Uid(in ReadOnlySpan<byte> buffer) => new(buffer);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Uid(in Memory<byte> buffer) => new(buffer.Span);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Uid(in ReadOnlyMemory<byte> buffer) => new(buffer.Span);

        /// <summary>
        /// Cast as string
        /// </summary>
        /// <param name="uid">UID</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator string(in Uid uid) => uid.ToString();

        /// <summary>
        /// Cast from a string
        /// </summary>
        /// <param name="str">String</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Uid(in string str) => Parse(str);

        /// <summary>
        /// Cast as <see cref="UidExt"/>
        /// </summary>
        /// <param name="uid"><see cref="UidExt"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static implicit operator UidExt(in Uid uid)
        {
#if !NO_UNSAFE
            if (STRUCTURE_SIZE <= Settings.StackAllocBorder)
            {
                Span<byte> buffer = stackalloc byte[STRUCTURE_SIZE];
                uid.GetBytes(buffer);
                return new(buffer);
            }
            else
            {
#endif
                using RentedArrayRefStruct<byte> buffer = new(len: STRUCTURE_SIZE);
                uid.GetBytes(buffer.Span);
                return new(buffer.Span);
#if !NO_UNSAFE
            }
#endif
        }

        /// <summary>
        /// Cast from <see cref="UidExt"/>
        /// </summary>
        /// <param name="uid"><see cref="UidExt"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static implicit operator Uid(in UidExt uid)
        {
#if !NO_UNSAFE
            if (STRUCTURE_SIZE <= Settings.StackAllocBorder)
            {
                Span<byte> buffer = stackalloc byte[STRUCTURE_SIZE];
                uid.GetBytes(buffer);
                return new(buffer);
            }
            else
            {
#endif
                using RentedArrayRefStruct<byte> buffer = new(len: STRUCTURE_SIZE);
                uid.GetBytes(buffer.Span);
                return new(buffer.Span);
#if !NO_UNSAFE
            }
#endif
        }

        /// <summary>
        /// Parse from a string
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>UID</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static Uid Parse(in ReadOnlySpan<char> str)
        {
            if (str.Length != STRING_LEN) throw new ArgumentOutOfRangeException(nameof(str));
#if !NO_UNSAFE
            if (STRUCTURE_SIZE <= Settings.StackAllocBorder)
            {
                Span<byte> buffer = stackalloc byte[STRUCTURE_SIZE];
                str.Decode(res: buffer);
                return new(buffer);
            }
            else
            {
#endif
                using RentedArrayRefStruct<byte> buffer = new(len: STRUCTURE_SIZE);
                str.Decode(res: buffer.Array);
                return new(buffer.Span);
#if !NO_UNSAFE
            }
#endif
        }

        /// <summary>
        /// Try parsing from a string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="result">UID</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static bool TryParse(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out Uid? result)
        {
            try
            {
                if (str.Length != STRING_LEN)
                {
                    result = null;
                    return false;
                }
#if !NO_UNSAFE
                if (STRUCTURE_SIZE <= Settings.StackAllocBorder)
                {
                    Span<byte> buffer = stackalloc byte[STRUCTURE_SIZE];
                    str.Decode(res: buffer);
                    result = new(buffer);
                }
                else
                {
#endif
                    using RentedArrayRefStruct<byte> buffer = new(len: STRUCTURE_SIZE);
                    str.Decode(res: buffer.Array);
                    result = new(buffer.Span);
#if !NO_UNSAFE
                }
#endif
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
