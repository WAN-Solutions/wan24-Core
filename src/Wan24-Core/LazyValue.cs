using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Lazy value
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    public sealed class LazyValue<T>
    {
        /// <summary>
        /// Factory
        /// </summary>
        private readonly Func<T> Factory;
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        private readonly object SyncObject = new();
        /// <summary>
        /// Value
        /// </summary>
        private T? _Value = default;
        /// <summary>
        /// Has a value?
        /// </summary>
        private bool HasValue = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="factory">Factory</param>
        public LazyValue(in Func<T> factory) => Factory = factory;

        /// <summary>
        /// Value
        /// </summary>
        public T Value
        {
            get
            {
                lock (SyncObject)
                {
                    if (HasValue) return _Value!;
                    HasValue = true;
                    return _Value = Factory();
                }
            }
        }

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="lazyValue">Lazy value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator T(in LazyValue<T> lazyValue) => lazyValue.Value;

        /// <summary>
        /// Cast as has-value-flag
        /// </summary>
        /// <param name="lazyValue">Lazy value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator bool(in LazyValue<T> lazyValue) => lazyValue.HasValue;
    }
}
