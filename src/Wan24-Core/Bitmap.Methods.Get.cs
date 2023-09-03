namespace wan24.Core
{
    // Get methods
    public partial class Bitmap
    {
        /// <summary>
        /// Get bits
        /// </summary>
        /// <param name="offset">Bit offset</param>
        /// <param name="count">Number of bits to get</param>
        /// <returns>Bits</returns>
        public virtual bool[] GetBits(long offset, in long count)
        {
            if (count < 1) return Array.Empty<bool>();
            lock (SyncObject)
            {
                if (offset < 0 || offset >= BitCount) throw new ArgumentOutOfRangeException(nameof(offset));
                if (offset + count > BitCount) throw new ArgumentOutOfRangeException(nameof(count));
                bool[] res = new bool[count];
                long byteOffset = offset >> 3;
                for (int i = 0; i != count; res[i] = ((_Map[byteOffset] >> (int)(offset - (byteOffset << 3))) & 1) == 1, offset++, byteOffset = offset >> 3, i++) ;
                return res;
            }
        }

        /// <summary>
        /// Determine if any bit is set within a range
        /// </summary>
        /// <param name="startBitIncluding">Start bit offset including</param>
        /// <param name="endBitIncluding">End bit offset including</param>
        /// <returns>If any bit is set</returns>
        public virtual bool IsAnyBitSet(in long startBitIncluding = 0, long? endBitIncluding = null)
        {
            lock (SyncObject)
            {
                long bitCount = BitCount;
                endBitIncluding ??= bitCount - 1;
                if (startBitIncluding < 0 || startBitIncluding >= bitCount) throw new ArgumentOutOfRangeException(nameof(startBitIncluding));
                if (endBitIncluding < startBitIncluding || endBitIncluding >= bitCount) throw new ArgumentOutOfRangeException(nameof(endBitIncluding));
                long offset = startBitIncluding;
                for (; (offset & byte.MaxValue) != 0 && offset <= endBitIncluding; offset++)
                    if (this[offset])
                        return true;
                if (offset >= endBitIncluding) return false;
                long endByteOffset = endBitIncluding.Value >> 3;
                for (long byteOffset = offset >> 3; byteOffset <= endByteOffset; byteOffset++)
                    if (_Map[byteOffset] != 0)
                        return true;
                for (offset = (endByteOffset << 3) + 1; offset <= endBitIncluding; offset++)
                    if (this[offset])
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Determine if all bits are set within a range
        /// </summary>
        /// <param name="startBitIncluding">Start bit offset including</param>
        /// <param name="endBitIncluding">End bit offset including</param>
        /// <returns>If all bits are set</returns>
        public virtual bool AllBitsAreSet(in long startBitIncluding = 0, long? endBitIncluding = null)
        {
            lock (SyncObject)
            {
                long bitCount = BitCount;
                endBitIncluding ??= bitCount - 1;
                if (startBitIncluding < 0 || startBitIncluding >= bitCount) throw new ArgumentOutOfRangeException(nameof(startBitIncluding));
                if (endBitIncluding < startBitIncluding || endBitIncluding >= bitCount) throw new ArgumentOutOfRangeException(nameof(endBitIncluding));
                long offset = startBitIncluding;
                for (; (offset & byte.MaxValue) != 0 && offset <= endBitIncluding; offset++)
                    if (!this[offset])
                        return false;
                if (offset >= endBitIncluding) return true;
                long endByteOffset = endBitIncluding.Value >> 3;
                for (long byteOffset = offset >> 3; byteOffset <= endByteOffset; byteOffset++)
                    if (_Map[byteOffset] != byte.MaxValue)
                        return false;
                for (offset = (endByteOffset << 3) + 1; offset <= endBitIncluding; offset++)
                    if (!this[offset])
                        return false;
                return true;
            }
        }
    }
}
