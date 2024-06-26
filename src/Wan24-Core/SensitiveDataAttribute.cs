﻿namespace wan24.Core
{
    /// <summary>
    /// Attribute for sensitive data
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class SensitiveDataAttribute() : Attribute()
    {
        /// <summary>
        /// Can this attribute sanitize a sensitive value?
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
