using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace wan24.Core
{
    /// <summary>
    /// Secure char array (will delete its contents when disposing; not thread-safe)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
#if NO_UNSAFE
    public ref struct SecureCharArrayRefStruct
#else
    public unsafe ref struct SecureCharArrayRefStruct
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
        public SecureCharArrayRefStruct(in char[] array)
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
        public SecureCharArrayRefStruct(in long len)
        {
            Array = new char[len];
            Handle = GCHandle.Alloc(Array, GCHandleType.Pinned);
#if !NO_UNSAFE
            Ptr = (char*)Handle.AddrOfPinnedObject();
#endif
        }

        /// <summary>
        /// Get/set an element
        /// </summary>
        /// <param name="offset">Index</param>
        /// <returns>Element</returns>
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

        /// <summary>
        /// Get a range
        /// </summary>
        /// <param name="range">Range</param>
        /// <returns>Range memory</returns>
        public readonly Memory<char> this[Range range]
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
        public readonly Memory<char> this[Index start, Index end]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Array[new Range(start, end)];
        }

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
        /// Array
        /// </summary>
        public char[] Array { get; private set; }

        /// <summary>
        /// Span
        /// </summary>
        public readonly Span<char> Span
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

        /// <summary>
        /// Pointer
        /// </summary>
        public readonly IntPtr IntPtr => Handle.AddrOfPinnedObject();

        /// <summary>
        /// Detach the secured byte array and dispose this instance
        /// </summary>
        /// <returns>Unsecure byte array</returns>
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
        public override readonly bool Equals(object? obj) => Array.Equals(obj);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public readonly bool Equals(Memory<char> other) => Memory.Equals(other);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override readonly int GetHashCode() => Array.GetHashCode();

        /// <summary>
        /// Dispose
        /// </summary>
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
        public static implicit operator char[](in SecureCharArrayRefStruct arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Span<char>(in SecureCharArrayRefStruct arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Memory<char>(in SecureCharArrayRefStruct arr) => arr.Memory;

#if !NO_UNSAFE
        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator char*(in SecureCharArrayRefStruct arr) => arr.Ptr;
#endif

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IntPtr(in SecureCharArrayRefStruct arr) => arr.IntPtr;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in SecureCharArrayRefStruct arr) => arr.Length;

        /// <summary>
        /// Cast as Int64 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator long(in SecureCharArrayRefStruct arr) => arr.LongLength;

        /// <summary>
        /// Cast as <see cref="SecureByteArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static implicit operator SecureByteArray(in SecureCharArrayRefStruct arr)
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
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static implicit operator SecureByteArrayStructSimple(in SecureCharArrayRefStruct arr)
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
        public static explicit operator SecureCharArrayRefStruct(in char[] arr) => new(arr);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in SecureCharArrayRefStruct left, in SecureCharArrayRefStruct right) => left.Equals(right);

        /// <summary>
        /// Not equal
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in SecureCharArrayRefStruct left, in SecureCharArrayRefStruct right) => !left.Equals(right);
    }
}
