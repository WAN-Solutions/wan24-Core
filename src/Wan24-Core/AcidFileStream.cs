namespace wan24.Core
{
    /// <summary>
    /// ACID file stream
    /// </summary>
    public static class AcidFileStream
    {
        /// <summary>
        /// Create
        /// </summary>
        /// <param name="stream">File stream</param>
        /// <param name="autoFlush">Automatic flush after each write operation?</param>
        /// <param name="mode">Backup file mode</param>
        /// <returns>ACID file stream (don't forget to commit and dispose!)</returns>
        public static AcidStream<FileStream> Create(in FileStream stream, in bool autoFlush = true, in UnixFileMode? mode = null)
        {
            string backupFn = GetBackupFileName(stream.Name);
            bool needRollback = File.Exists(backupFn);
            FileStreamOptions options = new()
            {
                Mode = needRollback ? FileMode.Open : FileMode.Create,
                Access = FileAccess.ReadWrite,
                Share = FileShare.None,
                BufferSize = 4096,
                Options = FileOptions.RandomAccess | FileOptions.WriteThrough | FileOptions.Asynchronous
            };
#pragma warning disable CA1416 // Platform specific
            if (!needRollback && ENV.IsLinux) options.UnixCreateMode = mode ?? Settings.CreateFileMode;
#pragma warning restore CA1416 // Platform specific
            FileStream backup = new(backupFn, options);
            try
            {
                if (!needRollback) AcidStream.InitializeBackupStream(stream, backup, flush: true);
                AcidStream<FileStream> res = new(stream, backup)
                {
                    AutoFlush = autoFlush,
                    AutoFlushBackup = true
                };
                if (needRollback)
                {
                    AcidStream<FileStream>.PerformRollback(res);
                    needRollback = false;
                }
                res.OnDisposed += (s, e) =>
                {
                    if (!res.NeedsCommit && File.Exists(backupFn)) File.Delete(backupFn);
                };
                return res;
            }
            catch
            {
                backup.Dispose();
                if (!needRollback && File.Exists(backupFn)) File.Delete(backupFn);
                throw;
            }
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="stream">File stream</param>
        /// <param name="autoFlush">Automatic flush after each write operation?</param>
        /// <param name="mode">Backup file mode</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ACID file stream (don't forget to commit and dispose!)</returns>
        public static async Task<AcidStream<FileStream>> CreateAsync(FileStream stream, bool autoFlush = true, UnixFileMode? mode = null, CancellationToken cancellationToken = default)
        {
            string backupFn = GetBackupFileName(stream.Name);
            bool needRollback = File.Exists(backupFn);
            FileStreamOptions options = new()
            {
                Mode = needRollback ? FileMode.Open : FileMode.Create,
                Access = FileAccess.ReadWrite,
                Share = FileShare.None,
                BufferSize = 4096,
                Options = FileOptions.RandomAccess | FileOptions.WriteThrough | FileOptions.Asynchronous
            };
#pragma warning disable CA1416 // Platform specific
            if (!needRollback && ENV.IsLinux) options.UnixCreateMode = mode ?? Settings.CreateFileMode;
#pragma warning restore CA1416 // Platform specific
            FileStream backup = new(backupFn, options);
            try
            {
                if (!needRollback) await AcidStream.InitializeBackupStreamAsync(stream, backup, flush: true, cancellationToken: cancellationToken).DynamicContext();
                AcidStream<FileStream> res = new(stream, backup)
                {
                    AutoFlush = autoFlush,
                    AutoFlushBackup = true
                };
                if (needRollback)
                {
                    await AcidStream<FileStream>.PerformRollbackAsync(res, cancellationToken: cancellationToken).DynamicContext();
                    needRollback = false;
                }
                res.OnDisposed += (s, e) =>
                {
                    if (!res.NeedsCommit && File.Exists(backupFn)) File.Delete(backupFn);
                };
                return res;
            }
            catch
            {
                await backup.DisposeAsync().DynamicContext();
                if (!needRollback && File.Exists(backupFn)) File.Delete(backupFn);
                throw;
            }
        }

        /// <summary>
        /// Get the ACID backup filename for a filename
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <returns>ACID backup filename</returns>
        public static string GetBackupFileName(in string fileName) => Path.Combine(Path.GetFullPath(Path.GetDirectoryName(fileName)!), $".acid.{Path.GetFileName(fileName)}");

        /// <summary>
        /// Determine if a file needs a rollback (ACID backup file still exists)
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <returns>Needs a rollback?</returns>
        public static bool NeedsRollback(in string fileName) => File.Exists(GetBackupFileName(fileName));
    }
}
