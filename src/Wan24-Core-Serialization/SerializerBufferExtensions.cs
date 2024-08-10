namespace wan24.Core
{
    /// <summary>
    /// Buffer extensions
    /// </summary>
    public static class SerializerBufferExtensions
    {
        /// <summary>
        /// Deserialize an object from a serialized data buffer
        /// </summary>
        /// <param name="buffer">Serialized data buffer</param>
        /// <param name="type">Object type to deserialize</param>
        /// <param name="serializerVersionIncluded">If the serializer version was included</param>
        /// <param name="cleanBuffer">If to clean the internal buffer</param>
        /// <returns>Deserialized object</returns>
        public static object Deserialize(this ReadOnlySpan<byte> buffer, in Type type, in bool serializerVersionIncluded = true, in bool cleanBuffer = false)
        {
            using RentedArrayRefStruct<byte> temp = new(buffer.Length, clean: false)
            {
                Clear = cleanBuffer
            };
            buffer.CopyTo(temp.Span);
            using MemoryStream ms = new(temp.Array, 0, buffer.Length);
            //TODO Deserialize the serializer version
            //TODO Deserialize type from ms
            throw new NotImplementedException();
        }
        /// <summary>
        /// Deserialize an object from a serialized data buffer
        /// </summary>
        /// <typeparam name="T">Object type to deserialize</typeparam>
        /// <param name="buffer">Serialized data buffer</param>
        /// <param name="serializerVersionIncluded">If the serializer version was included</param>
        /// <param name="cleanBuffer">If to clean the internal buffer</param>
        /// <returns>Deserialized object</returns>
        public static T Deserialize<T>(this ReadOnlySpan<byte> buffer, in bool serializerVersionIncluded = true, in bool cleanBuffer = false)
            => (T)Deserialize(buffer, typeof(T), serializerVersionIncluded, cleanBuffer);

        /// <summary>
        /// Deserialize an object from a serialized data buffer
        /// </summary>
        /// <param name="buffer">Serialized data buffer</param>
        /// <param name="type">Object type to deserialize</param>
        /// <param name="serializerVersionIncluded">If the serializer version was included</param>
        /// <returns>Deserialized object</returns>
        public static object Deserialize(this byte[] buffer, in Type type, in bool serializerVersionIncluded = true)
        {
            using MemoryStream ms = new(buffer);
            //TODO Deserialize the serializer version
            //TODO Deserialize type from ms
            throw new NotImplementedException();
        }
        /// <summary>
        /// Deserialize an object from a serialized data buffer
        /// </summary>
        /// <typeparam name="T">Object type to deserialize</typeparam>
        /// <param name="buffer">Serialized data buffer</param>
        /// <param name="serializerVersionIncluded">If the serializer version was included</param>
        /// <returns>Deserialized object</returns>
        public static T Deserialize<T>(this byte[] buffer, in bool serializerVersionIncluded = true)
            => (T)Deserialize(buffer, typeof(T), serializerVersionIncluded);
    }
}
