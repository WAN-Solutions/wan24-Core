using System.ComponentModel.DataAnnotations;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="UidExt"/> validation attribute
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="requiredId">(Minimum) Required ID</param>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class UidExtAttribute(int requiredId) : UidAttribute()
    {
        /// <summary>
        /// (Minimum) Required ID
        /// </summary>
        public int RequiredId { get; } = requiredId;

        /// <summary>
        /// Maiximum Required ID
        /// </summary>
        public int? MaxRequiredId { get; set; }

        /// <summary>
        /// Allow an <see cref="Uid"/> value to be re-interpreted as <see cref="UidExt"/>?
        /// </summary>
        public bool AllowUid { get; set; }

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return null;
            if (base.IsValid(value, validationContext) is ValidationResult result) return result;
            UidExt uid = value switch
            {
                UidExt uidExt => uidExt,
                byte[] binary => binary,
                Memory<byte> memory => memory.Span,
                ReadOnlyMemory<byte> roMemory => roMemory,
                string str => str,
                _ => default
            };
            if (uid == default)
            {
                if (!AllowUid)
                    return new(
                        ErrorMessage ?? (validationContext.MemberName is null ? $"UID not allowed" : $"{validationContext.MemberName}: UID not allowed"),
                        validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                        );
                using RentedArrayRefStruct<byte> buffer = new(UidExt.STRUCTURE_SIZE);
                ((Uid)value).GetBytes(buffer.Span);
                uid = buffer.Span;
            }
            if ((MaxRequiredId.HasValue && (uid.Id < RequiredId || uid.Id > MaxRequiredId.Value)) || (!MaxRequiredId.HasValue && uid.Id != RequiredId))
                return new(
                    ErrorMessage ?? (validationContext.MemberName is null ? $"Invalid ID" : $"{validationContext.MemberName}: Invalid ID"),
                    validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                    );
            return null;
        }
    }
}
