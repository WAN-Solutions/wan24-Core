using System.Collections.Frozen;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using static wan24.Core.Logging;
using static wan24.Core.Logger;

namespace wan24.Core
{
    /// <summary>
    /// Port knocking helper
    /// </summary>
    public static class PortKnocking
    {
        /// <summary>
        /// Allowed WebSocket URI schemes
        /// </summary>
        private static readonly FrozenSet<string> WebSocketSchemes = new string[] { "wss", "ws" }.ToFrozenSet();
        /// <summary>
        /// Allowed http(s) URI schemes
        /// </summary>
        private static readonly FrozenSet<string> HttpSchemes = new string[] { "https", "http" }.ToFrozenSet();

        /// <summary>
        /// Call a TCP port sequence by sending SYN packets
        /// </summary>
        /// <param name="target">Target IP address</param>
        /// <param name="delay">Delay between packets</param>
        /// <param name="progress">Progress (updated after each port was contacted)</param>
        /// <param name="serviceProvider">Service provider to use for getting the <see cref="TcpClient"/> instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="ports">TCP port sequence</param>
        public static async Task CallTcpSynSequenceAsync(
            IPAddress target,
            TimeSpan delay = default,
            ProcessingProgress? progress = null,
            IAsyncServiceProvider? serviceProvider = null,
            CancellationToken cancellationToken = default,
            params int[] ports
            )
        {
            EnsureValidIpAddress(target);
            EnsureValidPorts(ports);
            cancellationToken.ThrowIfCancellationRequested();
            if (delay <= TimeSpan.Zero) delay = TimeSpan.FromMilliseconds(20);
            using CancellationTokenSource processing = new();
            using CancellationTokenSource cancellation = cancellationToken.CombineWith(processing.Token);
            TimeSpan waitTime;
            DateTime continueAt = DateTime.Now;
            if (Trace) WriteTrace($"TCP port knocking to {target}:{string.Join('/', ports.Select(p => p.ToString()))}");
            TcpClient client = await serviceProvider!.GetServiceAsync(typeof(TcpClient), cancellationToken).DynamicContext() as TcpClient ?? new();
            try
            {
                foreach (int port in ports)
                    try
                    {
                        continueAt += delay;
                        cancellation.TryReset();
                        processing.TryReset();
                        processing.CancelAfter(delay);
                        if (Trace) WriteTrace($"Call TCP {target}:{port}");
                        try
                        {
                            // Can't use raw sockets here 'cause of possible OS restrictions
                            await client.ConnectAsync(new(target, port), cancellation.Token).DynamicContext();
                        }
                        catch (OperationCanceledException ex)
                        {
                            // Throw, if canceled - otherwise continue the port knocking sequence
                            if (!ex.CancellationToken.IsEqualTo(cancellation.Token)) throw;
                            continue;
                        }
                        catch (Exception ex)
                        {
                            if (Debug) WriteDebug($"Exception when knocking TCP {target}:{port}: ({ex.GetType().Name}) {ex.Message}");
                        }
                        // Usually a firewall will swallow the SYN packet silently and the connect operation fails with timeout - this only in case if not:
                        client.Dispose();
                        client = await serviceProvider!.GetServiceAsync(typeof(TcpClient), cancellationToken).DynamicContext() as TcpClient ?? new();
                        waitTime = continueAt - DateTime.Now;
                        if (waitTime > TimeSpan.Zero) await Task.Delay(waitTime, cancellationToken).DynamicContext();
                    }
                    finally
                    {
                        progress?.Update();
                    }
            }
            finally
            {
                client.Dispose();
            }
            if (Trace) WriteTrace($"TCP port knocking to {target} done");
        }

        /// <summary>
        /// Call a WebSocket URI sequence
        /// </summary>
        /// <param name="delay">Delay between connection attempts</param>
        /// <param name="clientFactory">WebSocket client factory</param>
        /// <param name="progress">Progress (updated after each WebSocket URI was contacted)</param>
        /// <param name="serviceProvider">Service provider to use for getting the <see cref="ClientWebSocket"/> instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="uris">WebSocket URI sequence</param>
        public static async Task CallWebSocketSequenceAsync(
            TimeSpan delay = default,
            ClientWebSocketFactory_Delegate? clientFactory = null,
            ProcessingProgress? progress = null,
            IAsyncServiceProvider? serviceProvider = null,
            CancellationToken cancellationToken = default,
            params Uri[] uris
            )
        {
            EnsureValidUris(uris, WebSocketSchemes);
            cancellationToken.ThrowIfCancellationRequested();
            if (delay <= TimeSpan.Zero) delay = TimeSpan.FromMilliseconds(20);
            using CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            TimeSpan waitTime;
            DateTime continueAt = DateTime.Now;
            ClientWebSocket? client = null;
            if (Trace) WriteTrace($"WebSocket knocking to {uris.Length} URIs");
            try
            {
                foreach (Uri uri in uris)
                    try
                    {
                        client ??= clientFactory?.Invoke(uri) ?? await serviceProvider!.GetServiceAsync(typeof(ClientWebSocket), cancellationToken).DynamicContext() as ClientWebSocket ?? new();
                        continueAt += delay;
                        cancellation.TryReset();
                        cancellation.CancelAfter(delay);
                        if (Trace) WriteTrace($"Call WebSocket \"{uri}\"");
                        try
                        {
                            await client.ConnectAsync(uri, cancellation.Token).DynamicContext();
                        }
                        catch (OperationCanceledException ex)
                        {
                            // Throw, if canceled - otherwise continue the port knocking sequence
                            if (!ex.CancellationToken.IsEqualTo(cancellation.Token)) throw;
                            continue;
                        }
                        catch (Exception ex)
                        {
                            if (Debug) WriteDebug($"Exception when knocking WebSocket \"{uri}\": ({ex.GetType().Name}) {ex.Message}");
                        }
                        // Usually a firewall will swallow the connection attempts silently and the connect operation fails with timeout - this only in case if not:
                        client.Dispose();
                        client = clientFactory?.Invoke(uri) ?? await serviceProvider!.GetServiceAsync(typeof(ClientWebSocket), cancellationToken).DynamicContext() as ClientWebSocket ?? new();
                        waitTime = continueAt - DateTime.Now;
                        if (waitTime > TimeSpan.Zero) await Task.Delay(waitTime, cancellationToken).DynamicContext();
                    }
                    finally
                    {
                        progress?.Update();
                    }
            }
            finally
            {
                client?.Dispose();
            }
            if (Trace) WriteTrace($"WebSocket knocking to {uris.Length} URIs done");
        }

        /// <summary>
        /// Call a http(s) URI sequence
        /// </summary>
        /// <param name="client">http client to use (won't be disposed)</param>
        /// <param name="requestFactory">http request message factory</param>
        /// <param name="delay">Delay between connection attempts</param>
        /// <param name="progress">Progress (updated after each http(s) URI was contacted)</param>
        /// <param name="serviceProvider">Service provider to use for getting the <see cref="HttpClient"/> instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="uris">http(s) URI sequence</param>
        public static async Task CallHttpSequenceAsync(
            HttpClient? client = null,
            RequestMessageFactory_Delegate? requestFactory = null,
            TimeSpan delay = default,
            ProcessingProgress? progress = null,
            IAsyncServiceProvider? serviceProvider = null,
            CancellationToken cancellationToken = default,
            params Uri[] uris
            )
        {
            EnsureValidUris(uris, HttpSchemes);
            cancellationToken.ThrowIfCancellationRequested();
            if (delay <= TimeSpan.Zero) delay = TimeSpan.FromMilliseconds(20);
            using CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using OptionalDisposer clientDisposer = new(client, client is null);
            if (client is null) serviceProvider ??= DiHelper.Instance;
            TimeSpan waitTime;
            DateTime continueAt = DateTime.Now;
            if (Trace) WriteTrace($"http knocking to {uris.Length} URIs");
            client ??= await serviceProvider!.GetServiceAsync(typeof(HttpClient), cancellationToken).DynamicContext() as HttpClient ?? new();
            foreach (Uri uri in uris)
                try
                {
                    continueAt += delay;
                    cancellation.TryReset();
                    cancellation.CancelAfter(delay);
                    if (Trace) WriteTrace($"Call URI \"{uri}\"");
                    try
                    {
                        using HttpRequestMessage request = requestFactory?.Invoke(uri) ?? new(HttpMethod.Get, uri);
                        using HttpResponseMessage response = await client.SendAsync(request, cancellation.Token).DynamicContext();
                    }
                    catch (OperationCanceledException ex)
                    {
                        // Throw, if canceled - otherwise continue the port knocking sequence
                        if (!ex.CancellationToken.IsEqualTo(cancellation.Token)) throw;
                        continue;
                    }
                    catch (Exception ex)
                    {
                        if (Debug) WriteDebug($"Exception when knocking URI \"{uri}\": ({ex.GetType().Name}) {ex.Message}");
                    }
                    // Usually a firewall will swallow the requests silently and the send operation fails with timeout - this only in case if not:
                    waitTime = continueAt - DateTime.Now;
                    if (waitTime > TimeSpan.Zero) await Task.Delay(waitTime, cancellationToken).DynamicContext();
                }
                finally
                {
                    progress?.Update();
                }
            if (Trace) WriteTrace($"http knocking to {uris.Length} URIs done");
        }

        /// <summary>
        /// Call an UDP port sequence by sending empty packets
        /// </summary>
        /// <param name="target">Target IP address</param>
        /// <param name="client">UDP client to use (won't be disposed)</param>
        /// <param name="delay">Delay between packets</param>
        /// <param name="payload">Payload to send with an UDP packet (empty per default)</param>
        /// <param name="progress">Progress (updated after each port was contacted)</param>
        /// <param name="serviceProvider">Service provider to use for getting the <see cref="UdpClient"/> instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="ports">UDP port sequence</param>
        public static async Task CallUdpSequenceAsync(
            IPAddress target,
            UdpClient? client = null,
            TimeSpan delay = default,
            ReadOnlyMemory<byte>? payload = null,
            ProcessingProgress? progress = null,
            IAsyncServiceProvider? serviceProvider = null,
            CancellationToken cancellationToken = default,
            params int[] ports
            )
        {
            EnsureValidIpAddress(target);
            EnsureValidPorts(ports);
            cancellationToken.ThrowIfCancellationRequested();
            payload ??= Array.Empty<byte>();
            using OptionalDisposer clientDisposer = new(client, client is null);
            if (client is null)
            {
                serviceProvider ??= DiHelper.Instance;
                client = await serviceProvider!.GetServiceAsync(typeof(UdpClient), cancellationToken).DynamicContext() as UdpClient ?? new(target.AddressFamily);
            }
            if (Trace) WriteTrace($"UDP port knocking to {target}:{string.Join('/', ports.Select(p => p.ToString()))}");
            foreach (int port in ports)
            {
                if (Trace) WriteTrace($"Call UDP {target}:{port}");
                await client!.SendAsync(payload.Value, new(target, port), cancellationToken).DynamicContext();
                if (delay > TimeSpan.Zero) await Task.Delay(delay, cancellationToken).DynamicContext();
                progress?.Update();
            }
            if (Trace) WriteTrace($"UDP port knocking to {target} done");
        }

        /// <summary>
        /// Ensure a valid target IP address
        /// </summary>
        /// <param name="target">IP address</param>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static void EnsureValidIpAddress(in IPAddress target)
        {
            if (!target.AddressFamily.In(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6)) throw new ArgumentException("Invalid IP address", nameof(target));
        }

        /// <summary>
        /// Ensure valid IP ports
        /// </summary>
        /// <param name="ports">Ports</param>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static void EnsureValidPorts(in int[] ports)
        {
            if (ports.Length < 1 || ports.Any(p => p < 1 || p > ushort.MaxValue)) throw new ArgumentOutOfRangeException(nameof(ports));
        }

        /// <summary>
        /// Ensure valid URIs
        /// </summary>
        /// <param name="uris">URIs</param>
        /// <param name="allowedSchemes">Allowed URI schemes</param>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static void EnsureValidUris(in Uri[] uris, FrozenSet<string> allowedSchemes)
        {
            if (uris.Length < 1 || uris.Any(u => !u.Scheme.In(allowedSchemes)))
                throw new ArgumentException($"Require at last one and only {string.Join('/', allowedSchemes)} URI(s)", nameof(uris));
        }

        /// <summary>
        /// Delegate for a WebSocket client factory
        /// </summary>
        /// <param name="uri">Current URI</param>
        /// <returns>WebSocket client (will be disposed!)</returns>
        public delegate ClientWebSocket ClientWebSocketFactory_Delegate(Uri uri);

        /// <summary>
        /// Delegate for a http request message factory
        /// </summary>
        /// <param name="uri">Current URI</param>
        /// <returns>http request message (will be disposed!)</returns>
        public delegate HttpRequestMessage RequestMessageFactory_Delegate(Uri uri);
    }
}
