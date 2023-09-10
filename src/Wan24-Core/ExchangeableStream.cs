namespace wan24.Core
{
    /// <summary>
    /// Exchangeable stream
    /// </summary>
    public class ExchangeableStream : ExchangeableStream<Stream>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leavepen">Leave the base stream open when disposing?</param>
        public ExchangeableStream(Stream baseStream, bool leavepen = false) : base(baseStream, leavepen) { }
    }

    /// <summary>
    /// Exchangeable stream
    /// </summary>
    /// <typeparam name="T">Base stream type</typeparam>
    public class ExchangeableStream<T> : WrapperStream<T> where T : Stream
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leavepen">Leave the base stream open when disposing?</param>
        public ExchangeableStream(T baseStream, bool leavepen = false) : base(baseStream, leavepen) { }

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
