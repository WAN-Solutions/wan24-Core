using System.Collections;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// 32 bit integer numeric range (from low to high)
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial record struct IntRange : IEnumerable<int>, IComparable
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

        /// <summary>
        /// Get the value from an index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        public int this[in long index] => index >= 0 && index < Count ? (int)(From + index) : throw new ArgumentOutOfRangeException(nameof(index));

        /// <summary>
        /// Number of values in the range
        /// </summary>
        public long Count => To - From + 1;

        /// <summary>
        /// All numbers of the range
        /// </summary>
        public IEnumerable<int> Range
        {
            get
            {
                for (int i = From; i <= To; i++) yield return i;
            }
        }

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
        public int[] ToArray(in int start = 0, in long? count = null, in int step = 1)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(start);
            ArgumentOutOfRangeException.ThrowIfLessThan(step, 1);
            if (count.HasValue)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(count.Value);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(start + (count.Value * step), Count);
            }
            ArgumentOutOfRangeException.ThrowIfGreaterThan(start + (count ?? 0) * step, Count - 1);
            int[] res = new int[count ?? Count];
            for (int i = 0, len = res.Length; i < len; res[i] = From + start + i * step, i++) ;
            return res;
        }

        /// <summary>
        /// Write to an array
        /// </summary>
        /// <param name="arr">Array</param>
        /// <param name="start">Start index</param>
        /// <param name="step">Stepping</param>
        /// <returns>Number of elements written to the array</returns>
        public int ToArray(in Span<int> arr, in int start = 0, in int step = 1)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(start);
            ArgumentOutOfRangeException.ThrowIfLessThan(step, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(start, Count - 1);
            int res = 0;
            for (int i = 0, value; i < arr.Length; arr[i] = value, res++, i++)
            {
                value = From + start + i * step;
                if (value > To) break;
            }
            return res;
        }

        /// <summary>
        /// Get as enumerable
        /// </summary>
        /// <param name="start">Start index</param>
        /// <param name="count">Number of values to include</param>
        /// <param name="step">Stepping</param>
        /// <returns>Array</returns>
        public IEnumerable<int> AsEnumerable(int start = 0, long? count = null, int step = 1)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(start);
            ArgumentOutOfRangeException.ThrowIfLessThan(step, 1);
            if (count.HasValue)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(count.Value);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(start + (count.Value * step), Count);
            }
            ArgumentOutOfRangeException.ThrowIfGreaterThan(start + (count ?? 0) * step, Count - 1);
            long len = count ?? Count;
            for (int i = start; i < len; i++) yield return From + i * step;
        }

        /// <summary>
        /// Get as serialized data
        /// </summary>
        /// <returns>Serialized data</returns>
        public byte[] GetBytes()
        {
            byte[] res = new byte[STRUCTURE_SIZE];
            GetBytes(res);
            return res;
        }

        /// <summary>
        /// Get as serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public void GetBytes(in Span<byte> data)
        {
            if (data.Length < STRUCTURE_SIZE)
                throw new ArgumentOutOfRangeException(nameof(data));
            From.GetBytes(data);
            To.GetBytes(data[TO_OFFSET..]);
        }

        /// <inheritdoc/>
        public IEnumerator<int> GetEnumerator() => Range.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => Range.GetEnumerator();

        /// <inheritdoc/>
        public int CompareTo(object? obj) => obj is IntRange range ? Count.CompareTo(range.Count) : -1;

        /// <inheritdoc/>
        public override string ToString() => $"{From}-{To}";

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
        /// Parse from a string
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Range</returns>
        public static IntRange Parse(in string str)
        {
            string[] temp = str.Split('-', 2);
            if (temp.Length != 2) throw new FormatException("Invalid string format");
            return new(int.Parse(temp[0]), int.Parse(temp[1]));
        }

        /// <summary>
        /// Try parsing from a string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryParse(in string str, out IntRange result)
        {
            string[] temp = str.Split('-', 2);
            if (temp.Length != 2 || !int.TryParse(temp[0], out int from) || !int.TryParse(temp[1], out int to) || from > to)
            {
                result = Zero;
                return false;
            }
            result = new(from, to);
            return true;
        }
    }
}
