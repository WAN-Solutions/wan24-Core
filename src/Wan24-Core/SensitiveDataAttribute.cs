namespace wan24.Core
{
    /// <summary>
    /// Attribute for sensitive data
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SensitiveDataAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SensitiveDataAttribute() : base() { }

        /// <summary>
        /// Can this attribute senitize a sensitive value?
        /// </summary>
        public virtual bool CanSanitizeValue => false;

        /// <summary>
        /// Sanitize a sensitive value
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="property">Property name</param>
        /// <param name="value">Value</param>
        /// <returns>Sanitized value</returns>
        public virtual object? CreateSanitizedValue(object obj, string property, object? value) => throw new NotImplementedException();
    }
}
