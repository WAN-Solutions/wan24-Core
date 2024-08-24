using System.Net.NetworkInformation;

namespace wan24.Core
{
    /// <summary>
    /// Network events
    /// </summary>
    public class NetworkEvents : DisposableBase
    {
        /// <summary>
        /// Check timer
        /// </summary>
        protected readonly System.Timers.Timer CheckTimer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapter">Network adapter</param>
        /// <param name="checkInterval">State check interval (default is one second)</param>
        public NetworkEvents(in NetworkInterface adapter, in TimeSpan? checkInterval = null) : base(asyncDisposing: false)
        {
            Adapter = adapter;
            CheckTimer = new()
            {
                AutoReset = true,
                Interval = checkInterval?.TotalMilliseconds ?? TimeSpan.FromSeconds(1).TotalMilliseconds
            };
            CheckTimer.Elapsed += (s, e) =>
            {
                try
                {
                    CheckAdapterState();
                }
                catch(Exception ex)
                {
                    ErrorHandling.Handle(new("Network events check timer failed", ex, tag: this));
                }
            };
            CheckAdapterState();
            CheckTimer.Start();
        }

        /// <summary>
        /// Network adapter
        /// </summary>
        public NetworkInterface Adapter { get; }

        /// <summary>
        /// State check interval
        /// </summary>
        public TimeSpan CheckInterval => TimeSpan.FromMilliseconds(CheckTimer.Interval);

        /// <summary>
        /// If the <see cref="Adapter"/> is connected
        /// </summary>
        public bool IsConnected { get; protected set; }

        /// <summary>
        /// Last connected time
        /// </summary>
        public DateTime LastConnected { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Last disconnected time
        /// </summary>
        public DateTime LastDisconnected { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Connected event (set when connected)
        /// </summary>
        public ResetEvent ConnectedEvent { get; } = new();

        /// <summary>
        /// Disconnected event (set when connected)
        /// </summary>
        public ResetEvent DisconnectedEvent { get; } = new();

        /// <summary>
        /// Check the adapter state
        /// </summary>
        protected virtual void CheckAdapterState()
        {
            bool wasConnected = IsConnected,
                isConnected = Adapter.OperationalStatus == OperationalStatus.Up;
            if (wasConnected == isConnected) return;
            IsConnected = isConnected;
            if (wasConnected)
            {
                LastDisconnected = DateTime.Now;
                ConnectedEvent.Reset();
                DisconnectedEvent.Set();
                RaiseOnDisconnected();
            }
            else
            {
                LastConnected = DateTime.Now;
                DisconnectedEvent.Reset();
                ConnectedEvent.Set();
                RaiseOnConnected();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            CheckTimer.Dispose();
            ConnectedEvent.Dispose();
            DisconnectedEvent.Dispose();
        }

        /// <summary>
        /// Delegate for a network event
        /// </summary>
        /// <param name="sender">Network events</param>
        /// <param name="e">Arguments</param>
        public delegate void NetworkEvent_Delegate(NetworkEvents? sender, EventArgs e);

        /// <summary>
        /// Raised when the network adapter was connected
        /// </summary>
        public event NetworkEvent_Delegate? OnConnected;
        /// <summary>
        /// Raise the <see cref="OnConnected"/> event
        /// </summary>
        protected virtual void RaiseOnConnected() => OnConnected?.Invoke(this, new());

        /// <summary>
        /// Raised when the network adapter was disconnected
        /// </summary>
        public event NetworkEvent_Delegate? OnDisconnected;
        /// <summary>
        /// Raise the <see cref="OnDisconnected"/> event
        /// </summary>
        protected virtual void RaiseOnDisconnected() => OnDisconnected?.Invoke(this, new());
    }
}
