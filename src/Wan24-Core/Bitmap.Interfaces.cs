using System.Collections;

namespace wan24.Core
{
    // Interfaces
    public partial class Bitmap : IEnumerable<bool>, IDictionary<long, bool>, IReadOnlyDictionary<long, bool>
    {
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
    }
}
