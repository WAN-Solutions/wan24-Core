using System.ComponentModel.DataAnnotations;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="Uid"/> validation attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class UidAttribute : ValidationAttribute
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
                    return new(
                        ErrorMessage ?? (validationContext.MemberName is null ? $"Invalid length" : $"{validationContext.MemberName}: Invalid length"),
                        validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                        );
                if (binary is not null) uid = binary;
                else if (memory.HasValue) uid = memory.Value.Span;
                else uid = roMemory!.Value.Span;
            }
            else if (value is string str && !Uid.TryParse(str, out uid))
            {
                return new(
                    ErrorMessage ?? (validationContext.MemberName is null ? $"Invalid UID string format" : $"{validationContext.MemberName}: Invalid UID string format"),
                    validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                    );
            }
            else if (value is not Uid)
            {
                return new(
                    ErrorMessage ?? (validationContext.MemberName is null ? $"Invalid value ({typeof(Uid)} expected, {value.GetType()} given)" : $"{validationContext.MemberName}: Invalid value ({typeof(Uid)} expected, {value.GetType()} given)"),
                    validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                    );
            }
            else if (!uid.HasValue)
            {
                uid = (Uid)value;
            }
            if (!AllowFutureTime && uid.Value.Time > DateTime.UtcNow)
                return new(
                    ErrorMessage ?? (validationContext.MemberName is null ? $"Invalid future time" : $"{validationContext.MemberName}: Invalid future time"),
                    validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                    );
            return null;
        }
    }
}
