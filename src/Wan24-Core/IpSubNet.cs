using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// IP sub-net
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public readonly partial record struct IpSubNet
    {
        /// <summary>
        /// Network
        /// </summary>
        public readonly BigInteger Network;
        /// <summary>
        /// Mask bits length
        /// </summary>
        public readonly byte MaskBits;
        /// <summary>
        /// Is an IPv4 sub-net?
        /// </summary>
        public readonly bool IsIPv4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="network">Network</param>
        /// <param name="bits">Sub-net bits length</param>
        /// <param name="isIPv4">Is an IPv4 network?</param>
        public IpSubNet(in BigInteger network, in int bits, in bool isIPv4)
        {
            if (network < BigInteger.Zero || network > MaxIPv6 || (isIPv4 && network > MaxIPv4)) throw new ArgumentOutOfRangeException(nameof(network));
            if (bits < 0 || bits > (isIPv4 ? IPV4_BITS : IPV6_BITS)) throw new ArgumentOutOfRangeException(nameof(bits));
            Network = network;
            MaskBits = (byte)bits;
            IsIPv4 = isIPv4;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="subNet">Sub-net in IP/n CIDR notation</param>
        public IpSubNet(in ReadOnlySpan<char> subNet)
        {
            int netIndex = subNet.IndexOf('/');
            if (netIndex == -1) throw new ArgumentException("Sub-net must be notated as IP/n, where n is the sub-net bits length", nameof(subNet));
            int bits = int.Parse(subNet[(netIndex + 1)..]);
            IPAddress networkIp = IPAddress.Parse(subNet[..netIndex]);
            switch (networkIp.AddressFamily)
            {
                case AddressFamily.InterNetworkV6:
                    if (bits < 0 || bits > IPV6_BITS) throw new ArgumentOutOfRangeException(nameof(subNet));
                    IsIPv4 = false;
                    break;
                case AddressFamily.InterNetwork:
                    if (bits < 0 || bits > IPV4_BITS) throw new ArgumentOutOfRangeException(nameof(subNet));
                    IsIPv4 = true;
                    break;
                default:
                    throw new ArgumentException("Sub-net supports only IPv4/6 address masks", nameof(subNet));
            }
            Network = new(networkIp.GetAddressBytes(), isUnsigned: true, isBigEndian: true);
            MaskBits = (byte)bits;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="network">Network IP</param>
        /// <param name="mask">Network mask IP</param>
        public IpSubNet(in IPAddress network, in IPAddress mask)
        {
            Network = new(network.GetAddressBytes(), isUnsigned: true, isBigEndian: true);
            switch (network.AddressFamily)
            {
                case AddressFamily.InterNetworkV6:
                    {
                        byte[] maskBytes = mask.GetAddressBytes();
                        int popCount = 0;//TODO .NET 7: Use BigInteger.PopCount
                        for (
                            int i = 0, len = maskBytes.Length;
                            i < len;
                            popCount += BitOperations.PopCount(maskBytes[i]), i++
                            ) ;
                        MaskBits = (byte)popCount;
                        if (new BigInteger(maskBytes, isUnsigned: true, isBigEndian: true) != ((MaxIPv6 << (IPV6_BITS - popCount)) & MaxIPv6))
                            throw new ArgumentException($"Invalid mask", nameof(mask));
                        IsIPv4 = false;
                    }
                    break;
                case AddressFamily.InterNetwork:
                    {
                        uint maskBits = BinaryPrimitives.ReadUInt32BigEndian(mask.GetAddressBytes());
                        MaskBits = (byte)BitOperations.PopCount(maskBits);//TODO .NET 7: Use int.PopCount
                        if (maskBits != (MaskBits == 0 ? 0 : uint.MaxValue << (IPV4_BITS - MaskBits))) throw new ArgumentException($"Invalid mask", nameof(mask));
                        IsIPv4 = true;
                    }
                    break;
                default:
                    throw new ArgumentException("Sub-net supports only IPv4/6 addresses", nameof(network));
            }
            if (network.AddressFamily != mask.AddressFamily) throw new ArgumentException("Address family mismatch", nameof(mask));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="network">Network IP</param>
        /// <param name="bits">Sub-net bits length</param>
        public IpSubNet(in IPAddress network, in int bits)
        {
            switch (network.AddressFamily)
            {
                case AddressFamily.InterNetworkV6:
                    if (bits < 0 || bits > IPV6_BITS) throw new ArgumentOutOfRangeException(nameof(bits));
                    Network = new(network.GetAddressBytes(), isUnsigned: true, isBigEndian: true);
                    IsIPv4 = false;
                    break;
                case AddressFamily.InterNetwork:
                    if (bits < 0 || bits > IPV4_BITS) throw new ArgumentOutOfRangeException(nameof(bits));
                    Network = new(BinaryPrimitives.ReadUInt32BigEndian(network.GetAddressBytes()));
                    IsIPv4 = true;
                    break;
                default:
                    throw new ArgumentException("Sub-net supports only IPv4/6 addresses", nameof(network));
            }
            MaskBits = (byte)bits;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="network">IPv6 network IP</param>
        /// <param name="bits">Sub-net bits length</param>
        public IpSubNet(in BigInteger network, in int bits)
        {
            if (network < BigInteger.Zero || network > MaxIPv6) throw new ArgumentOutOfRangeException(nameof(network));
            if (bits < 0 || bits > IPV6_BITS) throw new ArgumentOutOfRangeException(nameof(bits));
            Network = network;
            MaskBits = (byte)bits;
            IsIPv4 = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="network">IPv4 network IP</param>
        /// <param name="bits">Sub-net bits length</param>
        public IpSubNet(in uint network, in int bits)
        {
            if (bits < 0 || bits > IPV4_BITS) throw new ArgumentOutOfRangeException(nameof(bits));
            Network = network;
            MaskBits = (byte)bits;
            IsIPv4 = true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer">Buffer (<see cref="IPV6_STRUCTURE_SIZE"/> or <see cref="IPV4_STRUCTURE_SIZE"/> required)</param>
        public IpSubNet(in ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < IPV4_STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(buffer));
            IsIPv4 = buffer[0] == 1;
            if (!IsIPv4 && buffer.Length < IPV6_STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(buffer));
            MaskBits = buffer[1];
            if (MaskBits > (IsIPv4 ? IPV4_BITS : IPV6_BITS)) throw new InvalidDataException("Invalid sub-net bits length");
            Network = new(buffer.Slice(2, IsIPv4 ? IPV4_BYTES : IPV6_BYTES), isUnsigned: true, isBigEndian: true);
            if (IsIPv4 && Network > MaxIPv4) throw new InvalidDataException("Invalid IPv4 network");
        }
    }
}
