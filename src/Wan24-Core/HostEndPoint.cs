using System.Collections;
using System.Diagnostics.CodeAnalysis;
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
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct HostEndPoint : IEnumerable<IPEndPoint>, IAsyncEnumerable<IPEndPoint>, ISerializeBinary<HostEndPoint>, ISerializeString<HostEndPoint>
    {
        /// <summary>
        /// Min. buffer length in bytes
        /// </summary>
        public const int MIN_BUFFER_LEN = 3;
        /// <summary>
        /// Max. buffer length in bytes
        /// </summary>
        public const int MAX_BUFFER_LEN = sizeof(ushort) + byte.MaxValue;
        /// <summary>
        /// Min. string length
        /// </summary>
        public const int MIN_STR_LEN = 4;
        /// <summary>
        /// Max. string length
        /// </summary>
        public const int MAX_STR_LEN = 5 + sizeof(char) + (byte.MaxValue << 1);

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
            if (buffer.Length > MAX_BUFFER_LEN) throw new InvalidDataException("Buffer too long");
            Hostname = Encoding.Unicode.GetString(buffer[..^2]);
            if (Uri.CheckHostName(Hostname) != UriHostNameType.Dns) throw new InvalidDataException("Invalid hostname");
            Port = buffer[^2..].ToUShort();
            if (Port < 0 || Port > ushort.MaxValue) throw new InvalidDataException("Invalid port number");
        }

        /// <inheritdoc/>
        public static int? MaxStructureSize => MAX_BUFFER_LEN;

        /// <inheritdoc/>
        public static int? MaxStringSize => MAX_STR_LEN;

        /// <inheritdoc/>
        int? ISerializeBinary.StructureSize => StructureSize;

        /// <inheritdoc/>
        int? ISerializeString.StringSize => Encoding.Unicode.GetByteCount(Hostname) + sizeof(char) + 5;

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

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public byte[] GetBytes()
        {
            byte[] res = new byte[StructureSize];
            GetBytes(res);
            return res;
        }

        /// <inheritdoc/>
        public int GetBytes(in Span<byte> buffer)
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
        public static implicit operator byte[](in HostEndPoint hep) => hep.GetBytes();

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

        /// <inheritdoc/>
        public static object DeserializeFrom(in ReadOnlySpan<byte> buffer) => new HostEndPoint(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out object? result)
        {
            try
            {
                if (buffer.Length < MIN_BUFFER_LEN || buffer.Length > MaxStructureSize!.Value)
                {
                    result = null;
                    return false;
                }
                string hostname = Encoding.Unicode.GetString(buffer[..^2]);
                if (Uri.CheckHostName(hostname) != UriHostNameType.Dns)
                {
                    result = null;
                    return false;
                }
                int port = buffer[^2..].ToUShort();
                if (port < 0 || port > ushort.MaxValue)
                {
                    result = null;
                    return false;
                }
                result = new HostEndPoint(hostname, port);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public static HostEndPoint DeserializeTypeFrom(in ReadOnlySpan<byte> buffer) => new(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeTypeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out HostEndPoint result)
        {
            try
            {
                if (buffer.Length < MIN_BUFFER_LEN || buffer.Length > MaxStructureSize!.Value)
                {
                    result = default;
                    return false;
                }
                string hostname = Encoding.Unicode.GetString(buffer[..^2]);
                if (Uri.CheckHostName(hostname) != UriHostNameType.Dns)
                {
                    result = default;
                    return false;
                }
                int port = buffer[^2..].ToUShort();
                if (port < 0 || port > ushort.MaxValue)
                {
                    result = default;
                    return false;
                }
                result = new HostEndPoint(hostname, port);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public static HostEndPoint Parse(in ReadOnlySpan<char> str)
        {
            if (str.Length < MIN_STR_LEN) throw new InvalidDataException("String too short");
            if (str.Length > MAX_STR_LEN) throw new InvalidDataException("String too long");
            int index = str.IndexOf(':');
            if (index < 0) throw new InvalidDataException("Invalid format");
            return new(new(str[..index]), int.Parse(str[(index + 1)..]));
        }

        /// <inheritdoc/>
        public static bool TryParse(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out HostEndPoint result)
        {
            if (str.Length < MIN_STR_LEN || str.Length > MAX_STR_LEN)
            {
                result = default;
                return false;
            }
            int index = str.IndexOf(':');
            if (index < 0)
            {
                result = default;
                return false;
            }
            if (!int.TryParse(str[(index + 1)..], out int port))
            {
                result = default;
                return false;
            }
            if (port < 0 || port > ushort.MaxValue)
            {
                result = default;
                return false;
            }
            string hostname = new(str[index..]);
            if (Uri.CheckHostName(hostname) != UriHostNameType.Dns)
            {
                result = default;
                return false;
            }
            result = new(hostname, port);
            return true;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object ParseObject(in ReadOnlySpan<char> str) => Parse(str);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool TryParseObject(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out object? result)
        {
            bool res;
            result = (res = TryParse(str, out HostEndPoint hep))
                ? hep
                : default;
            return res;
        }
    }
}
