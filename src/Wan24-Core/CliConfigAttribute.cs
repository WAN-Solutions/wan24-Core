namespace wan24.Core
{
    /// <summary>
    /// Attribute for a CLI argument configurable public static property
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CliConfigAttribute() : Attribute()
    {
    }
}
