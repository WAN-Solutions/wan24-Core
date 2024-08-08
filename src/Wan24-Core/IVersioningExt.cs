namespace wan24.Core
{
    /// <summary>
    /// Interface for an object which supports extended versioning
    /// </summary>
    public interface IVersioningExt
    {
        /// <summary>
        /// Determine if included in a version
        /// </summary>
        /// <param name="version">Version</param>
        /// <returns>If included</returns>
        bool IsIncluded(in int version);
    }
}
