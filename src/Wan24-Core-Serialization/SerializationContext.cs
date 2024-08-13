using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Serialization context
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SerializationContext : IDisposable
    {
        /// <summary>
        /// If to delete the options <c>Seen</c> property when disposing
        /// </summary>
        public readonly bool DeleteSeen;
        /// <summary>
        /// Serializer options
        /// </summary>
        public readonly SerializerOptions? SerializerOptions;
        /// <summary>
        /// Deserializer options
        /// </summary>
        public readonly DeserializerOptions? DeserializerOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Options</param>
        public SerializationContext(SerializerOptions options)
        {
            DeleteSeen = options.Seen is null;
            SerializerOptions = options;
            DeserializerOptions = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Options</param>
        public SerializationContext(DeserializerOptions options)
        {
            DeleteSeen = options.Seen is null;
            SerializerOptions = null;
            DeserializerOptions = options;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!DeleteSeen) return;
            if (SerializerOptions is not null) SerializerOptions.Seen = null;
            else DeserializerOptions!.Seen = null;
        }
    }
}
