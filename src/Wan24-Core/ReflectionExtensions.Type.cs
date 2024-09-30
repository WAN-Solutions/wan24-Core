using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Type
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Get the final element type of a multi-dimensional type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Final element type</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type GetFinalElementType(this Type type)
        {
            if (type.IsAbstract) throw new ArgumentException("Not an array type", nameof(type));
            Type res = type.GetElementType()!;
            while (res.HasElementType) res = res.GetElementType()!;
            return res;
        }

        /// <summary>
        /// Unwrap the final result type of a task type recursive
        /// </summary>
        /// <param name="task">Task type</param>
        /// <returns>Final result type</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type? UnwrapFinalTaskResultType(this Type task)
        {
            if (!typeof(Task).IsAssignableFrom(task)) throw new ArgumentException("Task type required", nameof(task));
            while (true)
            {
                if (!task.IsGenericType || !typeof(Task).IsAssignableFrom(task)) return task;
                task = task.GetGenericArgumentCached(index: 0);
            }
        }

        /// <summary>
        /// Determine if a type can be assigned from another type (matches the generic type definition, too)
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="other">Other type</param>
        /// <returns>If assignable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsAssignableFromExt(this Type type, in Type other)
            => type == other ||
            (!type.IsGenericTypeDefinition && !other.IsGenericTypeDefinition && type.IsAssignableFrom(other)) ||
            (type.IsGenericTypeDefinition && ((other.IsGenericType && type == EnsureGenericTypeDefinition(other)) || other.HasBaseType(type)));

        /// <summary>
        /// Determine if a type implements a base type
        /// </summary>
        /// <param name="type">Type (may be a generic type definition, but not an interface)</param>
        /// <param name="baseType">Base type (may be a generic type definition, but not an interface, can't be <see cref="object"/>)</param>
        /// <returns>If the base type is implemented</returns>
        public static bool HasBaseType(this Type type, in Type baseType)
        {
            if (type.IsInterface || type.IsValueType || baseType.IsInterface || baseType.IsValueType)
                return false;
            bool isGtd = baseType.IsGenericTypeDefinition;
            for (
                TypeInfoExt? current = type.BaseType is null ? null : TypeInfoExt.From(type.BaseType), obj = typeof(object);
                current is not null && current != obj;
                current = current.Type.BaseType is null ? null : TypeInfoExt.From(current.Type.BaseType)
                )
                if (isGtd)
                {
                    if (
                        !current.IsGenericType ||
                        (current.IsGenericTypeDefinition && baseType != current.Type) ||
                        (!current.IsGenericTypeDefinition && baseType != current.GetGenericTypeDefinition()!.Type)
                        )
                        continue;
                    return true;
                }
                else if (baseType == current.Type)
                {
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Determine if a type is a delegate type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If the type is a delegate type</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsDelegate(this Type type) => type.HasBaseType(typeof(MulticastDelegate));

        /// <summary>
        /// Get the delegate method of a delegate
        /// </summary>
        /// <param name="type">Delegate type</param>
        /// <returns>Delegate method</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static MethodInfo GetDelegateMethod(this Type type)
        {
            if (!type.IsDelegate()) throw new InvalidOperationException("Not a delegate type");
            return type.GetMethodCached("Invoke") ?? throw new InvalidProgramException($"Delegate method {type}.Invoke not found");
        }

        /// <summary>
        /// Ensure working with the generic type definition(, if possible)
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="throwOnError">Throw an exception, if the type isn't generic?</param>
        /// <returns>Given type or its generic type definition</returns>
        /// <exception cref="ArgumentException">Not generic</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type EnsureGenericTypeDefinition(this Type type, in bool throwOnError = false)
        {
            if (!type.IsGenericType)
            {
                if (throwOnError) throw new ArgumentException("Not generic", nameof(type));
                return type;
            }
            return type.IsGenericTypeDefinition
                ? type
                : type.GetGenericTypeDefinition();
        }

        /// <summary>
        /// Get the base types of a type (excluding <see cref="object"/>)
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Base types</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            for (Type? baseType = type.BaseType, obj = typeof(object); baseType is not null && baseType != obj; baseType = baseType.BaseType)
                yield return baseType;
        }

        /// <summary>
        /// Get the closest type of a type
        /// </summary>
        /// <param name="types">Types</param>
        /// <param name="type">Type</param>
        /// <returns>Closest type from <c>types</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type? GetClosestType<T>(this T types, Type type) where T : IEnumerable<Type>
        {
            if ((types.FirstOrDefault(t => t == type) ?? types.FirstOrDefault(t => t.IsAssignableFrom(type))) is Type res) return res;
            if (!type.IsGenericType || type.IsGenericTypeDefinition) return null;
            TypeInfoExt gtd = TypeInfoExt.From(type).GetGenericTypeDefinition() ?? throw new InvalidProgramException();
            return types.FirstOrDefault(t => t.IsGenericTypeDefinition && t == gtd.Type);
        }

        /// <summary>
        /// Reflect all (which was defined by <see cref="ReflectAttribute"/>, if <c>bindings</c> wasn't given - or <see cref="DEFAULT_BINDINGS"/>)
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindings">Bindings (<see cref="BindingFlags.GetField"/>, <see cref="BindingFlags.GetProperty"/> and <see cref="BindingFlags.InvokeMethod"/>)</param>
        /// <returns>Elements (<see cref="FieldInfoExt"/>, <see cref="PropertyInfoExt"/> or <see cref="MethodInfoExt"/>)</returns>
        public static IEnumerable<ICustomAttributeProvider> Reflect(this Type type, BindingFlags? bindings = null)
        {
            BindingFlags flags = bindings ?? type.GetCustomAttributeCached<ReflectAttribute>()?.Bindings ?? ALL_BINDINGS,
                cleanFlags = flags & (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy);
            if (cleanFlags == BindingFlags.Default) cleanFlags = DEFAULT_BINDINGS;
            if ((flags & BindingFlags.GetField) == BindingFlags.GetField)
                foreach (FieldInfoExt fi in type.GetFieldsCached(cleanFlags))
                    yield return fi;
            if ((flags & BindingFlags.GetProperty) == BindingFlags.GetProperty)
                foreach (PropertyInfoExt pi in type.GetPropertiesCached(cleanFlags))
                    yield return pi;
            if ((flags & BindingFlags.InvokeMethod) == BindingFlags.InvokeMethod)
                foreach (MethodInfoExt mi in type.GetMethodsCached(cleanFlags))
                    yield return mi;
        }

        /// <summary>
        /// Determine if a type is a task type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If the type is a task type</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsTask(this Type type) => typeof(Task).IsAssignableFrom(type) || IsValueTask(type);

        /// <summary>
        /// Determine if a type is a task type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If the type is a task type</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsValueTask(this Type type)
            => type.IsValueType &&
                (
                    type == typeof(ValueTask) ||
                    (
                        type.IsGenericType &&
                        type.GetGenericTypeDefinition() == typeof(ValueTask<>)
                    )
                );

        /// <summary>
        /// Get the parameterless constructor
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Constructor</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ConstructorInfoExt? GetParameterlessConstructor(this Type type)
            => type.GetConstructorsCached(ALL_BINDINGS).OrderBy(c => c.Constructor.IsStatic).FirstOrDefault(c => c.ParameterCount == 0);

        /// <summary>
        /// Get the common base type
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="includeInterfaces">If interfaces are included</param>
        /// <param name="includeMainType">If to include the main type</param>
        /// <returns>Common base type</returns>
        public static Type? CommonBaseType(this IEnumerable<Type> enumerable, bool includeInterfaces = false, bool includeMainType = true)
        {
            HashSet<Type> baseTypes = [];
            Type[] types;
            // Find all base types and interfaces
            foreach (Type type in enumerable)
            {
                types = [.. type.GetBaseTypes()];
                if (includeMainType) baseTypes.Add(type);
                baseTypes.AddRange(types);
                if (!includeInterfaces) continue;
                baseTypes.AddRange(type.GetInterfaces());
                foreach (Type t in types) baseTypes.AddRange(t.GetInterfaces());
            }
            // Filter out incompatible base types and interfaces
            foreach (Type type in enumerable)
                foreach (Type incompatible in baseTypes.Where(t => !t.IsAssignableFrom(type)).ToArray())
                    baseTypes.Remove(incompatible);
            return baseTypes.FirstOrDefault();
        }

        /// <summary>
        /// Get the real type (f.e. from a by-ref(-like) type)
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Real type</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type GetRealType(this Type type) => !type.IsArray && !type.IsEnum && !type.IsPointer && type.HasElementType ? type.GetElementType()! : type;
    }
}
