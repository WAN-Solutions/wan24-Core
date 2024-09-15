using System.Net.Sockets;

namespace wan24.Core
{
    // Properties
    public partial class UdpFallback
    {
        /// <summary>
        /// UDP client (will be disposed)
        /// </summary>
        public required virtual UdpClient UdpClient { get; init; }

        /// <summary>
        /// Fallback connector (the returned network stream should be non-blocking and will be disposed)
        /// </summary>
        public required virtual FallbackConnector_Delegate FallbackConnector { get; init; }

        /// <summary>
        /// If to keep the fallback connection alive, once established (if <see langword="false"/>, a new fallback connection will be created for each fallback case)
        /// </summary>
        public bool KeepFallbackAlive { get; init; }

        /// <summary>
        /// If to allow an UDP comeback after a persistent fallback connection was created
        /// </summary>
        public bool AllowUdpComeback { get; init; }

        /// <summary>
        /// Permanent fallback connection (will be disposed)
        /// </summary>
        public virtual NetworkStream? FallbackConnection { get; protected set; }

        /// <summary>
        /// Response waiting timeout
        /// </summary>
        public required TimeSpan ResponseTimeout { get; init; }

        /// <summary>
        /// If to clear used buffers
        /// </summary>
        public bool ClearBuffers { get; init; }

        /// <inheritdoc/>
        public override bool CanPause
        {
            get => true;
            protected set => throw new NotSupportedException();
        }
    }
}
