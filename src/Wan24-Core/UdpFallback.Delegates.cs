using System.Net;
using System.Net.Sockets;

namespace wan24.Core
{
    // Delegates
    public partial class UdpFallback
    {
        /// <summary>
        /// Delegate for a fallback connector
        /// </summary>
        /// <param name="fallback">Fallback</param>
        /// <param name="target">UDP target endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Network stream (should be non-blocking and will be disposed)</returns>
        public delegate Task<NetworkStream> FallbackConnector_Delegate(UdpFallback fallback, IPEndPoint? target, CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for an UDP response listener
        /// </summary>
        /// <param name="data">Received data</param>
        /// <param name="isFromFallback">If the received data comes from the fallback connection</param>
        /// <returns>If the received data was used (when trying UDP, <see langword="false"/> forwards the packet to other UDP listeners; when using the fallback, <see langword="false"/> will 
        /// require to receive more data first, before the listener is being called again with more received data)</returns>
        public delegate bool UdpResponseListener_Delegate(ReadOnlyMemory<byte> data, bool isFromFallback);
    }
}
