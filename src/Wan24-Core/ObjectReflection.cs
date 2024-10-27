using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Object reflection
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="Obj">Reflected object</param>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct ObjectReflection(in object Obj)
    {
        /// <summary>
        /// Object
        /// </summary>
        public readonly object Object = Obj;
        /// <summary>
        /// Type
        /// </summary>
        public readonly TypeInfoExt Type = TypeInfoExt.From(Obj.GetType());

        /// <summary>
        /// Get a field/property/method/delegate/event by its name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Field/property/method/delegate/event</returns>
        public ICustomAttributeProvider? this[string name] => Type[name];

        /// <summary>
        /// Get a custom attribute by its type
        /// </summary>
        /// <param name="type">Attribute type</param>
        /// <returns>Attribute</returns>
        public Attribute? this[Type type]
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Type[type];
        }

        /// <summary>
        /// Get values of all instance fields
        /// </summary>
        public Dictionary<string, object?> AllFieldValues
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                Dictionary<string, object?> res = [];
                foreach (FieldInfoExt fieldInfo in Type.Fields)
                    if (!fieldInfo.FieldInfo.IsStatic && fieldInfo.Getter is not null)
                        res[fieldInfo.Name] = fieldInfo.Getter(Object);
                return res;
            }
        }

        /// <summary>
        /// Get values of all instance properties
        /// </summary>
        public Dictionary<string, object?> AllPropertyValues
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                Dictionary<string, object?> res = [];
                foreach (PropertyInfoExt prop in Type.Properties)
                    if (prop.Getter is not null && !prop.Property.GetMethod!.IsStatic)
                        res[prop.Name] = prop.Getter(Object);
                return res;
            }
        }

        /// <summary>
        /// Get values of all instance fields and properties
        /// </summary>
        public Dictionary<string, object?> AllValues
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                Dictionary<string, object?> res = [];
                res.AddRange(AllFieldValues);
                res.AddRange(AllPropertyValues);
                return res;
            }
        }

        /// <summary>
        /// Determine if it's possible to get a value using <see cref="Get(in string)"/> from a field or property (also for static fields/properties)
        /// </summary>
        /// <param name="name">Field or property name</param>
        /// <returns>If can get</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool CanGet(in string name) => Type[name] switch
        {
            FieldInfoExt fieldInfo when fieldInfo.Getter is not null => true,
            PropertyInfoExt prop when prop.Getter is not null => true,
            _ => false
        };

        /// <summary>
        /// Determine if it's possible to set a value using <see cref="Set(in string, in object?)"/> to a field or property (also for static fields/properties)
        /// </summary>
        /// <param name="name">Field or property name</param>
        /// <returns>If can set</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool CanSet(in string name) => Type[name] switch
        {
            FieldInfoExt fieldInfo when fieldInfo.Setter is not null => true,
            PropertyInfoExt prop when prop.Setter is not null => true,
            _ => false
        };

        /// <summary>
        /// Get a field/property value (also for static fields/properties)
        /// </summary>
        /// <param name="name">Field or property name</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object? Get(in string name) => Object.GetValueReflected(name);

        /// <summary>
        /// Try getting a field/property value (also for static fields/properties)
        /// </summary>
        /// <param name="name">Field or property name</param>
        /// <param name="value">Value</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool TryGet(in string name, out object? value)
        {
            switch (Type[name])
            {
                case FieldInfoExt fieldInfo when fieldInfo.Getter is not null:
                    value = fieldInfo.Getter(Object);
                    return true;
                case PropertyInfoExt prop when prop.Getter is not null:
                    value = prop.Getter(Object);
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        /// <summary>
        /// Set a field/property value (also for static fields/properties)
        /// </summary>
        /// <param name="name">Field or property name</param>
        /// <param name="value">Value to set</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Set(in string name, in object? value) => Object.SetValueReflected(name, value);

        /// <summary>
        /// Try setting a field/property value (also for static fields/properties)
        /// </summary>
        /// <param name="name">Field or property name</param>
        /// <param name="value">Value to set</param>
        /// <returns>If succeed</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool TrySet(in string name, in object? value)
        {
            switch (Type[name])
            {
                case FieldInfoExt fieldInfo when fieldInfo.Setter is not null:
                    fieldInfo.Setter(Object, value);
                    return true;
                case PropertyInfoExt prop when prop.Setter is not null:
                    prop.Setter(Object, value);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Invoke a method (also for static methods)
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object? Invoke(string name, params object?[] param)
        {
            MethodInfoExt mi = Type.Methods.FirstOrDefault(m => m.Name == name) ?? throw new InvalidOperationException("Method not found");
            if (mi.Invoker is null) throw new InvalidOperationException("No useable method invoker");
            return mi.Invoker(Object, param);
        }

        /// <summary>
        /// Invoke a method (also for static methods)
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value (a task will be awaited for returning the task result)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public async Task<object?> InvokeAsync(string name, params object?[] param)
        {
            MethodInfoExt mi = Type.Methods.FirstOrDefault(m => m.Name == name) ?? throw new InvalidOperationException("Method not found");
            if (mi.Invoker is null) throw new InvalidOperationException("No useable method invoker");
            object? res = mi.Invoker(Object, param);
            return res is null ? null : await TaskHelper.GetAnyFinalTaskResultAsync(res).DynamicContext();
        }

        /// <summary>
        /// Clone the <see cref="Object"/> instance (using <see cref="ObjectMapping"/>)
        /// </summary>
        /// <returns>Clone</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object CloneInstance()
        {
            object res = Type.CreateInstance();
            try
            {
                Object.MapObjectTo(target: res);
            }
            catch
            {
                res.TryDispose();
                throw;
            }
            return res;
        }

        /// <summary>
        /// Clone the <see cref="Object"/> instance (using <see cref="ObjectMapping"/>)
        /// </summary>
        /// <returns>Clone</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public async Task<object> CloneInstanceAsync()
        {
            object res = Type.CreateInstance();
            try
            {
                await Object.MapObjectToAsync(target: res).DynamicContext();
            }
            catch
            {
                res.TryDispose();
                throw;
            }
            return res;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override string ToString() => $"{GetType()} of {Type.Type}";

        /// <summary>
        /// Cast as <see cref="TypeInfoExt"/>
        /// </summary>
        /// <param name="oa"><see cref="ObjectReflection"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator TypeInfoExt(in ObjectReflection oa) => oa.Type;

        /// <summary>
        /// Cast as <see cref="Type"/>
        /// </summary>
        /// <param name="oa"><see cref="ObjectReflection"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Type(in ObjectReflection oa) => oa.Type.Type;
    }
}
