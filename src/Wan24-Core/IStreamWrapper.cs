namespace wan24.Core
{
    /// <summary>
    /// Interface for a stream wrapper
    /// </summary>
    public interface IStreamWrapper : IStream, IStatusProvider
    {
        /// <summary>
        /// Wrapped base stream
        /// </summary>
        Stream BaseStream { get; }
        /// <summary>
        /// Leave the base stream open when disposing?
        /// </summary>
        bool LeaveOpen { get; set; }
    }
}
