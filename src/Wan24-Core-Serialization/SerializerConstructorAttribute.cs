namespace wan24.Core
{
    /// <summary>
    /// Attribute for the constructor to be used from the serializer for creating a new object instance
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class SerializerConstructorAttribute() : Attribute()
    {
    }
}
