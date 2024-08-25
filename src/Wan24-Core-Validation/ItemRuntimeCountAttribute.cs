using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// Runtime count limitation attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class ItemRuntimeCountLimitAttribute : ItemValidationAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxGetter">Maximum getter static property</param>
        /// <param name="minGetter">Minimum getter static property</param>
        /// <param name="target">Validation target</param>
        public ItemRuntimeCountLimitAttribute(string maxGetter, string? minGetter = null, ItemValidationTargets target = ItemValidationTargets.Item)
            : base(target, new RuntimeCountLimitAttribute(maxGetter, minGetter))
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxGetter">Maximum getter static property</param>
        /// <param name="min">Minimum</param>
        /// <param name="target">Validation target</param>
        public ItemRuntimeCountLimitAttribute(string maxGetter, long min, ItemValidationTargets target = ItemValidationTargets.Item)
            : base(target, new RuntimeCountLimitAttribute(maxGetter, min))
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="attr">Attribute instance to use</param>
        /// <param name="target">Validation target</param>
        protected ItemRuntimeCountLimitAttribute(in RuntimeCountLimitAttribute attr, ItemValidationTargets target = ItemValidationTargets.Item)
            : base(target, attr)
        { }
    }
}
