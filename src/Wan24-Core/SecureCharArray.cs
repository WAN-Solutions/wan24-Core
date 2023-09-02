﻿using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace wan24.Core
{
    /// <summary>
    /// Secure char array (will delete its contents when disposing)
    /// </summary>
#if NO_UNSAFE
    public sealed class SecureCharArray : SecureArrayBase<char>
#else
    public sealed unsafe class SecureCharArray : SecureArrayBase<char>
#endif
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="array">Array</param>
        public SecureCharArray(in char[] array) : base(array) { }

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
        public static implicit operator char[](in SecureCharArray arr) => arr.Array;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Span<char>(in SecureCharArray arr) => arr.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator Memory<char>(in SecureCharArray arr) => arr.Memory;
#if !NO_UNSAFE
        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator char*(in SecureCharArray arr) => arr.Ptr;
#endif

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator IntPtr(in SecureCharArray arr) => arr.IntPtr;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator int(in SecureCharArray arr) => arr.Length;

        /// <summary>
        /// Cast as Int64 (length value)
        /// </summary>
        /// <param name="arr">Array</param>
        public static implicit operator long(in SecureCharArray arr) => arr.LongLength;

        /// <summary>
        /// Cast as <see cref="SecureByteArray"/> (using UTF-8 encoding)
        /// </summary>
        /// <param name="arr">Array</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static implicit operator SecureByteArray(in SecureCharArray arr)
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
        public static implicit operator SecureByteArrayStruct(in SecureCharArray arr)
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
        public static explicit operator SecureCharArray(in char[] arr) => new(arr);
    }
}
