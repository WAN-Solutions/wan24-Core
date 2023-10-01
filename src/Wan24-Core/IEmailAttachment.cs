namespace wan24.Core
{
    /// <summary>
    /// Interface for an email attachment
    /// </summary>
    public interface IEmailAttachment : IDisposableObject
    {
        /// <summary>
        /// Filename
        /// </summary>
        string FileName { get; }
        /// <summary>
        /// MIME type
        /// </summary>
        string MimeType { get; }
        /// <summary>
        /// Stream
        /// </summary>
        Stream Stream { get; }
    }
}
