using System.ComponentModel.DataAnnotations;
using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// RGB 24 bit unsigned integer validation attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Parameter|AttributeTargets.ReturnValue, AllowMultiple = false)]
    public class RgbAttribute : ValidationAttributeBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RgbAttribute() : base() { }

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static RgbAttribute Instance { get; } = new();

        /// <summary>
        /// Sanitize?
        /// </summary>
        public bool Sanitize { get; set; }

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return null;
            if (value is not int rgb) return this.CreateValidationResult($"24 bit integer ({typeof(int)}) value expected ({value.GetType()} given)", validationContext);
            if (!Sanitize && !Rgb.IsValidRgbInt24(rgb)) return this.CreateValidationResult($"24 bit integer value 0-{Rgb.MAX_24BIT_RGB} required", validationContext);
            return null;
        }
    }
}
