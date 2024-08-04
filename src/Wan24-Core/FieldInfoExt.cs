using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Extended field information
    /// </summary>
    /// <param name="Field">Field</param>
    /// <param name="Getter">Getter</param>
    /// <param name="Setter">Setter</param>
    public sealed class FieldInfoExt(in FieldInfo Field, in Func<object?, object?>? Getter, in Action<object?, object?>? Setter) : ICustomAttributeProvider
    {
        /// <summary>
        /// Field
        /// </summary>
        public FieldInfo Field { get; } = Field;

        /// <summary>
        /// Field name
        /// </summary>
        public string Name => Field.Name;

        /// <summary>
        /// Field type
        /// </summary>
        public Type FieldType => Field.FieldType;

        /// <summary>
        /// If the field is read-only
        /// </summary>
        public bool IsInitOnly => Field.IsInitOnly;

        /// <summary>
        /// Getter delegate
        /// </summary>
        public Func<object?, object?>? Getter { get; } = Getter;

        /// <summary>
        /// Setter delegate
        /// </summary>
        public Action<object?, object?>? Setter { get; } = Setter;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public object[] GetCustomAttributes(bool inherit) => ((ICustomAttributeProvider)Field).GetCustomAttributes(inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => ((ICustomAttributeProvider)Field).GetCustomAttributes(attributeType, inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public bool IsDefined(Type attributeType, bool inherit) => ((ICustomAttributeProvider)Field).IsDefined(attributeType, inherit);

        /// <summary>
        /// Cast as <see cref="FieldInfo"/>
        /// </summary>
        /// <param name="fi"><see cref="FieldInfoExt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator FieldInfo(in FieldInfoExt fi) => fi.Field;
    }
}
