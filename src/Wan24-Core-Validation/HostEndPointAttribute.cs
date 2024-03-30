using System.ComponentModel.DataAnnotations;
using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// Host endpoint validation attribute (for validating <see cref="string"/> or <see cref="HostEndPoint"/> value properties)
    /// </summary>
    public class HostEndPointAttribute : ValidationAttributeBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public HostEndPointAttribute() : base() { }

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null || value is HostEndPoint) return null;
            if(value is not string endpoint)
                return this.CreateValidationResult($"Host endpoint value as {typeof(string)} or {typeof(HostEndPoint)} expected", validationContext);
            if (!HostEndPoint.TryParse(endpoint, out _)) return this.CreateValidationResult($"Invalid host endpoint value", validationContext);
            return null;
        }
    }
}
