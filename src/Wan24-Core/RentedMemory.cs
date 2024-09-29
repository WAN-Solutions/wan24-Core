using System.Buffers;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Pool rented memory (returns the memory to the pool, when disposed)
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public record struct RentedMemory<T> : IMemoryOwner<T>
    {
        /// <summary>
        /// Rented memory ownership
        /// </summary>
        private readonly IMemoryOwner<T> MemoryOwner;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length</param>
        /// <param name="pool">Pool</param>
        /// <param name="clean">Clean the rented array?</param>
        public RentedMemory(in int len, in MemoryPool<T>? pool = null, in bool clean = true)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(len, 1);
            MemoryOwner = (pool ?? MemoryPool<T>.Shared).Rent(len);
            Memory = MemoryOwner.Memory.Length == len ? MemoryOwner.Memory : MemoryOwner.Memory[..len];
            if (clean) Memory.Span.Clear();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Length</param>
        /// <param name="span">Span</param>
        /// <param name="pool">Pool</param>
        /// <param name="clean">Clean the rented array?</param>
        public RentedMemory(in int len, out Span<T> span, in MemoryPool<T>? pool = null, in bool clean = true)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(len, 1);
            MemoryOwner = (pool ?? MemoryPool<T>.Shared).Rent(len);
            Memory = MemoryOwner.Memory.Length == len ? MemoryOwner.Memory : MemoryOwner.Memory[..len];
            span = Memory.Span;
            if (clean) span.Clear();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner">Ownership</param>
        /// <param name="len">Length</param>
        /// <param name="clean">Clean the rented array?</param>
        public RentedMemory(in IMemoryOwner<T> owner, in int len = 0, in bool clean = false)
        {
            MemoryOwner = owner;
            Memory = MemoryOwner.Memory.Length == len ? MemoryOwner.Memory : MemoryOwner.Memory[..len];
            if (clean) Memory.Span.Clear();
        }

        /// <summary>
        /// Memory
        /// </summary>
        public Memory<T> Memory { readonly get; private set; }

        /// <summary>
        /// Memory as read-only
        /// </summary>
        public readonly ReadOnlyMemory<T> ReadOnlyMemory => Memory;

        /// <summary>
        /// If to clear the <see cref="Memory"/> when disposing
        /// </summary>
        public bool Clear { get; set; } = Settings.ClearBuffers;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Clear)
                if (Memory is Memory<byte> bytes)
                {
                    bytes.Span.Clean();
                }
                else
                {
                    Memory.Span.Clear();
                }
            Memory = Array.Empty<T>();
            MemoryOwner.Dispose();
        }

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="mem">Memory</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Span<T>(in RentedMemory<T> mem) => mem.Memory.Span;

        /// <summary>
        /// Cast as span
        /// </summary>
        /// <param name="mem">Memory</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ReadOnlySpan<T>(in RentedMemory<T> mem) => mem.Memory.Span;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="mem">Memory</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Memory<T>(in RentedMemory<T> mem) => mem.Memory;

        /// <summary>
        /// Cast as memory
        /// </summary>
        /// <param name="mem">Memory</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ReadOnlyMemory<T>(in RentedMemory<T> mem) => mem.Memory;

        /// <summary>
        /// Cast as Int32 (length value)
        /// </summary>
        /// <param name="mem">Memory</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator int(in RentedMemory<T> mem) => mem.Memory.Length;

        /// <summary>
        /// Cast from Int32 (length value)
        /// </summary>
        /// <param name="len">Length in bytes</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator RentedMemory<T>(in int len) => new(len);
    }
}
