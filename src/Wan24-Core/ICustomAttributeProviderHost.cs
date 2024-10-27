using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Interface for a custom attribute provider hosting object
    /// </summary>
    public interface ICustomAttributeProviderHost : ICustomAttributeProvider
    {
        /// <summary>
        /// Hosted custom attribute provider
        /// </summary>
        ICustomAttributeProvider Hosted { get; }
    }
}
