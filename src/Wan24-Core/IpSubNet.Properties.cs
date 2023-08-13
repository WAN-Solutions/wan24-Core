using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace wan24.Core
{
    // Properties
    public readonly partial record struct IpSubNet
    {
        /// <summary>
        /// Get an IP address
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>IP address</returns>
        public IPAddress this[in BigInteger index] => index < IPAddressCount ? GetIPAddress(Network + index) : throw new ArgumentOutOfRangeException(nameof(index));

        /// <summary>
        /// Number of bits of the network IP address family
        /// </summary>
        public int BitCount => ByteCount << 3;

        /// <summary>
        /// Number of bytes of the network IP address family
        /// </summary>
        public int ByteCount => IsIPv4 ? IPV4_BYTES : IPV6_BYTES;

        /// <summary>
        /// All bits of the network IP address family as full covering mask
        /// </summary>
        public BigInteger FullMask => IsIPv4 ? uint.MaxValue : IPv6Max;

        /// <summary>
        /// Number of IP addresses in the sub-net
        /// </summary>
        public BigInteger IPAddressCount => BigInteger.Pow(2, BitCount - MaskBits);

        /// <summary>
        /// Number of usable IP addresses in the sub-net
        /// </summary>
        public BigInteger UsableIPAddressCount => BigInteger.Max(1, IsIPv4 ? IPAddressCount - 2 : IPAddressCount - 1);

        /// <summary>
        /// Network IP address family
        /// </summary>
        public AddressFamily AddressFamily => IsIPv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;

        /// <summary>
        /// Network mask
        /// </summary>
        public BigInteger Mask => IsIPv4 ? BigInteger.Abs(~(uint.MaxValue >> MaskBits)) : BigInteger.Abs(BigInteger.Negate(IPv6Max >> MaskBits));

        /// <summary>
        /// Broadcast
        /// </summary>
        public BigInteger Broadcast => IsIPv4 ? Network | BigInteger.Abs(~Mask) : throw new InvalidOperationException();

        /// <summary>
        /// Gateway
        /// </summary>
        public BigInteger Gateway => Network & Mask;

        /// <summary>
        /// Get the network as IP address
        /// </summary>
        public IPAddress NetworkIPAddress => GetIPAddress(Network);

        /// <summary>
        /// Get the network mask as IP address
        /// </summary>
        public IPAddress MaskIPAddress => GetIPAddress(Mask);

        /// <summary>
        /// Get the broadcast IP address
        /// </summary>
        public IPAddress BroadcastIPAddress => IsIPv4 ? GetIPAddress(Broadcast) : throw new InvalidOperationException();

        /// <summary>
        /// Get the gateway IP address
        /// </summary>
        public IPAddress GatewayIPAddress => GetIPAddress(Gateway);

        /// <summary>
        /// First usable IP address
        /// </summary>
        public IPAddress FirstUsable => UsableIPAddressCount == 1 ? NetworkIPAddress : this[1];

        /// <summary>
        /// Last usable IP address
        /// </summary>
        public IPAddress LastUsable => this[UsableIPAddressCount - 1];

        /// <summary>
        /// All IP addresses of this sub-net
        /// </summary>
        public IEnumerable<IPAddress> IPAddresses
        {
            get
            {
                for (BigInteger i = 0, len = IPAddressCount; i < len; i++) yield return GetIPAddress(Network + i);
            }
        }

        /// <summary>
        /// All usable IP addresses of this sub-net
        /// </summary>
        public IEnumerable<IPAddress> UsableIPAddresses
        {
            get
            {
                for (BigInteger len = UsableIPAddressCount, i = !IsIPv4 || len == 1 ? 0U : 1; i < len; i++) yield return GetIPAddress(Network + i);
            }
        }
    }
}
