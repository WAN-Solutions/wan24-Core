using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Extended field information
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="Field">Field</param>
    /// <param name="Getter">Getter</param>
    /// <param name="Setter">Setter</param>
    public sealed record class FieldInfoExt(in FieldInfo Field, in Func<object?, object?>? Getter, in Action<object?, object?>? Setter) : ICustomAttributeProvider
    {
        /// <summary>
        /// Cache (key is the field hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, FieldInfoExt> Cache = [];

        /// <summary>
        /// If the property is nullable
        /// </summary>
        private bool? _IsNullable = null;
        /// <summary>
        /// Bindings
        /// </summary>
        private BindingFlags? _Bindings = null;

        /// <summary>
        /// Field
        /// </summary>
        public FieldInfo Field { get; } = Field;

        /// <summary>
        /// Bindings
        /// </summary>
        public BindingFlags Bindings => _Bindings ??= Field.GetBindingFlags();

        /// <summary>
        /// Field name
        /// </summary>
        public string Name => Field.Name;

        /// <summary>
        /// Field declaring type
        /// </summary>
        public Type? DeclaringType => Field.DeclaringType;

        /// <summary>
        /// Field type
        /// </summary>
        public Type FieldType => Field.FieldType;

        /// <summary>
        /// If the field is read-only
        /// </summary>
        public bool IsInitOnly => Field.IsInitOnly;

        /// <summary>
        /// If the field is nullable
        /// </summary>
        public bool IsNullable => _IsNullable ??= Field.IsNullable();

        /// <summary>
        /// Getter delegate
        /// </summary>
        public Func<object?, object?>? Getter { get; set; } = Getter;

        /// <summary>
        /// Setter delegate
        /// </summary>
        public Action<object?, object?>? Setter { get; set; } = Setter;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public object[] GetCustomAttributes(bool inherit) => Field.GetCustomAttributes(inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => Field.GetCustomAttributes(attributeType, inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public bool IsDefined(Type attributeType, bool inherit) => Field.IsDefined(attributeType, inherit);

        /// <summary>
        /// Cast as <see cref="FieldInfo"/>
        /// </summary>
        /// <param name="fi"><see cref="FieldInfoExt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator FieldInfo(in FieldInfoExt fi) => fi.Field;

        /// <summary>
        /// Cast from <see cref="FieldInfo"/>
        /// </summary>
        /// <param name="fi"><see cref="FieldInfo"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator FieldInfoExt(in FieldInfo fi) => From(fi);

        /// <summary>
        /// Cast as <see cref="FieldType"/>
        /// </summary>
        /// <param name="fi"><see cref="FieldInfoExt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator Type(in FieldInfoExt fi) => fi.FieldType;

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="fi">Field</param>
        /// <returns>Instance</returns>
        public static FieldInfoExt From(in FieldInfo fi)
        {
            int hc = fi.GetHashCode();
            if (Cache.TryGetValue(hc, out FieldInfoExt? res)) return res;
            res = new(fi, fi.CanCreateFieldGetter() ? fi.CreateFieldGetter() : null, fi.CanCreateFieldSetter() ? fi.CreateFieldSetter() : null);
            Cache.TryAdd(hc, res);
            return res;
        }
    }
}
