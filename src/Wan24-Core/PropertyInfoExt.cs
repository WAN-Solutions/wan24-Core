using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Property information
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="Property">Property</param>
    /// <param name="Getter">Getter</param>
    /// <param name="Setter">Setter</param>
    public sealed record class PropertyInfoExt(in PropertyInfo Property, in Func<object?, object?>? Getter, in Action<object?, object?>? Setter) : ICustomAttributeProviderHost
    {
        /// <summary>
        /// Cache (key is the property hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<PropertyInfo, PropertyInfoExt> Cache = [];

        /// <summary>
        /// If the property is init-only
        /// </summary>
        private bool? _IsInitOnly = null;
        /// <summary>
        /// If the property is nullable
        /// </summary>
        private bool? _IsNullable = null;
        /// <summary>
        /// Bindings
        /// </summary>
        private BindingFlags? _Bindings = null;

        /// <summary>
        /// Property
        /// </summary>
        public PropertyInfo Property { get; } = Property;

        /// <summary>
        /// Full name including namespace and type
        /// </summary>
        public string FullName
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => $"{Property.DeclaringType}.{Property.Name}";
        }

        /// <summary>
        /// Bindings
        /// </summary>
        public BindingFlags Bindings => GetBindings();

        /// <summary>
        /// Property type
        /// </summary>
        public Type PropertyType
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Property.PropertyType;
        }

        /// <summary>
        /// Property name
        /// </summary>
        public string Name
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Property.Name;
        }

        /// <summary>
        /// Property declaring type
        /// </summary>
        public Type? DeclaringType
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Property.DeclaringType;
        }

        /// <summary>
        /// Getter
        /// </summary>
        public Func<object?, object?>? Getter { get; set; } = Getter;

        /// <summary>
        /// Can read?
        /// </summary>
        public bool CanRead
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Property.CanRead;
        }

        /// <summary>
        /// If the property has a public getter
        /// </summary>
        public bool HasPublicGetter => Property.GetMethod?.IsPublic ?? false;

        /// <summary>
        /// Setter
        /// </summary>
        public Action<object?, object?>? Setter { get; set; } = Setter;

        /// <summary>
        /// If the property has a public setter
        /// </summary>
        public bool HasPublicSetter => Property.SetMethod?.IsPublic ?? false;

        /// <summary>
        /// Can write?
        /// </summary>
        public bool CanWrite
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Property.CanWrite;
        }

        /// <summary>
        /// If the property is init-only
        /// </summary>
        public bool IsInitOnly => GetIsInitOnly();

        /// <summary>
        /// If the property is nullable
        /// </summary>
        public bool IsNullable => GetIsNullable();

        /// <inheritdoc/>
        ICustomAttributeProvider ICustomAttributeProviderHost.Hosted => Property;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object[] GetCustomAttributes(bool inherit) => Property.GetCustomAttributes(inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => Property.GetCustomAttributes(attributeType, inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsDefined(Type attributeType, bool inherit) => Property.IsDefined(attributeType, inherit);

        /// <summary>
        /// Get a value converted (see <see cref="ValueConverterAttribute"/>)
        /// </summary>
        /// <param name="obj">Instance</param>
        /// <param name="ignoreMissingConverter">If to ignore a missing converter setup</param>
        /// <returns>Value</returns>
        [MemberNotNull(nameof(Getter))]
        public object? GetConverted(object? obj, bool ignoreMissingConverter = false)
        {
            if (Getter is null)
                throw new InvalidOperationException("No getter");
            if (Property.GetCustomAttributeCached<ValueConverterAttribute>() is ValueConverterAttribute attr)
            {
                if (attr.Converter is not null)
                {
                    return attr.Convert(this, Getter(obj));
                }
                else if (!ignoreMissingConverter)
                {
                    throw new InvalidOperationException("Missing value converter");
                }
            }
            else if (!ignoreMissingConverter)
            {
                throw new InvalidOperationException($"Missing {nameof(ValueConverterAttribute)}");
            }
            return Getter(obj);
        }

        /// <summary>
        /// Set a value converted (see <see cref="ValueConverterAttribute"/>)
        /// </summary>
        /// <param name="obj">Instance</param>
        /// <param name="value">Value to set</param>
        /// <param name="ignoreMissingConverter">If to ignore a missing converter setup</param>
        [MemberNotNull(nameof(Setter))]
        public void SetConverted(object? obj, object? value, bool ignoreMissingConverter = false)
        {
            if (Setter is null)
                throw new InvalidOperationException("No setter");
            if (Property.GetCustomAttributeCached<ValueConverterAttribute>() is ValueConverterAttribute attr)
            {
                if (attr.ReConverter is not null)
                {
                    Setter(obj, attr.ReConvert(this, value));
                }
                else if(ignoreMissingConverter)
                {
                    Setter(obj, value);
                }
                else
                {
                    throw new InvalidOperationException("Missing value converter");
                }
            }
            else if(!ignoreMissingConverter)
            {
                throw new InvalidOperationException($"Missing {nameof(ValueConverterAttribute)}");
            }
        }

        /// <summary>
        /// Get the bindings
        /// </summary>
        /// <returns>Bindings</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private BindingFlags GetBindings() => _Bindings ??= Property.GetBindingFlags();

        /// <summary>
        /// Get if nullable
        /// </summary>
        /// <returns>If nullable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool GetIsNullable() => _IsNullable ??= Property.IsNullable();

        /// <summary>
        /// Get if init-only
        /// </summary>
        /// <returns>If init-only</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool GetIsInitOnly() => _IsInitOnly ??= Property.IsInitOnly();

        /// <summary>
        /// Cast as <see cref="PropertyInfo"/>
        /// </summary>
        /// <param name="pi"><see cref="PropertyInfoExt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator PropertyInfo(in PropertyInfoExt pi) => pi.Property;

        /// <summary>
        /// Cast from <see cref="PropertyInfo"/>
        /// </summary>
        /// <param name="pi"><see cref="PropertyInfo"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator PropertyInfoExt(in PropertyInfo pi) => From(pi);

        /// <summary>
        /// Cast as <see cref="PropertyType"/>
        /// </summary>
        /// <param name="pi"><see cref="PropertyInfoExt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Type(in PropertyInfoExt pi) => pi.PropertyType;

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Instance</returns>
        public static PropertyInfoExt From(in PropertyInfo pi)
        {
            try
            {
                if (Cache.TryGetValue(pi, out PropertyInfoExt? res)) return res;
                res ??= new(pi, pi.CanCreatePropertyGetter() ? pi.CreatePropertyGetter() : null, pi.CanCreatePropertySetter() ? pi.CreatePropertySetter() : null);
                Cache.TryAdd(pi, res);
                return res;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create {typeof(PropertyInfoExt)} for {pi.DeclaringType}.{pi.Name}", ex);
            }
        }
    }
}
