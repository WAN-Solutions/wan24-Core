using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="UidExt"/> validation attribute
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="requiredId">(Minimum) Required ID</param>
    /// <param name="target">Validation target</param>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class ItemUidExtAttribute(int requiredId, ItemValidationTargets target) : ItemValidationAttribute(target, new UidExtAttribute(requiredId))
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requiredId">(Minimum) Required ID</param>
        public ItemUidExtAttribute(int requiredId) : this(requiredId, ItemValidationTargets.Item) { }

        /// <summary>
        /// Maximum Required ID
        /// </summary>
        public int? MaxRequiredId
        {
            get => ((UidExtAttribute)ValidationAttribute).MaxRequiredId;
            set => ((UidExtAttribute)ValidationAttribute).MaxRequiredId = value;
        }

        /// <summary>
        /// Allow an <see cref="Uid"/> value to be re-interpreted as <see cref="UidExt"/>?
        /// </summary>
        public bool AllowUid
        {
            get => ((UidExtAttribute)ValidationAttribute).AllowUid;
            set => ((UidExtAttribute)ValidationAttribute).AllowUid = value;
        }
    }
}
