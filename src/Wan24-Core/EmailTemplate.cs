namespace wan24.Core
{
    /// <summary>
    /// Email template
    /// </summary>
    public class EmailTemplate : EmailTemplateBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="subject">Subject</param>
        /// <param name="text">Text body</param>
        /// <param name="html">HTML body</param>
        /// <param name="attachments">Attachments</param>
        public EmailTemplate(in string subject, in string? text = null, in string? html = null, params IEmailAttachment[] attachments)
            : this(DefaultEmailType ?? throw new InvalidOperationException("No default email type"), subject, text, html, attachments)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="emailType">Email type (non-abstract <see cref="IEmail"/> class type)</param>
        /// <param name="subject">Subject</param>
        /// <param name="text">Text body</param>
        /// <param name="html">HTML body</param>
        /// <param name="attachments">Attachments</param>
        public EmailTemplate(in Type emailType, in string subject, in string? text = null, in string? html = null, params IEmailAttachment[] attachments)
            : base(subject, text, html, attachments)
        {
            try
            {
                if (!typeof(IEmail).IsAssignableFrom(emailType) || emailType.IsValueType || emailType.IsAbstract || emailType.IsInterface)
                    throw new ArgumentException("Invalid email type", nameof(emailType));
                EmailType = emailType;
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Default email type
        /// </summary>
        public static Type? DefaultEmailType { get; set; }

        /// <summary>
        /// Email type
        /// </summary>
        public Type EmailType { get; }

        /// <inheritdoc/>
        protected override IEmail CreateEmail(string fromEmail, string toEmail, string subject, string? text, string? html, HashSet<IEmailAttachment> attachments)
            => (IEmail)EmailType.ConstructAuto(usePrivate: false, fromEmail, toEmail, subject, text, html, attachments);
    }
}
