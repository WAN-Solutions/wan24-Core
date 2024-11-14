using System.Buffers;
using System.Runtime;

namespace wan24.Core
{
    // Static
    public partial class ArrayPoolStream
    {
        /// <summary>
        /// Default buffer size in bytes
        /// </summary>
        private static int _DefaultBufferSize = Settings.BufferSize;

        /// <summary>
        /// An object for static thread locking
        /// </summary>
        protected static readonly object StaticSyncObject = new();

        /// <summary>
        /// Default buffer size in bytes
        /// </summary>
        public static int DefaultBufferSize
        {
            get => _DefaultBufferSize;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
                lock (StaticSyncObject) _DefaultBufferSize = value;
            }
        }

        /// <summary>
        /// Cast as new byte array
        /// </summary>
        /// <param name="ms"><see cref="ArrayPoolStream"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator byte[](in ArrayPoolStream ms) => ms.ToArray();

        /// <summary>
        /// Cast as sequence
        /// </summary>
        /// <param name="ms"><see cref="ArrayPoolStream"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator ReadOnlySequence<byte>(in ArrayPoolStream ms) => ms.ToReadOnlySequence();

        /// <summary>
        /// Cast as length
        /// </summary>
        /// <param name="ms"><see cref="ArrayPoolStream"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator long(in ArrayPoolStream ms) => ms.Length;
    }
}
