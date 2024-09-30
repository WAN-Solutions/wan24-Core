using System.Collections.Immutable;

namespace wan24.Core
{
    /// <summary>
    /// Base class for an email template
    /// </summary>
    /// <typeparam name="T">Email type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="subject">Subject</param>
    /// <param name="text">Text body</param>
    /// <param name="html">HTML body</param>
    /// <param name="attachments">Attachments</param>
    public abstract class EmailTemplateBase<T>(in string subject, in string? text = null, in string? html = null, params IEmailAttachment[] attachments) : EmailTemplateBase(subject, text, html, attachments) where T : class, IEmail
    {
        /// <inheritdoc/>
        protected override IEmail CreateEmail(string fromEmail, string toEmail, string subject, string? text, string? html, HashSet<IEmailAttachment> attachments)
            => (T)typeof(T).ConstructAuto(usePrivate: false, fromEmail, toEmail, subject, text, html, attachments);
    }

    /// <summary>
    /// Base class for an email template
    /// </summary>
    public abstract class EmailTemplateBase : DisposableBase, IEmailTemplate
    {
        /// <summary>
        /// Attachments
        /// </summary>
        protected readonly HashSet<IEmailAttachment> _Attachments = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="subject">Subject</param>
        /// <param name="text">Text body</param>
        /// <param name="html">HTML body</param>
        /// <param name="attachments">Attachments</param>
        protected EmailTemplateBase(in string subject, in string? text = null, in string? html = null, params IEmailAttachment[] attachments) : this()
        {
            Subject = subject;
            TextBody = text;
            HtmlBody = html;
            _Attachments.AddRange(attachments);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected EmailTemplateBase() : base() { }

        /// <inheritdoc/>
        public virtual string Subject { get; } = null!;

        /// <inheritdoc/>
        public virtual string? TextBody { get; }

        /// <inheritdoc/>
        public virtual string? HtmlBody { get; }

        /// <inheritdoc/>
        public virtual ImmutableArray<IEmailAttachment> Attachments => IfUndisposed(() => _Attachments.AsReadOnly());

        /// <inheritdoc/>
        public virtual IEmail CreateEmail(string fromEmail, string toEmail, Dictionary<string, string>? parserData = null, params IEmailAttachment[] attachments)
        {
            HashSet<IEmailAttachment> emailAttachments = new(_Attachments.Count + attachments.Length);
            IEmailAttachment[]? att = null;
            try
            {
                (string subject, string? text, string? html, att) = GetData(parserData);
                emailAttachments.AddRange(attachments);
                return CreateEmail(fromEmail, toEmail, subject, text, html, emailAttachments);
            }
            catch
            {
                att?.DisposeAll();
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IEmail> CreateEmailAsync(
            string fromEmail,
            string toEmail,
            Dictionary<string, string>? parserData = null,
            CancellationToken cancellationToken = default,
            params IEmailAttachment[] attachments
            )
        {
            HashSet<IEmailAttachment> emailAttachments = new(_Attachments.Count + attachments.Length);
            IEmailAttachment[]? att = null;
            try
            {
                (string subject, string? text, string? html, att) = await GetDataAsync(parserData, cancellationToken).DynamicContext();
                emailAttachments.AddRange(attachments);
                return CreateEmail(fromEmail, toEmail, subject, text, html, emailAttachments);
            }
            catch
            {
                att?.DisposeAll();
                throw;
            }
        }

        /// <summary>
        /// Create an email
        /// </summary>
        /// <param name="fromEmail">Sender email address</param>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Subject</param>
        /// <param name="text">Text body</param>
        /// <param name="html">HTML body</param>
        /// <param name="attachments">Attachments</param>
        /// <returns></returns>
        protected abstract IEmail CreateEmail(string fromEmail, string toEmail, string subject, string? text, string? html, HashSet<IEmailAttachment> attachments);

        /// <summary>
        /// Get the data of this template for creating a new email instance
        /// </summary>
        /// <param name="parserData">Parser data</param>
        /// <returns>Template data</returns>
        protected virtual (string Subject, string? TextBody, string? HtmlBody, IEmailAttachment[] Attachments) GetData(Dictionary<string, string>? parserData)
        {
            string subject;
            string? text,
                html;
            List<EmailAttachment> emailAttachments = new(_Attachments.Count);
            try
            {
                emailAttachments.AddRange(from a in _Attachments select new EmailAttachment(a));
                if (parserData is null)
                {
                    subject = Subject;
                    text = TextBody;
                    html = HtmlBody;
                }
                else
                {
                    subject = Subject.Parse(parserData);
                    text = TextBody?.Parse(parserData);
                    html = HtmlBody?.Parse(parserData);
                }
                return (subject, text, html, emailAttachments.ToArray());
            }
            catch
            {
                emailAttachments.DisposeAll();
                throw;
            }
        }

        /// <summary>
        /// Get the data of this template for creating a new email instance
        /// </summary>
        /// <param name="parserData">Parser data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Template data</returns>
        protected virtual async Task<(string Subject, string? TextBody, string? HtmlBody, IEmailAttachment[] Attachments)> GetDataAsync(
            Dictionary<string, string>? parserData,
            CancellationToken cancellationToken
            )
        {
            string subject;
            string? text,
                html;
            List<EmailAttachment> emailAttachments = new(_Attachments.Count);
            try
            {
                foreach (IEmailAttachment attachment in _Attachments)
                    emailAttachments.Add(await EmailAttachment.CreateAsync(attachment, cancellationToken: cancellationToken).DynamicContext());
                if (parserData is null)
                {
                    subject = Subject;
                    text = TextBody;
                    html = HtmlBody;
                }
                else
                {
                    subject = Subject.Parse(parserData);
                    text = TextBody?.Parse(parserData);
                    html = HtmlBody?.Parse(parserData);
                }
                return (subject, text, html, emailAttachments.ToArray());
            }
            catch
            {
                await emailAttachments.DisposeAllAsync().DynamicContext();
                throw;
            }
        }

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
