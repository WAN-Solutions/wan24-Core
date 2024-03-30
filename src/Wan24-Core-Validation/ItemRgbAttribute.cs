using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// Item RGB 24 bit integer validation attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class ItemRgbAttribute : ItemValidationAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ItemRgbAttribute() : base(RgbAttribute.Instance) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="target">Validation target</param>
        public ItemRgbAttribute(ItemValidationTargets target) : base(target, RgbAttribute.Instance) { }
    }
}
