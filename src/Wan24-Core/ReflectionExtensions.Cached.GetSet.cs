using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    // Get/set cached
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Faster <see cref="PropertyInfo.GetValue(object?)"/>
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="obj">Object</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? GetValueFast(this PropertyInfoExt pi, in object? obj)
        {
            if (pi.Getter is null) throw new InvalidOperationException("The property has no usable getter");
            return pi.Getter(obj);
        }

        /// <summary>
        /// Faster <see cref="PropertyInfo.GetValue(object?)"/>
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="obj">Object</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static object? GetValueFast(this PropertyInfo pi, in object? obj)
        {
            PropertyInfoExt prop = PropertyInfoExt.From(pi);
            if (prop.Getter is null) throw new InvalidOperationException("The property has no usable getter");
            return prop.Getter(obj);
        }

        /// <summary>
        /// Faster <see cref="PropertyInfo.SetValue(object?, object?)"/>
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="obj">Object</param>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void SetValueFast(this PropertyInfoExt pi, in object? obj, in object? value)
        {
            if (pi.Setter is null) throw new InvalidOperationException("The property has no usable setter");
            pi.Setter(obj, value);
        }

        /// <summary>
        /// Faster <see cref="PropertyInfo.SetValue(object?, object?)"/>
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="obj">Object</param>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static void SetValueFast(this PropertyInfo pi, in object? obj, in object? value)
        {
            PropertyInfoExt prop = PropertyInfoExt.From(pi);
            if (prop.Setter is null) throw new InvalidOperationException("The property has no usable setter");
            prop.Setter(obj, value);
        }

        /// <summary>
        /// Faster <see cref="FieldInfo.GetValue(object?)"/>
        /// </summary>
        /// <param name="fi">Field</param>
        /// <param name="obj">Object</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? GetValueFast(this FieldInfoExt fi, in object? obj)
        {
            if (fi.Getter is null) throw new InvalidOperationException("The field has no usable getter");
            return fi.Getter(obj);
        }

        /// <summary>
        /// Faster <see cref="FieldInfo.GetValue(object?)"/>
        /// </summary>
        /// <param name="fi">Field</param>
        /// <param name="obj">Object</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static object? GetValueFast(this FieldInfo fi, in object? obj)
        {
            FieldInfoExt field = FieldInfoExt.From(fi);
            if (field.Getter is null) throw new InvalidOperationException("The field has no usable getter");
            return field.Getter(obj);
        }

        /// <summary>
        /// Faster <see cref="FieldInfo.SetValue(object?, object?)"/>
        /// </summary>
        /// <param name="fi">Field</param>
        /// <param name="obj">Object</param>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void SetValueFast(this FieldInfoExt fi, in object? obj, in object? value)
        {
            if (fi.Setter is null) throw new InvalidOperationException("The field has no usable setter");
            fi.Setter(obj, value);
        }

        /// <summary>
        /// Faster <see cref="FieldInfo.SetValue(object?, object?)"/>
        /// </summary>
        /// <param name="fi">Field</param>
        /// <param name="obj">Object</param>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static void SetValueFast(this FieldInfo fi, in object? obj, in object? value)
        {
            FieldInfoExt field = FieldInfoExt.From(fi);
            if (field.Setter is null) throw new InvalidOperationException("The field has no usable setter");
            field.Setter(obj, value);
        }
    }
}
