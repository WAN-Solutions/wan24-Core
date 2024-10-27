using System.ComponentModel.DataAnnotations;
using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// Physical ethernet MAC address validation attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false)]
    public class MacAttribute() : ValidationAttributeBase()
    {
        /// <summary>
        /// Allow a group address
        /// </summary>
        public bool AllowGroup { get; set; } = true;

        /// <summary>
        /// Allow an individual address
        /// </summary>
        public bool AllowIndividual { get; set; } = true;

        /// <summary>
        /// Allow a local address
        /// </summary>
        public bool AllowLocal { get; set; } = true;

        /// <summary>
        /// Allow an universal address
        /// </summary>
        public bool AllowUniversal { get; set; } = true;

        /// <summary>
        /// Allow an IPv4 multicast address
        /// </summary>
        public bool AllowIPv4Multicast { get; set; } = true;

        /// <summary>
        /// Allow an IPv6 multicast address
        /// </summary>
        public bool AllowIPv6Multicast { get; set; } = true;

        /// <summary>
        /// Allow an IPv4 multicast address
        /// </summary>
        public bool RequireIPv4Multicast { get; set; }

        /// <summary>
        /// Allow an IPv6 multicast address
        /// </summary>
        public bool RequireIPv6Multicast { get; set; }

        /// <summary>
        /// Allow the broadcast address
        /// </summary>
        public bool AllowBroadcdast { get; set; } = true;

        /// <summary>
        /// Required IPv4 multicast address lower border (<c>0 - 0x00_7fffff</c>)
        /// </summary>
        public uint? IPv4MulticastLower { get; set; }

        /// <summary>
        /// Required IPv4 multicast address upper border (<c>0 - 0x00_7fffff</c>)
        /// </summary>
        public uint? IPv4MulticastUpper { get; set; }

        /// <summary>
        /// Required IPv6 multicast address lower border
        /// </summary>
        public uint? IPv6MulticastLower { get; set; }

        /// <summary>
        /// Required IPv6 multicast address upper border
        /// </summary>
        public uint? IPv6MulticastUpper { get; set; }

        /// <summary>
        /// Allowed OUIs (24 bit each)
        /// </summary>
        public uint[]? AllowedOUI { get; set; }

        /// <summary>
        /// Vendor specific ID lower border
        /// </summary>
        public uint? IdLower { get; set; }

        /// <summary>
        /// Vendor specific ID upper border
        /// </summary>
        public uint? IdUpper { get; set; }

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return null;
            // Get the MAC address structure
            MacAddress mac;
            switch (value)
            {
                case string address:
                    if (!MacAddress.TryParse(address, out mac))
                        return this.CreateValidationResult("Invalid MAC address string format", validationContext);
                    break;
                case ulong address:
                    mac = new(address);
                    break;
                case byte[] address:
                    if (address.Length != MacAddress.SERIALIZED_STRUCTURE_SIZE)
                        return this.CreateValidationResult("Invalid MAC address data length", validationContext);
                    mac = new(address);
                    break;
                default:
                    return this.CreateValidationResult($"Unsupported value type {value.GetType()}", validationContext);
            }
            // Validate allowed addresses
            if (!AllowGroup && mac.IsGroup) return this.CreateValidationResult("Group address isn't allowed", validationContext);
            if (!AllowIndividual && mac.IsIndividual) return this.CreateValidationResult("Individual address isn't allowed", validationContext);
            if (!AllowLocal && mac.IsLocal) return this.CreateValidationResult("Local address isn't allowed", validationContext);
            if (!AllowUniversal && mac.IsUniversal) return this.CreateValidationResult("Universal address isn't allowed", validationContext);
            if (!AllowIPv4Multicast && mac.IsIPv4Multicast) return this.CreateValidationResult("IPv4 multicast address isn't allowed", validationContext);
            if (!AllowIPv6Multicast && mac.IsIPv6Multicast) return this.CreateValidationResult("IPv6 multicast address isn't allowed", validationContext);
            if (!AllowBroadcdast && mac.IsBroadcast) return this.CreateValidationResult("Broadcast address isn't allowed", validationContext);
            // Validate required addresses
            if (RequireIPv4Multicast && !mac.IsIPv4Multicast) return this.CreateValidationResult("IPv4 multicast address required", validationContext);
            if (RequireIPv6Multicast && !mac.IsIPv6Multicast) return this.CreateValidationResult("IPv6 multicast address required", validationContext);
            // Validate IPv4 multicast ID range
            if (mac.IsIPv4Multicast && (IPv4MulticastLower.HasValue || IPv4MulticastUpper.HasValue))
            {
                if (IPv4MulticastLower.HasValue && mac.IPv4MulticastId < IPv4MulticastLower.Value)
                    return this.CreateValidationResult($"IPv4 multicast ID must be greater than or equal to {IPv4MulticastLower.Value}", validationContext);
                if (IPv4MulticastUpper.HasValue && mac.IPv4MulticastId > IPv4MulticastUpper.Value)
                    return this.CreateValidationResult($"IPv4 multicast ID must be lower than or equal to {IPv4MulticastUpper.Value}", validationContext);
            }
            // Validate IPv6 multicast ID range
            if (mac.IsIPv6Multicast && (IPv6MulticastLower.HasValue || IPv6MulticastUpper.HasValue))
            {
                if (IPv6MulticastLower.HasValue && mac.IPv6MulticastId < IPv6MulticastLower.Value)
                    return this.CreateValidationResult($"IPv6 multicast ID must be greater than or equal to {IPv6MulticastLower.Value}", validationContext);
                if (IPv6MulticastUpper.HasValue && mac.IPv6MulticastId > IPv6MulticastUpper.Value)
                    return this.CreateValidationResult($"IPv6 multicast ID must be lower than or equal to {IPv6MulticastUpper.Value}", validationContext);
            }
            // Validate OUI
            if (
                AllowedOUI is not null && 
                AllowedOUI.Length > 0 && 
                !mac.IsIPv4Multicast && 
                !mac.IsIPv6Multicast && 
                !mac.IsBroadcast && 
                !AllowedOUI.Enumerate().Contains(mac.OUI)
                )
                return this.CreateValidationResult("Invalid OUI", validationContext);
            // Validate vendor ID
            if((IdLower.HasValue||IdUpper.HasValue)&&!mac.IsIPv4Multicast&&!mac.IsIPv6Multicast&&!mac.IsBroadcast)
            {
                if (IdLower.HasValue && mac.VendorId < IdLower.Value)
                    return this.CreateValidationResult($"Vendor specific ID must be greater than or equal to {IdLower.Value}", validationContext);
                if (IdUpper.HasValue && mac.VendorId > IdUpper.Value)
                    return this.CreateValidationResult($"Vendor specific ID must be lower than or equal to {IdUpper.Value}", validationContext);
            }
            return null;
        }
    }
}
