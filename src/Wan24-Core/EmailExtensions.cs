using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="IEmail"/> (and <see cref="IEmailTemplate"/>) extensions
    /// </summary>
    public static class EmailExtensions
    {
        /// <summary>
        /// Send the email
        /// </summary>
        /// <param name="email">Email (will be disposed!)</param>
        /// <param name="mta">MTA</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Send(this IEmail email, IMta? mta = null)
        {
            if (mta is null) return EmailHelper.Send(email);
            try
            {
                return mta.Send(email);
            }
            finally
            {
                email.Dispose();
            }
        }

        /// <summary>
        /// Send the email
        /// </summary>
        /// <param name="email">Email (will be disposed!)</param>
        /// <param name="connection">Connection</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Send(this IEmail email, IMtaConnection connection)
        {
            try
            {
                return connection.Send(email);
            }
            finally
            {
                email.Dispose();
            }
        }

        /// <summary>
        /// Send the email
        /// </summary>
        /// <param name="email">Email (will be disposed!)</param>
        /// <param name="mta">MTA</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<bool> SendAsync(this IEmail email, IMta? mta = null, CancellationToken cancellationToken = default)
        {
            if (mta is null) return await EmailHelper.SendAsync(email, cancellationToken).DynamicContext();
            try
            {
                return await mta.SendAsync(email, cancellationToken).DynamicContext();
            }
            finally
            {
                await email.DisposeAsync().DynamicContext();
            }
        }

        /// <summary>
        /// Send the email
        /// </summary>
        /// <param name="email">Email (will be disposed!)</param>
        /// <param name="connection">Connection</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<bool> SendAsync(this IEmail email, IMtaConnection connection, CancellationToken cancellationToken = default)
        {
            try
            {
                return await connection.SendAsync(email, cancellationToken).DynamicContext();
            }
            finally
            {
                await email.DisposeAsync().DynamicContext();
            }
        }

        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="template">Email template</param>
        /// <param name="fromEmail">Sender email address</param>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="parserData">Parser data</param>
        /// <param name="attachments">Attachments (will be disposed!)</param>
        /// <param name="mta">MTA</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Send(
            this IEmailTemplate template, 
            string fromEmail, 
            string toEmail, 
            Dictionary<string, string>? parserData = null, 
            IMta? mta = null, 
            params IEmailAttachment[] attachments
            )
            => Send(template.CreateEmail(fromEmail, toEmail, parserData, attachments), mta);

        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="template">Email template</param>
        /// <param name="fromEmail">Sender email address</param>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="connection">Connection</param>
        /// <param name="parserData">Parser data</param>
        /// <param name="attachments">Attachments (will be disposed!)</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Send(
            this IEmailTemplate template,
            string fromEmail,
            string toEmail,
            IMtaConnection connection,
            Dictionary<string, string>? parserData = null,
            params IEmailAttachment[] attachments
            )
            => Send(template.CreateEmail(fromEmail, toEmail, parserData, attachments), connection);

        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="template">Email template</param>
        /// <param name="fromEmail">Sender email address</param>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="parserData">Parser data</param>
        /// <param name="mta">MTA</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="attachments">Attachments (will be disposed!)</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<bool> SendAsync(
            this IEmailTemplate template, 
            string fromEmail, 
            string toEmail,
            Dictionary<string, string>? parserData = null,
            IMta? mta = null,
            CancellationToken cancellationToken = default,
            params IEmailAttachment[] attachments
            )
            => await SendAsync(await template.CreateEmailAsync(fromEmail, toEmail, parserData, cancellationToken, attachments).DynamicContext(), mta, cancellationToken)
                .DynamicContext();

        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="template">Email template</param>
        /// <param name="fromEmail">Sender email address</param>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="connection">Connection</param>
        /// <param name="parserData">Parser data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="attachments">Attachments (will be disposed!)</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<bool> SendAsync(
            this IEmailTemplate template,
            string fromEmail,
            string toEmail,
            IMtaConnection connection,
            Dictionary<string, string>? parserData = null,
            CancellationToken cancellationToken = default,
            params IEmailAttachment[] attachments
            )
            => await SendAsync(await template.CreateEmailAsync(fromEmail, toEmail, parserData, cancellationToken, attachments).DynamicContext(), connection, cancellationToken)
                .DynamicContext();
    }
}
