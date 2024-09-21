using System.ComponentModel.DataAnnotations;
using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// Host endpoint validation attribute (for validating <see cref="string"/> or <see cref="HostEndPoint"/> value properties)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class HostEndPointAttribute() : ValidationAttributeBase()
    {
        /// <summary>
        /// If to check if the endpoint exists (you need to implement <see cref="CheckExists(HostEndPoint)"/> in order to use this feature!)
        /// </summary>
        public bool CheckIfExists { get; set; }

        /// <summary>
        /// If <see cref="CheckExists(HostEndPoint)"/> should use cached results
        /// </summary>
        public bool UseCache { get; set; } = true;

        /// <summary>
        /// Check if an IP/host endpoint exists
        /// </summary>
        /// <param name="hostEndPoint">Host endpoint (may be <see langword="default"/>)</param>
        /// <returns>If exists</returns>
        /// <exception cref="NotImplementedException">You need to implement the <see cref="CheckExists(HostEndPoint)"/> method in order to be able to use 
        /// <c>CheckIfExists</c></exception>
        protected virtual bool CheckExists(HostEndPoint hostEndPoint) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null || value is HostEndPoint) return null;
            if (value is not string endpoint)
                return this.CreateValidationResult($"Host endpoint value as {typeof(string)} or {typeof(HostEndPoint)} expected", validationContext);
            if (!HostEndPoint.TryParse(endpoint, out HostEndPoint hostEndpoint)) return this.CreateValidationResult("Invalid host endpoint value", validationContext);
            if (CheckIfExists && !CheckExists(hostEndpoint)) return this.CreateValidationResult($"Host endpoint {hostEndpoint} not found", validationContext);
            return null;
        }
    }
}
