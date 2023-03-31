using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace wan24.Core
{
    /// <summary>
    /// Secure byte array (will delete its contents when disposing)
    /// </summary>
#if NO_UNSAFE
    public sealed class SecureByteArray : DisposableBase, IEnumerable<byte>, IEnumerable, IEquatable<Memory<byte>>
#else
    public sealed unsafe class SecureByteArray : DisposableBase, IEnumerable<byte>, IEnumerable, IEquatable<Memory<byte>>
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
        /// Is detached?
        /// </summary>
        private bool Detached = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="array">Array</param>
        public SecureByteArray(byte[] array) : base()
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
        public SecureByteArray(long len) : this(new byte[len]) { }

        /// <summary>
        /// Get/set a byte
        /// </summary>
        /// <param name="offset">Index</param>
        /// <returns>Byte</returns>
        public byte this[int offset]
        {
            get => IfUndisposed(_Array[offset]);
            set
            {
                EnsureUndisposed();
                _Array[offset] = value;
            }
        }

        /// <summary>
        /// Get a range
        /// </summary>
        /// <param name="range">Range</param>
        /// <returns>Range memory</returns>
        public Memory<byte> this[Range range] => IfUndisposed(() => _Array[range]);

        /// <summary>
        /// Get a range
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <returns>Range memory</returns>
        public Memory<byte> this[Index start, Index end] => IfUndisposed(() => _Array[new Range(start, end)]);

        /// <summary>
        /// Length
        /// </summary>
        public int Length => IfUndisposed(_Array.Length);

        /// <summary>
        /// Length
        /// </summary>
        public long LongLength => IfUndisposed(_Array.LongLength);

        /// <summary>
        /// Array
        /// </summary>
        public byte[] Array => IfUndisposed(_Array);

        /// <summary>
        /// Span
        /// </summary>
        public Span<byte> Span => Array;

        /// <summary>
        /// Memory
        /// </summary>
        public Memory<byte> Memory => Array;
#if !NO_UNSAFE
        /// <summary>
        /// Pointer
        /// </summary>
        public byte* Ptr
        {
            get
            {
                EnsureUndisposed();
                return _Ptr;
            }
        }
#endif

        /// <summary>
        /// Pointer
        /// </summary>
        public IntPtr IntPtr => IfUndisposed(() => Handle.AddrOfPinnedObject());

        /// <summary>
        /// Detach the secured byte array and dispose this instance
        /// </summary>
        /// <returns>Unsecure byte array</returns>
        public byte[] DetachAndDispose()
        {
            EnsureUndisposed();
            Detached = true;
            Dispose();
            return _Array;
        }

        /// <inheritdoc/>
        public IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)_Array).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => _Array.GetEnumerator();

        /// <inheritdoc/>
        public override bool Equals(object? obj) => _Array.Equals(obj);

        /// <inheritdoc/>
        public bool Equals(Memory<byte> other) => Memory.Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => _Array.GetHashCode();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (Detached || _Array.Length < 1) return;
                RandomNumberGenerator.Fill(_Array);
                System.Array.Clear(_Array);
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
        public static implicit operator byte[](SecureByteArray arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Span<byte>(SecureByteArray arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Memory<byte>(SecureByteArray arr) => arr.Memory;
#if !NO_UNSAFE
        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator byte*(SecureByteArray arr) => arr.Ptr;
#endif

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator IntPtr(SecureByteArray arr) => arr.IntPtr;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator int(SecureByteArray arr) => arr.Length;

        /// <summary>
        /// Cast as Int64 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator long(SecureByteArray arr) => arr.LongLength;

        /// <summary>
        /// Cast a byte array as secure byte array
        /// </summary>
        /// <param name="arr">Byte array</param>
        public static explicit operator SecureByteArray(byte[] arr) => new(arr);
    }
}
