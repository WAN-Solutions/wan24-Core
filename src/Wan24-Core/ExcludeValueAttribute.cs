namespace wan24.Core
{
    /// <summary>
    /// Attribute for <see cref="AutoValueObjectBase{T}"/> properties to exclude from the objects hash code calculation
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public sealed class ExcludeValueAttribute() : Attribute()
    {
    }
}
