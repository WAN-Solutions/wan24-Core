using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace wan24.Core
{
    /// <summary>
    /// Secure byte array (will delete its contents when disposing)
    /// </summary>
    public sealed unsafe class SecureByteArray : DisposableBase, IEnumerable<byte>, IEnumerable
    {
        /// <summary>
        /// Array
        /// </summary>
        private readonly byte[] _Array;
        /// <summary>
        /// Handle
        /// </summary>
        private readonly GCHandle Handle;
        /// <summary>
        /// Pointer
        /// </summary>
        private readonly byte* _Ptr;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="array">Array</param>
        public SecureByteArray(byte[] array) : base()
        {
            _Array = array;
            Handle = GCHandle.Alloc(_Array, GCHandleType.Pinned);
            _Ptr = (byte*)Handle.AddrOfPinnedObject();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length in bytes</param>
        public SecureByteArray(long len) : this(new byte[len]) { }

        /// <summary>
        /// Get/set a byte
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        public byte this[int index]
        {
            get => IfUndisposed(_Array[index]);
            set
            {
                EnsureUndisposed();
                _Array[index] = value;
            }
        }

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

        /// <summary>
        /// Pointer
        /// </summary>
        public IntPtr IntPtr => IfUndisposed(() => Handle.AddrOfPinnedObject());

        /// <inheritdoc/>
        public IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)_Array).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => _Array.GetEnumerator();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (_Array.Length < 1) return;
            RandomNumberGenerator.Fill(_Array);
            System.Array.Clear(_Array);
            if (Handle.IsAllocated) Handle.Free();
        }
    }
}
