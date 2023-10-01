using System.Collections.ObjectModel;

namespace wan24.Core
{
    /// <summary>
    /// Interface for an email template
    /// </summary>
    public interface IEmailTemplate : IDisposableObject
    {
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
        /// Attachments
        /// </summary>
        ReadOnlyCollection<IEmailAttachment> Attachments { get; }
        /// <summary>
        /// Create an email from this template
        /// </summary>
        /// <param name="fromEmail">Sender email address</param>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="parserData">Parser data</param>
        /// <param name="attachments">Attachments</param>
        /// <returns>Email</returns>
        IEmail CreateEmail(string fromEmail, string toEmail, Dictionary<string, string>? parserData = null, params IEmailAttachment[] attachments);
        /// <summary>
        /// Create an email from this template
        /// </summary>
        /// <param name="fromEmail">Sender email address</param>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="parserData">Parser data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="attachments">Attachments</param>
        /// <returns>Email</returns>
        Task<IEmail> CreateEmailAsync(
            string fromEmail, 
            string toEmail,
            Dictionary<string, string>? parserData = null, 
            CancellationToken cancellationToken = default, 
            params IEmailAttachment[] attachments
            );
    }
}
