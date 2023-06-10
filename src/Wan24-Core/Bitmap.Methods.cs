using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Methods
    public partial class Bitmap
    {
        /// <summary>
        /// Set a new bitmap size
        /// </summary>
        /// <param name="count">Number of bytes</param>
        /// <exception cref="InternalBufferOverflowException">New bitmap is larger than <see cref="int.MaxValue"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetSize(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            lock (SyncObject)
            {
                long len = GetByteCount(count);
                if (len > int.MaxValue) throw new InternalBufferOverflowException();
                byte[] map = len == 0 ? Array.Empty<byte>() : new byte[len];
                if (len > 0) _Map.AsSpan(0, Math.Min(_Map.Length, map.Length)).CopyTo(map.AsSpan());
                _Map = map;
                if (_Map.Length < GetByteCount(BitCount)) BitCount = _Map.Length << 3;
            }
        }

        /// <summary>
        /// Exchange the bitmap
        /// </summary>
        /// <param name="map">New bitmap</param>
        /// <returns>Old bitmap</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// <returns>First new bit offset</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual long AddBits(params bool[] bits)
        {
            if (bits.Length == 0) return BitCount;
            lock (SyncObject)
            {
                long offset = BitCount;
                AddBits(bits.Length);
                SetBits(offset, bits);
                return offset;
            }
        }

        /// <summary>
        /// Add bits
        /// </summary>
        /// <param name="count">Number of bits to add</param>
        /// <returns>First new bit offset</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual long AddBits(int count)
        {
            if (count < 1) return BitCount;
            lock (SyncObject)
            {
                long res = BitCount;
                //FIXME Endless loop?
                while (BitCount + count > _Map.LongLength << 3) SetSize((int)(_Map.LongLength + IncreaseSize));
                BitCount += count;
                return res;
            }
        }
    }
}
