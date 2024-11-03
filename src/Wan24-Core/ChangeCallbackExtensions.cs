using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Change callback extensions
    /// </summary>
    public static class ChangeCallbackExtensions
    {
        /// <summary>
        /// Invoke all callbacks
        /// </summary>
        /// <param name="callbacks">Callbacks</param>
        /// <param name="state">State</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Invoke(this IEnumerable<ChangeCallback> callbacks, in object? state = null)
        {
            foreach (ChangeCallback callback in callbacks) callback.Invoke(state);
        }

        /// <summary>
        /// Invoke all callbacks
        /// </summary>
        /// <param name="callbacks">Callbacks</param>
        /// <param name="state">State</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Invoke(this ReadOnlySpan<ChangeCallback> callbacks, in object? state = null)
        {
            for (int i = 0, len = callbacks.Length; i < len; callbacks[i].Invoke(state), i++) ;
        }

        /// <summary>
        /// Invoke all callbacks
        /// </summary>
        /// <param name="callbacks">Callbacks</param>
        /// <param name="state">State</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Invoke(this ChangeCallback[] callbacks, in object? state = null)
        {
            for (int i = 0, len = callbacks.Length; i < len; callbacks[i].Invoke(state), i++) ;
        }

        /// <summary>
        /// Invoke all callbacks
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="callbacks">Callbacks</param>
        /// <param name="state">State</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Invoke<T>(this T callbacks, in object? state = null) where T : IList<ChangeCallback>
        {
            for (int i = 0, len = callbacks.Count; i < len; callbacks[i].Invoke(state), i++) ;
        }

        /// <summary>
        /// Invoke all callbacks
        /// </summary>
        /// <param name="callbacks">Callbacks</param>
        /// <param name="state">State</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Invoke(this ImmutableArray<ChangeCallback> callbacks, in object? state = null)
        {
            for (int i = 0, len = callbacks.Length; i < len; callbacks[i].Invoke(state), i++) ;
        }

        /// <summary>
        /// Invoke all callbacks
        /// </summary>
        /// <param name="callbacks">Callbacks</param>
        /// <param name="state">State</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Invoke(this FrozenSet<ChangeCallback> callbacks, in object? state = null) => Invoke(callbacks.Items, state);

        /// <summary>
        /// Invoke all callbacks
        /// </summary>
        /// <param name="callbacks">Callbacks</param>
        /// <param name="state">State</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Invoke(this Span<ChangeCallback> callbacks, in object? state = null) => Invoke((ReadOnlySpan<ChangeCallback>)callbacks, state);

        /// <summary>
        /// Invoke all callbacks
        /// </summary>
        /// <param name="callbacks">Callbacks</param>
        /// <param name="state">State</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Invoke(this Memory<ChangeCallback> callbacks, in object? state = null) => Invoke((ReadOnlySpan<ChangeCallback>)callbacks.Span, state);

        /// <summary>
        /// Invoke all callbacks
        /// </summary>
        /// <param name="callbacks">Callbacks</param>
        /// <param name="state">State</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Invoke(this ReadOnlyMemory<ChangeCallback> callbacks, in object? state = null) => Invoke(callbacks.Span, state);
    }
}
