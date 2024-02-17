using System.Net;
using System.Net.Sockets;

namespace wan24.Core
{
    /// <summary>
    /// Port knocking helper
    /// </summary>
    public static class PortKnocking
    {
        /// <summary>
        /// Call a TCP port sequence by sending SYN packets
        /// </summary>
        /// <param name="target">Target IP address</param>
        /// <param name="timeout">Timeout between packets</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="ports">TCP port sequence</param>
        public static async Task CallTcpSynSequenceAsync(IPAddress target, TimeSpan timeout, CancellationToken cancellationToken = default, params int[] ports)
        {
            if (ports.Length < 1 || ports.Any(p => p < 1 || p > ushort.MaxValue)) throw new ArgumentOutOfRangeException(nameof(ports));
            using CancellationTokenSource cts = new();
            DateTime started = DateTime.Now;
            TcpClient server = new();
            try
            {
                foreach (int port in ports)
                {
                    try
                    {
                        cts.TryReset();
                        if (timeout != TimeSpan.Zero)
                        {
                            using BoundCancellationTokenSource bcts = new(cancellationToken, cts.Token);
                            cts.CancelAfter(timeout);
                            started = DateTime.Now;
                            // Can't use raw sockets to send a SYN packet here 'cause of possible restrictions using raw sockets
                            await server.ConnectAsync(new(target, port), bcts.Token).DynamicContext();
                        }
                        else
                        {
                            // Can't use raw sockets to send a SYN packet here 'cause of possible restrictions using raw sockets
                            await server.ConnectAsync(new(target, port), cancellationToken).DynamicContext();
                        }
                        server.Dispose();
                        server = new();
                    }
                    catch (OperationCanceledException)
                    {
                        if (timeout == TimeSpan.Zero) throw;
                        cancellationToken.ThrowIfCancellationRequested();
                        continue;
                    }
                    catch
                    {
                    }
                    if (timeout != TimeSpan.Zero)
                    {
                        DateTime end = started + timeout;
                        if (end > DateTime.Now) await Task.Delay(end - DateTime.Now, cancellationToken).DynamicContext();
                    }
                }
            }
            finally
            {
                server.Dispose();
            }
        }

        /// <summary>
        /// Call an UDP port sequence by sending empty packets
        /// </summary>
        /// <param name="target">Target IP address</param>
        /// <param name="timeout">Timeout between packets</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="ports">UDP port sequence</param>
        public static async Task CallUdpEmptySequenceAsync(IPAddress target, TimeSpan timeout, CancellationToken cancellationToken = default, params int[] ports)
        {
            if (ports.Length < 1 || ports.Any(p => p < 1 || p > ushort.MaxValue)) throw new ArgumentOutOfRangeException(nameof(ports));
            ReadOnlyMemory<byte> empty = Array.Empty<byte>();
            using UdpClient server = new(target.AddressFamily);
            foreach (int port in ports)
            {
                await server.SendAsync(empty, new(target, port), cancellationToken).DynamicContext();
                if (timeout != TimeSpan.Zero) await Task.Delay(timeout, cancellationToken).DynamicContext();
            }
        }
    }
}
