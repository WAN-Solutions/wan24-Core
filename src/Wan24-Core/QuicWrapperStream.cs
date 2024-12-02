#if !RELEASE || PREVIEW || NET9_0_OR_GREATER
using System.Net.Quic;
using System.Runtime.Versioning;
using static wan24.Core.TranslationHelper;

//TODO Remove pre-compiler directives with .NET 9

#pragma warning disable CA1416 // Not available on all platforms
namespace wan24.Core
{
    /// <summary>
    /// QUIC wrapper stream
    /// </summary>
#if !NET9_0_OR_GREATER
    [RequiresPreviewFeatures]
#endif
    public partial class QuicWrapperStream : WrapperStream<QuicStream>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">QUIC stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        /// <param name="autoDispose">If to dispose automatic when all channels are closed</param>
        public QuicWrapperStream(QuicStream baseStream, in bool leaveOpen = false, in bool autoDispose = true) : base(baseStream, leaveOpen)
        {
            if (baseStream.CanWrite)
            {
                PeerReadClosedEvent = new();
                PeerReadClosedWatcher = PeerReadClosedWatcherAsync();
            }
            if (baseStream.CanRead)
            {
                PeerWriteClosedEvent = new();
                PeerWriteClosedWatcher = PeerWriteClosedWatcherAsync();
            }
            AutoDispose = autoDispose;
        }

        /// <summary>
        /// If QUIC is supported
        /// </summary>
        public static bool IsSupported => QuicListener.IsSupported && QuicConnection.IsSupported;

        /// <summary>
        /// If the QUIC stream is bi-directional
        /// </summary>
        public bool IsBiDirectional => BaseStream.Type == QuicStreamType.Bidirectional && CanRead && CanWrite;

        /// <summary>
        /// If the QUIC stream can't read and write at all
        /// </summary>
        public bool IsSilent => !CanRead && !CanWrite;

        /// <summary>
        /// If to dispose automatic when all channels are closed
        /// </summary>
        public bool AutoDispose { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<Status> State
        {
            get
            {
                foreach (Status status in base.State) yield return status;
                yield return new(__("ID"), BaseStream.Id, __("QUIC stream ID"));
                yield return new(__("Bi-directional"), IsBiDirectional, __("If the QUIC connection is bi-directional"));
                yield return new(__("Silent"), IsSilent, __("If all channels are closed"));
                yield return new(__("Dispose"), AutoDispose, __("If the stream disposes automatic when all channels are closed"));
            }
        }

        /// <summary>
        /// Wait for the peer to close the reading channel (so we can't write anymore)
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If the channel was closed</returns>
        public virtual async Task<bool> WaitPeerReadClosedAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (PeerWriteClosedEvent is not null && CanWrite)
                await WaitEventAsync(PeerWriteClosedEvent, timeout, cancellationToken).DynamicContext();
            return !CanWrite;
        }

        /// <summary>
        /// Wait for the peer to close the writing channel (so we can't read anymore)
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If the channel was closed</returns>
        public virtual async Task<bool> WaitPeerWriteClosedAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (PeerReadClosedEvent is not null && CanRead)
                await WaitEventAsync(PeerReadClosedEvent, timeout, cancellationToken).DynamicContext();
            return !CanRead;
        }

        /// <summary>
        /// Wait for all channels being closed
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If all channels are closed</returns>
        public virtual async Task<bool> WaitSilentAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (!IsSilent)
                await WaitEventAsync(SilentEvent, timeout, cancellationToken).DynamicContext();
            return IsSilent;
        }

        /// <summary>
        /// Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="completeWrites">Complete writes?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, bool completeWrites, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            return BaseStream.WriteAsync(buffer, completeWrites, cancellationToken);
        }

        /// <summary>
        /// Complete writes
        /// </summary>
        public virtual void CompleteWrites()
        {
            EnsureUndisposed();
            BaseStream.CompleteWrites();
        }

        /// <summary>
        /// Abort
        /// </summary>
        /// <param name="abortDirection">Direction</param>
        /// <param name="errorCode">Error code</param>
        public virtual void Abort(QuicAbortDirection abortDirection, long errorCode)
        {
            if (IsDisposing) return;
            BaseStream.Abort(abortDirection, errorCode);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (IsDisposing) return GetType().ToString();
            try
            {
                return $"{GetType()}, {BaseStream.Type}, {(CanRead ? "readable, " : string.Empty)}{(CanWrite ? "writable, " : string.Empty)}ID #{BaseStream.Id}";
            }
            catch
            {
                return GetType().ToString();
            }
        }

        /// <summary>
        /// Delegate for an event handler
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="e">Arguments</param>
        public delegate void QuicWrapperStreamEvent_Delegate(QuicWrapperStream stream, EventArgs e);

        /// <summary>
        /// Raised when the peer closed any channel
        /// </summary>
        public event QuicWrapperStreamEvent_Delegate? OnPeerClosedAny;

        /// <summary>
        /// Raised when the peer closed the read channel (the peer can't read anymore, we can't write anymore)
        /// </summary>
        public event QuicWrapperStreamEvent_Delegate? OnPeerReadClosed;

        /// <summary>
        /// Raised when the peer closed the write channel (the peer can't write anymore, we can't read anymore)
        /// </summary>
        public event QuicWrapperStreamEvent_Delegate? OnPeerWriteClosed;

        /// <summary>
        /// Raised when all channels are closed
        /// </summary>
        public event QuicWrapperStreamEvent_Delegate? OnSilent;
    }
}
#pragma warning restore CA1416 // Not available on all platforms
#endif
