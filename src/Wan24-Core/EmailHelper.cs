using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Email helper
    /// </summary>
    public static class EmailHelper
    {
        /// <summary>
        /// Email type
        /// </summary>
        private static Type? _EmailType = null;

        /// <summary>
        /// MTA connection
        /// </summary>
        public static IMtaConnection? MtaConnection { get; set; }

        /// <summary>
        /// Email type
        /// </summary>
        public static Type? EmailType
        {
            get => _EmailType;
            set
            {
                if (value is not null && (!typeof(IEmail).IsAssignableFrom(value) || value.IsValueType || value.IsAbstract || value.IsInterface))
                    throw new ArgumentException("Invalid email type", nameof(value));
                _EmailType = value;
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
        /// <returns>Email</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static IEmail Create(in string fromEmail, in string toEmail, in string subject, in string? text, in string? html, params IEmailAttachment[] attachments)
        {
            if (EmailType is not Type emailType) throw new InvalidOperationException("No email type");
            return (IEmail)emailType.ConstructAuto(usePrivate: false, fromEmail, toEmail, subject, text, html, attachments);
        }

        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="fromEmail">Sender email address</param>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Subject</param>
        /// <param name="text">Text body</param>
        /// <param name="html">HTML body</param>
        /// <param name="attachments">Attachments</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool Send(in string fromEmail, in string toEmail, in string subject, in string? text, in string? html, params IEmailAttachment[] attachments)
        {
            if (MtaConnection is not IMta mta) throw new InvalidOperationException("No MTA connection");
            using IEmail email = Create(fromEmail, toEmail, subject, text, html, attachments);
            return mta.Send(email);
        }

        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="fromEmail">Sender email address</param>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Subject</param>
        /// <param name="text">Text body</param>
        /// <param name="html">HTML body</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="attachments">Attachments</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<bool> SendAsync(
            string fromEmail,
            string toEmail,
            string subject,
            string? text,
            string? html,
            CancellationToken cancellationToken = default,
            params IEmailAttachment[] attachments
            )
        {
            if (MtaConnection is not IMta mta) throw new InvalidOperationException("No MTA connection");
            IEmail email = Create(fromEmail, toEmail, subject, text, html, attachments);
            await using (email.DynamicContext())
                return await mta.SendAsync(email, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="email">Email (will be disposed!)</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool Send(in IEmail email)
        {
            try
            {
                if (MtaConnection is not IMta mta) throw new InvalidOperationException("No MTA connection");
                return mta.Send(email);
            }
            finally
            {
                email.Dispose();
            }
        }

        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="email">Email (will be disposed!)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static async Task<bool> SendAsync(
            IEmail email,
            CancellationToken cancellationToken = default
            )
        {
            try
            {
                if (MtaConnection is not IMta mta) throw new InvalidOperationException("No MTA connection");
                return await mta.SendAsync(email, cancellationToken).DynamicContext();
            }
            finally
            {
                await email.DisposeAsync().DynamicContext();
            }
        }
    }
}
