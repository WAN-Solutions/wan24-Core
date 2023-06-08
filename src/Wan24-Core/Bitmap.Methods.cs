﻿namespace wan24.Core
{
    // Methods
    public partial class Bitmap
    {
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
    }
}