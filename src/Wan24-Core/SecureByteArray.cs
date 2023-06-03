namespace wan24.Core
{
    /// <summary>
    /// Secure byte array (will delete its contents when disposing)
    /// </summary>
#if NO_UNSAFE
    public sealed class SecureByteArray : SecureArrayBase<byte>
#else
    public sealed unsafe class SecureByteArray : SecureArrayBase<byte>
#endif
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="array">Array</param>
        public SecureByteArray(byte[] array) : base(array) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length in bytes</param>
        public SecureByteArray(long len) : this(new byte[len]) { }

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
        public static implicit operator byte[](SecureByteArray arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Span<byte>(SecureByteArray arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Memory<byte>(SecureByteArray arr) => arr.Memory;
#if !NO_UNSAFE
        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator byte*(SecureByteArray arr) => arr.Ptr;
#endif

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator IntPtr(SecureByteArray arr) => arr.IntPtr;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator int(SecureByteArray arr) => arr.Length;

        /// <summary>
        /// Cast as Int64 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator long(SecureByteArray arr) => arr.LongLength;

        /// <summary>
        /// Cast as <see cref="SecureCharArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator SecureCharArray(SecureByteArray arr) => new(arr.Span.ToUtf8Chars());

        /// <summary>
        /// Cast as <see cref="SecureCharArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator SecureCharArrayStruct(SecureByteArray arr) => new(arr.Span.ToUtf8Chars());

        /// <summary>
        /// Cast a byte array as secure byte array
        /// </summary>
        /// <param name="arr">Byte array</param>
        public static explicit operator SecureByteArray(byte[] arr) => new(arr);
    }
}
