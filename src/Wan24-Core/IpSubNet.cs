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
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial record struct IpSubNet
    {
        /// <summary>
        /// Mask bits length
        /// </summary>
        public readonly byte MaskBits;
        /// <summary>
        /// Is an IPv4 sub-net?
        /// </summary>
        public readonly bool IsIPv4;
        /// <summary>
        /// Network
        /// </summary>
        public readonly BigInteger Network;

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
        /// <exception cref="FormatException">Invalid sub-net CIDR notation</exception>
        public IpSubNet(in ReadOnlySpan<char> subNet)
        {
            int netIndex = subNet.IndexOf('/');
            if (netIndex == -1) throw new FormatException("Sub-net must be notated as IP/n, where n is the sub-net bits length");
            if (!int.TryParse(subNet[(netIndex + 1)..], out int bits))
                throw new FormatException("Invalid sub-net bits length value");
            if (bits < 0 || bits > IPV6_BITS)
                throw new FormatException("Invalid sub-net bits length");
            if (!IPAddress.TryParse(subNet[..netIndex], out IPAddress? networkIp))
                throw new FormatException("Invalid sub-net network IP address");
            IsIPv4 = networkIp.AddressFamily == AddressFamily.InterNetwork;
            if (!IsIPv4 && networkIp.AddressFamily != AddressFamily.InterNetworkV6) throw new FormatException("Sub-net supports only IPv4/6 addresses");
            if (IsIPv4 && bits > IPV4_BITS) throw new FormatException("Invalid IPv4 sub-net bits length");
            MaskBits = (byte)bits;
            Network = GetBigInteger(networkIp);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="network">Network IP</param>
        /// <param name="countAllBits">Count all possible network mask bits?</param>
        public IpSubNet(in IPAddress network, in bool countAllBits = false)
        {
            bool isIPv4 = network.AddressFamily == AddressFamily.InterNetwork;
            if (!isIPv4 && network.AddressFamily != AddressFamily.InterNetworkV6) throw new ArgumentException("Sub-net supports only IPv4/6 addresses", nameof(network));
            using RentedArrayStructSimple<byte> buffer = GetBytes(network);
            ReadOnlySpan<byte> bytes = buffer.Span;
            Network = new(bytes, isUnsigned: true, isBigEndian: true);
            if (countAllBits)
            {
                int bits = bytes.Length << 3;
                if ((bytes[^1] & 1) == 0) for (; --bits > 0 && ((bytes[bits >> 3] >> (8 - (bits & 7))) & 1) == 0;) ;
                MaskBits = (byte)bits;
            }
            else
            {
                int i = bytes.Length - 1;
                for (; i > -1 && bytes[i] == 0; i--) ;
                MaskBits = (byte)(++i << 3);
            }
            IsIPv4 = isIPv4;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="network">Network IP</param>
        /// <param name="mask">Network mask IP</param>
        public IpSubNet(in IPAddress network, in IPAddress mask)
        {
            Network = GetBigInteger(network);
            switch (network.AddressFamily)
            {
                case AddressFamily.InterNetworkV6:
                    using (RentedArrayStructSimple<byte> maskBuffer = GetBytes(mask))
                    {
                        ReadOnlySpan<byte> maskBytes = maskBuffer.Span;
                        BigInteger maskBits = new(maskBytes, isUnsigned: true, isBigEndian: true);
                        byte popCount = (byte)BigInteger.PopCount(maskBits);
                        MaskBits = popCount;
                        if (new BigInteger(maskBytes, isUnsigned: true, isBigEndian: true) != ((MaxIPv6 << (IPV6_BITS - popCount)) & MaxIPv6))
                            throw new ArgumentException("Invalid mask", nameof(mask));
                        IsIPv4 = false;
                    }
                    break;
                case AddressFamily.InterNetwork:
                    {
                        uint maskBits = BinaryPrimitives.ReadUInt32BigEndian(mask.GetAddressBytes());
                        MaskBits = (byte)uint.PopCount(maskBits);
                        if (maskBits != (MaskBits == 0 ? 0 : uint.MaxValue << (IPV4_BITS - MaskBits)))
                            throw new ArgumentException("Invalid mask", nameof(mask));
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
            bool isIPv4 = network.AddressFamily == AddressFamily.InterNetwork;
            if (!isIPv4 && network.AddressFamily != AddressFamily.InterNetworkV6) throw new ArgumentException("Sub-net supports only IPv4/6 addresses", nameof(network));
            if (bits < 0 || bits > (isIPv4 ? IPV4_BITS : IPV6_BITS)) throw new ArgumentOutOfRangeException(nameof(bits));
            Network = GetBigInteger(network);
            MaskBits = (byte)bits;
            IsIPv4 = isIPv4;
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
        }
    }
}
