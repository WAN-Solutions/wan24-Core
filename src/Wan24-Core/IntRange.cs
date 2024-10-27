using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// 32 bit integer numeric range (increasing)
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly record struct IntRange : IEnumerable<int>, IComparable, ISerializeBinary<IntRange>, ISerializeString<IntRange>
    {
        /// <summary>
        /// Structure size in bytes
        /// </summary>
        public const int STRUCTURE_SIZE = sizeof(int) << 1;
        /// <summary>
        /// From byte offset
        /// </summary>
        public const int FROM_OFFSET = 0;
        /// <summary>
        /// To byte offset
        /// </summary>
        public const int TO_OFFSET = FROM_OFFSET + sizeof(int);

        /// <summary>
        /// Zero range
        /// </summary>
        public static readonly IntRange Zero = new();
        /// <summary>
        /// Negative
        /// </summary>
        public static readonly IntRange Negative = new(int.MinValue, -1);
        /// <summary>
        /// Positive
        /// </summary>
        public static readonly IntRange Positive = new(1, int.MaxValue);
        /// <summary>
        /// Max. value
        /// </summary>
        public static readonly IntRange MaxValue = new(int.MinValue, int.MaxValue);

        /// <summary>
        /// From (including)
        /// </summary>
        [FieldOffset(FROM_OFFSET)]
        public readonly int From;
        /// <summary>
        /// To (including)
        /// </summary>
        [FieldOffset(TO_OFFSET)]
        public readonly int To;

        /// <summary>
        /// Constructor
        /// </summary>
        public IntRange()
        {
            From = 0;
            To = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="from">From (including)</param>
        /// <param name="to">To (including)</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public IntRange(in int from, in int to)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(to, from);
            From = from;
            To = to;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Serialized data</param>
        public IntRange(in ReadOnlySpan<byte> data)
        {
            if (data.Length < STRUCTURE_SIZE)
                throw new ArgumentOutOfRangeException(nameof(data));
            int from = data.ToInt(),
                to = data[TO_OFFSET..].ToInt();
            if (to < from)
                throw new InvalidDataException($"From {from} is greater than to {to}");
            From = from;
            To = to;
        }

        /// <inheritdoc/>
        public static int? MaxStructureSize => STRUCTURE_SIZE;

        /// <inheritdoc/>
        public static int? MaxStringSize => byte.MaxValue;

        /// <inheritdoc/>
        public int? StructureSize => STRUCTURE_SIZE;

        /// <inheritdoc/>
        public int? StringSize => null;

        /// <summary>
        /// Get the value from an index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        public int this[in long index]
        {
            get
            {
                if (index < 0 || index >= Count) throw new IndexOutOfRangeException(nameof(index));
                return (int)(From + index);
            }
        }

        /// <summary>
        /// Number of values in the range
        /// </summary>
        public long Count
        {
            get
            {
                if (From < 0 && To < 0) return 1L + (~From) - (~To);
                if (From < 0) return 2L + (~From) + To;
                return 1L + To - From;
            }
        }

        /// <summary>
        /// All numbers of the range
        /// </summary>
        public IEnumerable<int> EnumerableRange
        {
            get
            {
                for (int i = From; i <= To; i++) yield return i;
            }
        }

        /// <summary>
        /// Get as <see cref="Range"/>
        /// </summary>
        public Range AsRange => From >= 0 && To < int.MaxValue
            ? new(new(From), new(To + 1))
            : throw new InvalidOperationException("Invalid range values");

        /// <summary>
        /// Determine if this range fits another range
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>If this range fits the other range (the other range fully fits into this range)</returns>
        public bool DoesFit(in IntRange other) => other.From >= From && other.To <= To;

        /// <summary>
        /// Determine if this range intersects another range
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>If this range intersects the other range</returns>
        public bool DoesIntersect(in IntRange other) => (From >= other.From && From <= other.To) || (To <= other.To && To >= other.From);

        /// <summary>
        /// Get as array
        /// </summary>
        /// <param name="start">Start index</param>
        /// <param name="count">Number of values to include</param>
        /// <param name="step">Stepping</param>
        /// <returns>Array</returns>
        public int[] ToArray(in long start = 0, in long? count = null, in int step = 1) => [.. AsEnumerable(start, count, step)];

        /// <summary>
        /// Write to an array
        /// </summary>
        /// <param name="arr">Array</param>
        /// <param name="start">Start index</param>
        /// <param name="step">Stepping</param>
        /// <returns>Number of elements written to the array</returns>
        public int ToArray(in Span<int> arr, in long start = 0, in int step = 1)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(start);
            long total = Count;
            ArgumentOutOfRangeException.ThrowIfGreaterThan(start, total - 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(step, 1);
            int res = 0,
                len = arr.Length;
            for (long index = start; res < len && index < total; arr[res] = (int)(From + index), res++, index += step) ;
            return res;
        }

        /// <summary>
        /// Get as enumerable
        /// </summary>
        /// <param name="start">Start index</param>
        /// <param name="count">Number of values to include</param>
        /// <param name="step">Stepping</param>
        /// <returns>Array</returns>
        public IEnumerable<int> AsEnumerable(long start = 0, long? count = null, int step = 1)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(start);
            long total = Count,
                maxLen = total - 1;
            ArgumentOutOfRangeException.ThrowIfGreaterThan(start, maxLen);
            ArgumentOutOfRangeException.ThrowIfLessThan(step, 1);
            long len;
            if (count.HasValue)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(count.Value);
                len = start + count.Value * step;
                ArgumentOutOfRangeException.ThrowIfGreaterThan(len, maxLen);
            }
            else
            {
                len = total;
            }
            for (long index = start; index < len; index += step)
                yield return (int)(From + index);
        }

        /// <inheritdoc/>
        public byte[] GetBytes()
        {
            byte[] res = new byte[STRUCTURE_SIZE];
            GetBytes(res);
            return res;
        }

        /// <inheritdoc/>
        public int GetBytes(in Span<byte> data)
        {
            if (data.Length < STRUCTURE_SIZE)
                throw new ArgumentOutOfRangeException(nameof(data));
            From.GetBytes(data);
            To.GetBytes(data[TO_OFFSET..]);
            return STRUCTURE_SIZE;
        }

        /// <inheritdoc/>
        public IEnumerator<int> GetEnumerator() => EnumerableRange.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => EnumerableRange.GetEnumerator();

        /// <inheritdoc/>
        public int CompareTo(object? obj) => obj is IntRange range ? Count.CompareTo(range.Count) : -1;

        /// <inheritdoc/>
        public override string ToString() => $"{From};{To}";

        /// <summary>
        /// Get as serialized data
        /// </summary>
        /// <param name="range"><see cref="IntRange"/></param>
        public static implicit operator byte[](in IntRange range) => range.GetBytes();

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator IntRange(in byte[] data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator IntRange(in Span<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator IntRange(in Memory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator IntRange(in ReadOnlySpan<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator IntRange(in ReadOnlyMemory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast as <see cref="string"/>
        /// </summary>
        /// <param name="range"><see cref="IntRange"/></param>
        public static implicit operator string(in IntRange range) => range.ToString();

        /// <summary>
        /// Cast from a <see cref="string"/>
        /// </summary>
        /// <param name="str"><see cref="string"/></param>
        public static implicit operator IntRange(in string str) => Parse(str);

        /// <summary>
        /// Count is lower than
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If lower</returns>
        public static bool operator <(in IntRange a, in IntRange b) => a.Count < b.Count;

        /// <summary>
        /// Count is greater than
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If greater</returns>
        public static bool operator >(in IntRange a, in IntRange b) => a.Count > b.Count;

        /// <summary>
        /// Count is lower than or equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If lower or equal</returns>
        public static bool operator <=(in IntRange a, in IntRange b) => a.Count <= b.Count;

        /// <summary>
        /// Count is greater than or equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If greater or equal</returns>
        public static bool operator >=(in IntRange a, in IntRange b) => a.Count >= b.Count;

        /// <summary>
        /// Shift right
        /// </summary>
        /// <param name="range"><see cref="IntRange"/></param>
        /// <param name="count">Count</param>
        /// <returns><see cref="IntRange"/></returns>
        public static IntRange operator >>(in IntRange range, in int count) => new(range.From + count, range.To + count);

        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="range"><see cref="IntRange"/></param>
        /// <param name="count">Count</param>
        /// <returns><see cref="IntRange"/></returns>
        public static IntRange operator <<(in IntRange range, in int count) => new(range.From - count, range.To - count);

        /// <summary>
        /// Determine if values of an array are a range
        /// </summary>
        /// <param name="arr">Array</param>
        /// <returns>If the values in the given array are a range</returns>
        public static bool IsRange(in Span<int> arr)
        {
            int len = arr.Length;
            if (len < 2)
                return true;
            if ((long)arr[0] + len > int.MaxValue)
                return false;
            for (int i = 1, value = arr[0] + 1; i < len; i++, value++)
                if (arr[i] != value)
                    return false;
            return true;
        }

        /// <inheritdoc/>
        public static object DeserializeFrom(in ReadOnlySpan<byte> buffer) => new IntRange(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out object? result)
        {
            try
            {
                if (buffer.Length < STRUCTURE_SIZE)
                {
                    result = null;
                    return false;
                }
                int from = buffer.ToInt(),
                    to = buffer[TO_OFFSET..].ToInt();
                if (to < from)
                {
                    result = null;
                    return false;
                }
                result = new IntRange(from, to);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public static IntRange DeserializeTypeFrom(in ReadOnlySpan<byte> buffer) => new(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeTypeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out IntRange result)
        {
            try
            {
                if (buffer.Length < STRUCTURE_SIZE)
                {
                    result = default;
                    return false;
                }
                int from = buffer.ToInt(),
                    to = buffer[TO_OFFSET..].ToInt();
                if (to < from)
                {
                    result = default;
                    return false;
                }
                result = new IntRange(from, to);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <inheritdoc/>
        public static IntRange Parse(in ReadOnlySpan<char> str)
        {
            int index = str.IndexOf(';');
            if (index < 0) throw new FormatException("Invalid string format");
            return new(int.Parse(str[..index]), int.Parse(str[(index + 1)..]));
        }

        /// <inheritdoc/>
        public static bool TryParse(in ReadOnlySpan<char> str, out IntRange result)
        {
            int index = str.IndexOf(';');
            if (index < 0 || !int.TryParse(str[..index], out int from) || !int.TryParse(str[(index + 1)..], out int to) || from > to)
            {
                result = Zero;
                return false;
            }
            result = new(from, to);
            return true;
        }

        /// <inheritdoc/>
        public static object ParseObject(in ReadOnlySpan<char> str) => Parse(str);

        /// <inheritdoc/>
        public static bool TryParseObject(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out object? result)
        {
            bool res;
            result = (res = TryParse(str, out IntRange range))
                ? range
                : default(IntRange?);
            return res;
        }
    }
}
