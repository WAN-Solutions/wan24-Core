namespace wan24.Core
{
    // Set methods
    public partial class Bitmap
    {
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
        /// Set bits
        /// </summary>
        /// <param name="bits">Bits to set</param>
        /// <param name="offset">Start bit offset</param>
        public virtual void SetBits(IEnumerable<bool> bits, long offset = 0)
        {
            lock (SyncObject)
            {
                long bitCount = BitCount;
                if (offset < 0 || offset >= bitCount) throw new ArgumentOutOfRangeException(nameof(offset));
                long byteOffset = offset >> 3;
                int bit;
                foreach (bool b in bits)
                {
                    if (offset >= bitCount) throw new OverflowException();
                    bit = 1 << (int)(offset - (byteOffset << 3));
                    if (b)
                    {
                        _Map[byteOffset] |= (byte)bit;
                    }
                    else
                    {
                        _Map[byteOffset] &= (byte)~bit;
                    }
                    offset++;
                    byteOffset = offset >> 3;
                }
            }
        }

        /// <summary>
        /// Set all bits
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="startBitIncluding">Start bit offset including</param>
        /// <param name="endBitIncluding">End bit offset including</param>
        public virtual void SetAllBits(bool value, long startBitIncluding = 0, long? endBitIncluding = null)
        {
            lock (SyncObject)
            {
                long bitCount = BitCount;
                endBitIncluding ??= bitCount - 1;
                if (startBitIncluding < 0 || startBitIncluding >= bitCount) throw new ArgumentOutOfRangeException(nameof(startBitIncluding));
                if (endBitIncluding < startBitIncluding || endBitIncluding >= bitCount) throw new ArgumentOutOfRangeException(nameof(endBitIncluding));
                long offset = startBitIncluding;
                for (; (offset & byte.MaxValue) != 0 && offset <= endBitIncluding; this[offset] = value, offset++) ;
                if (offset >= endBitIncluding) return;
                long endByteOffset = endBitIncluding.Value >> 3;
                byte v = value ? byte.MaxValue : (byte)0;
                for (long byteOffset = offset >> 3; byteOffset <= endByteOffset; _Map[byteOffset] = v, byteOffset++) ;
                for (offset = (endByteOffset << 3) + 1; offset <= endBitIncluding; this[offset] = value, offset++) ;
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
    }
}
