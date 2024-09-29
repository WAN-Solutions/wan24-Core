using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace wan24.Core
{
    /// <summary>
    /// Secure char array (will delete its contents when disposing)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="array">Array</param>
#if NO_UNSAFE
    public sealed class SecureCharArray : SecureArrayBase<char>
#else
    public sealed unsafe class SecureCharArray(in char[] array) : SecureArrayBase<char>(array)
#endif
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length in chars</param>
        public SecureCharArray(in long len) : this(new char[len]) { }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (Detached || _Array.Length < 1) return;
                _Array.Clear();
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
        public static implicit operator char[](in SecureCharArray arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Span<char>(in SecureCharArray arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Memory<char>(in SecureCharArray arr) => arr.Memory;

#if !NO_UNSAFE
        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator char*(in SecureCharArray arr) => arr.Ptr;
#endif

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IntPtr(in SecureCharArray arr) => arr.IntPtr;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in SecureCharArray arr) => arr.Length;

        /// <summary>
        /// Cast as Int64 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator long(in SecureCharArray arr) => arr.LongLength;

        /// <summary>
        /// Cast as <see cref="SecureByteArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static implicit operator SecureByteArray(in SecureCharArray arr)
        {
            int len = arr.Length * 3;
#if !NO_UNSAFE
            if (len > Settings.StackAllocBorder)
            {
#endif
                using RentedMemoryRef<byte> buffer = new(len, clean: false);
                Span<byte> bufferSpan = buffer.Span;
                return new(bufferSpan[..Encoding.UTF8.GetBytes((ReadOnlySpan<char>)arr.Span, bufferSpan)].ToArray());
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
        public static implicit operator SecureByteArrayStruct(in SecureCharArray arr)
        {
            int len = arr.Length * 3;
#if !NO_UNSAFE
            if (len > Settings.StackAllocBorder)
            {
#endif
                using RentedMemoryRef<byte> buffer = new(len, clean: false);
                Span<byte> bufferSpan = buffer.Span;
                return new(bufferSpan[..Encoding.UTF8.GetBytes((ReadOnlySpan<char>)arr.Span, bufferSpan)].ToArray());
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
        [TargetedPatchingOptOut("Tiny method")]
        public static explicit operator SecureCharArray(in char[] arr) => new(arr);
    }
}
