using System.ComponentModel.DataAnnotations;

namespace wan24.Core
{
    /// <summary>
    /// Base class for an email
    /// </summary>
    public abstract class EmailBase : DisposableBase, IEmail
    {
        /// <summary>
        /// Attachments
        /// </summary>
        protected readonly HashSet<IEmailAttachment> _Attachments = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fromEmail">Sender email address</param>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Subject</param>
        /// <param name="text">Text body</param>
        /// <param name="html">HTML body</param>
        /// <param name="attachments">Attachments</param>
        protected EmailBase(
            in string fromEmail, 
            in string toEmail, 
            in string subject, 
            in string? text = null, 
            in string? html = null, 
            params IEmailAttachment[] attachments
            )
            : this()
        {
            FromEmail = fromEmail;
            ToEmail = toEmail;
            Subject = subject;
            TextBody = text;
            HtmlBody = html;
            _Attachments.AddRange(attachments);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected EmailBase() : base() { }

        /// <inheritdoc/>
        [EmailAddress]
        public virtual string FromEmail { get; } = null!;

        /// <inheritdoc/>
        [EmailAddress]
        public virtual string ToEmail { get; } = null!;

        /// <inheritdoc/>
        [RegularExpression(RegularExpressions.NO_NEW_LINE)]
        public virtual string Subject { get; } = null!;

        /// <inheritdoc/>
        public virtual string? TextBody { get; }

        /// <inheritdoc/>
        public virtual string? HtmlBody { get; }

        /// <inheritdoc/>
        public virtual IReadOnlyCollection<IEmailAttachment> Attachments => IfUndisposed(() => _Attachments.AsReadOnly());

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _Attachments.DisposeAll();
            _Attachments.Clear();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await _Attachments.DisposeAllAsync().DynamicContext();
            _Attachments.Clear();
        }
    }
}
