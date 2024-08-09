using System.Buffers.Text;

//TODO Add bootstrapper attribute

namespace wan24.Core
{
    /// <summary>
    /// Serializer bootstrapper
    /// </summary>
    public static class SerializerBootstrap
    {
        /// <summary>
        /// Boot
        /// </summary>
        public static void Boot()
        {
            StringValueConverter.NamedStringConverter[SerializerConstants.STRING_VALUE_CONVERTER_NAME] = (t, v) =>
            {
                using MemoryPoolStream ms = new(pool: SerializerOptions.DefaultBufferPool);
                //TODO Serialize v as t to ms
                using RentedArrayRefStruct<byte> buffer = new(len: (int)ms.Length);
                ms.Position = 0;
                ms.ReadExactly(buffer.Span);
                return Convert.ToBase64String(buffer.Span);
            };
            StringValueConverter.NamedValueConverter[SerializerConstants.STRING_VALUE_CONVERTER_NAME] = (t, s) =>
            {
                if (s is null) return null;
                using RentedArrayRefStruct<byte> buffer = new(len: Base64.GetMaxDecodedFromUtf8Length(s.Length));
                using MemoryStream ms = new(buffer.Array, 0, s.GetBase64Bytes(buffer.Span));
                //TODO Deserialize t from ms
                return null;
            };
            ObjectSerializer.NamedSerializers[SerializerConstants.OBJECT_SERIALIZER_NAME] = (n, o, t) =>
            {
                //TODO Serialize o to t
            };
            ObjectSerializer.NamedAsyncSerializers[SerializerConstants.OBJECT_SERIALIZER_NAME] = async (n, o, t, ct) =>
            {
                //TODO Serialize o to t
            };
            ObjectSerializer.NamedDeserializers[SerializerConstants.OBJECT_SERIALIZER_NAME] = (n, t, s) =>
            {
                //TODO Deserialize t from s
                return null;
            };
            ObjectSerializer.NamedAsyncDeserializers[SerializerConstants.OBJECT_SERIALIZER_NAME] = async (n, t, s, ct) =>
            {
                //TODO Deserialize t from s
                return null;
            };
            //TODO Bootstrap
        }
    }
}
