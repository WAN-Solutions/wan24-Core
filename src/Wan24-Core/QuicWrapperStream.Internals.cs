#if !RELEASE || PREVIEW
using System.Net.Quic;

//TODO Enable for release builds as soon as QUIC is not in .NET preview anymore

#pragma warning disable CA1416 // Not available on all platforms
namespace wan24.Core
{
    // Internals
    public partial class QuicWrapperStream
    {
        /// <summary>
        /// Cancellation (canceled when disposing)
        /// </summary>
        protected readonly CancellationTokenSource Cancellation = new();
        /// <summary>
        /// Read channel closed event (raised when the peer closes the read channel, so we can't write anymore)
        /// </summary>
        protected readonly ResetEvent? PeerReadClosedEvent = null;
        /// <summary>
        /// Write channel closed event (raised when the peer closes the write channel, so we can't read anymore)
        /// </summary>
        protected readonly ResetEvent? PeerWriteClosedEvent = null;
        /// <summary>
        /// Silent event (raised when all channels are closed)
        /// </summary>
        protected readonly ResetEvent SilentEvent = new();
        /// <summary>
        /// Watches the peer reading channel closing (we can't write anymore)
        /// </summary>
        protected readonly Task? PeerReadClosedWatcher = null;
        /// <summary>
        /// Watches the peer writing channel closing (we can't read anymore)
        /// </summary>
        protected readonly Task? PeerWriteClosedWatcher = null;

        /// <summary>
        /// Wait for an event
        /// </summary>
        /// <param name="e">Event</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task WaitEventAsync(ResetEvent e, TimeSpan timeout, CancellationToken cancellationToken)
        {
            try
            {
                if (timeout != default)
                {
                    await e.WaitAsync(timeout).WaitAsync(cancellationToken).DynamicContext();
                }
                else
                {
                    await e.WaitAsync(cancellationToken).DynamicContext();
                }
            }
            catch (TimeoutException)
            {
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken.IsEqualTo(cancellationToken))
            {
            }
        }

        /// <summary>
        /// Watch peer closing reads
        /// </summary>
        protected virtual async Task PeerReadClosedWatcherAsync()
        {
            if (Logging.Trace)
                Logging.WriteTrace($"{this} watching peer reads closing");
            try
            {
                await BaseStream.ReadsClosed.WaitAsync(Cancellation.Token).DynamicContext();
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken.IsEqualTo(Cancellation.Token))
            {
            }
            catch (QuicException)
            {
            }
            catch (Exception ex)
            {
                if (Logging.Trace)
                    Logging.WriteTrace($"{this} reads closing exceptional: {ex}");
            }
            finally
            {
                if (Logging.Trace)
                    Logging.WriteTrace($"{this} read closed by peer");
                PeerReadClosedEvent!.Set();
                RaiseOnPeerReadClosed();
            }
        }

        /// <summary>
        /// Watch peer closing writes
        /// </summary>
        protected virtual async Task PeerWriteClosedWatcherAsync()
        {
            if (Logging.Trace)
                Logging.WriteTrace($"{this} watching peer writes closing");
            try
            {
                await BaseStream.WritesClosed.WaitAsync(Cancellation.Token).DynamicContext();
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken.IsEqualTo(Cancellation.Token))
            {
            }
            catch (QuicException)
            {
            }
            catch (Exception ex)
            {
                if (Logging.Trace)
                    Logging.WriteTrace($"{this} writes closing exceptional: {ex}");
            }
            finally
            {
                if (Logging.Trace)
                    Logging.WriteTrace($"{this} write closed by peer");
                PeerWriteClosedEvent!.Set();
                RaiseOnPeerWriteClosed();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Cancellation.Cancel();
            try
            {
                PeerReadClosedWatcher?.GetAwaiter().GetResult();
            }
            catch
            {
            }
            try
            {
                PeerWriteClosedWatcher?.GetAwaiter().GetResult();
            }
            catch
            {
            }
            Cancellation.Dispose();
            PeerReadClosedEvent?.Dispose();
            PeerWriteClosedEvent?.Dispose();
            SilentEvent.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            await Cancellation.CancelAsync().DynamicContext();
            if (PeerReadClosedWatcher is not null)
                try
                {
                    await PeerReadClosedWatcher.DynamicContext();
                }
                catch
                {
                }
            if (PeerWriteClosedWatcher is not null)
                try
                {
                    await PeerWriteClosedWatcher.DynamicContext();
                }
                catch
                {
                }
            Cancellation.Dispose();
            if (PeerReadClosedEvent is not null) await PeerReadClosedEvent.DisposeAsync().DynamicContext();
            if (PeerWriteClosedEvent is not null) await PeerWriteClosedEvent.DisposeAsync().DynamicContext();
            await SilentEvent.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Raise the <see cref="OnPeerReadClosed"/> event
        /// </summary>
        /// <param name="e">Arguments</param>
        protected virtual void RaiseOnPeerClosedAny(EventArgs? e = null) => OnPeerClosedAny?.Invoke(this, e ?? new());

        /// <summary>
        /// Raise the <see cref="OnPeerReadClosed"/> event
        /// </summary>
        /// <param name="e">Arguments</param>
        protected virtual void RaiseOnPeerReadClosed(EventArgs? e = null)
        {
            e ??= new();
            OnPeerReadClosed?.Invoke(this, e);
            RaiseOnPeerClosedAny(e);
            if (IsSilent) RaiseOnSilent(e);
        }

        /// <summary>
        /// Raise the <see cref="OnPeerWriteClosed"/> event
        /// </summary>
        /// <param name="e">Arguments</param>
        protected virtual void RaiseOnPeerWriteClosed(EventArgs? e = null)
        {
            e ??= new();
            OnPeerWriteClosed?.Invoke(this, e);
            RaiseOnPeerClosedAny(e);
            if (IsSilent) RaiseOnSilent(e);
        }

        /// <summary>
        /// Raise the <see cref="OnSilent"/> event
        /// </summary>
        /// <param name="e">Arguments</param>
        protected virtual void RaiseOnSilent(EventArgs? e = null)
        {
            SilentEvent.Set();
            OnSilent?.Invoke(this, e ?? new());
            if (IsDisposing || !AutoDispose) return;
            if (Logging.Trace)
                Logging.WriteTrace($"{this} auto disposing after all channels are closed");
            _ = DisposeAsync().DynamicContext();
        }
    }
}
#pragma warning restore CA1416 // Not available on all platforms
#endif
