namespace wan24.Core
{
    /// <summary>
    /// Interface for an object which does opt in/out
    /// </summary>
    public interface IOptInOut
    {
        /// <summary>
        /// Opt in/out
        /// </summary>
        OptInOut Opt { get; }
    }
}
