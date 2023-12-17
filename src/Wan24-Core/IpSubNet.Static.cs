using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime;

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
        /// IPv6 bits
        /// </summary>
        public const int IPV6_BITS = sizeof(long) << 4;
        /// <summary>
        /// IPv6 bits
        /// </summary>
        public const int IPV6_BYTES = sizeof(long) << 1;
        /// <summary>
        /// IPv4 bits
        /// </summary>
        public const int IPV4_BITS = sizeof(uint) << 3;
        /// <summary>
        /// IPv4 bits
        /// </summary>
        public const int IPV4_BYTES = sizeof(uint);

        /// <summary>
        /// Max. IPv6 value
        /// </summary>
        public static readonly BigInteger MaxIPv6 = BigInteger.Subtract(BigInteger.Pow(new(2u), IPV6_BITS), BigInteger.One);
        /// <summary>
        /// Max. IPv4 value
        /// </summary>
        public static readonly BigInteger MaxIPv4 = new(uint.MaxValue);
        /// <summary>
        /// IPv6 loopback sub-net
        /// </summary>
        public static readonly IpSubNet LoopbackIPv6 = new(IPAddress.IPv6Loopback, IPV6_BITS);
        /// <summary>
        /// IPv4 loopback sub-net
        /// </summary>
        public static readonly IpSubNet LoopbackIPv4 = new(IPAddress.Loopback, IPV4_BITS);
        /// <summary>
        /// IPv6 zero sub-net
        /// </summary>
        public static readonly IpSubNet ZeroV6 = new(BigInteger.Zero, IPV6_BITS);
        /// <summary>
        /// IPv4 zero sub-net
        /// </summary>
        public static readonly IpSubNet ZeroV4 = new(0u, IPV4_BITS);

        /// <summary>
        /// Cast as <see cref="IPNetwork"/>
        /// </summary>
        /// <param name="net">Sub-net</param>
        public static implicit operator IPNetwork(in IpSubNet net) => net.AsIpNetwork;

        /// <summary>
        /// Cast from a <see cref="IPNetwork"/>
        /// </summary>
        /// <param name="net"><see cref="IPNetwork"/></param>
        public static implicit operator IpSubNet(in IPNetwork net) => new(net.BaseAddress, net.PrefixLength);

        /// <summary>
        /// Cast as IP address list
        /// </summary>
        /// <param name="net">Sub-net</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static implicit operator IPAddress[](in IpSubNet net)
        {
            BigInteger count = net.IPAddressCount;
            if (count > long.MaxValue) throw new OutOfMemoryException("IP address range is too large!");
            IPAddress[] res = new IPAddress[(long)count];
            for (long i = 0; i < count; res[i] = net.GetIPAddress(BigInteger.Add(net.MaskedNetwork, new(i))), i++) ;
            return res;
        }

        /// <summary>
        /// Cast from <see cref="IPAddress"/> (should be a network address; zero bytes count)
        /// </summary>
        /// <param name="network"><see cref="IPAddress"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IpSubNet(in IPAddress network) => new(network);

        /// <summary>
        /// Cast as <see cref="IPAddress"/> (network address)
        /// </summary>
        /// <param name="network"><see cref="IpSubNet"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IPAddress(in IpSubNet network) => network.MaskedNetworkIPAddress;

        /// <summary>
        /// Cast as bytes
        /// </summary>
        /// <param name="net"><see cref="IpSubNet"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator byte[](in IpSubNet net) => net.GetBytes();

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="data">Bytes</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IpSubNet(in byte[] data) => new(data);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="data">Bytes</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IpSubNet(in Span<byte> data) => new(data);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="data">Bytes</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IpSubNet(in Memory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="data">Bytes</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IpSubNet(in ReadOnlySpan<byte> data) => new(data);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="data">Bytes</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IpSubNet(in ReadOnlyMemory<byte> data) => new(data.Span);

        /// <summary>
        /// Does match an IP address (not a network address!)?
        /// </summary>
        /// <param name="net">Sub-net</param>
        /// <param name="ip">IP address</param>
        /// <returns>Does match?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in IpSubNet net, in IPAddress ip) => net.Includes(ip, throwOnError: false);

        /// <summary>
        /// Does not match an IP address (not a network address!)?
        /// </summary>
        /// <param name="net">Sub-net</param>
        /// <param name="ip">IP address</param>
        /// <returns>Does not match?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in IpSubNet net, in IPAddress ip) => !net.Includes(ip, throwOnError: false);

        /// <summary>
        /// Does match an IP address (not a network address!)?
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="net">Sub-net</param>
        /// <returns>Does match?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in IPAddress ip, in IpSubNet net) => net.Includes(ip, throwOnError: false);

        /// <summary>
        /// Does not match an IP address (not a network address!)?
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="net">Sub-net</param>
        /// <returns>Does not match?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in IPAddress ip, in IpSubNet net) => !net.Includes(ip, throwOnError: false);

        /// <summary>
        /// Lower than
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is A lower than B?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator <(in IpSubNet a, in IpSubNet b) => a.MaskBits < b.MaskBits;

        /// <summary>
        /// Greater than
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is A greater than B?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator >(in IpSubNet a, in IpSubNet b) => a.MaskBits > b.MaskBits;

        /// <summary>
        /// Lower or equal to
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is A lower or equal to B?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator <=(in IpSubNet a, in IpSubNet b) => a.MaskBits <= b.MaskBits;

        /// <summary>
        /// Greater or equal to
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Is A greater or equal to B?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator >=(in IpSubNet a, in IpSubNet b) => a.MaskBits >= b.MaskBits;

        /// <summary>
        /// Combine two sub-nets
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Combined sub-net</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static IpSubNet operator +(in IpSubNet a, in IpSubNet b) => a.CombineWith(b);

        /// <summary>
        /// Merge two compatible sub-nets
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Merged sub-net</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static IpSubNet operator |(in IpSubNet a, in IpSubNet b)
            => a.IsCompatibleWith(b) ? new(a.Network, Math.Min(a.MaskBits, b.MaskBits), a.IsIPv4) : throw new InvalidOperationException("Incompatible sub-nets");

        /// <summary>
        /// Determine if two sub-nets intersect, or if A fits into B
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>A, if A fits into B, or B, if A intersects B, or <see cref="ZeroV4"/>, if no intersection at all</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static IpSubNet operator &(in IpSubNet a, in IpSubNet b)
        {
            if (a.IsIPv4 == b.IsIPv4)
            {
                if (a.IsWithin(b)) return a;
                if (a.Intersects(b)) return b;
            }
            return ZeroV4;
        }

        /// <summary>
        /// Increase a sub-net size
        /// </summary>
        /// <param name="net">Sub-net</param>
        /// <param name="bits">Bits</param>
        /// <returns>Resized sub-net</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static IpSubNet operator <<(in IpSubNet net, in int bits)
        {
            int newBits = net.MaskBits + bits;
            if (newBits < 0) newBits = 0;
            else if (newBits > net.BitCount) newBits = net.BitCount;
            return new(net.Network, newBits, net.IsIPv4);
        }

        /// <summary>
        /// Decrease a sub-net size
        /// </summary>
        /// <param name="net">Sub-net</param>
        /// <param name="bits">Bits</param>
        /// <returns>Resized sub-net</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static IpSubNet operator >>(in IpSubNet net, in int bits)
        {
            int newBits = net.MaskBits - bits;
            if (newBits < 0) newBits = 0;
            else if (newBits > net.BitCount) newBits = net.BitCount;
            return new(net.Network, newBits, net.IsIPv4);
        }

        /// <summary>
        /// Parse from a string
        /// </summary>
        /// <param name="subNet">Sub-net in IP/n CIDR notation</param>
        /// <returns>Sub-net</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
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
                if (netIndex != -1 && IPAddress.TryParse(subNet[..netIndex], out IPAddress? networkIp) && int.TryParse(subNet[(netIndex + 1)..], out int bits))
                {
                    bool isIPv4 = networkIp.AddressFamily == AddressFamily.InterNetwork;
                    if (!isIPv4 && networkIp.AddressFamily != AddressFamily.InterNetworkV6)
                    {
                        Logging.WriteTrace($"Sub-net \"{subNet}\" has an unsupported IP address family");
                    }
                    else if (bits < 0 || bits > (isIPv4 ? IPV4_BITS : IPV6_BITS))
                    {
                        Logging.WriteTrace($"Sub-net \"{subNet}\" bit component exceeds the allowed range of the IP address family");
                    }
                    else
                    {
                        result = new(GetBigInteger(networkIp), bits, isIPv4);
                        return true;
                    }
                }
                else
                {
                    Logging.WriteTrace($"Sub-net \"{subNet}\" component parsing failed");
                }
            }
            catch (Exception ex)
            {
                Logging.WriteDebug($"IP sub-net parsing failed with an exception: {ex}");
            }
            result = default;
            return false;
        }
    }
}
