using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Reflection extensions
    /// </summary>
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Determine if a property is init-only
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Is init-only?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsInitOnly(this PropertyInfo pi)
            => pi.SetMethod is MethodInfo mi && mi.ReturnParameter.GetRequiredCustomModifiers().Any(m => m.Name == "IsExternalInit");

        /// <summary>
        /// Determine if a property is init-only
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Is init-only?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [Obsolete("Use PropertyInfoExt.IsInitOnly instead")]//TODO Remove in v3
        public static bool IsInitOnly(this PropertyInfoExt pi) => pi.IsInitOnly;

        /// <summary>
        /// Determine if a method is a property accessor
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>If the method is a property accessor</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsPropertyAccessor(this MethodInfo mi)
            => mi.DeclaringType is not null &&
                mi.IsSpecialName &&
                mi.DeclaringType.GetPropertiesCached(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(p => p.Property.GetMethod == mi || p.Property.SetMethod == mi);


        /// <summary>
        /// Determine if a method is an event handler control method (add/remove method of an <see cref="EventInfo"/>)
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>If the method is a property accessor</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsEventHandlerControl(this MethodInfo mi)
            => mi.DeclaringType is not null &&
                mi.IsSpecialName &&
                mi.DeclaringType.GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(e => e.AddMethod == mi || e.RemoveMethod == mi);

        /// <summary>
        /// Get the group name (see <see cref="GroupAttribute"/>)
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Group name</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string? GetGroupName(this PropertyInfo pi) => pi.GetCustomAttributeCached<GroupAttribute>()?.Name;

        /// <summary>
        /// Reflect all elements (which was defined by <see cref="IReflect"/> or <see cref="ReflectAttribute"/>, if <c>bindings</c> wasn't given - or <see cref="DEFAULT_BINDINGS"/>)
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="bindings">Bindings (<see cref="BindingFlags.GetField"/>, <see cref="BindingFlags.GetProperty"/> and <see cref="BindingFlags.InvokeMethod"/>)</param>
        /// <returns>Elements (<see cref="FieldInfoExt"/>, <see cref="PropertyInfoExt"/> or <see cref="MethodInfoExt"/>)</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<ICustomAttributeProvider> Reflect(this object obj, BindingFlags? bindings = null)
            => Reflect(obj.GetType(), bindings ?? (obj as IReflect)?.Bindings);
    }
}
