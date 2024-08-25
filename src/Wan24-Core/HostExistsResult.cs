using System.Net;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Result of a check if a hostname (<see cref="string"/>) or an IP (<see cref="IPAddress"/>) exists
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct HostExistsResult
    {
        /// <summary>
        /// If exists
        /// </summary>
        public readonly bool Exists;
        /// <summary>
        /// IP address
        /// </summary>
        public readonly IPAddress? IP;
        /// <summary>
        /// Exception
        /// </summary>
        public readonly Exception? Exception;
        /// <summary>
        /// Hostname
        /// </summary>
        public readonly string? Hostname;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hostname">Hostname</param>
        /// <param name="exists">If exists</param>
        /// <param name="exception">Exception</param>
        public HostExistsResult(in string hostname, in bool exists = true, in Exception? exception = null)
        {
            Exists = exists;
            Hostname = hostname;
            Exception = exception;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="exists">If exists</param>
        /// <param name="exception">Exception</param>
        public HostExistsResult(in IPAddress ip, in bool exists = true, in Exception? exception = null)
        {
            Exists = exists;
            IP = ip;
            Exception = exception;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => IP?.GetHashCode() ?? Hostname!.GetHashCode();

        /// <summary>
        /// Cast as <see cref="Exists"/>
        /// </summary>
        /// <param name="result"><see cref="HostExistsResult"/></param>
        public static implicit operator bool(in HostExistsResult result) => result.Exists;
    }
}
