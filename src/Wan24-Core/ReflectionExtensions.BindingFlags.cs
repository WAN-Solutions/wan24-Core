using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime;

namespace wan24.Core
{
    // Binding flags
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Determine if <see cref="BindingFlags"/> do match an object (which may be a field/property/method/constructor/event information)
        /// </summary>
        /// <param name="flags">Flags</param>
        /// <param name="obj">Object</param>
        /// <param name="type">Type</param>
        /// <returns>If match</returns>
        public static bool DoesMatch(this BindingFlags flags, in ICustomAttributeProvider obj, in Type? type = null)
        {
            bool wantPublic = IsPublic(flags),
                wantPrivate = IsNonPublic(flags),
                requirePublic = wantPublic && !wantPrivate,
                requirePrivate = wantPrivate && !wantPublic,
                wantStatic = obj is not Type && IsStatic(flags),
                wantInstance = obj is not Type && IsInstance(flags),
                requireStatic = obj is not Type && wantStatic && !wantInstance,
                requireInstance = obj is not Type && wantInstance && !wantStatic,
                isPublic,
                isStatic,
                isProtected;
            Type? declaringType;
            switch (obj)
            {
                case Type o:
                    isPublic = o.IsPublic || (o.IsDelegate() && o.IsVisible);
                    isStatic = o.GetConstructorsCached(ALL_BINDINGS).All(ci => ci.Constructor.IsStatic);
                    isProtected = o.IsNestedFamily;
                    declaringType = o.DeclaringType;
                    break;
                case TypeInfoExt o:
                    isPublic = o.Type.IsPublic || (o.Type.IsDelegate() && o.Type.IsVisible);
                    isStatic = o.Constructors.All(ci => ci.Constructor.IsStatic);
                    isProtected = o.Type.IsNestedFamily;
                    declaringType = o.DeclaringType;
                    break;
                case FieldInfo o:
                    isPublic = o.IsPublic;
                    isStatic = o.IsStatic;
                    isProtected = !isPublic && o.IsFamily;
                    declaringType = o.DeclaringType;
                    break;
                case FieldInfoExt o:
                    isPublic = o.FieldInfo.IsPublic;
                    isStatic = o.FieldInfo.IsStatic;
                    isProtected = !isPublic && o.FieldInfo.IsFamily;
                    declaringType = o.FieldInfo.DeclaringType;
                    break;
                case PropertyInfo o:
                    {
                        MethodInfo? mi = o.GetMethod ?? o.SetMethod;
                        isPublic = mi?.IsPublic ?? false;
                        isStatic = mi?.IsStatic ?? false;
                        isProtected = !isPublic && (mi?.IsFamily ?? false);
                        declaringType = o.DeclaringType;
                    }
                    break;
                case PropertyInfoExt o:
                    {
                        MethodInfo? mi = o.Property.GetMethod ?? o.Property.SetMethod;
                        isPublic = mi?.IsPublic ?? false;
                        isStatic = mi?.IsStatic ?? false;
                        isProtected = !isPublic && (mi?.IsFamily ?? false);
                        declaringType = o.DeclaringType;
                    }
                    break;
                case MethodInfo o:
                    isPublic = o.IsPublic;
                    isStatic = o.IsStatic;
                    isProtected = !isPublic && o.IsFamily;
                    declaringType = o.DeclaringType;
                    break;
                case MethodInfoExt o:
                    isPublic = o.Method.IsPublic;
                    isStatic = o.Method.IsStatic;
                    isProtected = !isPublic && o.Method.IsFamily;
                    declaringType = o.Method.DeclaringType;
                    break;
                case ConstructorInfo o:
                    isPublic = o.IsPublic;
                    isStatic = o.IsStatic;
                    isProtected = !isPublic && o.IsFamily;
                    declaringType = o.DeclaringType;
                    break;
                case ConstructorInfoExt o:
                    isPublic = o.Constructor.IsPublic;
                    isStatic = o.Constructor.IsStatic;
                    isProtected = !isPublic && o.Constructor.IsFamily;
                    declaringType = o.DeclaringType;
                    break;
                case EventInfo o:
                    {
                        MethodInfo? mi = o.AddMethod ?? o.RemoveMethod;
                        isPublic = mi?.IsPublic ?? false;
                        isStatic = mi?.IsStatic ?? false;
                        isProtected = !isPublic && (mi?.IsFamily ?? false);
                        declaringType = o.DeclaringType;
                    }
                    break;
                default:
                    return false;
            }
            if (requirePublic && !isPublic) return false;
            if (requirePrivate && isPublic) return false;
            if (requireStatic && !isStatic) return false;
            if (requireInstance && isStatic) return false;
            if (type is not null && type != declaringType)
            {
                if ((flags & BindingFlags.DeclaredOnly) == BindingFlags.DeclaredOnly) return false;
                if ((flags & BindingFlags.FlattenHierarchy) == BindingFlags.FlattenHierarchy && isStatic && !isPublic && !isProtected) return false;
            }
            return true;
        }

        /// <summary>
        /// Get the binding flags of a field/property/method/constructor/event information
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Binding flags</returns>
        public static BindingFlags GetBindingFlags(this ICustomAttributeProvider obj)
        {
            BindingFlags res = BindingFlags.Default;
            switch (obj)
            {
                case Type o:
                    res |= o.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
                    res |= o.GetConstructorsCached(ALL_BINDINGS).All(ci => ci.Constructor.IsStatic) ? BindingFlags.Static : BindingFlags.Instance;
                    break;
                case TypeInfoExt o:
                    res |= o.Type.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
                    res |= o.Constructors.All(ci => ci.Constructor.IsStatic) ? BindingFlags.Static : BindingFlags.Instance;
                    break;
                case FieldInfo o:
                    res |= o.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
                    res |= o.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
                    break;
                case FieldInfoExt o:
                    res |= o.FieldInfo.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
                    res |= o.FieldInfo.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
                    break;
                case PropertyInfo o:
                    {
                        MethodInfo? mi = o.GetMethod ?? o.SetMethod;
                        res |= mi?.IsPublic ?? false ? BindingFlags.Public : BindingFlags.NonPublic;
                        res |= mi?.IsStatic ?? false ? BindingFlags.Static : BindingFlags.Instance;
                    }
                    break;
                case PropertyInfoExt o:
                    {
                        MethodInfo? mi = o.Property.GetMethod ?? o.Property.SetMethod;
                        res |= mi?.IsPublic ?? false ? BindingFlags.Public : BindingFlags.NonPublic;
                        res |= mi?.IsStatic ?? false ? BindingFlags.Static : BindingFlags.Instance;
                    }
                    break;
                case MethodInfo o:
                    res |= o.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
                    res |= o.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
                    break;
                case MethodInfoExt o:
                    res |= o.Method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
                    res |= o.Method.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
                    break;
                case ConstructorInfo o:
                    res |= o.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
                    res |= o.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
                    break;
                case ConstructorInfoExt o:
                    res |= o.Constructor.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
                    res |= o.Constructor.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
                    break;
                case EventInfo o:
                    {
                        MethodInfo? mi = o.AddMethod ?? o.RemoveMethod;
                        res |= mi?.IsPublic ?? false ? BindingFlags.Public : BindingFlags.NonPublic;
                        res |= mi?.IsStatic ?? false ? BindingFlags.Static : BindingFlags.Instance;
                    }
                    break;
                default:
                    throw new ArgumentException("Unsupported object type", nameof(obj));
            }
            return res;
        }

        /// <summary>
        /// Is public?
        /// </summary>
        /// <param name="flags">Flags</param>
        /// <returns>If public</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsPublic(this BindingFlags flags) => (flags & BindingFlags.Public) == BindingFlags.Public;

        /// <summary>
        /// Is non-public?
        /// </summary>
        /// <param name="flags">Flags</param>
        /// <returns>If non-public</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsNonPublic(this BindingFlags flags) => (flags & BindingFlags.NonPublic) == BindingFlags.NonPublic;

        /// <summary>
        /// Is static?
        /// </summary>
        /// <param name="flags">Flags</param>
        /// <returns>If static</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsStatic(this BindingFlags flags) => (flags & BindingFlags.Static) == BindingFlags.Static;

        /// <summary>
        /// Is instance?
        /// </summary>
        /// <param name="flags">Flags</param>
        /// <returns>If instance</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsInstance(this BindingFlags flags) => (flags & BindingFlags.Instance) == BindingFlags.Instance;
    }
}
