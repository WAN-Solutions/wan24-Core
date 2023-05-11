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
        public static void Invoke(this IEnumerable<ChangeCallback> callbacks)
        {
            foreach (ChangeCallback callback in callbacks) callback.Invoke();
        }
    }
}
