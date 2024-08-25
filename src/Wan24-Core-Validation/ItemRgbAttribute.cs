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
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class ItemRgbAttribute(ItemValidationTargets target) : ItemValidationAttribute(target, RgbAttribute.Instance)
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ItemRgbAttribute() : this(ItemValidationTargets.Item) { }
    }
}
