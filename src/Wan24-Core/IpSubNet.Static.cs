﻿using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace wan24.Core
{
    // Static
    public readonly partial record struct IpSubNet
    {
        /// <summary>
        /// IPv6 Structure size in bytes (when using <see cref="GetBytes()"/>)
        /// </summary>
        public const int IPV6_STRUCTURE_SIZE = sizeof(bool) + sizeof(byte) + IPV6_BYTES;
        /// <summary>
        /// IPv4 structure size in bytes (when using <see cref="GetBytes()"/>)
        /// </summary>
        public const int IPV4_STRUCTURE_SIZE = sizeof(bool) + sizeof(byte) + IPV4_BYTES;
        /// <summary>
        /// IPv4 bits
        /// </summary>
        public const int IPV4_BITS = sizeof(uint) << 3;
        /// <summary>
        /// IPv4 bits
        /// </summary>
        public const int IPV4_BYTES = sizeof(uint);
        /// <summary>
        /// IPv6 bits
        /// </summary>
        public const int IPV6_BITS = sizeof(long) << 4;
        /// <summary>
        /// IPv6 bits
        /// </summary>
        public const int IPV6_BYTES = sizeof(long) << 1;

        /// <summary>
        /// Max. IPv6 value
        /// </summary>
        public static readonly BigInteger IPv6Max = BigInteger.Pow(2, IPV6_BITS);

        /// <summary>
        /// Cast as IP address list
        /// </summary>
        /// <param name="net">Sub-net</param>
        public static implicit operator IPAddress[](in IpSubNet net)
        {
            BigInteger count = net.IPAddressCount;
            if (count > long.MaxValue) throw new InvalidOperationException("IP address range is too large!");
            IPAddress[] res = new IPAddress[(long)count];
            for (long i = 0; i < count; res[i] = net.GetIPAddress(net.Network + i), i++) ;
            return res;
        }

        /// <summary>
        /// Cast as bytes
        /// </summary>
        /// <param name="net"><see cref="IpSubNet"/></param>
        public static implicit operator byte[](in IpSubNet net) => net.GetBytes();

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="data">Bytes</param>
        public static implicit operator IpSubNet(in byte[] data) => new(data);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="data">Bytes</param>
        public static implicit operator IpSubNet(in Span<byte> data) => new(data);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="data">Bytes</param>
        public static implicit operator IpSubNet(in Memory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="data">Bytes</param>
        public static implicit operator IpSubNet(in ReadOnlySpan<byte> data) => new(data);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="data">Bytes</param>
        public static implicit operator IpSubNet(in ReadOnlyMemory<byte> data) => new(data.Span);

        /// <summary>
        /// Does match an IP address?
        /// </summary>
        /// <param name="net">Sub-net</param>
        /// <param name="ip">IP address</param>
        /// <returns>Does match?</returns>
        public static bool operator ==(in IpSubNet net, in IPAddress ip) => net.DoesMatch(ip, throwOnError: false);

        /// <summary>
        /// Does not match an IP address?
        /// </summary>
        /// <param name="net">Sub-net</param>
        /// <param name="ip">IP address</param>
        /// <returns>Does not match?</returns>
        public static bool operator !=(in IpSubNet net, in IPAddress ip) => !(net == ip);

        /// <summary>
        /// Does match an IP address?
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="net">Sub-net</param>
        /// <returns>Does match?</returns>
        public static bool operator ==(in IPAddress ip, in IpSubNet net) => net.DoesMatch(ip, throwOnError: false);

        /// <summary>
        /// Does not match an IP address?
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="net">Sub-net</param>
        /// <returns>Does not match?</returns>
        public static bool operator !=(in IPAddress ip, in IpSubNet net) => !(ip == net);

        /// <summary>
        /// Parse from a string
        /// </summary>
        /// <param name="subNet">Sub-net in IP/n CIDR notation</param>
        /// <returns>Sub-net</returns>
        public static IpSubNet Parse(in ReadOnlySpan<char> subNet) => new(subNet);

        /// <summary>
        /// Try parsing from a string
        /// </summary>
        /// <param name="subNet">Sub-net in IP/n CIDR notation</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryParse(in ReadOnlySpan<char> subNet, out IpSubNet result)
        {
            try
            {
                int netIndex = subNet.IndexOf('/');
                if (netIndex == -1 || !IPAddress.TryParse(subNet[..netIndex], out IPAddress? networkIp) || !int.TryParse(subNet[(netIndex + 1)..], out int bits))
                {
                    result = default;
                    return false;
                }
                switch (networkIp.AddressFamily)
                {
                    case AddressFamily.InterNetworkV6:
                        if (bits < 0 || bits > IPV6_BITS)
                        {
                            result = default;
                            return false;
                        }
                        result = new(new BigInteger(networkIp.GetAddressBytes(), isUnsigned: true, isBigEndian: true), bits);
                        break;
                    case AddressFamily.InterNetwork:
                        if (bits < 0 || bits > IPV4_BITS)
                        {
                            result = default;
                            return false;
                        }
                        result = new(BinaryPrimitives.ReadUInt32BigEndian(networkIp.GetAddressBytes()), bits);
                        break;
                    default:
                        result = default;
                        return false;
                }
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}