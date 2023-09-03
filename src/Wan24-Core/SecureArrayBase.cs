using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;
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
        protected SecureArrayBase(in T[] array) : base()
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
        protected SecureArrayBase(in long len) : this(new T[len]) { }

        /// <inheritdoc/>
        public T this[int offset]
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
        public Memory<T> this[Range range]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array[range];
        }

        /// <inheritdoc/>
        public Memory<T> this[Index start, Index end]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array[new Range(start, end)];
        }

        /// <inheritdoc/>
        public int Length
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.Length;
        }

        /// <inheritdoc/>
        public long LongLength
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.LongLength;
        }

        /// <inheritdoc/>
        public T[] Array => IfUndisposed(_Array);

        /// <inheritdoc/>
        public Span<T> Span
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.AsSpan();
        }

        /// <inheritdoc/>
        public Memory<T> Memory
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
        [TargetedPatchingOptOut("Just a method adapter")]
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Array).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => Array.GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override bool Equals([NotNullWhen(true)] object? obj) => Array.Equals(obj);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public bool Equals(Memory<T> other) => Memory.Equals(other);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override int GetHashCode() => Array.GetHashCode();
    }
}
