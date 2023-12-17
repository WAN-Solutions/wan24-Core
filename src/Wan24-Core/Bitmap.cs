using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Bitmap (little endian)
    /// </summary>
    public partial class Bitmap
    {
        /// <summary>
        /// Bitmap
        /// </summary>
        protected byte[] _Map;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialSize">Initial bitmap size in bytes</param>
        /// <param name="increaseSize">Number of bytes to enlarge the bitmap when adding new bits which overflow the current bitmap size</param>
        /// <param name="bitCount">Initial bit count</param>
        public Bitmap(in long initialSize = ushort.MaxValue, in int increaseSize = ushort.MaxValue, in int bitCount = 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(initialSize);
            ArgumentOutOfRangeException.ThrowIfLessThan(increaseSize, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(initialSize, GetByteCount(bitCount));
            _Map = initialSize == 0 ? [] : new byte[initialSize];
            IncreaseSize = increaseSize;
            BitCount = bitCount;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bitmap">Initial bitmap</param>
        /// <param name="increaseSize">Number of bytes to enlarge the bitmap when adding new bits which overflow the current bitmap size</param>
        /// <param name="bitCount">Initial bit count (if <see langword="null"/>, the initial bit count will be the number of bits in the given initial bitmap)</param>
        public Bitmap(in byte[] bitmap, in int increaseSize = ushort.MaxValue, in int? bitCount = null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(increaseSize, 1);
            if (bitCount is not null && GetByteCount(bitCount.Value) > bitmap.LongLength) throw new ArgumentOutOfRangeException(nameof(bitCount));
            _Map = bitmap;
            IncreaseSize = increaseSize;
            BitCount = bitCount ?? (bitmap.LongLength << 3);
        }

        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        public object SyncObject { get; } = new();

        /// <summary>
        /// Get/set a bit
        /// </summary>
        /// <param name="offset">Bit offset</param>
        /// <returns>Bit</returns>
        public bool this[long offset]
        {
            get
            {
                lock (SyncObject)
                {
                    if (offset < 0 || offset >= BitCount) throw new ArgumentOutOfRangeException(nameof(offset));
                    long byteOffset = offset >> 3;
                    return ((_Map[byteOffset] >> (int)(offset - (byteOffset << 3))) & 1) == 1;
                }
            }
            set
            {
                lock (SyncObject)
                {
                    if (offset < 0 || offset >= BitCount) throw new ArgumentOutOfRangeException(nameof(offset));
                    long byteOffset = offset >> 3;
                    int bit = 1 << (int)(offset - (byteOffset << 3));
                    if (value)
                    {
                        _Map[byteOffset] |= (byte)bit;
                    }
                    else
                    {
                        _Map[byteOffset] &= (byte)~bit;
                    }
                }
            }
        }

        /// <summary>
        /// Get/set bits (not thread-safe)
        /// </summary>
        /// <param name="start">Start (inclusive)</param>
        /// <param name="end">End (exclusive)</param>
        /// <returns>Bits</returns>
        /// <exception cref="ArgumentOutOfRangeException">The range is invalid or the bitmap size changed during processing (lock <see cref="SyncObject"/> for 
        /// thread-safety)</exception>
        public bool[] this[in int start, in int end]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => this[new Range(start, end)];
            [TargetedPatchingOptOut("Just a method adapter")]
            set => this[new Range(start, end)] = value;
        }

        /// <summary>
        /// Get/set bits (not thread-safe)
        /// </summary>
        /// <param name="start">Start (inclusive)</param>
        /// <param name="end">End (exclusive)</param>
        /// <returns>Bits</returns>
        /// <exception cref="ArgumentOutOfRangeException">The range is invalid or the bitmap size changed during processing (lock <see cref="SyncObject"/> for 
        /// thread-safety)</exception>
        public bool[] this[in Index start, in Index end]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => this[new Range(start, end)];
            [TargetedPatchingOptOut("Just a method adapter")]
            set => this[new Range(start, end)] = value;
        }

        /// <summary>
        /// Get/set bits (not thread-safe)
        /// </summary>
        /// <param name="range">Range</param>
        /// <returns>Bits</returns>
        /// <exception cref="InvalidOperationException">The bitmap is too huge (><see cref="int.MaxValue"/> bits) for indexed access</exception>
        /// <exception cref="ArgumentOutOfRangeException">The range is invalid or the bitmap size changed during processing (lock <see cref="SyncObject"/> for 
        /// thread-safety)</exception>
        public bool[] this[in Range range]
        {
            get
            {
                long bitCount = BitCount;
                if (bitCount > int.MaxValue) throw new InvalidOperationException("The bitmap is too huge for indexed access");
                (int offset, int len) = range.GetOffsetAndLength((int)bitCount);
                return GetBits(offset, len);
            }
            set
            {
                if (value.Length == 0) return;
                long bitCount = BitCount;
                if (bitCount > int.MaxValue) throw new InvalidOperationException("The bitmap is too huge for indexed access");
                SetBits(range.GetOffsetAndLength((int)bitCount).Offset, value);
            }
        }

        /// <summary>
        /// Bitmap size in bytes
        /// </summary>
        public long Size => _Map.LongLength;

        /// <summary>
        /// Number of bytes to enlarge the bitmap when adding new bits which overflow the current bitmap size
        /// </summary>
        public int IncreaseSize { get; }

        /// <summary>
        /// Number of used bits
        /// </summary>
        public long BitCount { get; protected set; }

        /// <summary>
        /// Current bitmap (the complete bit buffer, which may be larger than the current bit count)
        /// </summary>
        public ReadOnlyMemory<byte> Map => _Map.AsMemory();

        /// <summary>
        /// Current bitmap as memory
        /// </summary>
        public ReadOnlyMemory<byte> AsMemory => _Map.AsMemory(0, (int)GetByteCount(BitCount));

        /// <inheritdoc/>
        ICollection<long> IDictionary<long, bool>.Keys
        {
            get
            {
                long bitCount = BitCount;
                if (bitCount > int.MaxValue) throw new InvalidOperationException("The bitmap is too huge for getting a key collection");
                return bitCount == 0 ? [] : Enumerable.Range(0, (int)bitCount).Select(n => (long)n).ToList();
            }
        }
    }
}
