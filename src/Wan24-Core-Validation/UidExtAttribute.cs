using System.ComponentModel.DataAnnotations;
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
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class UidExtAttribute(int requiredId) : UidAttribute()
    {
        /// <summary>
        /// (Minimum) Required ID
        /// </summary>
        public int RequiredId { get; } = requiredId;

        /// <summary>
        /// Maximum Required ID
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
                ReadOnlyMemory<byte> roMemory => roMemory.Span,
                string str => str,
                _ => default
            };
            if (uid == default)
            {
                if (!AllowUid) return this.CreateValidationResult($"UID not allowed", validationContext);
                using RentedMemoryRef<byte> buffer = new(UidExt.STRUCTURE_SIZE);
                Span<byte> bufferSpan = buffer.Span;
                ((Uid)value).GetBytes(bufferSpan);
                uid = bufferSpan;
            }
            if (uid.Id < RequiredId || (MaxRequiredId.HasValue && uid.Id > MaxRequiredId.Value) || (!MaxRequiredId.HasValue && uid.Id != RequiredId))
                return this.CreateValidationResult($"Invalid ID", validationContext);
            return null;
        }
    }
}
