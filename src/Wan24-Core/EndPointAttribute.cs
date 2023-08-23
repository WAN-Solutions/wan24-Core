using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;

namespace wan24.Core
{
    /// <summary>
    /// Endpoint validation attribute (for validating <see cref="string"/> as <see cref="IPEndPoint"/> or <see cref="HostEndPoint"/>)
    /// </summary>
    public class EndPointAttribute : ValidationAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="allowedIpSubNets">Allowed IP sub-nets (CIDR notation; the value needs to fit into one of these; if none are given, the value only needs to be a valid sub-net)</param>
        public EndPointAttribute(params string[] allowedIpSubNets) : base() => AllowedIpSubnets = allowedIpSubNets.Select(subNet => new IpSubNet(subNet)).ToArray();

        /// <summary>
        /// Allowed IP sub-nets (CIDR notation; the value needs to fit into one of these; if none are given, the value only needs to be a valid sub-net)
        /// </summary>
        public ReadOnlyMemory<IpSubNet> AllowedIpSubnets { get; }

        /// <summary>
        /// Allow an IPv4 sub-net?
        /// </summary>
        public bool AllowIPv4 { get; set; } = true;

        /// <summary>
        /// Allow an IPv6 sub-net?
        /// </summary>
        public bool AllowIPv6 { get; set; } = true;

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (!AllowIPv4 && !AllowIPv6) throw new InvalidOperationException();
            if (value is null) return null;
            if (value is not string endpoint)
                return new(
                    ErrorMessage ?? (validationContext.MemberName is null ? $"Endpoint value as {typeof(string)} expected" : $"{validationContext.MemberName}: Endpoint value as {typeof(string)} expected"),
                    validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                    );
            if (!IPEndPoint.TryParse(endpoint, out IPEndPoint? ipEndpoint) && !HostEndPoint.TryParse(endpoint, out _))
                return new(
                    ErrorMessage ?? (validationContext.MemberName is null ? $"Invalid host/IP endpoint value" : $"{validationContext.MemberName}: Invalid host/IP endpoint value"),
                    validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                    );
            if (ipEndpoint is null) return null;
            if (!AllowIPv4 && ipEndpoint.Address.AddressFamily == AddressFamily.InterNetwork)
                return new(
                    ErrorMessage ?? (validationContext.MemberName is null ? $"IPv6 endpoint required" : $"{validationContext.MemberName}: IPv6 endpoint required"),
                    validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                    );
            else if (!AllowIPv6 && ipEndpoint.Address.AddressFamily == AddressFamily.InterNetworkV6)
                return new(
                    ErrorMessage ?? (validationContext.MemberName is null ? $"IPv4 endpoint required" : $"{validationContext.MemberName}: IPv4 endpoint required"),
                    validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                    );
            else if (AllowedIpSubnets.Length != 0)
            {
                ReadOnlySpan<IpSubNet> subNets = AllowedIpSubnets.Span;
                int denied = 0;
                for (int i = 0, len = AllowedIpSubnets.Length; i < len; i++)
                    if (ipEndpoint.Address != subNets[i])
                        denied++;
                if (denied == subNets.Length)
                    return new(
                        ErrorMessage ?? (validationContext.MemberName is null ? $"IP endpoint is in denied sub-net" : $"{validationContext.MemberName}: IP endpoint is in denied sub-net"),
                        validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                        );
            }
            return null;
        }
    }
}
