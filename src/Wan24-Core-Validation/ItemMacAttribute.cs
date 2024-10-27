using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// Physical ethernet MAC address item validation attribute
    /// </summary>
    /// <param name="target">Validation target</param>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class ItemMacAttribute(ItemValidationTargets target) : ItemValidationAttribute(target, new MacAttribute())
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ItemMacAttribute() : this(ItemValidationTargets.Item) { }

        /// <summary>
        /// Base validation attribute
        /// </summary>
        public MacAttribute MacAttribute => (MacAttribute)ValidationAttribute;

        /// <summary>
        /// Allow a group address
        /// </summary>
        public bool AllowGroup
        {
            get => MacAttribute.AllowGroup;
            set => MacAttribute.AllowGroup = value;
        }

        /// <summary>
        /// Allow an individual address
        /// </summary>
        public bool AllowIndividual
        {
            get => MacAttribute.AllowIndividual;
            set => MacAttribute.AllowIndividual = value;
        }

        /// <summary>
        /// Allow a local address
        /// </summary>
        public bool AllowLocal
        {
            get => MacAttribute.AllowLocal;
            set => MacAttribute.AllowLocal = value;
        }

        /// <summary>
        /// Allow an universal address
        /// </summary>
        public bool AllowUniversal
        {
            get => MacAttribute.AllowUniversal;
            set => MacAttribute.AllowUniversal = value;
        }

        /// <summary>
        /// Allow an IPv4 multicast address
        /// </summary>
        public bool AllowIPv4Multicast
        {
            get => MacAttribute.AllowIPv4Multicast;
            set => MacAttribute.AllowIPv4Multicast = value;
        }

        /// <summary>
        /// Allow an IPv6 multicast address
        /// </summary>
        public bool AllowIPv6Multicast
        {
            get => MacAttribute.AllowIPv6Multicast;
            set => MacAttribute.AllowIPv6Multicast = value;
        }

        /// <summary>
        /// Allow an IPv4 multicast address
        /// </summary>
        public bool RequireIPv4Multicast
        {
            get => MacAttribute.RequireIPv4Multicast;
            set => MacAttribute.RequireIPv4Multicast = value;
        }

        /// <summary>
        /// Allow an IPv6 multicast address
        /// </summary>
        public bool RequireIPv6Multicast
        {
            get => MacAttribute.RequireIPv6Multicast;
            set => MacAttribute.RequireIPv6Multicast = value;
        }

        /// <summary>
        /// Allow the broadcast address
        /// </summary>
        public bool AllowBroadcdast
        {
            get => MacAttribute.AllowBroadcdast;
            set => MacAttribute.AllowBroadcdast = value;
        }

        /// <summary>
        /// Required IPv4 multicast address lower border (<c>0 - 0x00_7fffff</c>)
        /// </summary>
        public uint? IPv4MulticastLower
        {
            get => MacAttribute.IPv4MulticastLower;
            set => MacAttribute.IPv4MulticastLower = value;
        }

        /// <summary>
        /// Required IPv4 multicast address upper border (<c>0 - 0x00_7fffff</c>)
        /// </summary>
        public uint? IPv4MulticastUpper
        {
            get => MacAttribute.IPv4MulticastUpper;
            set => MacAttribute.IPv4MulticastUpper = value;
        }

        /// <summary>
        /// Required IPv6 multicast address lower border
        /// </summary>
        public uint? IPv6MulticastLower
        {
            get => MacAttribute.IPv6MulticastLower;
            set => MacAttribute.IPv6MulticastLower = value;
        }

        /// <summary>
        /// Required IPv6 multicast address upper border
        /// </summary>
        public uint? IPv6MulticastUpper
        {
            get => MacAttribute.IPv6MulticastUpper;
            set => MacAttribute.IPv6MulticastUpper = value;
        }

        /// <summary>
        /// Allowed OUIs (24 bit each)
        /// </summary>
        public uint[]? AllowedOUI
        {
            get => MacAttribute.AllowedOUI;
            set => MacAttribute.AllowedOUI = value;
        }

        /// <summary>
        /// Vendor specific ID lower border
        /// </summary>
        public uint? IdLower
        {
            get => MacAttribute.IdLower;
            set => MacAttribute.IdLower = value;
        }

        /// <summary>
        /// Vendor specific ID upper border
        /// </summary>
        public uint? IdUpper
        {
            get => MacAttribute.IdUpper;
            set => MacAttribute.IdUpper = value;
        }
    }
}
