using System.Collections;
using System.Net;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace wan24.Core
{
    /// <summary>
    /// Host endpoint
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public readonly record struct HostEndPoint : IEnumerable<IPEndPoint>, IAsyncEnumerable<IPEndPoint>
    {
        /// <summary>
        /// Min. buffer length in bytes
        /// </summary>
        private const int MIN_BUFFER_LEN = 3;

        /// <summary>
        /// Port (<c>0..65535</c>)
        /// </summary>
        public readonly int Port;
        /// <summary>
        /// Hostname
        /// </summary>
        public readonly string Hostname;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hostname">Hostname</param>
        /// <param name="port">Port (<c>0..65535</c>)</param>
        public HostEndPoint(in string hostname, in int port)
        {
            if (Uri.CheckHostName(hostname) != UriHostNameType.Dns) throw new ArgumentException("Invalid hostname", nameof(hostname));
            if (port < 0 || port > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(port));
            Hostname = hostname;
            Port = port;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer">Bytes</param>
        public HostEndPoint(in ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < MIN_BUFFER_LEN) throw new InvalidDataException("Buffer too short");
            Hostname = Encoding.Unicode.GetString(buffer[..^2]);
            if (Uri.CheckHostName(Hostname) != UriHostNameType.Dns) throw new InvalidDataException("Invalid hostname");
            Port = buffer[^2..].ToUShort();
            if (Port < 0 || Port > ushort.MaxValue) throw new InvalidDataException("Invalid port number");
        }

        /// <summary>
        /// Resolved IP endpoints for the hostname
        /// </summary>
        public IEnumerable<IPEndPoint> ResolvedIpEndPoints
        {
            get
            {
                foreach (IPAddress ip in Dns.GetHostAddresses(Hostname)) yield return new IPEndPoint(ip, Port);
            }
        }

        /// <summary>
        /// All resolved IP endpoints for the hostname
        /// </summary>
        public IPEndPoint[] AllResolvedIpEndPoints
        {
            get
            {
                IPAddress[] ips = Dns.GetHostAddresses(Hostname);
                int len = ips.Length;
                IPEndPoint[] res = new IPEndPoint[len];
                for (int i = 0; i < len; res[i] = new(ips[i], Port), i++) ;
                return res;
            }
        }

        /// <summary>
        /// Structure size in bytes
        /// </summary>
        public int StructureSize
        {
            [TargetedPatchingOptOut("Tiny method")]
            get => Encoding.Unicode.GetByteCount(Hostname) + sizeof(ushort);
        }

        /// <summary>
        /// Resolve IP endpoints for the hostname
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>IP endpoints</returns>
        public async IAsyncEnumerable<IPEndPoint> ResolveIpEndPointsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (IPAddress ip in await Dns.GetHostAddressesAsync(Hostname, cancellationToken).DynamicContext()) yield return new(ip, Port);
        }

        /// <summary>
        /// Resolve all IP endpoints for the hostname
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>IP endpoints</returns>
        public async Task<IPEndPoint[]> ResolveAllIpEndPointsAsync(CancellationToken cancellationToken = default)
        {
            IPAddress[] ips = await Dns.GetHostAddressesAsync(Hostname, cancellationToken).DynamicContext();
            int len = ips.Length;
            IPEndPoint[] res = new IPEndPoint[len];
            for (int i = 0; i < len; res[i] = new(ips[i], Port), i++) ;
            return res;
        }

        /// <summary>
        /// Get the bytes for this host endpoint
        /// </summary>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public byte[] ToBytes()
        {
            byte[] res = new byte[StructureSize];
            ToBytes(res);
            return res;
        }

        /// <summary>
        /// Get the bytes for this host endpoint
        /// </summary>
        /// <param name="buffer">Buffer (must fit <see cref="StructureSize"/>)</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public int ToBytes(in Span<byte> buffer)
        {
            if (buffer.Length < MIN_BUFFER_LEN) throw new OutOfMemoryException();
            byte[] hostname = Encoding.Unicode.GetBytes(Hostname);
            int len = hostname.Length + sizeof(ushort);
            if (buffer.Length < len) throw new OutOfMemoryException();
            hostname.AsSpan().CopyTo(buffer);
            Port.GetBytes(buffer[hostname.Length..]);
            return len;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public override string ToString() => $"{Hostname}:{Port}";

        /// <summary>
        /// Parse a host endpoint
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Host endpoint</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static HostEndPoint Parse(in string str)
        {
            string[] info = str.Split(':');
            if (info.Length != 2) throw new InvalidDataException();
            return new(info[0], int.Parse(info[1]));
        }

        /// <summary>
        /// Try parsing a host endpoint
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="result">Host endpoint</param>
        /// <returns>If succeeded</returns>
        public static bool TryParse(in string str, out HostEndPoint result)
        {
            if (!str.Contains(':'))
            {
                result = default;
                return false;
            }
            string[] info = str.Split(':');
            if (info.Length != 2)
            {
                result = default;
                return false;
            }
            if (!int.TryParse(info[1], out int port))
            {
                result = default;
                return false;
            }
            if (port < 0 || port > ushort.MaxValue)
            {
                result = default;
                return false;
            }
            if (Uri.CheckHostName(info[0]) != UriHostNameType.Dns)
            {
                result = default;
                return false;
            }
            result = new(info[0], port);
            return true;
        }

        /// <inheritdoc/>
        public IEnumerator<IPEndPoint> GetEnumerator() => ResolvedIpEndPoints.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => ResolvedIpEndPoints.GetEnumerator();

        /// <inheritdoc/>
        public IAsyncEnumerator<IPEndPoint> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => ResolveIpEndPointsAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);

        /// <summary>
        /// Cast as bytes
        /// </summary>
        /// <param name="hep"><see cref="HostEndPoint"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator byte[](in HostEndPoint hep) => hep.ToBytes();

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator HostEndPoint(in byte[] buffer) => new(buffer);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator HostEndPoint(in Span<byte> buffer) => new(buffer);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator HostEndPoint(in ReadOnlySpan<byte> buffer) => new(buffer);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator HostEndPoint(in Memory<byte> buffer) => new(buffer.Span);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator HostEndPoint(in ReadOnlyMemory<byte> buffer) => new(buffer.Span);

        /// <summary>
        /// Cast as port number
        /// </summary>
        /// <param name="hep"><see cref="HostEndPoint"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in HostEndPoint hep) => hep.Port;

        /// <summary>
        /// Cast as hostname
        /// </summary>
        /// <param name="hep"><see cref="HostEndPoint"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator string(in HostEndPoint hep) => hep.Hostname;

        /// <summary>
        /// Cast as first IP endpoint
        /// </summary>
        /// <param name="hep"><see cref="HostEndPoint"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator IPEndPoint?(in HostEndPoint hep) => hep.ResolvedIpEndPoints.FirstOrDefault();
    }
}
