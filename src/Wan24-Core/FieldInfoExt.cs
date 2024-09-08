using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

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
        /// Full name including namespace and type
        /// </summary>
        public string FullName
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => $"{Field.DeclaringType}.{Field.Name}";
        }

        /// <summary>
        /// Bindings
        /// </summary>
        public BindingFlags Bindings => GetBindings();

        /// <summary>
        /// Field name
        /// </summary>
        public string Name
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Field.Name;
        }

        /// <summary>
        /// Field declaring type
        /// </summary>
        public Type? DeclaringType
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Field.DeclaringType;
        }

        /// <summary>
        /// Field type
        /// </summary>
        public Type FieldType
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Field.FieldType;
        }

        /// <summary>
        /// If the field is read-only
        /// </summary>
        public bool IsInitOnly
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Field.IsInitOnly;
        }

        /// <summary>
        /// If the field is nullable
        /// </summary>
        public bool IsNullable => GetIsNullable();

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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object[] GetCustomAttributes(bool inherit) => Field.GetCustomAttributes(inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => Field.GetCustomAttributes(attributeType, inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsDefined(Type attributeType, bool inherit) => Field.IsDefined(attributeType, inherit);

        /// <summary>
        /// Get the bindings
        /// </summary>
        /// <returns>Bindings</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private BindingFlags GetBindings() => _Bindings ??= Field.GetBindingFlags();

        /// <summary>
        /// Get if nullable
        /// </summary>
        /// <returns>If nullable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool GetIsNullable() => _IsNullable ??= Field.IsNullable();

        /// <summary>
        /// Cast as <see cref="FieldInfo"/>
        /// </summary>
        /// <param name="fi"><see cref="FieldInfoExt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator FieldInfo(in FieldInfoExt fi) => fi.Field;

        /// <summary>
        /// Cast from <see cref="FieldInfo"/>
        /// </summary>
        /// <param name="fi"><see cref="FieldInfo"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator FieldInfoExt(in FieldInfo fi) => From(fi);

        /// <summary>
        /// Cast as <see cref="FieldType"/>
        /// </summary>
        /// <param name="fi"><see cref="FieldInfoExt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Type(in FieldInfoExt fi) => fi.FieldType;

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="fi">Field</param>
        /// <returns>Instance</returns>
        public static FieldInfoExt From(in FieldInfo fi)
        {
            try
            {
                int hc = fi.GetHashCode();
                if (Cache.TryGetValue(hc, out FieldInfoExt? res)) return res;
                res = new(fi, fi.CanCreateFieldGetter() ? fi.CreateFieldGetter() : null, fi.CanCreateFieldSetter() ? fi.CreateFieldSetter() : null);
                Cache.TryAdd(hc, res);
                return res;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create {typeof(FieldInfoExt)} for {fi.DeclaringType}.{fi.Name}", ex);
            }
        }
    }
}
