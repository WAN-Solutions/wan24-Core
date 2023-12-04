using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Disposable thread-safe value
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="initialValue">Initial value</param>
    public class DisposableThreadSafeValue<T>(in T? initialValue = default) : ThreadSafeValue<T>(initialValue) where T : IDisposable
    {
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
