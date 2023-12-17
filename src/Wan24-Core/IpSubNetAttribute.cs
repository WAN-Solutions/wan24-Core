using System.ComponentModel.DataAnnotations;

namespace wan24.Core
{
    /// <summary>
    /// IP sub-net validation attribute (for validating <see cref="string"/> or <see cref="IpSubNet"/> value properties)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="allowedIpSubNets">Allowed IP sub-nets (CIDR notation; the value needs to fit into one of these; if none are given, the value only needs to be a valid sub-net)</param>
    public class IpSubNetAttribute(params string[] allowedIpSubNets) : ValidationAttribute()
    {
        /// <summary>
        /// Allowed IP sub-nets (CIDR notation; the value needs to fit into one of these; if none are given, the value only needs to be a valid sub-net)
        /// </summary>
        public ReadOnlyMemory<IpSubNet> AllowedIpSubnets { get; } = allowedIpSubNets.Select(subNet => new IpSubNet(subNet)).ToArray();

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
            IpSubNet net;
            if (value is string subNet)
            {
                if (!IpSubNet.TryParse(subNet, out net))
                    return new(
                        ErrorMessage ?? (validationContext.MemberName is null ? $"Invalid IP sub-net value" : $"{validationContext.MemberName}: Invalid IP sub-net value"),
                        validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                        );
            }
            else if (value is IpSubNet ipSubNet)
            {
                net = ipSubNet;
            }
            else
            {
                return new(
                    ErrorMessage ?? (validationContext.MemberName is null ? $"IP sub-net value as {typeof(string)} or {typeof(IpSubNet)} expected" : $"{validationContext.MemberName}: IP sub-net value as {typeof(string)} or {typeof(IpSubNet)} expected"),
                    validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                    );
            }
            if (!AllowIPv4 && net.IsIPv4)
                return new(
                    ErrorMessage ?? (validationContext.MemberName is null ? $"IPv6 IP sub-net required" : $"{validationContext.MemberName}: IPv6 sub-net required"),
                    validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                    );
            if (!AllowIPv6 && !net.IsIPv4)
                return new(
                    ErrorMessage ?? (validationContext.MemberName is null ? $"IPv4 IP sub-net required" : $"{validationContext.MemberName}: IPv4 sub-net required"),
                    validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                    );
            if (AllowedIpSubnets.Length == 0) return null;
            ReadOnlySpan<IpSubNet> subNets = AllowedIpSubnets.Span;
            int denied = 0;
            for (int i = 0, len = AllowedIpSubnets.Length; i < len; i++)
                if ((net & subNets[i]) != net)
                    denied++;
            if (denied == subNets.Length)
                return new(
                    ErrorMessage ?? (validationContext.MemberName is null ? $"Denied IP sub-net value" : $"{validationContext.MemberName}: Denied IP sub-net value"),
                    validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                    );
            return null;
        }
    }
}
