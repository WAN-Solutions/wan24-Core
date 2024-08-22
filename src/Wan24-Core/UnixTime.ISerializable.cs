using System.Runtime.Serialization;
using System.Security;

namespace wan24.Core
{
    // ISerializable implementation
    public readonly partial record struct UnixTime : ISerializable
    {
        /// <inheritdoc/>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context) => info.AddValue(nameof(EpochSeconds), EpochSeconds);
    }
}
