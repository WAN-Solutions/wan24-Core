namespace wan24.Core
{
    /// <summary>
    /// Interface for a stream
    /// </summary>
    public interface IStream : IDisposableObject
    {
        /// <summary>
        /// Display name
        /// </summary>
        string? Name { get; set; }
        /// <summary>
        /// Is closed?
        /// </summary>
        bool IsClosed { get; }
        /// <summary>
        /// Can read?
        /// </summary>
        bool CanRead { get; }
        /// <summary>
        /// Can seek?
        /// </summary>
        bool CanSeek { get; }
        /// <summary>
        /// Can write?
        /// </summary>
        bool CanWrite { get; }
        /// <summary>
        /// Can timeout?
        /// </summary>
        bool CanTimeout { get; }
        /// <summary>
        /// Length in bytes
        /// </summary>
        long Length { get; }
        /// <summary>
        /// Position byte offset
        /// </summary>
        long Position { get; set; }
        /// <summary>
        /// Read timeout ms
        /// </summary>
        int ReadTimeout { get; set; }
        /// <summary>
        /// Write timeout ms
        /// </summary>
        int WriteTimeout { get; set; }
        /// <summary>
        /// Flush
        /// </summary>
        void Flush();
        /// <summary>
        /// Flush
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task FlushAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Byte offset</param>
        /// <param name="count">Byte count</param>
        /// <returns>Number of bytes red</returns>
        int Read(byte[] buffer, int offset, int count);
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes red</returns>
        int Read(Span<byte> buffer);
        /// <summary>
        /// Read one byte
        /// </summary>
        /// <returns>The red byte or <c>-1</c>, if failed (end of stream)</returns>
        int ReadByte();
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Byte offset</param>
        /// <param name="count">Byte count</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of bytes red</returns>
        Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of bytes red</returns>
        ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
        /// <summary>
        /// Seek
        /// </summary>
        /// <param name="offset">Byte offset</param>
        /// <param name="origin">Seek origin</param>
        /// <returns>New position byte offset</returns>
        long Seek(long offset, SeekOrigin origin);
        /// <summary>
        /// Set the stream length
        /// </summary>
        /// <param name="value">Length in bytes</param>
        void SetLength(long value);
        /// <summary>
        /// Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Byte offset</param>
        /// <param name="count">Byte count</param>
        void Write(byte[] buffer, int offset, int count);
        /// <summary>
        /// Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        void Write(ReadOnlySpan<byte> buffer);
        /// <summary>
        /// Write one byte
        /// </summary>
        /// <param name="value">Byte</param>
        void WriteByte(byte value);
        /// <summary>
        /// Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Byte offset</param>
        /// <param name="count">Byte count</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
        /// <summary>
        /// Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);
        /// <summary>
        /// Copy to another stream
        /// </summary>
        /// <param name="destination">Target</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        void CopyTo(Stream destination, int bufferSize = 81_920);
        /// <summary>
        /// Copy to another stream
        /// </summary>
        /// <param name="destination">Target</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CopyToAsync(Stream destination, int bufferSize = 81_920, CancellationToken cancellationToken = default);
        /// <summary>
        /// Begin read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Byte offset</param>
        /// <param name="count">Byte count</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State</param>
        /// <returns>Result</returns>
        IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state);
        /// <summary>
        /// End read
        /// </summary>
        /// <param name="asyncResult">Result</param>
        /// <returns>Number of red bytes</returns>
        int EndRead(IAsyncResult asyncResult);
        /// <summary>
        /// Begin write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Byte offset</param>
        /// <param name="count">Byte count</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State</param>
        /// <returns>Result</returns>
        IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state);
        /// <summary>
        /// End write
        /// </summary>
        /// <param name="asyncResult">Result</param>
        void EndWrite(IAsyncResult asyncResult);
        /// <summary>
        /// Close
        /// </summary>
        void Close();
    }
}
