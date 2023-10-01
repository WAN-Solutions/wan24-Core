namespace wan24.Core
{
    /// <summary>
    /// Email attachment
    /// </summary>
    public class EmailAttachment : DisposableBase, IEmailAttachment
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="stream">Stream</param>
        public EmailAttachment(in string fileName, in string mimeType, in Stream stream) : base()
        {
            FileName = fileName;
            MimeType = mimeType;
            Stream = stream;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="data">Data</param>
        public EmailAttachment(in string fileName, in string mimeType, in byte[] data) : base()
        {
            FileName = fileName;
            MimeType = mimeType;
            Stream = new MemoryPoolStream(data);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="localFileName">Local filename</param>
        public EmailAttachment(in string fileName, in string mimeType, in string localFileName) : base()
        {
            FileName = fileName;
            MimeType = mimeType;
            Stream = new FileStream(localFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="attachment">Attachment to copy</param>
        /// <param name="fileName">New filename</param>
        /// <param name="mimeType">New MIME type</param>
        /// <param name="memoryStreamPool">Memory stream pool to use</param>
        /// <param name="fileStreamPool">File stream pool to use</param>
        public EmailAttachment(
            in IEmailAttachment attachment, 
            in string? fileName = null, 
            in string? mimeType = null,
            in StreamPool<PooledMemoryStream>? memoryStreamPool = null, 
            in StreamPool<PooledTempFileStream>? fileStreamPool = null
            )
            : base()
        {
            FileName = fileName ?? attachment.FileName;
            MimeType = mimeType ?? attachment.MimeType;
            Stream = new PooledTempStream(attachment.Stream.Length, memoryStreamPool, fileStreamPool);
            attachment.Stream.Position = 0;
            attachment.Stream.CopyTo(Stream);
            Stream.Position = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="estimatedLength">Estimated stream length in bytes</param>
        /// <param name="memoryStreamPool">Memory stream pool to use</param>
        /// <param name="fileStreamPool">File stream pool to use</param>
        protected EmailAttachment(
            in string fileName,
            in string mimeType,
            in long estimatedLength,
            in StreamPool<PooledMemoryStream>? memoryStreamPool = null,
            in StreamPool<PooledTempFileStream>? fileStreamPool = null
            )
            : base()
        {
            FileName = fileName;
            MimeType = mimeType;
            Stream = new PooledTempStream(estimatedLength, memoryStreamPool, fileStreamPool);
        }

        /// <inheritdoc/>
        public virtual string FileName { get; }

        /// <inheritdoc/>
        public virtual string MimeType { get; }

        /// <inheritdoc/>
        public virtual Stream Stream { get; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => Stream.Dispose();

        /// <inheritdoc/>
        protected override async Task DisposeCore() => await Stream.DisposeAsync().DynamicContext();

        /// <summary>
        /// Copy an existng email attachment
        /// </summary>
        /// <param name="attachment">Attachment to copy</param>
        /// <param name="fileName">New filename</param>
        /// <param name="mimeType">New MIME type</param>
        /// <param name="memoryStreamPool">Memory stream pool to use</param>
        /// <param name="fileStreamPool">File stream pool to use</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Email attachment</returns>
        public static async Task<EmailAttachment> CreateAsync(
            IEmailAttachment attachment,
            string? fileName = null,
            string? mimeType = null,
            StreamPool<PooledMemoryStream>? memoryStreamPool = null,
            StreamPool<PooledTempFileStream>? fileStreamPool = null,
            CancellationToken cancellationToken = default
            )
        {
            EmailAttachment res = new(fileName ?? attachment.FileName, mimeType ?? attachment.MimeType, attachment.Stream.Length, memoryStreamPool, fileStreamPool);
            try
            {
                attachment.Stream.Position = 0;
                await attachment.Stream.CopyToAsync(res.Stream, cancellationToken).DynamicContext();
                res.Stream.Position = 0;
                return res;
            }
            catch
            {
                await res.DisposeAsync().DynamicContext();
                throw;
            }
        }
    }
}
