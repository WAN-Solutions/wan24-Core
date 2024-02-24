using System.Runtime;

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
        public static void Invoke(this IEnumerable<ChangeCallback> callbacks, in object? state = null)
        {
            foreach (ChangeCallback callback in callbacks) callback.Invoke(state);
        }
    }
}
