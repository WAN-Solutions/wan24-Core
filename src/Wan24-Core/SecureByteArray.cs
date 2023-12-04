using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Secure byte array (will delete its contents when disposing)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="array">Array</param>
#if NO_UNSAFE
    public sealed class SecureByteArray : SecureArrayBase<byte>
#else
    public sealed unsafe class SecureByteArray(in byte[] array) : SecureArrayBase<byte>(array)
#endif
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length in bytes</param>
        public SecureByteArray(in long len) : this(new byte[len]) { }

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
        /// Cast as byte array
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator byte[](in SecureByteArray arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Span<byte>(in SecureByteArray arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Memory<byte>(in SecureByteArray arr) => arr.Memory;

#if !NO_UNSAFE
        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator byte*(in SecureByteArray arr) => arr.Ptr;
#endif

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IntPtr(in SecureByteArray arr) => arr.IntPtr;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in SecureByteArray arr) => arr.Length;

        /// <summary>
        /// Cast as Int64 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator long(in SecureByteArray arr) => arr.LongLength;

        /// <summary>
        /// Cast as <see cref="SecureCharArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator SecureCharArray(in SecureByteArray arr) => new(arr.Span.ToUtf8Chars());

        /// <summary>
        /// Cast as <see cref="SecureCharArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator SecureCharArrayStruct(in SecureByteArray arr) => new(arr.Span.ToUtf8Chars());

        /// <summary>
        /// Cast a byte array as secure byte array
        /// </summary>
        /// <param name="arr">Byte array</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static explicit operator SecureByteArray(in byte[] arr) => new(arr);
    }
}
