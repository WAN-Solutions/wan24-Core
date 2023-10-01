namespace wan24.Core
{
    /// <summary>
    /// Interface for an email
    /// </summary>
    public interface IEmail : IDisposableObject
    {
        /// <summary>
        /// Sender email address
        /// </summary>
        string FromEmail { get; }
        /// <summary>
        /// Recipient email address
        /// </summary>
        string ToEmail { get; }
        /// <summary>
        /// Subject
        /// </summary>
        string Subject { get; }
        /// <summary>
        /// Text body
        /// </summary>
        string? TextBody { get; }
        /// <summary>
        /// HTML body
        /// </summary>
        string? HtmlBody { get; }
        /// <summary>
        /// Attached files
        /// </summary>
        IReadOnlyCollection<IEmailAttachment> Attachments { get; }
    }
}
