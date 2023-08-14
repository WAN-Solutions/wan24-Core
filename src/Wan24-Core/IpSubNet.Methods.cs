using System.Collections;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;

namespace wan24.Core
{
    // Methods
    public readonly partial record struct IpSubNet : IEnumerable<IPAddress>, IComparable<IpSubNet>, IComparable, IEquatable<IpSubNet>
    {
        /// <summary>
        /// Determine if this sub-net includes an IP address (won't do IPv4/6 conversions!)
        /// </summary>
        /// <param name="ip">IP address (address family needs to be matching!)</param>
        /// <param name="throwOnError">Throw an exception on IP address family mismatch?</param>
        /// <returns>Sub-net includes the given IP address?</returns>
        /// <exception cref="ArgumentException">IP address family mismatch</exception>
        public bool Includes(in IPAddress ip, in bool throwOnError = true)
        {
            if (ip.AddressFamily != AddressFamily)
            {
                if (!throwOnError) return false;
                throw new ArgumentException("IP address family mismatch", nameof(ip));
            }
            BigInteger mask = Mask,
                bits = new BigInteger(ip.GetAddressBytes(), isUnsigned: true, isBigEndian: true) & mask;
            return bits == (Network & mask /* <- MaskedNetwork */);
        }

        /// <summary>
        /// Determine if this sub-net is compatible with another sub-net (same address family and network, but maybe a different size)
        /// </summary>
        /// <param name="net">Sub-net</param>
        /// <returns>Is compatible?</returns>
        public bool IsCompatibleWith(in IpSubNet net) => IsIPv4 == net.IsIPv4 && MaskedNetwork == net.MaskedNetwork;

        /// <summary>
        /// Determine if this sub-net intersects with another sub-net (won't do IPv4/6 conversions!)
        /// </summary>
        /// <param name="net">Sub-net (address family needs to be matching)</param>
        /// <returns>If this sub-net intersects</returns>
        public bool Intersects(in IpSubNet net)
            => net.AddressFamily == AddressFamily && (net == this[BigInteger.Zero] || net == this[BigInteger.Subtract(IPAddressCount, BigInteger.One)]);

        /// <summary>
        /// Determine if this sub-net is within another sub-net (won't do IPv4/6 conversions!)
        /// </summary>
        /// <param name="net">Sub-net (address family needs to be matching)</param>
        /// <returns>If this sub-net is within</returns>
        public bool IsWithin(in IpSubNet net)
            => net.AddressFamily == AddressFamily && net == this[BigInteger.Zero] && net == this[BigInteger.Subtract(IPAddressCount, BigInteger.One)];

        /// <summary>
        /// Combine this sub-net with another sub-net
        /// </summary>
        /// <param name="net">Sub-net (address family needs to be matching)</param>
        /// <returns>Combined sub-net</returns>
        public IpSubNet CombineWith(in IpSubNet net)
        {
            if (IsIPv4 != net.IsIPv4) throw new InvalidOperationException("Incompatible sub-net address family");
            BigInteger masked = MaskedNetwork,
                netMasked = net.MaskedNetwork,
                minNet = BigInteger.Min(masked, netMasked);
            int bits = 0;
            BigInteger mask;
            if (IsIPv4)
            {
                for (; bits <= IPV4_BITS; bits++)
                {
                    if (BigInteger.Compare(((masked >> bits) & MaxIPv4), ((netMasked >> bits) & MaxIPv4)) == 0) break;
                }
                Logging.WriteInfo($"{bits}");
                bits = Math.Min(IPV4_BITS, Math.Max(Math.Max(MaskBits, net.MaskBits), IPV4_BITS - bits));
                Logging.WriteInfo($"{bits}");
                mask = (MaxIPv4 >> bits) & MaxIPv4;
            }
            else
            {
                for (bits = IPV4_BITS; bits <= IPV6_BITS; bits++)
                {
                    mask = MaxIPv6 >> bits;
                    if ((masked & mask) != (netMasked & mask)) break;
                }
                mask = MaxIPv6 << (BitCount - bits);
            }
            return new(minNet & mask, bits, IsIPv4);
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <returns>Bytes (<see cref="IPV6_STRUCTURE_SIZE"/> or <see cref="IPV4_STRUCTURE_SIZE"/>)</returns>
        public byte[] GetBytes()
        {
            byte[] res = new byte[IsIPv4 ? IPV4_STRUCTURE_SIZE : IPV6_STRUCTURE_SIZE];
            GetBytes(res);
            return res;
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="buffer">Buffer (<see cref="IPV6_STRUCTURE_SIZE"/> or <see cref="IPV4_STRUCTURE_SIZE"/> required)</param>
        public void GetBytes(in Span<byte> buffer)
        {
            if (buffer.Length < (IsIPv4 ? IPV4_STRUCTURE_SIZE : IPV6_STRUCTURE_SIZE)) throw new OutOfMemoryException();
            buffer[0] = (byte)(IsIPv4 ? 1 : 0);
            buffer[1] = MaskBits;
            byte[] bytes = Network.ToByteArray(isUnsigned: true, isBigEndian: true);
            bytes.AsSpan(0, Math.Min(bytes.Length, IsIPv4 ? IPV4_BYTES : IPV6_BYTES)).CopyTo(buffer.Slice(2, IsIPv4 ? IPV4_BYTES : IPV6_BYTES));
            if (bytes.Length != (IsIPv4 ? IPV4_BYTES : IPV6_BYTES)) buffer.Slice(2 + bytes.Length, (IsIPv4 ? IPV4_BYTES : IPV6_BYTES) - bytes.Length).Clear();
        }

        /// <inheritdoc/>
        public override string ToString() => $"{NetworkIPAddress}/{MaskBits}";

        /// <summary>
        /// Get a sub-net IP address range enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<IPAddress> GetEnumerator() => IPAddresses.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => IPAddresses.GetEnumerator();

        /// <summary>
        /// Compare this instance mask bits lengh with another instances mask bits length
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Result</returns>
        public int CompareTo(IpSubNet other) => MaskBits.CompareTo(other.MaskBits);

        /// <inheritdoc/>
        int IComparable.CompareTo(object? obj)
        {
            if (obj is null) return 1;
            return obj is IpSubNet net ? MaskBits.CompareTo(net.MaskBits) : throw new ArgumentException("Not an IP sub-net", nameof(obj));
        }

        /// <summary>
        /// Get IP bits as IP address
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>IP address</returns>
        private IPAddress GetIPAddress(BigInteger bits)
        {
            int byteCount = ByteCount;
            using RentedArrayStruct<byte> buffer = new(byteCount);
            byte[] bytes = bits.ToByteArray(isUnsigned: true, isBigEndian: true);
            int len = Math.Min(bytes.Length, byteCount);
            bytes.AsSpan(0, len).CopyTo(buffer.Span[(byteCount - len)..]);
            return new(buffer.Span);
        }
    }
}
