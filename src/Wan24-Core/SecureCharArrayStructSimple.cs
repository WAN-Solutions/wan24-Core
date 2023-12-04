using System.Collections;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace wan24.Core
{
    /// <summary>
    /// Secure char array (will delete its contents when disposing; not thread-safe)
    /// </summary>
#if NO_UNSAFE
    public struct SecureCharArrayStructSimple : ISecureArray<char>
#else
    public unsafe struct SecureCharArrayStructSimple : ISecureArray<char>
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
        public SecureCharArrayStructSimple(in char[] array)
        {
            Array = array;
            Handle = GCHandle.Alloc(Array, GCHandleType.Pinned);
#if !NO_UNSAFE
            Ptr = (char*)Handle.AddrOfPinnedObject();
#endif
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length in chars</param>
        public SecureCharArrayStructSimple(in long len) : this(new char[len]) { }

        /// <inheritdoc/>
        public readonly char this[int offset]
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
        public readonly Memory<char> this[Range range]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array[range];
        }

        /// <inheritdoc/>
        public readonly Memory<char> this[Index start, Index end]
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
        public char[] Array { get; private set; }

        /// <inheritdoc/>
        public readonly Span<char> Span
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array.AsSpan();
        }

        /// <inheritdoc/>
        public readonly Memory<char> Memory
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
        public char* Ptr { get; }
#endif

        /// <inheritdoc/>
        public readonly IntPtr IntPtr => Handle.AddrOfPinnedObject();

        /// <inheritdoc/>
        public char[] DetachAndDispose()
        {
            if (Detached) throw new InvalidOperationException();
            Detached = true;
            char[] res = Array;
            Array = [];
            Dispose();
            return res;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public readonly IEnumerator<char> GetEnumerator() => ((IEnumerable<char>)Array).GetEnumerator();

        /// <inheritdoc/>
        readonly IEnumerator IEnumerable.GetEnumerator() => Array.GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override readonly bool Equals(object? obj) => Array.Equals(obj);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public readonly bool Equals(Memory<char> other) => Memory.Equals(other);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override readonly int GetHashCode() => Array.GetHashCode();

        /// <inheritdoc/>
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
        /// Cast as char array
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator char[](in SecureCharArrayStructSimple arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Span<char>(in SecureCharArrayStructSimple arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Memory<char>(in SecureCharArrayStructSimple arr) => arr.Memory;

#if !NO_UNSAFE
        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator char*(in SecureCharArrayStructSimple arr) => arr.Ptr;
#endif

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IntPtr(in SecureCharArrayStructSimple arr) => arr.IntPtr;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in SecureCharArrayStructSimple arr) => arr.Length;

        /// <summary>
        /// Cast as Int64 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator long(in SecureCharArrayStructSimple arr) => arr.LongLength;

        /// <summary>
        /// Cast as <see cref="SecureByteArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static implicit operator SecureByteArray(in SecureCharArrayStructSimple arr)
        {
            int len = arr.Length * 3;
#if !NO_UNSAFE
            if (len > Settings.StackAllocBorder)
            {
#endif
                using RentedArrayRefStruct<byte> buffer = new(len, clean: false);
                return new(buffer.Span[..Encoding.UTF8.GetBytes((ReadOnlySpan<char>)arr.Span, buffer.Span)].ToArray());
#if !NO_UNSAFE
            }
            else
            {
                Span<byte> buffer = stackalloc byte[len];
                return new(buffer[..Encoding.UTF8.GetBytes((ReadOnlySpan<char>)arr.Span, buffer)].ToArray());
            }
#endif
        }

        /// <summary>
        /// Cast as <see cref="SecureByteArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static implicit operator SecureByteArrayStructSimple(in SecureCharArrayStructSimple arr)
        {
            int len = arr.Length * 3;
#if !NO_UNSAFE
            if (len > Settings.StackAllocBorder)
            {
#endif
                using RentedArrayRefStruct<byte> buffer = new(len, clean: false);
                return new(buffer.Span[..Encoding.UTF8.GetBytes((ReadOnlySpan<char>)arr.Span, buffer.Span)].ToArray());
#if !NO_UNSAFE
            }
            else
            {
                Span<byte> buffer = stackalloc byte[len];
                return new(buffer[..Encoding.UTF8.GetBytes((ReadOnlySpan<char>)arr.Span, buffer)].ToArray());
            }
#endif
        }

        /// <summary>
        /// Cast a char array as secure char array
        /// </summary>
        /// <param name="arr">Char array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static explicit operator SecureCharArrayStructSimple(in char[] arr) => new(arr);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in SecureCharArrayStructSimple left, in SecureCharArrayStructSimple right) => left.Equals(right);

        /// <summary>
        /// Not equal
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in SecureCharArrayStructSimple left, in SecureCharArrayStructSimple right) => !left.Equals(right);
    }
}
