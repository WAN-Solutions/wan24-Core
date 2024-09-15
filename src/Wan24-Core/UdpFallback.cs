using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;

namespace wan24.Core
{
    /// <summary>
    /// UDP fallback (uses a fallback connection, if UDP doesn't get a response within a timeout)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="preUdpResponseListener">UDP response pre-listener</param>
    /// <param name="postUdpResponseListener">UDP response post-listener</param>
    public partial class UdpFallback(
        in IEnumerable<UdpFallback.UdpResponseListener_Delegate>? preUdpResponseListener = null,
        in ImmutableArray<UdpFallback.UdpResponseListener_Delegate>? postUdpResponseListener = null
        )
        : HostedServiceBase()
    {
        /// <summary>
        /// Thread synchronization for setting up a persistent fallback connection
        /// </summary>
        protected readonly SemaphoreSync FallbackSync = new();
        /// <summary>
        /// UDP response pre-listener
        /// </summary>
        protected readonly ConcurrentList<UdpResponseListener_Delegate> UdpResponsePreListener = preUdpResponseListener is null
            ? []
            : [.. preUdpResponseListener];
        /// <summary>
        /// UDP response post-listener
        /// </summary>
        protected readonly ImmutableArray<UdpResponseListener_Delegate> UdpResponsePostListener = postUdpResponseListener ?? [];
        /// <summary>
        /// Fallback connection disposer
        /// </summary>
        protected AutoDisposer<NetworkStream>? FallbackConnectionDisposer = null;

        /// <summary>
        /// Send an UDP packet and wait for the response (use a fallback connection, on response timeout)
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <param name="response">Buffer for receiving the response</param>
        /// <param name="responseVerifier">UDP response verifier (gets a received message and needs to return if the message can be used (or more data is required, in case the fallback 
        /// connection is in use))</param>
        /// <param name="target">UDP target endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of bytes written to <c>response</c></returns>
        public virtual async Task<int> SendAsync(
            ReadOnlyMemory<byte> data,
            Memory<byte> response,
            UdpResponseListener_Delegate responseVerifier,
            IPEndPoint? target = null,
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            if (CanPause) await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            AutoDisposer<NetworkStream>.Context? context;
            NetworkStream? fallbackConnection = null;
            // Use a persistent fallback connection, if available and applicable at this time
            AutoDisposer<NetworkStream>? disposer = FallbackConnectionDisposer;
            try
            {
                context = disposer is not null && target is null && !AllowUdpComeback
                    ? await disposer.UseObjectExclusiveAsync("Persistent fallback connection", cancellationToken).DynamicContext()
                    : null;
            }
            catch when (disposer?.ShouldDispose ?? false)
            {
                context = null;
            }
            // Try UDP first
            if (context is null && IsRunning)
            {
                int res = await TryUdpAsync(data, response, responseVerifier, target, cancellationToken).DynamicContext();
                if (res >= 0) return res;
            }
            // Use a fallback connection
            try
            {
                // Use a persistent fallback connection, if available
                if (context is null && target is null && (disposer = FallbackConnectionDisposer) is not null)
                    try
                    {
                        context = await disposer.UseObjectExclusiveAsync("Persistent fallback connection", cancellationToken).DynamicContext();
                    }
                    catch when (disposer?.ShouldDispose ?? false)
                    {
                        context = null;
                    }
                // Use the fallback connection
                (fallbackConnection, context) = await GetFallbackConnectionAsync(context, target, cancellationToken).DynamicContext();
                if (disposer is null) RaiseOnFallback();
                return await UseFallbackAsync(fallbackConnection, data, response, responseVerifier, cancellationToken).DynamicContext();
            }
            finally
            {
                if (context is not null)
                {
                    await context.DisposeAsync().DynamicContext();
                }
                else if (fallbackConnection is not null)
                {
                    await fallbackConnection.DisposeAsync().DynamicContext();
                }
            }
        }

        /// <summary>
        /// Try sending and receiving an UDP packet
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <param name="response">Buffer for receiving the response</param>
        /// <param name="responseVerifier">UDP response verifier (gets a received message and needs to return if the message can be used (or more data is required))</param>
        /// <param name="target">UDP target endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of bytes written to <c>response</c> (if <c>-1</c>, the fallback will be used)</returns>
        protected virtual async Task<int> TryUdpAsync(
            ReadOnlyMemory<byte> data,
            Memory<byte> response,
            UdpResponseListener_Delegate responseVerifier,
            IPEndPoint? target = null,
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            TaskCompletionSource<int> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            bool UdpListener(ReadOnlyMemory<byte> data, bool isFromFallback)
            {
                if (!responseVerifier(data, isFromFallback)) return false;
                data.Span.CopyTo(response.Span);
                tcs.SetResult(data.Length);
                return true;
            }
            UdpResponsePreListener.Add(UdpListener);
            try
            {
                await UdpClient.SendAsync(data, target, cancellationToken).DynamicContext();
                using CancellationTokenSource cts = new(ResponseTimeout);
                try
                {
                    using Cancellations cancellation = new(cancellationToken, cts.Token, CancelToken);
                    return await tcs.Task.WaitAsync(cancellation).DynamicContext();
                }
                catch (OperationCanceledException ex) when (ex.CancellationToken.IsEqualTo(cts.Token) || ex.CancellationToken.IsEqualTo(CancelToken))
                {
                }
            }
            catch (ObjectDisposedException) when (!IsRunning)
            {
            }
            finally
            {
                UdpResponsePreListener.Remove(UdpListener);
            }
            return -1;
        }

        /// <summary>
        /// Get the fallback connection
        /// </summary>
        /// <param name="context">Existing fallback connection usage context</param>
        /// <param name="target">UDP target endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Fallback connection to use and its auto-disposer context</returns>
        protected virtual async Task<(NetworkStream FallbackConnection, AutoDisposer<NetworkStream>.Context? DisposerContext)> GetFallbackConnectionAsync(
            AutoDisposer<NetworkStream>.Context? context,
            IPEndPoint? target,
            CancellationToken cancellationToken
            )
        {
            while (EnsureUndisposed())
            {
                using SemaphoreSyncContext? ssc = KeepFallbackAlive && target is null
                    ? await FallbackSync.SyncContextAsync(cancellationToken).DynamicContext()
                    : null;
                AutoDisposer<NetworkStream>? disposer = FallbackConnectionDisposer;
                // Unlock the object, if a persistent fallback connection exists already
                if (disposer is not null && ssc.HasValue) ssc.Value.Dispose();
                // Try using an existing fallback connection usage
                if (context is not null)
                {
                    if (disposer is not null && context.Disposer == disposer) return (context.Object, context);
                    await context.DisposeAsync().DynamicContext();
                    context = null;
                }
                // Try creating a new fallback connection usage
                if (disposer is not null && target is null)
                {
                    try
                    {
                        context = await disposer.UseObjectExclusiveAsync("Fallback connection", cancellationToken).DynamicContext();
                    }
                    catch when (disposer.ShouldDispose)
                    {
                        continue;
                    }
                    return (context.Object, context);
                }
                // Use a temporary fallback connection, if required
                if (target is not null || !KeepFallbackAlive) return (await FallbackConnector(this, target, cancellationToken).DynamicContext(), DisposerContext: null);
                // Create a persistent fallback connection
                NetworkStream fallbackConnection = await FallbackConnector(this, target: null, cancellationToken).DynamicContext();
                FallbackConnectionDisposer = new(fallbackConnection)
                {
                    OnlyExclusiveObjectUsage = true
                };
                FallbackConnection = fallbackConnection;
                if (!AllowUdpComeback) await StopAsync(CancellationToken.None).DynamicContext();
                return (FallbackConnection, await FallbackConnectionDisposer.UseObjectExclusiveAsync("New fallback connection", cancellationToken).DynamicContext());
            }
            throw new InvalidProgramException();
        }

        /// <summary>
        /// Send a packet using a fallback connection
        /// </summary>
        /// <param name="fallbackConnection">The fallback connection to use</param>
        /// <param name="data">Data to send</param>
        /// <param name="response">Buffer for receiving the response</param>
        /// <param name="responseVerifier">UDP response verifier (gets a received message and needs to return if the message can be used (or more data is required))</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of bytes written to <c>response</c></returns>
        protected virtual async Task<int> UseFallbackAsync(
            NetworkStream fallbackConnection,
            ReadOnlyMemory<byte> data,
            Memory<byte> response,
            UdpResponseListener_Delegate responseVerifier,
            CancellationToken cancellationToken = default
            )
        {
            EnsureUndisposed();
            await fallbackConnection.WriteAsync(data, cancellationToken).DynamicContext();
            int len = 0;
            using CancellationTokenSource cts = new(ResponseTimeout);
            try
            {
                using Cancellations cancellation = new(cancellationToken, cts.Token);
                while (len < response.Length)
                {
                    len += await fallbackConnection.ReadAsync(response[len..], cancellation).DynamicContext();
                    if (responseVerifier(response[..len], isFromFallback: true)) break;
                }
            }
            catch(OperationCanceledException ex) when (ex.CancellationToken.IsEqualTo(cts.Token))
            {
                if (!responseVerifier(response[..len], isFromFallback: true))
                    throw new TimeoutException($"{GetType()} didn't respond within {ResponseTimeout}", ex);
            }
            return len;
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
        {
            try
            {
                UdpReceiveResult packet;
                bool wasUsed;
                while (EnsureUndisposed(throwException: false))
                {
                    // Receive the next incoming UDP packet
                    if (CanPause) await PauseEvent.WaitAsync(CancelToken).DynamicContext();
                    packet = await UdpClient.ReceiveAsync(CancelToken).DynamicContext();
                    // Run UDP listener
                    try
                    {
                        wasUsed = RunUdpListener(packet.Buffer);
                    }
                    finally
                    {
                        if (ClearBuffers) packet.Buffer.Clear();
                    }
                    // Perform UDP comeback, if applicable
                    if (wasUsed && FallbackConnectionDisposer is not null && AllowUdpComeback) await PerformUdpComebackAsync().DynamicContext();
                }
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken.IsEqualTo(CancelToken))
            {
            }
            finally
            {
                UdpClient.Dispose();
            }
        }

        /// <summary>
        /// Run UDP listener
        /// </summary>
        /// <param name="data">Received data</param>
        /// <returns>If the data was used</returns>
        protected virtual bool RunUdpListener(Memory<byte> data)
        {
            // Run pre-listener
            int i = 0;
            foreach (UdpResponseListener_Delegate listener in UdpResponsePreListener)
                try
                {
                    if (listener(data, isFromFallback: false)) return true;
                    i++;
                }
                catch (Exception ex)
                {
                    ErrorHandling.Handle(new($"UDP response pre-listener #{++i} {listener} failed exceptional", ex));
                }
            // Run post-listener
            i = 0;
            for (int len = UdpResponsePostListener.Length; i < len; i++)
                try
                {
                    if (UdpResponsePostListener[i](data, isFromFallback: false)) return true;
                }
                catch (Exception ex)
                {
                    ErrorHandling.Handle(new($"UDP response post-listener #{i + 1} {UdpResponsePostListener[i]} failed exceptional", ex));
                }
            return false;
        }

        /// <summary>
        /// Perform an UDP comeback
        /// </summary>
        protected virtual async Task PerformUdpComebackAsync()
        {
            bool raiseEvent = false;
            using (SemaphoreSyncContext ssc = await FallbackSync.SyncContextAsync(CancelToken).DynamicContext())
                if (FallbackConnectionDisposer is AutoDisposer<NetworkStream> disposer)
                {
                    FallbackConnection = null;
                    await disposer.SetShouldDisposeAsync().DynamicContext();
                    FallbackConnectionDisposer = null;
                    raiseEvent = true;
                }
            if (raiseEvent) RaiseOnComeback();
        }
    }
}
