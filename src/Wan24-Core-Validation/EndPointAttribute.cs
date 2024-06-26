﻿using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// Endpoint validation attribute (for validating <see cref="string"/> as <see cref="IPEndPoint"/> or <see cref="HostEndPoint"/>)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="allowedIpSubNets">Allowed IP sub-nets (CIDR notation; the value needs to fit into one of these; if none are given, the value only needs to be a valid 
    /// sub-net)</param>
    public class EndPointAttribute(params string[] allowedIpSubNets) : ValidationAttributeBase()
    {
        /// <summary>
        /// Allowed IP sub-nets (CIDR notation; the value needs to fit into one of these; if none are given, the value only needs to be a valid sub-net)
        /// </summary>
        public FrozenSet<IpSubNet> AllowedIpSubnets { get; } = allowedIpSubNets.Select(subNet => new IpSubNet(subNet)).ToFrozenSet();

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
            if (value is not string endpoint) return this.CreateValidationResult($"Endpoint value as {typeof(string)} expected", validationContext);
            if (!IPEndPoint.TryParse(endpoint, out IPEndPoint? ipEndpoint) && !HostEndPoint.TryParse(endpoint, out _))
                return this.CreateValidationResult($"Invalid host/IP endpoint value", validationContext);
            if (ipEndpoint is null) return null;
            if (!AllowIPv4 && ipEndpoint.Address.AddressFamily == AddressFamily.InterNetwork)
                return this.CreateValidationResult($"IPv6 endpoint required", validationContext);
            else if (!AllowIPv6 && ipEndpoint.Address.AddressFamily == AddressFamily.InterNetworkV6)
                return this.CreateValidationResult($"IPv4 endpoint required", validationContext);
            else if (AllowedIpSubnets.Count != 0)
            {
                int denied = 0;
                for (int i = 0, len = AllowedIpSubnets.Count; i < len; i++)
                    if (ipEndpoint.Address != AllowedIpSubnets.Items[i])
                        denied++;
                if (denied == AllowedIpSubnets.Count) return this.CreateValidationResult($"IP endpoint is in denied sub-net", validationContext);
            }
            return null;
        }
    }
}
