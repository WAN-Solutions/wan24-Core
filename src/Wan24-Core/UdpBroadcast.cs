﻿using System.Net;
using System.Net.Sockets;

namespace wan24.Core
{
    /// <summary>
    /// UDP broadcast helper (broadcaster and listener)
    /// </summary>
    /// <typeparam name="T">Broadcast response type</typeparam>
    public abstract class UdpBroadcast<T> : HostedServiceBase
    {
        /// <summary>
        /// Service event (raised when serving)
        /// </summary>
        protected readonly ResetEvent ServiceEvent = new(initialState: false);
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync BroadcastSync = new();
        /// <summary>
        /// Run the packet handler in background (asynchronous, without waiting for a call to finish)?
        /// </summary>
        protected readonly bool BackgroundPacketHandler;
        /// <summary>
        /// UDP listener
        /// </summary>
        protected UdpClient? Listener = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="udpEndPoint">UDP endpoint</param>
        /// <param name="mask">Network mask</param>
        /// <param name="broadcastPort">Broadcast UDP port</param>
        /// <param name="backgroundPacketHandler">Run the packet handler in background (asynchronous, without waiting for a call to finish)?</param>
        protected UdpBroadcast(in IPEndPoint udpEndPoint, in IPAddress mask, int broadcastPort = 0, in bool backgroundPacketHandler = false) : base()
        {
            if (udpEndPoint.Address.AddressFamily != AddressFamily.InterNetwork) throw new ArgumentException("IPv6 doesn't support broadcast", nameof(udpEndPoint));
            if (broadcastPort < 1) broadcastPort = udpEndPoint.Port;
            if (broadcastPort < 1 || broadcastPort > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(broadcastPort));
            BackgroundPacketHandler = backgroundPacketHandler;
            UdpEndPoint = udpEndPoint;
            NetworkMask = mask;
            BroadcastPort = broadcastPort;
        }

        /// <summary>
        /// UDP endpoint
        /// </summary>
        public IPEndPoint UdpEndPoint { get; }

        /// <summary>
        /// Network mask
        /// </summary>
        public IPAddress NetworkMask { get; }

        /// <summary>
        /// Broadcast UDP port
        /// </summary>
        public int BroadcastPort { get; }

        /// <summary>
        /// Broadcast (using the running services UDP listener)
        /// </summary>
        /// <param name="payload">Payload</param>
        /// <param name="port">Broadcast UDP port (or <c>0</c> for using the port from <see cref="UdpEndPoint"/>)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task BroadcastAsync(byte[] payload, int port = 0, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            UdpClient listener = Listener ?? throw new InvalidOperationException("Service isn't running");
            await listener.SendAsync(payload, new IPEndPoint(IPAddress.Broadcast, port != 0 ? port : BroadcastPort), cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Broadcast (using an own UDP listener; service must be stopped)
        /// </summary>
        /// <param name="payload">Payload</param>
        /// <param name="timeout">Timeout for receiving responses</param>
        /// <param name="maxResponseCount">Maximum response count before cancelling (<c>0</c> for no limit)</param>
        /// <param name="port">Broadcast UDP port (or <c>0</c> for using the port from <see cref="UdpEndPoint"/>)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Broadcast response type</returns>
        public async Task<T?[]> BroadcastAsync(byte[] payload, TimeSpan timeout, int maxResponseCount = 0, int port = 0, CancellationToken cancellationToken = default)
        {
            using SemaphoreSyncContext ssc = await BroadcastSync.SyncContextAsync(cancellationToken).DynamicContext();
            EnsureUndisposed();
            if (Listener is not null) throw new InvalidOperationException("Service is running");
            using UdpClient listener = new(UdpEndPoint)
            {
                EnableBroadcast = true
            };
#pragma warning disable CA1416 // Supported on Windows only
            if (ENV.IsWindows) listener.AllowNatTraversal(allowed: true);
#pragma warning restore CA1416 // Supported on Windows only
            listener.Client.DontFragment = true;
            ConfigureBroadcastListener(listener);
            using CancellationTokenSource cancellations = cancellationToken.CombineWith(timeout);
            List<T?> res = [];
            Func<Task> receiver = async () =>
            {
                try
                {
                    while (!cancellations.IsCancellationRequested)
                    {
                        res.Add(HandleResponse(await listener.ReceiveAsync(cancellations.Token).DynamicContext()));
                        if (maxResponseCount < 1) continue;
                        if (res.Count == maxResponseCount) return;
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (TimeoutException)
                {
                }
            };
            Task receiverTask = receiver.StartLongRunningTask(cancellationToken: cancellations.Token);
            await listener.SendAsync(payload, new IPEndPoint(IPAddress.Broadcast, port != 0 ? port : BroadcastPort), cancellations.Token).DynamicContext();
            await receiverTask.DynamicContext();
            return res.Count == 0 ? [] : [.. res];
        }

        /// <summary>
        /// Configure the broadcast UDP listener
        /// </summary>
        /// <param name="listener">Listener</param>
        protected virtual void ConfigureBroadcastListener(UdpClient listener) => ConfigureListener(listener);

        /// <summary>
        /// Configure the UDP listener
        /// </summary>
        /// <param name="listener">Listener</param>
        protected virtual void ConfigureUdpListener(UdpClient listener) => ConfigureListener(listener);

        /// <summary>
        /// Configure an UDP listener
        /// </summary>
        /// <param name="listener">Listener</param>
        protected virtual void ConfigureListener(UdpClient listener) { }

        /// <summary>
        /// Handle an UDP response packet
        /// </summary>
        /// <param name="response">Response</param>
        /// <returns>Response</returns>
        protected abstract T? HandleResponse(in UdpReceiveResult response);

        /// <summary>
        /// Handle an UDP packet
        /// </summary>
        /// <param name="packet">Packet</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected abstract Task HandleReceivedAsync(UdpReceiveResult packet, CancellationToken cancellationToken);

        /// <inheritdoc/>
        protected override async Task AfterStartAsync(CancellationToken cancellationToken)
        {
            await ServiceEvent.WaitAsync(cancellationToken).DynamicContext();
            await base.AfterStartAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
        {
            await Task.Yield();
            using UdpClient listener = Listener = new(UdpEndPoint)
            {
                EnableBroadcast = true
            };
            ServiceEvent.Set();
            try
            {
#pragma warning disable CA1416 // Supported on Windows only
                if (ENV.IsWindows) listener.AllowNatTraversal(allowed: true);
#pragma warning restore CA1416 // Supported on Windows only
                listener.Client.DontFragment = true;
                ConfigureUdpListener(listener);
                CancellationToken ct = Cancellation!.Token;
                UdpReceiveResult packet;
                while (!ct.IsCancellationRequested)
                {
                    packet = await listener.ReceiveAsync(ct).DynamicContext();
                    if (BackgroundPacketHandler)
                    {
                        _ = HandleReceivedAsync(packet, ct).DynamicContext();
                    }
                    else
                    {
                        await HandleReceivedAsync(packet, ct).DynamicContext();
                    }
                }
            }
            finally
            {
                ServiceEvent.Reset();
                Listener = null;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            BroadcastSync.Dispose();
            ServiceEvent.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            BroadcastSync.Dispose();
            await ServiceEvent.DisposeAsync().DynamicContext();
        }
    }
}
