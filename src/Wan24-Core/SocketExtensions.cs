using System.Net.Sockets;

namespace wan24.Core
{
    /// <summary>
    /// Socket extensions
    /// </summary>
    public static class SocketExtensions
    {
        /// <summary>
        /// One byte waste buffer (may be written from multiple threads, but never red)
        /// </summary>
        private static readonly RentedArrayStruct<byte> OneByteWasteBuffer = new(len: 1);

        /// <summary>
        /// Determine if a socket is connected (may return <see langword="false"/>, if even if connected - not 100% safe...)
        /// </summary>
        /// <param name="socket">Socket</param>
        /// <returns>Is connected?</returns>
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return socket.Connected && !(socket.Poll(microSeconds: 1, SelectMode.SelectRead) && socket.Receive(OneByteWasteBuffer.Span, SocketFlags.Peek) == 0);
            }
            catch
            {
                return false;
            }
        }
    }
}
