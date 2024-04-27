namespace wan24.Core
{
    // Delete
    public static partial class FsHelper
    {
        /// <summary>
        /// Delete file handler
        /// </summary>
        public static DeleteFile_Delegate? DeleteFileHandler { get; set; } = DefaultDeleteFileHandlerAsync;

        /// <summary>
        /// Delete folder handler (needs to delete recursive; files should be deleted already)
        /// </summary>
        public static DeleteFolder_Delegate? DeleteFolderHandler { get; set; }

        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="fn">Filename</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task DeleteFileAsync(string fn, CancellationToken cancellationToken = default)
        {
            if (DeleteFileHandler is not DeleteFile_Delegate deleteFile)
            {
                File.Delete(fn);
            }
            else
            {
                await deleteFile(fn, cancellationToken).DynamicContext();
            }
        }

        /// <summary>
        /// Delete a folder recursive
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="parallelDeleteFile">Number of files to delete in parallel</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task DeleteFolderAsync(string path, int? parallelDeleteFile = null, CancellationToken cancellationToken = default)
        {
            if (parallelDeleteFile.HasValue)
            {
                DeleteFileQueue queue = new(parallelDeleteFile.Value, cancellationToken);
                await using (queue.DynamicContext())
                {
                    await queue.StartAsync(cancellationToken).DynamicContext();
                    foreach (string fn in Directory.EnumerateFiles(path, "*", new EnumerationOptions() { RecurseSubdirectories = true }))
                        await queue.EnqueueAsync(fn, cancellationToken).DynamicContext();
                    await queue.WaitBoringAsync(cancellationToken).DynamicContext();
                }
            }
            else
            {
                foreach (string fn in Directory.EnumerateFiles(path, "*", new EnumerationOptions() { RecurseSubdirectories = true }))
                    await DeleteFileAsync(fn, cancellationToken).DynamicContext();
            }
            if (DeleteFolderHandler is not DeleteFolder_Delegate deleteFolder)
            {
                Directory.Delete(path, recursive: true);
            }
            else
            {
                await deleteFolder(path, cancellationToken).DynamicContext();
            }
        }

        /// <summary>
        /// Default delete file handler (see <see cref="DeleteFile_Delegate"/>; overwrites with zero and then random data before deleting; won't check for SSD HDD)
        /// </summary>
        /// <param name="fn">Filename</param>
        /// <param name="cancellationToken">Cancellation tokens</param>
        public static async Task DefaultDeleteFileHandlerAsync(string fn, CancellationToken cancellationToken)
        {
            FileStream fs = CreateFileStream(
                fn,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None,
                options: FileOptions.SequentialScan | FileOptions.DeleteOnClose | FileOptions.Asynchronous
                );
            await using (fs.DynamicContext())
            {
                long len = fs.Length;
                fs.Position = 0;
                using (ZeroStream zero = new())
                    await zero.CopyExactlyPartialToAsync(fs, len, cancellationToken: cancellationToken).DynamicContext();
                fs.Position = 0;
                await RandomStream.Instance.CopyExactlyPartialToAsync(fs, len, cancellationToken: cancellationToken).DynamicContext();
                fs.SetLength(0);
            }
        }

        /// <summary>
        /// Delete file handler
        /// </summary>
        /// <param name="fn">Filename</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate Task DeleteFile_Delegate(string fn, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete folder handler (needs to delete recursive; files should be deleted already)
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate Task DeleteFolder_Delegate(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete file queue
        /// </summary>
        private sealed class DeleteFileQueue : ParallelItemQueueWorkerBase<string>
        {
            /// <summary>
            /// Cancellation registration
            /// </summary>
            private readonly CancellationTokenRegistration Registration;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="threads">Number of threads</param>
            /// <param name="cancellationToken">Cancellation token</param>
            public DeleteFileQueue(in int threads, in CancellationToken cancellationToken) : base(threads << 1, threads)
                => Registration = cancellationToken.Register(async () => await DisposeAsync().DynamicContext());

            /// <inheritdoc/>
            protected override Task ProcessItem(string item, CancellationToken cancellationToken)
                => DeleteFileAsync(item, cancellationToken);

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                Registration.Dispose();
                base.Dispose(disposing);
            }

            /// <inheritdoc/>
            protected override async Task DisposeCore()
            {
                await Registration.DisposeAsync().DynamicContext();
                await base.DisposeCore().DynamicContext();
            }
        }
    }
}
