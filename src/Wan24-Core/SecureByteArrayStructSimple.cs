using System.Collections;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Secure byte array (will delete its contents when disposing; not thread-safe)
    /// </summary>
#if NO_UNSAFE
    public struct SecureByteArrayStructSimple : ISecureArray<byte>
#else
    public unsafe struct SecureByteArrayStructSimple : ISecureArray<byte>
#endif
    {
        /// <summary>
        /// Handle
        /// </summary>
        private readonly GCHandle Handle;
        /// <summary>
        /// Is detached?
        /// </summary>
        private bool Detached = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="array">Array</param>
        public SecureByteArrayStructSimple(in byte[] array)
        {
            Array = array;
            Handle = GCHandle.Alloc(Array, GCHandleType.Pinned);
#if !NO_UNSAFE
            Ptr = (byte*)Handle.AddrOfPinnedObject();
#endif
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length in bytes</param>
        public SecureByteArrayStructSimple(in long len) : this(new byte[len]) { }

        /// <inheritdoc/>
        public readonly byte this[int offset]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array[offset];
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set => Array[offset] = value;
        }

        /// <inheritdoc/>
        public readonly Memory<byte> this[Range range]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array[range];
        }

        /// <inheritdoc/>
        public readonly Memory<byte> this[Index start, Index end]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array[new Range(start, end)];
        }

        /// <inheritdoc/>
        public readonly int Length
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.Length;
        }

        /// <inheritdoc/>
        public readonly long LongLength
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.LongLength;
        }

        /// <inheritdoc/>
        public byte[] Array { get; private set; }

        /// <inheritdoc/>
        public readonly Span<byte> Span => Array.AsSpan();

        /// <inheritdoc/>
        public readonly Memory<byte> Memory => Array.AsMemory();

#if !NO_UNSAFE
        /// <summary>
        /// Pointer
        /// </summary>
        public byte* Ptr { get; }
#endif

        /// <inheritdoc/>
        public readonly IntPtr IntPtr
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Handle.AddrOfPinnedObject();
        }

        /// <inheritdoc/>
        public byte[] DetachAndDispose()
        {
            if (Detached) throw new InvalidOperationException();
            Detached = true;
            byte[] res = Array;
            Array = [];
            Dispose();
            return res;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public readonly IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)Array).GetEnumerator();

        /// <inheritdoc/>
        readonly IEnumerator IEnumerable.GetEnumerator() => Array.GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override readonly bool Equals(object? obj) => Array.Equals(obj);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public readonly bool Equals(Memory<byte> other) => Memory.Equals(other);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override readonly int GetHashCode() => Array.GetHashCode();

        /// <inheritdoc/>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Dispose()
        {
            try
            {
                if (!Detached && Array.Length > 0) Array.Clear();
            }
            finally
            {
                if (Handle.IsAllocated) Handle.Free();
                Detached = true;
            }
        }

        /// <summary>
        /// Cast as byte array
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator byte[](in SecureByteArrayStructSimple arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Span<byte>(in SecureByteArrayStructSimple arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Memory<byte>(in SecureByteArrayStructSimple arr) => arr.Memory;

#if !NO_UNSAFE
        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator byte*(in SecureByteArrayStructSimple arr) => arr.Ptr;
#endif

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IntPtr(in SecureByteArrayStructSimple arr) => arr.IntPtr;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in SecureByteArrayStructSimple arr) => arr.Length;

        /// <summary>
        /// Cast as Int64 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator long(in SecureByteArrayStructSimple arr) => arr.LongLength;

        /// <summary>
        /// Cast as <see cref="SecureCharArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator SecureCharArray(in SecureByteArrayStructSimple arr) => new(arr.Span.ToUtf8Chars());

        /// <summary>
        /// Cast as <see cref="SecureCharArrayStruct"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator SecureCharArrayStruct(in SecureByteArrayStructSimple arr) => new(arr.Span.ToUtf8Chars());

        /// <summary>
        /// Cast a byte array as secure byte array
        /// </summary>
        /// <param name="arr">Byte array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static explicit operator SecureByteArrayStructSimple(in byte[] arr) => new(arr);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in SecureByteArrayStructSimple left, in SecureByteArrayStructSimple right) => left.Equals(right);

        /// <summary>
        /// Not equal
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in SecureByteArrayStructSimple left, in SecureByteArrayStructSimple right) => !left.Equals(right);
    }
}
