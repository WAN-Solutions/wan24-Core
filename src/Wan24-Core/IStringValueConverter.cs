using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Interface for a type which supports display string/value conversion
    /// </summary>
    public interface IStringValueConverter
    {
        /// <summary>
        /// Get the display string for this object
        /// </summary>
        string DisplayString { get; }
        /// <summary>
        /// Create a instance from a display string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="value">Value</param>
        /// <returns>Succeed?</returns>
        static bool TryParse(string? str, [NotNullWhen(returnValue: true)] out IStringValueConverter? value) => throw new NotImplementedException();
    }

    /// <summary>
    /// Interface for a type which supports display string/value conversion
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public interface IStringValueConverter<T> : IStringValueConverter
    {
        /// <summary>
        /// Create a instance from a display string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="value">Value</param>
        /// <returns>Succeed?</returns>
        static bool TryParse(string? str, [NotNullWhen(returnValue: true)] out T? value) => throw new NotImplementedException();
    }
}
