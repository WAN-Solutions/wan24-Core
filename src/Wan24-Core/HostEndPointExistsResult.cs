using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Result of a check if a <see cref="HostEndPoint"/> exists
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct HostEndPointExistsResult
    {
        /// <summary>
        /// If exists
        /// </summary>
        public readonly bool Exists;
        /// <summary>
        /// Endpoint
        /// </summary>
        public readonly HostEndPoint EndPoint;
        /// <summary>
        /// Exception
        /// </summary>
        public readonly Exception? Exception;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="endPoint">Endpoint</param>
        /// <param name="exists">If exists</param>
        /// <param name="exception">Exception</param>
        public HostEndPointExistsResult(in HostEndPoint endPoint, in bool exists = true, in Exception? exception = null)
        {
            Exists = exists;
            EndPoint = endPoint;
            Exception = exception;
        }

        /// <summary>
        /// Cast as <see cref="Exists"/>
        /// </summary>
        /// <param name="result"><see cref="HostEndPointExistsResult"/></param>
        public static implicit operator bool(in HostEndPointExistsResult result) => result.Exists;

        /// <summary>
        /// Cast as <see cref="EndPoint"/>
        /// </summary>
        /// <param name="result"><see cref="HostEndPointExistsResult"/></param>
        public static implicit operator HostEndPoint(in HostEndPointExistsResult result) => result.EndPoint;
    }
}
