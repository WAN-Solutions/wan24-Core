using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime;
using System.Runtime.CompilerServices;

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
        {
            [TargetedPatchingOptOut("Tiny method")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return index >= BigInteger.Zero && index < IPAddressCount
                    ? GetIPAddress(BigInteger.Add(MaskedNetwork, index))
                    : throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

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
                if (startIndex <= BigInteger.Zero || startIndex >= ipc) throw new ArgumentOutOfRangeException(nameof(startIndex));
                if (count == BigInteger.Zero) yield break;
                BigInteger stop = BigInteger.Add(BigInteger.Add(startIndex, count), BigInteger.One);
                if (stop > ipc) throw new ArgumentOutOfRangeException(nameof(count));
                for (
                    BigInteger i = startIndex, network = MaskedNetwork;
                    i != stop;
                    i = BigInteger.Add(i, BigInteger.One)
                    )
                    yield return GetIPAddress(BigInteger.Add(network, i));
            }
        }

        /// <summary>
        /// Number of bits of the network IP address family
        /// </summary>
        public int BitCount
        {
            [TargetedPatchingOptOut("Tiny method")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return IsIPv4 ? IPV4_BITS : IPV6_BITS;
            }
        }

        /// <summary>
        /// Number of bytes of the network IP address family
        /// </summary>
        public int ByteCount
        {
            [TargetedPatchingOptOut("Tiny method")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return IsIPv4 ? IPV4_BYTES : IPV6_BYTES;
            }
        }

        /// <summary>
        /// Structure size in bytes when calling <see cref="GetBytes()"/>
        /// </summary>
        public int StructureSize
        {
            [TargetedPatchingOptOut("Tiny method")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return IsIPv4 ? IPV4_STRUCTURE_SIZE : IPV6_STRUCTURE_SIZE;
            }
        }

        /// <summary>
        /// All bits of the network IP address family as full covering mask
        /// </summary>
        public BigInteger FullMask
        {
            [TargetedPatchingOptOut("Tiny method")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return IsIPv4 ? MaxIPv4 : MaxIPv6;
            }
        }

        /// <summary>
        /// Number of IP addresses in the sub-net
        /// </summary>
        public BigInteger IPAddressCount
        {
            [TargetedPatchingOptOut("Tiny method")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return BigInteger.Pow(new(2u), BitCount - MaskBits);
            }
        }

        /// <summary>
        /// Number of usable IP addresses in the sub-net
        /// </summary>
        public BigInteger UsableIPAddressCount
        {
            [TargetedPatchingOptOut("Tiny method")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return BigInteger.Max(BigInteger.One, IsIPv4 ? BigInteger.Subtract(IPAddressCount, new(2u)) : BigInteger.Subtract(IPAddressCount, BigInteger.One));
            }
        }

        /// <summary>
        /// Network IP address family
        /// </summary>
        public AddressFamily AddressFamily
        {
            [TargetedPatchingOptOut("Tiny method")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return IsIPv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;
            }
        }

        /// <summary>
        /// Network mask
        /// </summary>
        public BigInteger Mask
        {
            [TargetedPatchingOptOut("Tiny method")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return IsIPv4 ? (MaxIPv4 << (IPV4_BITS - MaskBits)) & MaxIPv4 : (MaxIPv6 << (IPV6_BITS - MaskBits)) & MaxIPv6;
            }
        }

        /// <summary>
        /// Broadcast
        /// </summary>
        public BigInteger Broadcast
        {
            [TargetedPatchingOptOut("Tiny method")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return IsIPv4 ? Network | ((IsIPv4 ? MaxIPv4 : MaxIPv6) >> MaskBits) : throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Masked network address
        /// </summary>
        public BigInteger MaskedNetwork
        {
            [TargetedPatchingOptOut("Tiny method")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Network & Mask;
            }
        }

        /// <summary>
        /// Get the network as IP address
        /// </summary>
        public IPAddress NetworkIPAddress
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetIPAddress(Network);
            }
        }

        /// <summary>
        /// Get the network mask as IP address
        /// </summary>
        public IPAddress MaskIPAddress
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetIPAddress(Mask);
            }
        }

        /// <summary>
        /// Get the broadcast IP address
        /// </summary>
        public IPAddress BroadcastIPAddress
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetIPAddress(Broadcast);
            }
        }

        /// <summary>
        /// Get the masked network IP address
        /// </summary>
        public IPAddress MaskedNetworkIPAddress
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetIPAddress(MaskedNetwork);
            }
        }

        /// <summary>
        /// First usable IP address
        /// </summary>
        public IPAddress FirstUsable
        {
            [TargetedPatchingOptOut("Tiny method")]
            get
            {
                return UsableIPAddressCount == BigInteger.One ? NetworkIPAddress : this[BigInteger.One];
            }
        }

        /// <summary>
        /// Last usable IP address
        /// </summary>
        public IPAddress LastUsable
        {
            [TargetedPatchingOptOut("Tiny method")]
            get
            {
                return this[IsIPv4 ? UsableIPAddressCount : BigInteger.Subtract(IPAddressCount, BigInteger.One)];
            }
        }

        /// <summary>
        /// All IP addresses of this sub-net
        /// </summary>
        public IEnumerable<IPAddress> IPAddresses
        {
            get
            {
                for (
                    BigInteger i = BigInteger.Zero, len = IPAddressCount, network = MaskedNetwork;
                    i != len;
                    i = BigInteger.Add(i, BigInteger.One)
                    )
                    yield return GetIPAddress(BigInteger.Add(network, i));
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
                    BigInteger len = UsableIPAddressCount, i = !IsIPv4 || len == BigInteger.One ? BigInteger.Zero : BigInteger.One, network = MaskedNetwork;
                    i != len;
                    i = BigInteger.Add(i, BigInteger.One)
                    )
                    yield return GetIPAddress(BigInteger.Add(network, i));
            }
        }

        /// <summary>
        /// Is a LAN sub-net?
        /// </summary>
        public bool IsLan
        {
            [TargetedPatchingOptOut("Tiny method")]
            get
            {
                for (int i = 0, len = NetworkHelper.LAN.Count; i != len; i++)
                    if (IsWithin(NetworkHelper.LAN[i]))
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Is a loopback sub-net?
        /// </summary>
        public bool IsLoopback
        {
            [TargetedPatchingOptOut("Tiny method")]
            get
            {
                for (int i = 0, len = NetworkHelper.LoopBack.Count; i != len; i++)
                    if (IsWithin(NetworkHelper.LoopBack[i]))
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Is a WAN sub-net?
        /// </summary>
        public bool IsWan
        {
            [TargetedPatchingOptOut("Tiny method")]
            get
            {
                for (int i = 0, len = NetworkHelper.LoopBack.Count; i != len; i++)
                    if (IsWithin(NetworkHelper.LoopBack[i]))
                        return false;
                for (int i = 0, len = NetworkHelper.LAN.Count; i != len; i++)
                    if (IsWithin(NetworkHelper.LAN[i]))
                        return false;
                return true;
            }
        }

        /// <summary>
        /// IP network kind
        /// </summary>
        public IpNetworkKind NetworkKind
        {
            [TargetedPatchingOptOut("Tiny method")]
            get
            {
                if (IsLoopback) return IpNetworkKind.Loopback;
                if (IsLan) return IpNetworkKind.LAN;
                return IpNetworkKind.WAN;
            }
        }

        /// <summary>
        /// Get the pre-defined parent sub-net of this loopback or LAN sub-net (returns this, if this is a WAN sub-net)
        /// </summary>
        public IpSubNet ParentSubNet
        {
            [TargetedPatchingOptOut("Tiny method")]
            get
            {
                for (int i = 0, len = NetworkHelper.LoopBack.Count; i != len; i++)
                    if (IsWithin(NetworkHelper.LoopBack[i]))
                        return NetworkHelper.LoopBack[i];
                for (int i = 0, len = NetworkHelper.LAN.Count; i != len; i++)
                    if (IsWithin(NetworkHelper.LAN[i]))
                        return NetworkHelper.LAN[i];
                return this;
            }
        }

        /// <summary>
        /// Get all network interfaces which have a matching unicast IP address configured
        /// </summary>
        public IEnumerable<NetworkInterface> NetworkInterfaces
        {
            get
            {
                AddressFamily addressFamily = AddressFamily;
                BigInteger mask = Mask,
                    maskedNetwork = Network & mask /* <- MaskedNetwork */;
                foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
                    foreach (UnicastIPAddressInformation unicast in adapter.GetIPProperties().UnicastAddresses)
                    {
                        if (unicast.Address.AddressFamily != addressFamily || (GetBigInteger(unicast.Address) & mask) != maskedNetwork) continue;
                        yield return adapter;
                        break;
                    }
            }
        }

        /// <summary>
        /// Get all matching unicast IP address configuration informations
        /// </summary>
        public IEnumerable<UnicastIPAddressInformation> UnicastAddresses
        {
            get
            {
                AddressFamily addressFamily = AddressFamily;
                BigInteger mask = Mask,
                    maskedNetwork = Network & mask /* <- MaskedNetwork */;
                foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
                    foreach (UnicastIPAddressInformation unicast in adapter.GetIPProperties().UnicastAddresses)
                    {
                        if (unicast.Address.AddressFamily != addressFamily || (GetBigInteger(unicast.Address) & mask) != maskedNetwork) continue;
                        yield return unicast;
                    }
            }
        }
    }
}
