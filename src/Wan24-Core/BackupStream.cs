namespace wan24.Core
{
    /// <summary>
    /// Backup stream (writes red data into another stream)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="backupStream">Backup stream (red data from the base stream will be written to this stream)</param>
    /// <param name="leaveBaseOpen">Leave the base stream open when disposing?</param>
    /// <param name="leaveBackupOpen">Leave the backup stream open when disposing?</param>
    public class BackupStream(in Stream baseStream, in Stream backupStream, in bool leaveBaseOpen = false, in bool leaveBackupOpen = false)
        : BackupStream<Stream>(baseStream, backupStream, leaveBaseOpen, leaveBackupOpen)
    {
    }

    /// <summary>
    /// Backup stream (writes red data into another stream)
    /// </summary>
    /// <typeparam name="T">Base stream type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="backupStream">Backup stream (red data from the base stream will be written to this stream)</param>
    /// <param name="leaveBaseOpen">Leave the base stream open when disposing?</param>
    /// <param name="leaveBackupOpen">Leave the backup stream open when disposing?</param>
    public class BackupStream<T>(in T baseStream, in Stream backupStream, in bool leaveBaseOpen = false, in bool leaveBackupOpen = false)
        : WrapperStream<T>(baseStream, leaveBaseOpen)
        where T : Stream
    {

        /// <summary>
        /// Backup stream (red data from the base stream will be written to this stream)
        /// </summary>
        public Stream Backup { get; } = backupStream;

        /// <summary>
        /// Leave the backup stream open when disposing?
        /// </summary>
        public bool LeaveBackupOpen { get; set; } = leaveBackupOpen;

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int res = base.Read(buffer, offset, count);
            if (res != 0) Backup.Write(buffer, offset, res);
            return res;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            int res = base.Read(buffer);
            if (res != 0) Backup.Write(buffer[..res]);
            return res;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int res = await base.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
            if (res != 0) await Backup.WriteAsync(buffer, offset, res, cancellationToken).DynamicContext();
            return res;
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int res = await base.ReadAsync(buffer, cancellationToken).DynamicContext();
            if (res != 0) await Backup.WriteAsync(buffer[..res], cancellationToken).DynamicContext();
            return res;
        }

        /// <inheritdoc/>
        public override void Close()
        {
            if (IsClosed) return;
            base.Close();
            if (LeaveBackupOpen) return;
            Backup.Close();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            if (LeaveBackupOpen) return;
            await Backup.DisposeAsync().DynamicContext();
        }
    }
}
