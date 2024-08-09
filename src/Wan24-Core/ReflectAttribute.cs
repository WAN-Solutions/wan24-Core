using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Attribute for types which tell what to reflect using <see cref="ReflectionExtensions.Reflect(Type, BindingFlags?)"/>
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="bindings">Bindings (use <see cref="BindingFlags.GetField"/>, <see cref="BindingFlags.GetProperty"/> and <see cref="BindingFlags.InvokeMethod"/> to include elements)</param>
    [AttributeUsage(AttributeTargets.Class)]
    public class ReflectAttribute(BindingFlags bindings = ReflectionExtensions.DEFAULT_BINDINGS) : Attribute()
    {
        /// <summary>
        /// Bindings (use <see cref="BindingFlags.GetField"/>, <see cref="BindingFlags.GetProperty"/> and <see cref="BindingFlags.InvokeMethod"/> to include elements)
        /// </summary>
        public BindingFlags Bindings { get; } = bindings;
    }
}
