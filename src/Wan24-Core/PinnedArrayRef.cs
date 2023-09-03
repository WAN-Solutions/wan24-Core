#if !NO_UNSAFE
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Pinned array (not thread-safe)
    /// </summary>
    /// <typeparam name="T">Pointer type</typeparam>
    public unsafe readonly ref struct PinnedArrayRef<T> where T : struct
    {
        /// <summary>
        /// Array pin
        /// </summary>
        private readonly GCHandle ArrayPin;
        /// <summary>
        /// Array
        /// </summary>
        public readonly T[] Array;
        /// <summary>
        /// Array pointer
        /// </summary>
#pragma warning disable CS8500 // Takes the address of, gets the size of, or declares a pointer to a managed type
        public readonly T* ArrayPtr;
#pragma warning restore CS8500 // Takes the address of, gets the size of, or declares a pointer to a managed type

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="array">Array</param>
        public PinnedArrayRef(in T[] array)
        {
            Array = array;
            ArrayPin = GCHandle.Alloc(array, GCHandleType.Pinned);
#pragma warning disable CS8500 // Takes the address of, gets the size of, or declares a pointer to a managed type
            ArrayPtr = (T*)ArrayPin.AddrOfPinnedObject();
#pragma warning restore CS8500 // Takes the address of, gets the size of, or declares a pointer to a managed type
        }

        /// <summary>
        /// Get/set an array element
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns>Element</returns>
        public T this[int offset]
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => ArrayPtr[offset];
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set => ArrayPtr[offset] = value;
        }

        /// <summary>
        /// Is disposed?
        /// </summary>
        public bool IsDisposed
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => !ArrayPin.IsAllocated;
        }

        /// <summary>
        /// Array pointer
        /// </summary>
        public IntPtr Pointer
        {
            [TargetedPatchingOptOut("Just a method adapter")]
            get => ArrayPin.AddrOfPinnedObject();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (ArrayPin.IsAllocated) ArrayPin.Free();
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override int GetHashCode() => Array.GetHashCode();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override bool Equals([NotNullWhen(true)] object? obj) => Array.Equals(obj);

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="pin">Pin</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#pragma warning disable CS8500 // Takes the address of, gets the size of, or declares a pointer to a managed type
        public static implicit operator T*(in PinnedArrayRef<T> pin) => pin.ArrayPtr;
#pragma warning restore CS8500 // Takes the address of, gets the size of, or declares a pointer to a managed type

        /// <summary>
        /// Cast as pointer
        /// </summary>
        /// <param name="pin">Pin</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IntPtr(in PinnedArrayRef<T> pin) => pin.Pointer;

        /// <summary>
        /// Cast as array
        /// </summary>
        /// <param name="pin">Pin</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator T[](in PinnedArrayRef<T> pin) => pin.Array;

        /// <summary>
        /// Cast as length
        /// </summary>
        /// <param name="pin">Pin</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in PinnedArrayRef<T> pin) => pin.Array.Length;

        /// <summary>
        /// Cast as length
        /// </summary>
        /// <param name="pin">Pin</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator long(in PinnedArrayRef<T> pin) => pin.Array.LongLength;

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Are equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in PinnedArrayRef<T> a, in PinnedArrayRef<T> b) => a.Array.Equals(b.Array);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Are not equal?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in PinnedArrayRef<T> a, in PinnedArrayRef<T> b) => !(a == b);
    }
}
#endif
