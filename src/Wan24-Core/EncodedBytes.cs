using System.Collections;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Encoded bytes structure
    /// </summary>
    public readonly record struct EncodedBytes : IList, IStructuralComparable, IStructuralEquatable
    {
        /// <summary>
        /// Encoded data
        /// </summary>
        public readonly char[] Data;
        /// <summary>
        /// Length
        /// </summary>
        public readonly int Length;
        /// <summary>
        /// Decoded length
        /// </summary>
        public readonly int DecodedLength;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Encoded data</param>
        public EncodedBytes(in char[] data)
        {
            Data = data;
            Length = data.Length;
            DecodedLength = ByteEncoding.GetDecodedLength(Length);
        }

        /// <summary>
        /// Get/set a character
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Character</returns>
        public char this[in int index]
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Data[index];
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set => Data[index] = value;
        }

        /// <summary>
        /// Get/set a character
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Character</returns>
        public char this[in Index index]
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Data[index];
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set => Data[index] = value;
        }

        /// <inheritdoc/>
        object? IList.this[int index]
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Data[index];
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set
            {
                if (value is null) throw new ArgumentNullException(nameof(value));
                Data[index] = (char)Convert.ChangeType(value, typeof(char));
            }
        }

        /// <inheritdoc/>
        public bool IsFixedSize
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => true;
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => false;
        }

        /// <inheritdoc/>
        public int Count
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Length;
        }

        /// <inheritdoc/>
        public bool IsSynchronized
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => false;
        }

        /// <inheritdoc/>
        public object SyncRoot
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Data;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int Add(object? value) => throw new NotSupportedException();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Clear() => Array.Clear(Data);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int CompareTo(object? other, IComparer comparer) => ((IStructuralComparable)Data).CompareTo(other, comparer);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Contains(object? value) => Array.IndexOf(Data, value) != -1;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void CopyTo(Array array, int index) => Data.CopyTo(array, index);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Equals(object? other, IEqualityComparer comparer) => ((IStructuralEquatable)Data).Equals(other, comparer);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public IEnumerator GetEnumerator() => Data.GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)Data).GetHashCode(comparer);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int IndexOf(object? value) => Array.IndexOf(Data, value);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Insert(int index, object? value) => throw new NotSupportedException();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Remove(object? value) => throw new NotSupportedException();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void RemoveAt(int index) => throw new NotSupportedException();

        /// <summary>
        /// Decode a compact encoded numeric value
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <returns>Number</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public T DecodeCompactNumber<T>() where T : struct, IConvertible => Data.DecodeCompactNumber<T>();

        /// <summary>
        /// Cast as byte array
        /// </summary>
        /// <param name="eb"><see cref="EncodedBytes"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator byte[](in EncodedBytes eb) => eb.Data.Decode();

        /// <summary>
        /// Cast as length
        /// </summary>
        /// <param name="eb"><see cref="EncodedBytes"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator int(in EncodedBytes eb) => eb.Length;

        /// <summary>
        /// Cast as <see cref="string"/>
        /// </summary>
        /// <param name="eb"><see cref="EncodedBytes"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator string(in EncodedBytes eb) => new(eb.Data);

        /// <summary>
        /// Cast as <see cref="EncodedBytes"/>
        /// </summary>
        /// <param name="data">Data</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator EncodedBytes(in string data) => new(data.ToCharArray());

        /// <summary>
        /// Cast as <see cref="EncodedBytes"/>
        /// </summary>
        /// <param name="data">Data</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator EncodedBytes(in byte[] data) => new(data.Encode());

        /// <summary>
        /// Cast as <see cref="EncodedBytes"/>
        /// </summary>
        /// <param name="data">Data</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator EncodedBytes(in Span<byte> data) => new(data.Encode());

        /// <summary>
        /// Cast as <see cref="EncodedBytes"/>
        /// </summary>
        /// <param name="data">Data</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator EncodedBytes(in ReadOnlySpan<byte> data) => new(data.Encode());

        /// <summary>
        /// Cast as <see cref="EncodedBytes"/>
        /// </summary>
        /// <param name="data">Data</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator EncodedBytes(in Memory<byte> data) => new(data.Span.Encode());

        /// <summary>
        /// Cast as <see cref="EncodedBytes"/>
        /// </summary>
        /// <param name="data">Data</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator EncodedBytes(in ReadOnlyMemory<byte> data) => new(data.Span.Encode());

        /// <summary>
        /// Cast as char array
        /// </summary>
        /// <param name="eb"><see cref="EncodedBytes"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator char[](in EncodedBytes eb) => eb.Data;

        /// <summary>
        /// Cast as char span
        /// </summary>
        /// <param name="eb"><see cref="EncodedBytes"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Span<char>(in EncodedBytes eb) => eb.Data;

        /// <summary>
        /// Cast as char span
        /// </summary>
        /// <param name="eb"><see cref="EncodedBytes"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ReadOnlySpan<char>(in EncodedBytes eb) => eb.Data;

        /// <summary>
        /// Cast as char memory
        /// </summary>
        /// <param name="eb"><see cref="EncodedBytes"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Memory<char>(in EncodedBytes eb) => eb.Data;

        /// <summary>
        /// Cast as char memory
        /// </summary>
        /// <param name="eb"><see cref="EncodedBytes"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ReadOnlyMemory<char>(in EncodedBytes eb) => eb.Data;
    }
}
