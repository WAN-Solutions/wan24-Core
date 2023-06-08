using System.Collections;
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
        /// Array
        /// </summary>
        private readonly byte[] _Array;
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
        public SecureByteArrayStruct(byte[] array)
        {
            _Array = array;
            Handle = GCHandle.Alloc(_Array, GCHandleType.Pinned);
#if !NO_UNSAFE
            _Ptr = (byte*)Handle.AddrOfPinnedObject();
#endif
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length in bytes</param>
        public SecureByteArrayStruct(long len) : this(new byte[len]) { }

        /// <inheritdoc/>
        public readonly byte this[int offset]
        {
            get => IfUndisposed(_Array[offset]);
            set
            {
                EnsureUndisposed();
                _Array[offset] = value;
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
                    return _Array[range];
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
                    return _Array[new Range(start, end)];
                }
            }
        }

        /// <inheritdoc/>
        public readonly int Length => IfUndisposed(_Array.Length);

        /// <inheritdoc/>
        public readonly long LongLength => IfUndisposed(_Array.LongLength);

        /// <inheritdoc/>
        public readonly byte[] Array => IfUndisposed(_Array);

        /// <inheritdoc/>
        public readonly Span<byte> Span => Array;

        /// <inheritdoc/>
        public readonly Memory<byte> Memory => Array;

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
            Dispose();
            return _Array;
        }

        /// <inheritdoc/>
        public readonly IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)_Array).GetEnumerator();

        /// <inheritdoc/>
        readonly IEnumerator IEnumerable.GetEnumerator() => _Array.GetEnumerator();

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj) => _Array.Equals(obj);

        /// <inheritdoc/>
        public readonly bool Equals(Memory<byte> other) => Memory.Equals(other);

        /// <inheritdoc/>
        public override readonly int GetHashCode() => _Array.GetHashCode();

        /// <summary>
        /// Ensure an undisposed object state
        /// </summary>
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
        private readonly tValue IfUndisposed<tValue>(tValue value)
        {
            EnsureUndisposed();
            return value;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (SyncObject)
            {
                if (IsDisposed) return;
                IsDisposed = true;
            }
            try
            {
                if (!Detached && _Array.Length > 0) _Array.Clear();
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
        public static implicit operator byte[](SecureByteArrayStruct arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Span<byte>(SecureByteArrayStruct arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Memory<byte>(SecureByteArrayStruct arr) => arr.Memory;

#if !NO_UNSAFE
        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator byte*(SecureByteArrayStruct arr) => arr.Ptr;
#endif

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator IntPtr(SecureByteArrayStruct arr) => arr.IntPtr;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator int(SecureByteArrayStruct arr) => arr.Length;

        /// <summary>
        /// Cast as Int64 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator long(SecureByteArrayStruct arr) => arr.LongLength;

        /// <summary>
        /// Cast as <see cref="SecureCharArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator SecureCharArray(SecureByteArrayStruct arr) => new(arr.Span.ToUtf8Chars());

        /// <summary>
        /// Cast as <see cref="SecureCharArrayStruct"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator SecureCharArrayStruct(SecureByteArrayStruct arr) => new(arr.Span.ToUtf8Chars());

        /// <summary>
        /// Cast a byte array as secure byte array
        /// </summary>
        /// <param name="arr">Byte array</param>
        public static explicit operator SecureByteArrayStruct(byte[] arr) => new(arr);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals?</returns>
        public static bool operator ==(SecureByteArrayStruct left, SecureByteArrayStruct right) => left.Equals(right);

        /// <summary>
        /// Not equal
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equal?</returns>
        public static bool operator !=(SecureByteArrayStruct left, SecureByteArrayStruct right) => !left.Equals(right);
    }
}
