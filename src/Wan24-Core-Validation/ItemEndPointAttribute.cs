using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// Item RGB 24 bit integer validation attribute
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="target">Validation target</param>
    /// <param name="allowedIpSubNets">Allowed IP sub-nets (CIDR notation; the value needs to fit into one of these; if none are given, the value only needs to be a valid 
    /// sub-net)</param>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class ItemEndPointAttribute(ItemValidationTargets target, params string[] allowedIpSubNets) : ItemValidationAttribute(target, new EndPointAttribute(allowedIpSubNets))
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="allowedIpSubNets">Allowed IP sub-nets (CIDR notation; the value needs to fit into one of these; if none are given, the value only needs to be a valid 
        /// sub-net)</param>
        public ItemEndPointAttribute(params string[] allowedIpSubNets) : this(ItemValidationTargets.Item, allowedIpSubNets) { }

        /// <summary>
        /// Allow an IPv4 sub-net?
        /// </summary>
        public bool AllowIPv4
        {
            get => ((EndPointAttribute)ValidationAttribute).AllowIPv4;
            set => ((EndPointAttribute)ValidationAttribute).AllowIPv4 = value;
        }

        /// <summary>
        /// Allow an IPv6 sub-net?
        /// </summary>
        public bool AllowIPv6
        {
            get => ((EndPointAttribute)ValidationAttribute).AllowIPv6;
            set => ((EndPointAttribute)ValidationAttribute).AllowIPv6 = value;
        }
    }
}
