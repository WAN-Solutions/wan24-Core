using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Secure byte array (will delete its contents when disposing; not thread-safe)
    /// </summary>
#if NO_UNSAFE
    public ref struct SecureByteArrayRefStruct
#else
    public unsafe ref struct SecureByteArrayRefStruct
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
        public readonly byte* Ptr;
#endif
        /// <summary>
        /// Is detached?
        /// </summary>
        private bool Detached = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="array">Array</param>
        public SecureByteArrayRefStruct(in byte[] array)
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
        public SecureByteArrayRefStruct(in long len) : this(new byte[len]) { }

        /// <summary>
        /// Get/set an element
        /// </summary>
        /// <param name="offset">Index</param>
        /// <returns>Element</returns>
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

        /// <summary>
        /// Get a range
        /// </summary>
        /// <param name="range">Range</param>
        /// <returns>Range memory</returns>
        public readonly Memory<byte> this[Range range]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array[range];
        }

        /// <summary>
        /// Get a range
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <returns>Range memory</returns>
        public readonly Memory<byte> this[Index start, Index end]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array[new Range(start, end)];
        }

        /// <summary>
        /// Array
        /// </summary>
        public byte[] Array { get; private set; }

        /// <summary>
        /// Length
        /// </summary>
        public readonly int Length
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.Length;
        }

        /// <summary>
        /// Length
        /// </summary>
        public readonly long LongLength
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.LongLength;
        }

        /// <summary>
        /// Span
        /// </summary>
        public readonly Span<byte> Span
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.AsSpan();
        }

        /// <summary>
        /// Memory
        /// </summary>
        public readonly Memory<byte> Memory
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.AsMemory();
        }

        /// <summary>
        /// Pointer
        /// </summary>
        public readonly IntPtr IntPtr => Handle.AddrOfPinnedObject();

        /// <summary>
        /// Detach the secured byte array and dispose this instance
        /// </summary>
        /// <returns>Unsecure byte array</returns>
        public byte[] DetachAndDispose()
        {
            if (Detached) throw new InvalidOperationException();
            Detached = true;
            byte[] res = Array;
            Array = System.Array.Empty<byte>();
            Dispose();
            return res;
        }

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
        /// Dispose
        /// </summary>
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
        public static implicit operator byte[](in SecureByteArrayRefStruct arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Span<byte>(in SecureByteArrayRefStruct arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Memory<byte>(in SecureByteArrayRefStruct arr) => arr.Memory;

#if !NO_UNSAFE
        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator byte*(in SecureByteArrayRefStruct arr) => arr.Ptr;
#endif

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IntPtr(in SecureByteArrayRefStruct arr) => arr.IntPtr;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in SecureByteArrayRefStruct arr) => arr.Length;

        /// <summary>
        /// Cast as Int64 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator long(in SecureByteArrayRefStruct arr) => arr.LongLength;

        /// <summary>
        /// Cast as <see cref="SecureCharArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator SecureCharArray(in SecureByteArrayRefStruct arr) => new(arr.Span.ToUtf8Chars());

        /// <summary>
        /// Cast as <see cref="SecureCharArrayStruct"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator SecureCharArrayStruct(in SecureByteArrayRefStruct arr) => new(arr.Span.ToUtf8Chars());

        /// <summary>
        /// Cast a byte array as secure byte array
        /// </summary>
        /// <param name="arr">Byte array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static explicit operator SecureByteArrayRefStruct(in byte[] arr) => new(arr);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in SecureByteArrayRefStruct left, in SecureByteArrayRefStruct right) => left.Equals(right);

        /// <summary>
        /// Not equal
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in SecureByteArrayRefStruct left, in SecureByteArrayRefStruct right) => !left.Equals(right);
    }
}
