using System.Net.Sockets;

namespace wan24.Core
{
    // Disposing
    public partial class UdpFallback
    {
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (FallbackConnectionDisposer is AutoDisposer<NetworkStream> disposer) disposer.ShouldDispose = true;
            FallbackSync.Dispose();
            FallbackConnectionDisposer = null;
            FallbackConnection = null;
            UdpResponsePreListener.Clear();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            if (FallbackConnectionDisposer is AutoDisposer<NetworkStream> disposer) await disposer.SetShouldDisposeAsync().DynamicContext();
            await FallbackSync.DisposeAsync().DynamicContext();
            FallbackConnectionDisposer = null;
            FallbackConnection = null;
            UdpResponsePreListener.Clear();
        }
    }
}
