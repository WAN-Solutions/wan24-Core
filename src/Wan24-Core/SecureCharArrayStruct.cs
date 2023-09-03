using System.Collections;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace wan24.Core
{
    /// <summary>
    /// Secure char array (will delete its contents when disposing)
    /// </summary>
#if NO_UNSAFE
    public struct SecureCharArrayStruct : ISecureArray<char>
#else
    public unsafe struct SecureCharArrayStruct : ISecureArray<char>
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
        private readonly char* _Ptr;
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
        public SecureCharArrayStruct(in char[] array)
        {
            Array = array;
            Handle = GCHandle.Alloc(Array, GCHandleType.Pinned);
#if !NO_UNSAFE
            _Ptr = (char*)Handle.AddrOfPinnedObject();
#endif
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length in chars</param>
        public SecureCharArrayStruct(in long len) : this(new char[len]) { }

        /// <inheritdoc/>
        public readonly char this[int offset]
        {
            get => IfUndisposed(Array[offset]);
            set
            {
                EnsureUndisposed();
                Array[offset] = value;
            }
        }

        /// <inheritdoc/>
        public readonly Memory<char> this[Range range]
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
        public readonly Memory<char> this[Index start, Index end]
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
        public readonly int Length => IfUndisposed(Array.Length);

        /// <inheritdoc/>
        public readonly long LongLength => IfUndisposed(Array.LongLength);

        /// <inheritdoc/>
        public char[] Array { get; private set; }

        /// <inheritdoc/>
        public readonly Span<char> Span
        {
            get
            {
                EnsureUndisposed();
                return Array.AsSpan();
            }
        }

        /// <inheritdoc/>
        public readonly Memory<char> Memory
        {
            get
            {
                EnsureUndisposed();
                return Array.AsMemory();
            }
        }

#if !NO_UNSAFE
        /// <summary>
        /// Pointer
        /// </summary>
        public readonly char* Ptr
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
        public char[] DetachAndDispose()
        {
            EnsureUndisposed();
            Detached = true;
            char[] res = Array;
            Array = System.Array.Empty<char>();
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

        /// <summary>
        /// Ensure an undisposed object state
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly tValue IfUndisposed<tValue>(in tValue value)
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
        public static implicit operator char[](in SecureCharArrayStruct arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Span<char>(in SecureCharArrayStruct arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Memory<char>(in SecureCharArrayStruct arr) => arr.Memory;

#if !NO_UNSAFE
        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator char*(in SecureCharArrayStruct arr) => arr.Ptr;
#endif

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IntPtr(in SecureCharArrayStruct arr) => arr.IntPtr;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in SecureCharArrayStruct arr) => arr.Length;

        /// <summary>
        /// Cast as Int64 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator long(in SecureCharArrayStruct arr) => arr.LongLength;

        /// <summary>
        /// Cast as <see cref="SecureByteArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static implicit operator SecureByteArray(in SecureCharArrayStruct arr)
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
        public static implicit operator SecureByteArrayStruct(in SecureCharArrayStruct arr)
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
        public static explicit operator SecureCharArrayStruct(in char[] arr) => new(arr);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in SecureCharArrayStruct left, in SecureCharArrayStruct right) => left.Equals(right);

        /// <summary>
        /// Not equal
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in SecureCharArrayStruct left, in SecureCharArrayStruct right) => !left.Equals(right);
    }
}
