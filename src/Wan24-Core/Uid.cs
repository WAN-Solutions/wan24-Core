using System.Diagnostics.CodeAnalysis;
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
        public const int STRUCTURE_SIZE = sizeof(long) << 1;
        /// <summary>
        /// Time byte offset
        /// </summary>
        public const int TIME_OFFSET = 0;
        /// <summary>
        /// Random byte offset
        /// </summary>
        public const int RANDOM_OFFSET = sizeof(long);

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
        public Uid()
        {
            Time = DateTime.UtcNow;
            using RentedArrayRefStruct<byte> buffer = new(len: sizeof(long));
            RandomNumberGenerator.Fill(buffer.Span);
            Random = buffer.Span.ToLong();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="time">Time</param>
        public Uid(in DateTime time)
        {
            Time = time.Kind == DateTimeKind.Utc ? time : time.ToUniversalTime();
            using RentedArrayRefStruct<byte> buffer = new(len: sizeof(long));
            RandomNumberGenerator.Fill(buffer.Span);
            Random = buffer.Span.ToLong();
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
        public Span<byte> GetBytes(in Span<byte> buffer)
        {
            if (buffer.Length < STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(buffer));
            Time.ToUniversalTime().Ticks.GetBytes(buffer);
            Random.GetBytes(buffer[RANDOM_OFFSET..]);
            return buffer;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            using RentedArrayRefStruct<byte> buffer = new(len: STRUCTURE_SIZE);
            GetBytes(buffer.Span);
            using RentedArrayRefStruct<char> strBuffer = new(len: ByteEncoding.GetEncodedLength(buffer.Span));
            buffer.Span.Encode(res: strBuffer.Array);
            return new(strBuffer.Span);
        }

        /// <summary>
        /// Cast as serialized data
        /// </summary>
        /// <param name="uid">UID</param>
        public static implicit operator byte[](in Uid uid) => uid.GetBytes();

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        public static implicit operator Uid(in byte[] buffer) => new(buffer);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        public static implicit operator Uid(in Span<byte> buffer) => new(buffer);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        public static implicit operator Uid(in ReadOnlySpan<byte> buffer) => new(buffer);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        public static implicit operator Uid(in Memory<byte> buffer) => new(buffer.Span);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        public static implicit operator Uid(in ReadOnlyMemory<byte> buffer) => new(buffer.Span);

        /// <summary>
        /// Cast as string
        /// </summary>
        /// <param name="uid">UID</param>
        public static implicit operator string(in Uid uid) => uid.ToString();

        /// <summary>
        /// Cast from a string
        /// </summary>
        /// <param name="str">String</param>
        public static implicit operator Uid(in string str) => Parse(str);

        /// <summary>
        /// Parse from a string
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>UID</returns>
        public static Uid Parse(in ReadOnlySpan<char> str)
        {
            if (ByteEncoding.GetDecodedLength(str) != STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(str));
            using RentedArrayRefStruct<byte> buffer = new(len: STRUCTURE_SIZE);
            str.Decode(res: buffer.Array);
            return new(buffer.Span);
        }

        /// <summary>
        /// Try parsing from a string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="result">UID</param>
        /// <returns>If succeed</returns>
        public static bool TryParse(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out Uid? result)
        {
            result = null;
            if (ByteEncoding.GetDecodedLength(str) != STRUCTURE_SIZE) return false;
            try
            {
                using RentedArrayRefStruct<byte> buffer = new(len: STRUCTURE_SIZE);
                str.Decode(res: buffer.Array);
                result = new(buffer.Span);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
