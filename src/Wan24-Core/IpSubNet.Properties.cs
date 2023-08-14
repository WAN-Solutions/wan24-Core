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
        public IPAddress this[in BigInteger index]
            => index < IPAddressCount
                ? GetIPAddress(BigInteger.Add(MaskedNetwork, index))
                : throw new ArgumentOutOfRangeException(nameof(index));

        /// <summary>
        /// Get an IP address range within this sub-net
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="count">Count</param>
        /// <returns>IP addresses</returns>
        public IEnumerable<IPAddress> this[BigInteger startIndex, BigInteger count]
        {
            get
            {
                BigInteger ipc = IPAddressCount;
                if (startIndex >= ipc) throw new ArgumentOutOfRangeException(nameof(startIndex));
                BigInteger stop = BigInteger.Add(startIndex, count);
                if (stop >= ipc) throw new ArgumentOutOfRangeException(nameof(count));
                for (
                    BigInteger i = startIndex;
                    i <= stop;
                    i = BigInteger.Add(i, BigInteger.One)
                    )
                    yield return GetIPAddress(BigInteger.Add(MaskedNetwork, i));
            }
        }

        /// <summary>
        /// Number of bits of the network IP address family
        /// </summary>
        public int BitCount => IsIPv4 ? IPV4_BITS : IPV6_BITS;

        /// <summary>
        /// Number of bytes of the network IP address family
        /// </summary>
        public int ByteCount => IsIPv4 ? IPV4_BYTES : IPV6_BYTES;

        /// <summary>
        /// Structure size in bytes when calling <see cref="GetBytes()"/>
        /// </summary>
        public int StructureSize => IsIPv4 ? IPV4_STRUCTURE_SIZE : IPV6_STRUCTURE_SIZE;

        /// <summary>
        /// All bits of the network IP address family as full covering mask
        /// </summary>
        public BigInteger FullMask => IsIPv4 ? MaxIPv4 : MaxIPv6;

        /// <summary>
        /// Number of IP addresses in the sub-net
        /// </summary>
        public BigInteger IPAddressCount => BigInteger.Pow(new(2u), BitCount - MaskBits);

        /// <summary>
        /// Number of usable IP addresses in the sub-net
        /// </summary>
        public BigInteger UsableIPAddressCount
            => BigInteger.Max(BigInteger.One, IsIPv4 ? BigInteger.Subtract(IPAddressCount, new(2u)) : BigInteger.Subtract(IPAddressCount, BigInteger.One));

        /// <summary>
        /// Network IP address family
        /// </summary>
        public AddressFamily AddressFamily => IsIPv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;

        /// <summary>
        /// Network mask
        /// </summary>
        public BigInteger Mask => IsIPv4 ? (MaxIPv4 << (IPV4_BITS - MaskBits)) & MaxIPv4 : (MaxIPv6 << (IPV6_BITS - MaskBits)) & MaxIPv6;

        /// <summary>
        /// Broadcast
        /// </summary>
        public BigInteger Broadcast => IsIPv4 ? Network | ((IsIPv4 ? MaxIPv4 : MaxIPv6) >> MaskBits) : throw new InvalidOperationException();

        /// <summary>
        /// Masked network address
        /// </summary>
        public BigInteger MaskedNetwork => Network & Mask;

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
        public IPAddress BroadcastIPAddress => GetIPAddress(Broadcast);

        /// <summary>
        /// Get the masked network IP address
        /// </summary>
        public IPAddress MaskedNetworkIPAddress => GetIPAddress(MaskedNetwork);

        /// <summary>
        /// First usable IP address
        /// </summary>
        public IPAddress FirstUsable => UsableIPAddressCount == BigInteger.One ? NetworkIPAddress : this[BigInteger.One];

        /// <summary>
        /// Last usable IP address
        /// </summary>
        public IPAddress LastUsable => this[IsIPv4 ? UsableIPAddressCount : BigInteger.Subtract(IPAddressCount, BigInteger.One)];

        /// <summary>
        /// All IP addresses of this sub-net
        /// </summary>
        public IEnumerable<IPAddress> IPAddresses
        {
            get
            {
                for (
                    BigInteger i = BigInteger.Zero, len = IPAddressCount;
                    i < len;
                    i = BigInteger.Add(i, BigInteger.One)
                    )
                    yield return GetIPAddress(BigInteger.Add(MaskedNetwork, i));
            }
        }

        /// <summary>
        /// All usable IP addresses of this sub-net
        /// </summary>
        public IEnumerable<IPAddress> UsableIPAddresses
        {
            get
            {
                for (
                    BigInteger len = UsableIPAddressCount, i = !IsIPv4 || len == BigInteger.One ? BigInteger.Zero : BigInteger.One;
                    i < len;
                    i = BigInteger.Add(i, BigInteger.One)
                    )
                    yield return GetIPAddress(BigInteger.Add(MaskedNetwork, i));
            }
        }

        /// <summary>
        /// Is a LAN sub-net?
        /// </summary>
        public bool IsLan
        {
            get
            {
                foreach (IpSubNet net in NetworkHelper.LAN)
                    if (IsWithin(net))
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Is a loopback sub-net?
        /// </summary>
        public bool IsLoopback
        {
            get
            {
                foreach (IpSubNet net in NetworkHelper.LoopBack)
                    if (IsWithin(net))
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Is a WAN sub-net?
        /// </summary>
        public bool IsWan
        {
            get
            {
                foreach (IpSubNet net in NetworkHelper.LAN.Concat(NetworkHelper.LoopBack))
                    if (IsWithin(net))
                        return false;
                return true;
            }
        }
    }
}
