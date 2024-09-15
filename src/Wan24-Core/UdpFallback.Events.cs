namespace wan24.Core
{
    // Events
    public partial class UdpFallback
    {
        /// <summary>
        /// Delegate for an event handler
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        public delegate void Fallback_Delegate(UdpFallback sender, EventArgs e);

        /// <summary>
        /// Raised when a fallback connection is going to be used (raised only once, if the fallback connection is persistent, and UDP can't come back)
        /// </summary>
        public event Fallback_Delegate? OnFallback;
        /// <summary>
        /// Raise the <see cref="OnFallback"/> event
        /// </summary>
        protected virtual void RaiseOnFallback() => OnFallback?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raised when the UDP connection came back after a persistent fallback connection was created
        /// </summary>
        public event Fallback_Delegate? OnComeback;
        /// <summary>
        /// Raise the <see cref="OnComeback"/> event
        /// </summary>
        protected virtual void RaiseOnComeback() => OnComeback?.Invoke(this, EventArgs.Empty);
    }
}
