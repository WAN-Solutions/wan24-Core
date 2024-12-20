﻿using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime;
using System.Runtime.CompilerServices;

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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Includes(in IPAddress ip, in bool throwOnError = true)
        {
            if (ip.AddressFamily != AddressFamily) return throwOnError ? throw new ArgumentException("IP address family mismatch", nameof(ip)) : false;
            BigInteger mask = Mask;
            return (GetBigInteger(ip) & mask) == (Network & mask /* <- MaskedNetwork */);
        }

        /// <summary>
        /// Determine if this sub-net is compatible with another sub-net (same address family and network, but maybe a different size)
        /// </summary>
        /// <param name="net">Sub-net</param>
        /// <returns>Is compatible?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsCompatibleWith(in IpSubNet net) => IsIPv4 == net.IsIPv4 && MaskedNetwork == net.MaskedNetwork;

        /// <summary>
        /// Determine if this sub-net intersects with another sub-net (won't do IPv4/6 conversions!)
        /// </summary>
        /// <param name="net">Sub-net (address family needs to be matching)</param>
        /// <returns>If this sub-net intersects</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Intersects(in IpSubNet net)
            => net.AddressFamily == AddressFamily && (net == this[BigInteger.Zero] || net == this[BigInteger.Subtract(IPAddressCount, BigInteger.One)]);

        /// <summary>
        /// Determine if this sub-net is within another sub-net (won't do IPv4/6 conversions!)
        /// </summary>
        /// <param name="net">Sub-net (address family needs to be matching)</param>
        /// <returns>If this sub-net is within</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsWithin(in IpSubNet net)
            => net.AddressFamily == AddressFamily && net == this[BigInteger.Zero] && net == this[BigInteger.Subtract(IPAddressCount, BigInteger.One)];

        /// <summary>
        /// Combine this sub-net with another sub-net
        /// </summary>
        /// <param name="net">Sub-net (address family needs to be matching)</param>
        /// <returns>Combined sub-net</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public IpSubNet CombineWith(in IpSubNet net)
        {
            if (IsIPv4 != net.IsIPv4) throw new InvalidOperationException("Incompatible sub-net address family");
            BigInteger masked = MaskedNetwork,
                netMasked = net.MaskedNetwork;
            int bits = 0,
                allBits = BitCount;
            for (; bits <= allBits && BigInteger.Compare(masked >> bits, netMasked >> bits) != 0; bits++) ;
            return new(
                BigInteger.Min(masked, netMasked) & (FullMask << bits),
                Math.Max(0, Math.Min(Math.Min(MaskBits, net.MaskBits), allBits - bits)),
                IsIPv4
                );
        }

        /// <summary>
        /// Get all matching unicast IP address configuration information
        /// </summary>
        /// <param name="adapter">Ethernet adapter</param>
        /// <returns>Unicast IP address configuration information</returns>
        public IEnumerable<UnicastIPAddressInformation> GetUnicastAddresses(NetworkInterface adapter)
        {
            AddressFamily addressFamily = AddressFamily;
            BigInteger mask = Mask,
                maskedNetwork = Network & mask /* <- MaskedNetwork */;
            foreach (UnicastIPAddressInformation unicast in adapter.GetIPProperties().UnicastAddresses)
            {
                if (unicast.Address.AddressFamily != addressFamily || (GetBigInteger(unicast.Address) & mask) != maskedNetwork) continue;
                yield return unicast;
            }
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public byte[] GetBytes()
        {
            byte[] res = new byte[IsIPv4 ? IPV4_STRUCTURE_SIZE : IPV6_STRUCTURE_SIZE];
            GetBytes(res);
            return res;
        }

        /// <inheritdoc/>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int GetBytes(in Span<byte> buffer)
        {
            if (buffer.Length < (IsIPv4 ? IPV4_STRUCTURE_SIZE : IPV6_STRUCTURE_SIZE)) throw new OutOfMemoryException();
            buffer[0] = (byte)(IsIPv4 ? 1 : 0);
            buffer[1] = MaskBits;
            if (!Network.TryWriteBytes(buffer[2..], out int written, isUnsigned: true, isBigEndian: true)) throw new InvalidProgramException();
            int byteCount = ByteCount;
            if (written != byteCount) buffer.Slice(2 + byteCount, byteCount - written).Clear();
            return IsIPv4 ? IPV4_STRUCTURE_SIZE : IPV6_STRUCTURE_SIZE;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override string ToString() => $"{NetworkIPAddress}/{MaskBits}";

        /// <summary>
        /// Get a sub-net IP address range enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public IEnumerator<IPAddress> GetEnumerator() => IPAddresses.GetEnumerator();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        IEnumerator IEnumerable.GetEnumerator() => IPAddresses.GetEnumerator();

        /// <summary>
        /// Compare this instance mask bits length with another instances mask bits length
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Result</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public IPAddress GetIPAddress(in BigInteger bits)
        {
            if (bits < BigInteger.Zero || bits > MaxIPv6) throw new ArgumentOutOfRangeException(nameof(bits));
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(ByteCount);
            Span<byte> bufferSpan = buffer.Span;
            if (!bits.TryWriteBytes(bufferSpan, out int written, isUnsigned: true, isBigEndian: true))
                throw new ArgumentOutOfRangeException(nameof(bits), $"{written} bytes written");
            if (written != buffer.Length)
            {
                bufferSpan[..written].CopyTo(bufferSpan[(buffer.Length - written)..]);
                bufferSpan[..written].Clear();
            }
            return new(bufferSpan);
#else
            Span<byte> buffer = stackalloc byte[ByteCount];
            if (!bits.TryWriteBytes(buffer, out int written, isUnsigned: true, isBigEndian: true))
                throw new ArgumentOutOfRangeException(nameof(bits), $"{written} bytes written");
            if (written != buffer.Length)
            {
                buffer[..written].CopyTo(buffer[(buffer.Length - written)..]);
                buffer[..written].Clear();
            }
            return new(buffer);
#endif
        }

        /// <summary>
        /// Get an IP address as <see cref="BigInteger"/>
        /// </summary>
        /// <param name="ip"><see cref="IPAddress"/></param>
        /// <returns><see cref="BigInteger"/></returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static BigInteger GetBigInteger(in IPAddress ip)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork && ip.AddressFamily != AddressFamily.InterNetworkV6)
                throw new ArgumentException("Sub-net supports only IPv4/6 addresses", nameof(ip));
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = GetBytes(ip);
            return new(buffer.Span, isUnsigned: true, isBigEndian: true);
#else
            Span<byte> buffer = stackalloc byte[ip.AddressFamily == AddressFamily.InterNetwork ? IPV4_BYTES : IPV6_BYTES];
            if (!ip.TryWriteBytes(buffer, out int written) || written != buffer.Length) throw new ArgumentException("Invalid IP address", nameof(ip));
            return new(buffer, isUnsigned: true, isBigEndian: true);
#endif
        }

        /// <summary>
        /// Get IP address bytes
        /// </summary>
        /// <param name="ip"><see cref="IPAddress"/></param>
        /// <returns>Bytes</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static RentedMemory<byte> GetBytes(in IPAddress ip)
        {
            int len = ip.AddressFamily == AddressFamily.InterNetwork ? IPV4_BYTES : IPV6_BYTES;
            RentedMemory<byte> res = new(len, clean: false);
            try
            {
                if (ip.TryWriteBytes(res.Memory.Span, out int written) && len == written) return res;
                throw new ArgumentException("Invalid IP address", nameof(ip));
            }
            catch
            {
                res.Dispose();
                throw;
            }
        }
    }
}
