using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Dummy disposable
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly record struct DummyDisposable : IDisposable
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static readonly DummyDisposable Instance = new();

        /// <inheritdoc/>
        void IDisposable.Dispose() { }
    }
}
