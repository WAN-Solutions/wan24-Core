﻿using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Get/set/invoke
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Invoke a method using reflections
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="method">Method name (may be static also)</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object? InvokeReflected(this object obj, string method, params object?[] param)
        {
            if (
                obj.GetType().GetMethodsCached(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(m => m.Name == method && m.Parameters.Length == param.Length) is not MethodInfoExt mi
                )
                throw new InvalidOperationException("Method not found");
            if (mi.Method.IsGenericMethod) throw new InvalidOperationException($"Can't invoke generic method {mi.Method.DeclaringType}.{mi.Name}`{mi.GenericArguments.Length}");
            return InvokeFast(mi, mi.Method.IsStatic ? null : obj, param);
        }

        /// <summary>
        /// Invoke a generic method using reflections
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="method">Method name (may be static also)</param>
        /// <param name="genericArgs">Generic arguments</param>
        /// <param name="param">Parameters</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object? InvokeReflectedGeneric(this object obj, string method, Type[] genericArgs, params object?[] param)
        {
            if (
                obj.GetType().GetMethodsCached(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(m => m.Name == method && m.Method.IsGenericMethod && m.GenericArguments.Length == genericArgs.Length && m.Parameters.Length == param.Length)
                        is not MethodInfoExt mi
                )
                throw new InvalidOperationException("Method not found");
            return InvokeFast(mi.Method.MakeGenericMethod(genericArgs), mi.Method.IsStatic ? null : obj, param);
        }

        /// <summary>
        /// Get a field/property value using reflections
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="fieldOrProperty">Field or property name (may be static)</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object? GetValueReflected(this object obj, string fieldOrProperty)
        {
            Type type = obj.GetType();
            if(type.GetPropertyCached(fieldOrProperty, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) is PropertyInfoExt pi)
            {
                if (pi.Getter is null) throw new InvalidOperationException($"Property {type}.{pi.Name} has no usable getter");
                return pi.Getter(pi.Property.GetMethod!.IsStatic ? null : obj);
            }
            else if(type.GetFieldCached(fieldOrProperty, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) is FieldInfoExt fi)
            {
                if (fi.Getter is null) throw new InvalidOperationException($"Field {type}.{fi.Name} has no usable getter");
                return fi.Getter(obj);
            }
            throw new InvalidOperationException("Field/property not found");
        }

        /// <summary>
        /// Set a field/property value using reflections
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="fieldOrProperty">Field or property name (may be static)</param>
        /// <param name="value">Value to set</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetValueReflected(this object obj, string fieldOrProperty, object? value)
        {
            Type type = obj.GetType();
            if (type.GetPropertyCached(fieldOrProperty, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) is PropertyInfoExt pi)
            {
                if (pi.Setter is null) throw new InvalidOperationException($"Property {type}.{pi.Name} has no usable setter");
                pi.Setter(pi.Property.SetMethod!.IsStatic ? null : obj, value);
            }
            else if (type.GetFieldCached(fieldOrProperty, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) is FieldInfoExt fi)
            {
                if (fi.Setter is null) throw new InvalidOperationException($"Field {type}.{fi.Name} has no usable setter");
                fi.Setter(obj, value);
            }
            else
            {
                throw new InvalidOperationException("Field/property not found");
            }
        }
    }
}
