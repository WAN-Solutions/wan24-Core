using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// Host endpoint validation attribute (for validating <see cref="string"/> or <see cref="HostEndPoint"/> value properties)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="target">Validation target</param>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class ItemHostEndPointAttribute(ItemValidationTargets target) : ItemValidationAttribute(target, RgbAttribute.Instance)
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ItemHostEndPointAttribute() : this(ItemValidationTargets.Item) { }
    }
}
