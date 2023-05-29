using System.Buffers;
using System.Buffers.Binary;
using System.Collections;

namespace wan24.Core
{
    /// <summary>
    /// Bitmap (little endian)
    /// </summary>
    public class Bitmap : IEnumerable<bool>, IDictionary<long, bool>, IReadOnlyDictionary<long, bool>
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
        public Bitmap(long initialSize = ushort.MaxValue, int increaseSize = ushort.MaxValue, int bitCount = 0)
        {
            if (initialSize < 0) throw new ArgumentOutOfRangeException(nameof(initialSize));
            if (increaseSize < 1) throw new ArgumentOutOfRangeException(nameof(increaseSize));
            if (GetByteCount(bitCount) > initialSize) throw new ArgumentOutOfRangeException(nameof(bitCount));
            _Map = initialSize == 0 ? Array.Empty<byte>() : new byte[initialSize];
            IncreaseSize = increaseSize;
            BitCount = bitCount;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bitmap">Initial bitmap</param>
        /// <param name="increaseSize">Number of bytes to enlarge the bitmap when adding new bits which overflow the current bitmap size</param>
        /// <param name="bitCount">Initial bit count (if <see langword="null"/>, the initial bit count will be the number of bits in the given initial bitmap)</param>
        public Bitmap(byte[] bitmap, int increaseSize = ushort.MaxValue, int? bitCount = null)
        {
            if (increaseSize < 1) throw new ArgumentOutOfRangeException(nameof(increaseSize));
            if (bitCount != null && GetByteCount(bitCount.Value) > bitmap.LongLength) throw new ArgumentOutOfRangeException(nameof(bitCount));
            _Map = bitmap;
            IncreaseSize = increaseSize;
            BitCount = bitCount ?? bitmap.LongLength << 3;
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
        public bool[] this[int start, int end]
        {
            get => this[new Range(start, end)];
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
        public bool[] this[Index start, Index end]
        {
            get => this[new Range(start, end)];
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
        public bool[] this[Range range]
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
                return bitCount == 0 ? new List<long>() : Enumerable.Range(0, (int)bitCount).Cast<long>().ToList();
            }
        }

        /// <inheritdoc/>
        ICollection<bool> IDictionary<long, bool>.Values => ((IEnumerable<bool>)this).ToList();

        /// <inheritdoc/>
        int ICollection<KeyValuePair<long, bool>>.Count => (int)BitCount;

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<long, bool>>.IsReadOnly => false;

        /// <inheritdoc/>
        IEnumerable<long> IReadOnlyDictionary<long, bool>.Keys
        {
            get
            {
                for (long offset = 0; offset < BitCount; offset++) yield return offset;
            }
        }

        /// <inheritdoc/>
        IEnumerable<bool> IReadOnlyDictionary<long, bool>.Values => this;

        /// <inheritdoc/>
        int IReadOnlyCollection<KeyValuePair<long, bool>>.Count
        {
            get
            {
                long bitCount = BitCount;
                if (bitCount > int.MaxValue) throw new InvalidOperationException("The bitmap is too huge for getting an Int32 bit count");
                return (int)bitCount;
            }
        }

        /// <summary>
        /// Set a new bitmap size
        /// </summary>
        /// <param name="count">Number of bytes</param>
        /// <exception cref="InternalBufferOverflowException">New bitmap is larger than <see cref="int.MaxValue"/></exception>
        public virtual void SetSize(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            lock (SyncObject)
            {
                long len = GetByteCount(count);
                if (len > int.MaxValue) throw new InternalBufferOverflowException();
                byte[] map = len == 0 ? Array.Empty<byte>() : new byte[len];
                if (len > 0) _Map.AsSpan().CopyTo(map.AsSpan(0, Math.Min(_Map.Length, map.Length)));
                _Map = map;
                if (_Map.Length < GetByteCount(BitCount)) BitCount = _Map.Length << 3;
            }
        }

        /// <summary>
        /// Exchange the bitmap
        /// </summary>
        /// <param name="map">New bitmap</param>
        /// <returns>Old bitmap</returns>
        public virtual byte[] ExchangeBitmap(byte[] map)
        {
            lock (SyncObject)
            {
                byte[] res = _Map;
                _Map = map;
                if (_Map.Length < GetByteCount(BitCount)) BitCount = _Map.Length << 3;
                return res;
            }
        }

        /// <summary>
        /// Set a new bit count (when shrinking and later expanding, old bits won't be cleared!)
        /// </summary>
        /// <param name="count">Number of bits (must not exceed the bitmap size)</param>
        public virtual void SetBitCount(long count)
        {
            lock (SyncObject)
            {
                if (count < 0 || GetByteCount(count) > _Map.LongLength) throw new ArgumentOutOfRangeException(nameof(count));
                BitCount = count;
            }
        }

        /// <summary>
        /// Add bits
        /// </summary>
        /// <param name="bits">Bits to add</param>
        public virtual void AddBits(params bool[] bits)
        {
            if (bits.Length == 0) return;
            lock (SyncObject)
            {
                long offset = BitCount;
                AddBits(bits.Length);
                SetBits(offset, bits);
            }
        }

        /// <summary>
        /// Add bits
        /// </summary>
        /// <param name="count">Number of bits to add</param>
        public virtual void AddBits(int count)
        {
            if (count < 1) return;
            lock (SyncObject)
            {
                while (BitCount + count > _Map.LongLength << 3) SetSize((int)(_Map.LongLength + IncreaseSize));
                BitCount += count;
            }
        }

        /// <summary>
        /// Set bits
        /// </summary>
        /// <param name="offset">Start bit offset</param>
        /// <param name="bits">Bits to set</param>
        public virtual void SetBits(long offset, params bool[] bits)
        {
            if (bits.Length == 0) return;
            lock (SyncObject)
            {
                if (offset < 0 || offset >= BitCount) throw new ArgumentOutOfRangeException(nameof(offset));
                if (BitCount + bits.Length > _Map.Length << 3) throw new OverflowException();
                long byteOffset = offset >> 3;
                for (int i = 0, bit; i < bits.Length; this[offset] = bits[i], offset++, byteOffset = offset >> 3, i++)
                {
                    bit = 1 << (int)(offset - (byteOffset << 3));
                    if (bits[i])
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
        /// Update a bit
        /// </summary>
        /// <param name="offset">Bit offset</param>
        /// <param name="value">Value</param>
        /// <returns>Was updated?</returns>
        public virtual bool UpdateBit(long offset, bool value)
        {
            lock (SyncObject)
            {
                if (offset < 0 || offset >= BitCount) throw new ArgumentOutOfRangeException(nameof(offset));
                long byteOffset = offset >> 3;
                if (((_Map[byteOffset] >> (int)(offset - (byteOffset << 3))) & 1) == 1 == value) return false;
                int bit = 1 << (int)(offset - (byteOffset << 3));
                if (value)
                {
                    _Map[byteOffset] |= (byte)bit;
                }
                else
                {
                    _Map[byteOffset] &= (byte)~bit;
                }
                return true;
            }
        }

        /// <summary>
        /// Get bits
        /// </summary>
        /// <param name="offset">Bit offset</param>
        /// <param name="count">Number of bits to get</param>
        /// <returns>Bits</returns>
        public virtual bool[] GetBits(long offset, long count)
        {
            if (count < 1) return Array.Empty<bool>();
            lock (SyncObject)
            {
                if (offset < 0 || offset >= BitCount) throw new ArgumentOutOfRangeException(nameof(offset));
                if (offset + count > BitCount) throw new ArgumentOutOfRangeException(nameof(count));
                bool[] res = new bool[count];
                long byteOffset = offset >> 3;
                for (int i = 0; i < count; res[i] = ((_Map[byteOffset] >> (int)(offset - (byteOffset << 3))) & 1) == 1, offset++, byteOffset = offset >> 3, i++) ;
                return res;
            }
        }

        /// <summary>
        /// Get the bitmap as byte array
        /// </summary>
        /// <param name="buffer">Target buffer</param>
        /// <returns>Bitmap</returns>
        /// <exception cref="InternalBufferOverflowException">Bitmap is larger than <see cref="int.MaxValue"/></exception>
        public byte[] ToArray(byte[]? buffer = null)
        {
            lock (SyncObject)
            {
                if (_Map.LongLength == 0) return _Map;
                if (_Map.LongLength > int.MaxValue) throw new InternalBufferOverflowException();
                long len = GetByteCount(BitCount);
                byte[] res = buffer ?? new byte[len];
                _Map.AsSpan(0, (int)Math.Min(len, res.LongLength)).CopyTo(res.AsSpan());
                return res;
            }
        }

        /// <summary>
        /// Get the bitmap as byte array
        /// </summary>
        /// <param name="buffer">Target buffer</param>
        /// <returns>Bitmap</returns>
        /// <exception cref="InternalBufferOverflowException">Bitmap is larger than <see cref="int.MaxValue"/></exception>
        public Span<byte> ToSpan(Span<byte> buffer)
        {
            lock (SyncObject)
            {
                if (_Map.LongLength == 0) return _Map;
                if (_Map.LongLength > int.MaxValue) throw new InternalBufferOverflowException();
                _Map.AsSpan(0, (int)Math.Min(GetByteCount(BitCount), buffer.Length)).CopyTo(buffer);
                return buffer;
            }
        }

        /// <summary>
        /// Get the bitmap as byte
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public byte ToByte(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(byte), (mem) => mem.Span[0]);

        /// <summary>
        /// Get the bitmap as signed byte
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public sbyte ToSByte(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(byte), (mem) => (sbyte)mem.Span[0]);

        /// <summary>
        /// Get the bitmap as unsigned short
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public ushort ToUShort(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(ushort), (mem) => BinaryPrimitives.ReadUInt16LittleEndian(mem.Span));

        /// <summary>
        /// Get the bitmap as short
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public short ToShort(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(short), (mem) => BinaryPrimitives.ReadInt16LittleEndian(mem.Span));

        /// <summary>
        /// Get the bitmap as unsigned int
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public uint ToUInt(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(uint), (mem) => BinaryPrimitives.ReadUInt32LittleEndian(mem.Span));

        /// <summary>
        /// Get the bitmap as int
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public int ToInt(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(int), (mem) => BinaryPrimitives.ReadInt32LittleEndian(mem.Span));

        /// <summary>
        /// Get the bitmap as unsigned long
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public ulong ToULong(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(ulong), (mem) => BinaryPrimitives.ReadUInt64LittleEndian(mem.Span));

        /// <summary>
        /// Get the bitmap as long
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Value</returns>
        public long ToLong(int offset = 0) => ExecuteWithEnsuredSpan(offset, sizeof(long), (mem) => BinaryPrimitives.ReadInt64LittleEndian(mem.Span));

        /// <inheritdoc/>
        public IEnumerator<bool> GetEnumerator()
        {
            bool value;
            for (long offset = 0, byteOffset = 0; offset < BitCount; offset++, byteOffset = offset >> 3)
            {
                lock (SyncObject)
                {
                    if (offset >= BitCount) break;
                    value = ((_Map[byteOffset] >> (int)(offset - (byteOffset << 3))) & 1) == 1;
                }
                yield return value;
            }
        }

        /// <summary>
        /// Ensure executing a function having a size matching span of the bitmap
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="offset">Byte offset</param>
        /// <param name="count">Byte count</param>
        /// <param name="function">Function</param>
        /// <returns>Return value</returns>
        protected T ExecuteWithEnsuredSpan<T>(int offset, int count, Func<ReadOnlyMemory<byte>, T> function)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
            lock (SyncObject)
            {
                long byteCount = GetByteCount(BitCount);
                if (offset < byteCount)
                {
                    if (offset + count < byteCount)
                    {
                        return function(_Map.AsMemory(offset, count));
                    }
                    else
                    {
                        using RentedArray<byte> buffer = new(count);
                        _Map.AsSpan(offset, (int)(byteCount - offset)).CopyTo(buffer.Span);
                        return function(buffer.Memory);
                    }
                }
                else
                {
                    using RentedArray<byte> buffer = new(count);
                    return function(buffer.Memory);
                }
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        void IDictionary<long, bool>.Add(long key, bool value)
        {
            lock (SyncObject)
            {
                if (key != BitCount) throw new ArgumentOutOfRangeException(nameof(key));
                AddBits(value);
            }
        }

        /// <inheritdoc/>
        bool IDictionary<long, bool>.ContainsKey(long key) => key >= 0 && key < BitCount;

        /// <inheritdoc/>
        bool IDictionary<long, bool>.Remove(long key) => throw new NotSupportedException();

        /// <inheritdoc/>
        bool IDictionary<long, bool>.TryGetValue(long key, out bool value)
        {
            lock (SyncObject)
            {
                if (key < 0 || key >= BitCount)
                {
                    value = default;
                    return false;
                }
                long byteOffset = key >> 3;
                value = ((_Map[byteOffset] >> (int)(key - (byteOffset << 3))) & 1) == 1;
                return true;
            }
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<long, bool>>.Add(KeyValuePair<long, bool> item)
        {
            lock (SyncObject)
            {
                if (item.Key != BitCount) throw new ArgumentOutOfRangeException(nameof(item));
                AddBits(item.Value);
            }
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<long, bool>>.Clear() => ExchangeBitmap(Array.Empty<byte>());

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<long, bool>>.Contains(KeyValuePair<long, bool> item) => this[item.Key] == item.Value;

        /// <inheritdoc/>
        void ICollection<KeyValuePair<long, bool>>.CopyTo(KeyValuePair<long, bool>[] array, int arrayIndex)
        {
            if (array.LongLength - arrayIndex < BitCount) throw new ArgumentOutOfRangeException(nameof(array));
            arrayIndex--;
            long offset = -1;
            using IEnumerator<bool> enumerator = GetEnumerator();
            while (enumerator.MoveNext()) array[++arrayIndex] = new(++offset, enumerator.Current);

        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<long, bool>>.Remove(KeyValuePair<long, bool> item) => throw new NotSupportedException();

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<long, bool>> IEnumerable<KeyValuePair<long, bool>>.GetEnumerator()
        {
            long offset = -1;
            using IEnumerator<bool> enumerator = GetEnumerator();
            while (enumerator.MoveNext()) yield return new(++offset, enumerator.Current);
        }

        /// <inheritdoc/>
        bool IReadOnlyDictionary<long, bool>.ContainsKey(long key) => ((IDictionary<long, bool>)this).ContainsKey(key);

        /// <inheritdoc/>
        bool IReadOnlyDictionary<long, bool>.TryGetValue(long key, out bool value) => ((IDictionary<long, bool>)this).TryGetValue(key, out value);

        /// <summary>
        /// Cast as byte array
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        public static implicit operator byte[](Bitmap bitmap) => bitmap._Map;

        /// <summary>
        /// Cast from byte array
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        public static implicit operator Bitmap(byte[] bitmap) => new(bitmap);

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        public static implicit operator Span<byte>(Bitmap bitmap) => bitmap._Map.AsSpan();

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        public static implicit operator Memory<byte>(Bitmap bitmap) => bitmap._Map.AsMemory();

        /// <summary>
        /// Cast as bit count
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        public static implicit operator long(Bitmap bitmap) => bitmap.BitCount;

        /// <summary>
        /// Cast as bit array
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        public static implicit operator bool[](Bitmap bitmap) => ((IEnumerable<bool>)bitmap).ToArray();

        /// <summary>
        /// Cast from bit array
        /// </summary>
        /// <param name="bits">Bits</param>
        public static implicit operator Bitmap(bool[] bits)
        {
            Bitmap res = new(initialSize: GetByteCount(bits.LongLength));
            res.SetBits(offset: 0, bits);
            return res;
        }

        /// <summary>
        /// Cast from byte
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator Bitmap(byte value) => new(new byte[] { value });

        /// <summary>
        /// Cast from signed byte
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator Bitmap(sbyte value) => new(new byte[] { (byte)value });

        /// <summary>
        /// Cast from unsigned short
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator Bitmap(ushort value) => new(value.GetBytes());

        /// <summary>
        /// Cast from short
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator Bitmap(short value) => new(value.GetBytes());

        /// <summary>
        /// Cast from unsigned int
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator Bitmap(uint value) => new(value.GetBytes());

        /// <summary>
        /// Cast from int
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator Bitmap(int value) => new(value.GetBytes());

        /// <summary>
        /// Cast from unsigned long
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator Bitmap(ulong value) => new(value.GetBytes());

        /// <summary>
        /// Cast from long
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator Bitmap(long value) => new(value.GetBytes());

        /// <summary>
        /// Cast from enumeration
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator Bitmap(Enum value)
        {
            Type type = value.GetType().GetEnumUnderlyingType();
            object numeric = Convert.ChangeType(value, type);
            if (numeric is byte b) return (Bitmap)b;
            else if (numeric is sbyte sb) return (Bitmap)sb;
            else if (numeric is ushort us) return (Bitmap)us;
            else if (numeric is short s) return (Bitmap)s;
            else if (numeric is uint ui) return (Bitmap)ui;
            else if (numeric is int i) return (Bitmap)i;
            else if (numeric is ulong ul) return (Bitmap)ul;
            else if (numeric is long l) return (Bitmap)l;
            throw new InvalidProgramException($"Unsupported enumeration {value.GetType()} underlying numeric type {type}");
        }

        /// <summary>
        /// Get the number of bytes required for covering a number of bits
        /// </summary>
        /// <param name="bitCount">Bit count</param>
        /// <returns>Byte count</returns>
        public static long GetByteCount(long bitCount) => (bitCount & 7) == 0 ? bitCount >> 3 : (bitCount >> 3) + 1;
    }
}
