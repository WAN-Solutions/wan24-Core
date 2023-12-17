using System.Collections;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Secure byte array (will delete its contents when disposing)
    /// </summary>
#if NO_UNSAFE
    public struct SecureByteArrayStruct : ISecureArray<byte>
#else
    public unsafe struct SecureByteArrayStruct : ISecureArray<byte>
#endif
    {
        /// <summary>
        /// Handle
        /// </summary>
        private readonly GCHandle Handle;
#if !NO_UNSAFE
        /// <summary>
        /// Pointer
        /// </summary>
        private readonly byte* _Ptr;
#endif
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        private readonly object SyncObject = new();
        /// <summary>
        /// Is disposed?
        /// </summary>
        private bool IsDisposed = false;
        /// <summary>
        /// Is detached?
        /// </summary>
        private bool Detached = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="array">Array</param>
        public SecureByteArrayStruct(in byte[] array)
        {
            Array = array;
            Handle = GCHandle.Alloc(Array, GCHandleType.Pinned);
#if !NO_UNSAFE
            _Ptr = (byte*)Handle.AddrOfPinnedObject();
#endif
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length in bytes</param>
        public SecureByteArrayStruct(in long len) : this(new byte[len]) { }

        /// <inheritdoc/>
        public readonly byte this[int offset]
        {
            get => IfUndisposed(Array[offset]);
            set
            {
                EnsureUndisposed();
                Array[offset] = value;
            }
        }

        /// <inheritdoc/>
        public readonly Memory<byte> this[Range range]
        {
            get
            {
                lock (SyncObject)
                {
                    EnsureUndisposed();
                    return Array[range];
                }
            }
        }

        /// <inheritdoc/>
        public readonly Memory<byte> this[Index start, Index end]
        {
            get
            {
                lock (SyncObject)
                {
                    EnsureUndisposed();
                    return Array[new Range(start, end)];
                }
            }
        }

        /// <inheritdoc/>
        public byte[] Array { get; private set; }

        /// <inheritdoc/>
        public readonly int Length => IfUndisposed(Array.Length);

        /// <inheritdoc/>
        public readonly long LongLength => IfUndisposed(Array.LongLength);

        /// <inheritdoc/>
        public readonly Span<byte> Span
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.AsSpan();
        }

        /// <inheritdoc/>
        public readonly Memory<byte> Memory
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.AsMemory();
        }

#if !NO_UNSAFE
        /// <summary>
        /// Pointer
        /// </summary>
        public readonly byte* Ptr
        {
            get
            {
                EnsureUndisposed();
                return _Ptr;
            }
        }
#endif

        /// <inheritdoc/>
        public readonly IntPtr IntPtr
        {
            get
            {
                lock (SyncObject)
                {
                    EnsureUndisposed();
                    return Handle.AddrOfPinnedObject();
                }
            }
        }

        /// <inheritdoc/>
        public byte[] DetachAndDispose()
        {
            EnsureUndisposed();
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

        /// <summary>
        /// Ensure an undisposed object state
        /// </summary>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private readonly void EnsureUndisposed()
        {
            lock (SyncObject) if (!IsDisposed) return;
            throw new ObjectDisposedException(ToString());
        }

        /// <summary>
        /// Return a value if not disposing/disposed
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private readonly tValue IfUndisposed<tValue>(tValue value)
        {
            EnsureUndisposed();
            return value;
        }

        /// <inheritdoc/>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Dispose()
        {
            lock (SyncObject)
            {
                if (IsDisposed) return;
                IsDisposed = true;
            }
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
        public static implicit operator byte[](in SecureByteArrayStruct arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Span<byte>(in SecureByteArrayStruct arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Memory<byte>(in SecureByteArrayStruct arr) => arr.Memory;

#if !NO_UNSAFE
        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator byte*(in SecureByteArrayStruct arr) => arr.Ptr;
#endif

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IntPtr(in SecureByteArrayStruct arr) => arr.IntPtr;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in SecureByteArrayStruct arr) => arr.Length;

        /// <summary>
        /// Cast as Int64 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator long(in SecureByteArrayStruct arr) => arr.LongLength;

        /// <summary>
        /// Cast as <see cref="SecureCharArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator SecureCharArray(in SecureByteArrayStruct arr) => new(arr.Span.ToUtf8Chars());

        /// <summary>
        /// Cast as <see cref="SecureCharArrayStruct"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator SecureCharArrayStruct(in SecureByteArrayStruct arr) => new(arr.Span.ToUtf8Chars());

        /// <summary>
        /// Cast a byte array as secure byte array
        /// </summary>
        /// <param name="arr">Byte array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static explicit operator SecureByteArrayStruct(in byte[] arr) => new(arr);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in SecureByteArrayStruct left, in SecureByteArrayStruct right) => left.Equals(right);

        /// <summary>
        /// Not equal
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in SecureByteArrayStruct left, in SecureByteArrayStruct right) => !left.Equals(right);
    }
}
