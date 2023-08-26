using System.ComponentModel.DataAnnotations;

namespace wan24.Core
{
    /// <summary>
    /// Host endpoint validation attribute (for validating <see cref="string"/> or <see cref="HostEndPoint"/> value properties)
    /// </summary>
    public class HostEndPointAttribute : ValidationAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public HostEndPointAttribute() : base() { }

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null || value is HostEndPoint) return null;
            if (value is string endpoint && !HostEndPoint.TryParse(endpoint, out _))
                return new(
                    ErrorMessage ?? (validationContext.MemberName is null ? $"Invalid host endpoint value" : $"{validationContext.MemberName}: Invalid host endpoint value"),
                    validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                    );
            return new(
                ErrorMessage ?? (validationContext.MemberName is null ? $"Host endpoint value as {typeof(string)} or {typeof(HostEndPoint)} expected" : $"{validationContext.MemberName}: Host endpoint value as {typeof(string)} or {typeof(HostEndPoint)} expected"),
                validationContext.MemberName is null ? null : new string[] { validationContext.MemberName }
                );
        }
    }
}
