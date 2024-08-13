namespace wan24.Core
{
    /// <summary>
    /// Serializer object extensions
    /// </summary>
    public static class SerializerObjectExtensions
    {
        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="includeSerializerVersion">If to include the serializer version number</param>
        /// <param name="cleanBuffer">If to clean the buffer</param>
        /// <returns>Serialized object data</returns>
        public static byte[] Serialize(this object obj, in bool includeSerializerVersion = true, in bool cleanBuffer = false)
        {
            using MemoryPoolStream ms = new(pool: SerializerSettings.BufferPool)
            {
                CleanReturned = cleanBuffer
            };
            //TODO Include serializer version
            //TODO Serialize obj to ms
            return ms.ToArray();
        }

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="buffer">Serialized object data buffer</param>
        /// <param name="includeSerializerVersion">If to include the serializer version number</param>
        /// <param name="cleanBuffer">If to clean the internal buffer</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int Serialize(this object obj, in Span<byte> buffer, in bool includeSerializerVersion = true, in bool cleanBuffer = false)
        {
            using Stream stream = new LimitedLengthStream(new MemoryPoolStream(pool: SerializerSettings.BufferPool)
            {
                CleanReturned = cleanBuffer
            }, buffer.Length);
            //TODO Include serializer version
            //TODO Serialize obj to ms
            stream.Position = 0;
            return stream.Read(buffer);
        }
    }
}
