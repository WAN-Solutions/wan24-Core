using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a secure array
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
#if NO_UNSAFE
    public abstract class SecureArrayBase<T> : DisposableBase, ISecureArray<T> where T : struct
#else
    public abstract unsafe class SecureArrayBase<T> : DisposableBase, ISecureArray<T> where T : struct
#endif
    {
        /// <summary>
        /// Array
        /// </summary>
        protected readonly T[] _Array;
        /// <summary>
        /// Handle
        /// </summary>
        protected readonly GCHandle Handle;
#if !NO_UNSAFE
#pragma warning disable CS8500
        /// <summary>
        /// Pointer
        /// </summary>
        protected readonly T* _Ptr;
#pragma warning restore CS8500
#endif
        /// <summary>
        /// Is detached?
        /// </summary>
        protected bool Detached = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="array">Array</param>
        protected SecureArrayBase(T[] array) : base()
        {
            _Array = array;
            Handle = GCHandle.Alloc(_Array, GCHandleType.Pinned);
#if !NO_UNSAFE
#pragma warning disable CS8500
            _Ptr = (T*)Handle.AddrOfPinnedObject();
#pragma warning restore CS8500
#endif
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length in bytes</param>
        protected SecureArrayBase(long len) : this(new T[len]) { }

        /// <inheritdoc/>
        public T this[int offset]
        {
            get => IfUndisposed(_Array[offset]);
            set
            {
                EnsureUndisposed();
                _Array[offset] = value;
            }
        }

        /// <inheritdoc/>
        public Memory<T> this[Range range] => IfUndisposed(() => _Array[range]);

        /// <inheritdoc/>
        public Memory<T> this[Index start, Index end] => IfUndisposed(() => _Array[new Range(start, end)]);

        /// <inheritdoc/>
        public int Length => IfUndisposed(_Array.Length);

        /// <inheritdoc/>
        public long LongLength => IfUndisposed(_Array.LongLength);

        /// <inheritdoc/>
        public T[] Array => IfUndisposed(_Array);

        /// <inheritdoc/>
        public Span<T> Span => Array;

        /// <inheritdoc/>
        public Memory<T> Memory => Array;

#if !NO_UNSAFE
        /// <summary>
        /// Pointer
        /// </summary>
#pragma warning disable CS8500
        public T* Ptr
#pragma warning restore CS8500
        {
            get
            {
                EnsureUndisposed();
                return _Ptr;
            }
        }
#endif

        /// <inheritdoc/>
        public IntPtr IntPtr => IfUndisposed(() => Handle.AddrOfPinnedObject());

        /// <inheritdoc/>
        public T[] DetachAndDispose()
        {
            EnsureUndisposed();
            Detached = true;
            Dispose();
            return _Array;
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_Array).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => _Array.GetEnumerator();

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj) => _Array.Equals(obj);

        /// <inheritdoc/>
        public bool Equals(Memory<T> other) => Memory.Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => _Array.GetHashCode();
    }
}
