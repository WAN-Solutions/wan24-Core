namespace wan24.Core
{
    /// <summary>
    /// IP network kind enumeration
    /// </summary>
    [Flags]
    public enum IpNetworkKind : byte
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Loopback (local system)
        /// </summary>
        Loopback = 1,
        /// <summary>
        /// Local Area Network (private intranet)
        /// </summary>
        LAN = 2,
        /// <summary>
        /// Wide Area Network (public internet)
        /// </summary>
        WAN = 4
    }
}
