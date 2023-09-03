using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Disposable thread-safe value
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    public class DisposableThreadSafeValue<T> : ThreadSafeValue<T> where T : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialValue">Initial value</param>
        public DisposableThreadSafeValue(in T? initialValue = default) : base(initialValue) { }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _Value?.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            if (_Value is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                _Value?.Dispose();
            }
        }

        /// <summary>
        /// Cast as new instance
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator DisposableThreadSafeValue<T>(in T? value) => new(value);
    }
}
