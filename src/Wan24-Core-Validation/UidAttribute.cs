using System.ComponentModel.DataAnnotations;
using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="Uid"/> validation attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class UidAttribute : ValidationAttributeBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UidAttribute() : base() { }

        /// <summary>
        /// Allow a future time?
        /// </summary>
        public bool AllowFutureTime { get; set; } = true;

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return null;
            byte[]? binary = value as byte[];
            Memory<byte>? memory = value is Memory<byte> mem ? mem : default(Memory<byte>?);
            ReadOnlyMemory<byte>? roMemory = value is ReadOnlyMemory<byte> roMem ? roMem : default(ReadOnlyMemory<byte>?);
            Uid? uid = null;
            if (binary is not null || memory.HasValue || roMemory.HasValue)
            {
                if (
                    (binary is not null && binary.Length < Uid.STRUCTURE_SIZE) ||
                    (memory.HasValue && memory.Value.Length < Uid.STRUCTURE_SIZE) ||
                    (roMemory.HasValue && roMemory.Value.Length < Uid.STRUCTURE_SIZE)
                    )
                    return this.CreateValidationResult($"Invalid length", validationContext);
                if (binary is not null) uid = binary;
                else if (memory.HasValue) uid = memory.Value.Span;
                else uid = roMemory!.Value.Span;
            }
            else
            {
                if (value is string str)
                {
                    if (!Uid.TryParse(str, out Uid uid2))
                        return this.CreateValidationResult($"Invalid UID string format", validationContext);
                    uid = uid2;
                }
                if (!uid.HasValue)
                    if (value is not string && value is not Uid)
                    {
                        return this.CreateValidationResult($"Invalid value ({typeof(Uid)} expected, {value.GetType()} given)", validationContext);
                    }
                    else
                    {
                        uid = (Uid)value;
                    }
            }
            if (!AllowFutureTime && uid.Value.Time > DateTime.UtcNow)
                return this.CreateValidationResult($"Invalid future time", validationContext);
            return null;
        }
    }
}
