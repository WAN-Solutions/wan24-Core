namespace wan24.Core
{
    /// <summary>
    /// <see cref="StreamSerializerTypes"/> extensions
    /// </summary>
    public static class StreamSerializerTypesExtensions
    {
        /// <summary>
        /// Determine if a named serializer is being used
        /// </summary>
        /// <param name="sst">Types</param>
        /// <returns>If a named serializer is being used</returns>
        public static bool UsesNamedSerializer(this StreamSerializerTypes sst) => (sst & StreamSerializerTypes.NamedSerializer) == StreamSerializerTypes.NamedSerializer;

        /// <summary>
        /// Determine if a type conversion is required
        /// </summary>
        /// <param name="sst">Types</param>
        /// <returns>If a type conversion is required</returns>
        public static bool RequiresTypeConversion(this StreamSerializerTypes sst) => (sst & StreamSerializerTypes.TypeConverted) == StreamSerializerTypes.TypeConverted;
    }
}
