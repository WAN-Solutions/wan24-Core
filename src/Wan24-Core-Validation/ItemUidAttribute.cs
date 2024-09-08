using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="Uid"/> validation attribute
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="target">Validation target</param>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class ItemUidAttribute(ItemValidationTargets target) : ItemValidationAttribute(target, new UidAttribute())
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ItemUidAttribute() : this(ItemValidationTargets.Item) { }

        /// <summary>
        /// Allow a future time?
        /// </summary>
        public bool AllowFutureTime
        {
            get => ((UidAttribute)ValidationAttribute).AllowFutureTime;
            set => ((UidAttribute)ValidationAttribute).AllowFutureTime = value;
        }
    }
}
