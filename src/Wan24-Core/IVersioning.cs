namespace wan24.Core
{
    /// <summary>
    /// Interface which is being used for versioning
    /// </summary>
    public interface IVersioning
    {
        /// <summary>
        /// Min. included version
        /// </summary>
        int FromVersion { get; }
        /// <summary>
        /// Max. included version
        /// </summary>
        int ToVersion { get; }
    }
}
