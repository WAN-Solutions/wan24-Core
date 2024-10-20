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

        /// <summary>
        /// If to check if the endpoint exists (you need to implement <see cref="CheckExists(IPEndPoint?, HostEndPoint)"/> in order to use this feature!)
        /// </summary>
        public bool CheckIfExists { get; set; }

        /// <summary>
        /// If <see cref="CheckExists(IPEndPoint?, HostEndPoint)"/> should use cached results
        /// </summary>
        public bool UseCache { get; set; } = true;

        /// <summary>
        /// Check if an IP/host endpoint exists
        /// </summary>
        /// <param name="ipEndPoint">IP endpoint</param>
        /// <param name="hostEndPoint">Host endpoint (may be <see langword="default"/>)</param>
        /// <returns>If exists</returns>
        /// <exception cref="NotImplementedException">You need to implement the <see cref="CheckExists(IPEndPoint?, HostEndPoint)"/> method in order to be able to use 
        /// <c>CheckIfExists</c></exception>
        protected virtual bool CheckExists(IPEndPoint? ipEndPoint, HostEndPoint hostEndPoint) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (!AllowIPv4 && !AllowIPv6) throw new InvalidOperationException();
            if (value is null) return null;
            if (value is not string endpoint) return this.CreateValidationResult($"Endpoint value as {typeof(string)} expected", validationContext);
            HostEndPoint hostEndpoint = default;
            if (!IPEndPoint.TryParse(endpoint, out IPEndPoint? ipEndpoint) && !HostEndPoint.TryParse(endpoint, out hostEndpoint))
                return this.CreateValidationResult("Invalid host/IP endpoint value", validationContext);
            if (CheckIfExists && (ipEndpoint is not null || hostEndpoint != default) && !CheckExists(ipEndpoint, hostEndpoint))
                return this.CreateValidationResult($"Endpoint {ipEndpoint ?? hostEndpoint} doesn't exist", validationContext);
            if (ipEndpoint is null) return null;
            if (!AllowIPv4 && ipEndpoint.Address.AddressFamily == AddressFamily.InterNetwork)
                return this.CreateValidationResult("IPv6 endpoint required", validationContext);
            else if (!AllowIPv6 && ipEndpoint.Address.AddressFamily == AddressFamily.InterNetworkV6)
                return this.CreateValidationResult("IPv4 endpoint required", validationContext);
            else if (AllowedIpSubnets.Count != 0)
            {
                int denied = 0;
                for (int i = 0, len = AllowedIpSubnets.Count; i < len; i++)
                    if (ipEndpoint.Address != AllowedIpSubnets.Items[i])
                        denied++;
                if (denied == AllowedIpSubnets.Count) return this.CreateValidationResult("IP endpoint is in denied sub-net", validationContext);
            }
            return null;
        }
    }
}
