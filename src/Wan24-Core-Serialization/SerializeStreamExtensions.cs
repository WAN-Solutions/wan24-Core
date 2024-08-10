namespace wan24.Core
{
    /// <summary>
    /// <see cref="ISerializeStream"/> extensions
    /// </summary>
    public static class SerializeStreamExtensions
    {
        /// <summary>
        /// Serialize the instance
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="includeSerializerVersion">If to include the serializer version number</param>
        /// <param name="cleanBuffer">If to clean the internal buffer</param>
        /// <returns>Serialized object data</returns>
        public static byte[] Serialize(this ISerializeStream obj, in bool includeSerializerVersion = true, in bool cleanBuffer = false)
        {
            using MemoryPoolStream ms = new(pool: SerializerOptions.DefaultBufferPool)
            {
                CleanReturned = cleanBuffer
            };
            //TODO Include serializer version number
            obj.SerializeTo(ms, options: null);
            return ms.ToArray();
        }

        /// <summary>
        /// Serialize the instance
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="buffer">Serialized object data buffer</param>
        /// <param name="includeSerializerVersion">If to include the serializer version number</param>
        /// <param name="cleanBuffer">If to clean the internal buffer</param>
        /// <returns>Number of bytes written to the buffer</returns>
        public static int Serialize(this ISerializeStream obj, in Span<byte> buffer, in bool includeSerializerVersion = true, in bool cleanBuffer = false)
        {
            using Stream stream = new LimitedLengthStream(new MemoryPoolStream(pool: SerializerOptions.DefaultBufferPool)
            {
                CleanReturned = cleanBuffer
            }, buffer.Length);
            //TODO Include serializer version number
            obj.SerializeTo(stream, options: null);
            stream.Position = 0;
            return stream.Read(buffer);
        }

        /// <summary>
        /// Deserialize from a serialized object data buffer
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="buffer">Serialized object data buffer</param>
        /// <param name="serializerVersionIncluded">If the serializer version was included</param>
        public static void Deserialize(this ISerializeStream obj, in byte[] buffer, in bool serializerVersionIncluded = true)
        {
            using MemoryStream ms = new(buffer);
            //TODO Deserialize the serializer version number
            obj.DeserializeFrom(ms, SerializerSettings.Version, options: null);//TODO Use deserialized serializer version
        }

        /// <summary>
        /// Deserialize from a serialized object data buffer
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="buffer">Serialized object data buffer</param>
        /// <param name="serializerVersionIncluded">If the serializer version was included</param>
        /// <param name="cleanBuffer">If to clean the internal buffer</param>
        public static void Deserialize(this ISerializeStream obj, in ReadOnlySpan<byte> buffer, in bool serializerVersionIncluded = true, in bool cleanBuffer = false)
        {
            using RentedArrayRefStruct<byte> temp = new(len: buffer.Length, clean: false)
            {
                Clear = cleanBuffer
            };
            buffer.CopyTo(temp.Span);
            using MemoryStream ms = new(temp.Array, 0, buffer.Length);
            //TODO Deserialize the serializer version number
            obj.DeserializeFrom(ms, SerializerSettings.Version, options: null);//TODO Use deserialized serializer version
        }
    }
}
