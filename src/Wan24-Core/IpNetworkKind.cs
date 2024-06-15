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
        [DisplayText("None")]
        None = 0,
        /// <summary>
        /// Loopback (local system)
        /// </summary>
        [DisplayText("Loopback")]
        Loopback = 1,
        /// <summary>
        /// Local Area Network (private intranet)
        /// </summary>
        [DisplayText("LAN")]
        LAN = 2,
        /// <summary>
        /// Wide Area Network (public internet)
        /// </summary>
        [DisplayText("WAN")]
        WAN = 4,
        /// <summary>
        /// All kinds
        /// </summary>
        [DisplayText("All kinds")]
        ALL = Loopback | LAN | WAN
    }
}
