using System.Buffers.Binary;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace wan24.Core
{
    // Methods
    public readonly partial record struct IpSubNet : IEnumerable<IPAddress>, IComparable<IpSubNet>
    {
        /// <summary>
        /// Determine if this sub-net matches an IP address
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="throwOnError">Throw an exception on IP address family mismatch?</param>
        /// <returns>Sub-net matches the given IP address?</returns>
        /// <exception cref="ArgumentException">IP address family mismatch</exception>
        public bool DoesMatch(in IPAddress ip, in bool throwOnError = true)
        {
            if (ip.AddressFamily != AddressFamily)
            {
                if (!throwOnError) return false;
                throw new ArgumentException("IP address family mismatch", nameof(ip));
            }
            BigInteger mask = Mask,
                bits = ip.AddressFamily switch
                {
                    AddressFamily.InterNetworkV6 => new BigInteger(ip.GetAddressBytes(), isUnsigned: true, isBigEndian: true),
                    AddressFamily.InterNetwork => BinaryPrimitives.ReadUInt32BigEndian(ip.GetAddressBytes()),
                    _ => throw new InvalidProgramException()
                } & mask;
            return bits == (Network & mask);
        }

        /// <summary>
        /// Get a range of IP addresses from this sub-net
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="count">Count</param>
        /// <returns>IP addresses</returns>
        public IEnumerable<IPAddress> GetRange(BigInteger startIndex, BigInteger count)
        {
            BigInteger ipc = IPAddressCount;
            if (startIndex >= ipc) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (startIndex + count >= ipc) throw new ArgumentOutOfRangeException(nameof(count));
            for (BigInteger i = startIndex, stop = startIndex + count; i <= stop; i++) yield return GetIPAddress(Network + i);
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
            if (IsIPv4)
            {
                BinaryPrimitives.WriteUInt32BigEndian(buffer[2..], (uint)Network);
            }
            else
            {
                Network.ToByteArray(isUnsigned: true, isBigEndian: true).AsSpan().CopyTo(buffer[2..]);
            }
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

        /// <summary>
        /// Get IP bits as IP address
        /// </summary>
        /// <param name="bits">Bits</param>
        /// <returns>IP address</returns>
        private IPAddress GetIPAddress(BigInteger bits)
        {
            using RentedArrayStruct<byte> buffer = new(len: ByteCount);
            if (IsIPv4)
            {
                BinaryPrimitives.WriteUInt32BigEndian(buffer.Span, (uint)bits);
            }
            else
            {
                byte[] bytes = bits.ToByteArray(isUnsigned: true, isBigEndian: true);
                bytes.AsSpan(0, Math.Min(bytes.Length, 16)).CopyTo(buffer.Span);
            }
            return new(buffer.Span);
        }
    }
}
