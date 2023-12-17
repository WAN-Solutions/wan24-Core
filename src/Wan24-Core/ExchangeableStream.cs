namespace wan24.Core
{
    /// <summary>
    /// Exchangeable stream
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class ExchangeableStream(Stream baseStream, bool leaveOpen = false) : ExchangeableStream<Stream>(baseStream, leaveOpen)
    {
    }

    /// <summary>
    /// Exchangeable stream
    /// </summary>
    /// <typeparam name="T">Base stream type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class ExchangeableStream<T>(T baseStream, bool leaveOpen = false) : WrapperStream<T>(baseStream, leaveOpen) where T : Stream
    {
        /// <summary>
        /// Set a new base stream
        /// </summary>
        /// <param name="newBaseStream">New base stream</param>
        /// <returns>Old base stream (needs to be disposed!)</returns>
        public T SetBaseStream(T newBaseStream)
        {
            EnsureUndisposed();
            T res = BaseStream;
            BaseStream = newBaseStream;
            return res;
        }
    }
}
