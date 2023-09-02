using System.Collections;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

//TODO SecureCharArrayStructSimple
//TODO SecureCharArrayRefStruct

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
        /// Array
        /// </summary>
        private readonly char[] _Array;
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
            _Array = array;
            Handle = GCHandle.Alloc(_Array, GCHandleType.Pinned);
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
            get => IfUndisposed(_Array[offset]);
            set
            {
                EnsureUndisposed();
                _Array[offset] = value;
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
                    return _Array[range];
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
                    return _Array[new Range(start, end)];
                }
            }
        }

        /// <inheritdoc/>
        public readonly int Length => IfUndisposed(_Array.Length);

        /// <inheritdoc/>
        public readonly long LongLength => IfUndisposed(_Array.LongLength);

        /// <inheritdoc/>
        public readonly char[] Array => IfUndisposed(_Array);

        /// <inheritdoc/>
        public readonly Span<char> Span => Array;

        /// <inheritdoc/>
        public readonly Memory<char> Memory => Array;

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
            Dispose();
            return _Array;
        }

        /// <inheritdoc/>
        public readonly IEnumerator<char> GetEnumerator() => ((IEnumerable<char>)_Array).GetEnumerator();

        /// <inheritdoc/>
        readonly IEnumerator IEnumerable.GetEnumerator() => _Array.GetEnumerator();

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj) => _Array.Equals(obj);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public readonly bool Equals(Memory<char> other) => Memory.Equals(other);

        /// <inheritdoc/>
        public override readonly int GetHashCode() => _Array.GetHashCode();

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
                if (!Detached && _Array.Length > 0) _Array.Clear();
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
        public static implicit operator char[](in SecureCharArrayStruct arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Span<char>(in SecureCharArrayStruct arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Memory<char>(in SecureCharArrayStruct arr) => arr.Memory;
#if !NO_UNSAFE
        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator char*(in SecureCharArrayStruct arr) => arr.Ptr;
#endif

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator IntPtr(in SecureCharArrayStruct arr) => arr.IntPtr;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator int(in SecureCharArrayStruct arr) => arr.Length;

        /// <summary>
        /// Cast as Int64 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator long(in SecureCharArrayStruct arr) => arr.LongLength;

        /// <summary>
        /// Cast as <see cref="SecureByteArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static implicit operator SecureByteArray(in SecureCharArrayStruct arr)
        {
#if !NO_UNSAFE
            if (arr.Length << 1 > Settings.StackAllocBorder)
            {
#endif
                using RentedArrayRefStruct<byte> buffer = new(arr.Length << 1, clean: false);
                return new(buffer.Span[..Encoding.UTF8.GetBytes((ReadOnlySpan<char>)arr.Span, buffer.Span)].ToArray());
#if !NO_UNSAFE
            }
            else
            {
                Span<byte> buffer = stackalloc byte[arr.Length << 1];
                return new(buffer[..Encoding.UTF8.GetBytes((ReadOnlySpan<char>)arr.Span, buffer)].ToArray());
            }
#endif
        }

        /// <summary>
        /// Cast as <see cref="SecureByteArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static implicit operator SecureByteArrayStruct(in SecureCharArrayStruct arr)
        {
#if !NO_UNSAFE
            if (arr.Length << 1 > Settings.StackAllocBorder)
            {
#endif
                using RentedArrayRefStruct<byte> buffer = new(arr.Length << 1, clean: false);
                return new(buffer.Span[..Encoding.UTF8.GetBytes((ReadOnlySpan<char>)arr.Span, buffer.Span)].ToArray());
#if !NO_UNSAFE
            }
            else
            {
                Span<byte> buffer = stackalloc byte[arr.Length << 1];
                return new(buffer[..Encoding.UTF8.GetBytes((ReadOnlySpan<char>)arr.Span, buffer)].ToArray());
            }
#endif
        }

        /// <summary>
        /// Cast a char array as secure char array
        /// </summary>
        /// <param name="arr">Char array</param>
        public static explicit operator SecureCharArrayStruct(in char[] arr) => new(arr);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals?</returns>
        public static bool operator ==(in SecureCharArrayStruct left, in SecureCharArrayStruct right) => left.Equals(right);

        /// <summary>
        /// Not equal
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equal?</returns>
        public static bool operator !=(in SecureCharArrayStruct left, in SecureCharArrayStruct right) => !left.Equals(right);
    }
}
