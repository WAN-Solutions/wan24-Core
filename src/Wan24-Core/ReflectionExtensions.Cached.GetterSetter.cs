using System.Linq.Expressions;
using System.Reflection;

//TODO Remove delegate related methods

namespace wan24.Core
{
    // Property getter/setter
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Getter delegate method
        /// </summary>
        private static readonly MethodInfo GetterDelegateMethod;
        /// <summary>
        /// Getter delegate method
        /// </summary>
        private static readonly MethodInfo GetterDelegateMethod2;
        /// <summary>
        /// Getter delegate method
        /// </summary>
        private static readonly MethodInfo GetterDelegateMethod3;
        /// <summary>
        /// Setter delegate method
        /// </summary>
        private static readonly MethodInfo SetterDelegateMethod;
        /// <summary>
        /// Getter delegate method
        /// </summary>
        private static readonly MethodInfo StaticGetterDelegateMethod;
        /// <summary>
        /// Getter delegate method
        /// </summary>
        private static readonly MethodInfo StaticGetterDelegateMethod2;
        /// <summary>
        /// Getter delegate method
        /// </summary>
        private static readonly MethodInfo StaticGetterDelegateMethod3;
        /// <summary>
        /// Setter delegate method
        /// </summary>
        private static readonly MethodInfo StaticSetterDelegateMethod;

        /// <summary>
        /// Constructor
        /// </summary>
        static ReflectionExtensions()
        {
            GetterDelegateMethod = typeof(ReflectionExtensions).GetMethod(nameof(CreateGetterDelegate), BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidProgramException($"Failed to reflect {typeof(ReflectionExtensions)}.{nameof(CreateGetterDelegate)}");
            GetterDelegateMethod2 = typeof(ReflectionExtensions).GetMethod(nameof(CreateGetterDelegate2), BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidProgramException($"Failed to reflect {typeof(ReflectionExtensions)}.{nameof(CreateGetterDelegate2)}");
            GetterDelegateMethod3 = typeof(ReflectionExtensions).GetMethod(nameof(CreateGetterDelegate3), BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidProgramException($"Failed to reflect {typeof(ReflectionExtensions)}.{nameof(CreateGetterDelegate3)}");
            SetterDelegateMethod = typeof(ReflectionExtensions).GetMethod(nameof(CreateSetterDelegate), BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidProgramException($"Failed to reflect {typeof(ReflectionExtensions)}.{nameof(CreateSetterDelegate)}");
            StaticGetterDelegateMethod = typeof(ReflectionExtensions).GetMethod(nameof(CreateStaticGetterDelegate), BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidProgramException($"Failed to reflect {typeof(ReflectionExtensions)}.{nameof(CreateStaticGetterDelegate)}");
            StaticGetterDelegateMethod2 = typeof(ReflectionExtensions).GetMethod(nameof(CreateStaticGetterDelegate2), BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidProgramException($"Failed to reflect {typeof(ReflectionExtensions)}.{nameof(CreateStaticGetterDelegate2)}");
            StaticGetterDelegateMethod3 = typeof(ReflectionExtensions).GetMethod(nameof(CreateStaticGetterDelegate3), BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidProgramException($"Failed to reflect {typeof(ReflectionExtensions)}.{nameof(CreateStaticGetterDelegate3)}");
            StaticSetterDelegateMethod = typeof(ReflectionExtensions).GetMethod(nameof(CreateStaticSetterDelegate), BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidProgramException($"Failed to reflect {typeof(ReflectionExtensions)}.{nameof(CreateStaticSetterDelegate)}");
        }

        /// <summary>
        /// Determine if a property getter can be created (using <see cref="CreatePropertyGetter(PropertyInfo)"/>)
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>If a getter can be created</returns>
        public static bool CanCreatePropertyGetter(this PropertyInfo pi)
            => pi.DeclaringType is not null &&
                !pi.DeclaringType.IsGenericTypeDefinition &&
                !pi.DeclaringType.ContainsGenericParameters &&
                pi.CanRead &&
                pi.GetMethod is not null &&
                (!pi.GetMethod.IsStatic || !pi.DeclaringType.IsInterface) &&
                !pi.PropertyType.IsByRef &&
                !pi.PropertyType.IsByRefLike &&
                pi.GetMethod.GetParameters().Length == 0;

        /// <summary>
        /// Determine if a property getter can be created (using <see cref="CreatePropertyGetter(PropertyInfo)"/>)
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>If a getter can be created</returns>
        public static bool CanCreatePropertySetter(this PropertyInfo pi)
            => pi.DeclaringType is not null &&
                !pi.DeclaringType.IsGenericTypeDefinition &&
                !pi.DeclaringType.ContainsGenericParameters &&
                pi.CanWrite &&
                pi.SetMethod is not null &&
                (!pi.SetMethod.IsStatic || !pi.DeclaringType.IsInterface) &&
                !pi.PropertyType.IsByRef &&
                !pi.PropertyType.IsByRefLike &&
                pi.SetMethod.GetParameters().Length == 1;

        /// <summary>
        /// Create a property getter
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Getter</returns>
        public static Func<object?, object?> CreatePropertyGetter(this PropertyInfo pi)
        {
            // https://tyrrrz.me/blog/expression-trees
            EnsureCanCreatePropertyGetter(pi);
            return pi.GetMethod!.IsStatic
                ? CreateStaticPropertyGetter2(pi)
                : CreateInstancePropertyGetter(pi);
        }

        /// <summary>
        /// Create an instance property getter
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Getter</returns>
        public static Func<object?, object?> CreateInstancePropertyGetter(this PropertyInfo pi)
        {
            EnsureCanCreatePropertyGetter(pi);
            if (pi.GetMethod!.IsStatic) throw new ArgumentException("Non-static property required", nameof(pi));
            ParameterExpression objArg = Expression.Parameter(typeof(object), "obj");
            return Expression.Lambda<Func<object?, object?>>(
                Expression.TypeAs(
                    Expression.Property(
                        pi.DeclaringType!.IsValueType
                            ? Expression.Convert(objArg, pi.DeclaringType)
                            : Expression.TypeAs(objArg, pi.DeclaringType),
                        pi),
                    typeof(object)
                    ),
                objArg
                )
                .Compile();
        }

        /// <summary>
        /// Create a typed instance property getter
        /// </summary>
        /// <typeparam name="tObj">Object type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="pi">Property</param>
        /// <returns>Getter</returns>
        public static Func<tObj, tValue?> CreateTypedInstancePropertyGetter<tObj, tValue>(this PropertyInfo pi)
        {
            EnsureCanCreatePropertyGetter(pi);
            if (!pi.DeclaringType!.IsAssignableFrom(typeof(tObj))) throw new ArgumentException("Object type mismatch", nameof(tObj));
            if (!pi.PropertyType.IsAssignableFrom(typeof(tValue))) throw new ArgumentException("Value type mismatch", nameof(tValue));
            if (pi.GetMethod!.IsStatic) throw new ArgumentException("Non-static property required", nameof(pi));
            ParameterExpression objArg = Expression.Parameter(typeof(tObj), "obj");
            MemberExpression getter = Expression.Property(objArg, pi);
            Expression? returnValue = typeof(tValue) == pi.PropertyType
                ? null
                : typeof(tValue).IsValueType
                    ? Expression.Convert(getter, typeof(tValue))
                    : Expression.TypeAs(getter, typeof(tValue));
            return Expression.Lambda<Func<tObj, tValue?>>(
                returnValue ?? getter,
                objArg
                )
                .Compile();
        }

        /// <summary>
        /// Create a static property getter
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Getter</returns>
        public static Func<object?> CreateStaticPropertyGetter(this PropertyInfo pi)
        {
            EnsureCanCreatePropertyGetter(pi);
            if (!pi.GetMethod!.IsStatic) throw new ArgumentException("Static property required", nameof(pi));
            return Expression.Lambda<Func<object?>>(
                pi.PropertyType.IsValueType
                    ? Expression.Convert(
                        Expression.Property(null, pi),
                        typeof(object)
                        )
                    : Expression.TypeAs(
                        Expression.Property(null, pi),
                        typeof(object)
                        )
                )
                .Compile();
        }

        /// <summary>
        /// Create a static property getter
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="pi">Property</param>
        /// <returns>Getter</returns>
        public static Func<T> CreateTypedStaticPropertyGetter<T>(this PropertyInfo pi)
        {
            EnsureCanCreatePropertyGetter(pi);
            if (!pi.DeclaringType!.IsAssignableFrom(typeof(T))) throw new ArgumentException("Object type mismatch", nameof(T));
            if (!pi.GetMethod!.IsStatic) throw new ArgumentException("Static property required", nameof(pi));
            MemberExpression getter = Expression.Property(null, pi);
            Expression? returnValue = typeof(T) == pi.PropertyType
                ? null
                : typeof(T).IsValueType
                    ? Expression.Convert(getter, typeof(T))
                    : Expression.TypeAs(getter, typeof(T));
            return Expression.Lambda<Func<T>>(
                returnValue ?? getter
                )
                .Compile();
        }

        /// <summary>
        /// Create a static property getter
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Getter</returns>
        public static Func<object?, object?> CreateStaticPropertyGetter2(this PropertyInfo pi)
        {
            Func<object?> getter = CreateStaticPropertyGetter(pi);
            return (obj) => getter();
        }

        /// <summary>
        /// Create a property setter
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Setter</returns>
        public static Action<object?, object?> CreatePropertySetter(this PropertyInfo pi)
        {
            EnsureCanCreatePropertySetter(pi);
            return pi.SetMethod!.IsStatic
                ? CreateStaticPropertySetter2(pi)
                : CreateInstancePropertySetter(pi);
        }

        /// <summary>
        /// Create an instance property setter
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Setter</returns>
        public static Action<object?, object?> CreateInstancePropertySetter(this PropertyInfo pi)
        {
            EnsureCanCreatePropertySetter(pi);
            if (pi.SetMethod!.IsStatic) throw new ArgumentException("Non-static property required", nameof(pi));
            ParameterExpression objArg = Expression.Parameter(typeof(object), "obj"),
                valueArg = Expression.Parameter(typeof(object), "value");
            return Expression.Lambda<Action<object?, object?>>(
                Expression.Assign(
                    Expression.Property(
                        pi.DeclaringType!.IsValueType
                            ? Expression.Convert(objArg, pi.DeclaringType)
                            : Expression.TypeAs(objArg, pi.DeclaringType),
                        pi
                        ),
                    pi.PropertyType.IsValueType
                        ? Expression.Convert(valueArg, pi.PropertyType)
                        : Expression.TypeAs(valueArg, pi.PropertyType)
                    ),
                objArg,
                valueArg
                )
                .Compile();
        }

        /// <summary>
        /// Create a typed instance property setter
        /// </summary>
        /// <typeparam name="tObj">Object type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="pi">Property</param>
        /// <returns>Setter</returns>
        public static Action<tObj, tValue?> CreateTypedInstancePropertySetter<tObj, tValue>(this PropertyInfo pi)
        {
            EnsureCanCreatePropertySetter(pi);
            if (!pi.DeclaringType!.IsAssignableFrom(typeof(tObj))) throw new ArgumentException("Object type mismatch", nameof(tObj));
            if (!pi.PropertyType.IsAssignableFrom(typeof(tValue))) throw new ArgumentException("Value type mismatch", nameof(tValue));
            if (pi.SetMethod!.IsStatic) throw new ArgumentException("Non-static property required", nameof(pi));
            ParameterExpression objArg = Expression.Parameter(typeof(tObj), "obj"),
                valueArg = Expression.Parameter(typeof(tValue), "value");
            return Expression.Lambda<Action<tObj, tValue?>>(
                Expression.Assign(
                    Expression.Property(objArg, pi),
                    valueArg
                    ),
                objArg,
                valueArg
                )
                .Compile();
        }

        /// <summary>
        /// Create a static property setter
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Setter</returns>
        public static Action<object?> CreateStaticPropertySetter(this PropertyInfo pi)
        {
            EnsureCanCreatePropertySetter(pi);
            if (!pi.SetMethod!.IsStatic) throw new ArgumentException("Static property required", nameof(pi));
            ParameterExpression valueArg = Expression.Parameter(typeof(object), "value");
            return Expression.Lambda<Action<object?>>(
                Expression.Assign(
                    Expression.Property(null, pi),
                    pi.PropertyType.IsValueType
                        ? Expression.Convert(valueArg, pi.PropertyType)
                        : Expression.TypeAs(valueArg, pi.PropertyType)
                    ),
                valueArg
                )
                .Compile();
        }

        /// <summary>
        /// Create a static property setter
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="pi">Property</param>
        /// <returns>Setter</returns>
        public static Action<T?> CreateTypedStaticPropertySetter<T>(this PropertyInfo pi)
        {
            EnsureCanCreatePropertySetter(pi);
            if (!pi.PropertyType.IsAssignableFrom(typeof(T))) throw new ArgumentException("Value type mismatch", nameof(T));
            if (!pi.SetMethod!.IsStatic) throw new ArgumentException("Static property required", nameof(pi));
            ParameterExpression valueArg = Expression.Parameter(typeof(T), "value");
            return Expression.Lambda<Action<T?>>(
                Expression.Assign(
                    Expression.Property(null, pi),
                    valueArg
                    ),
                valueArg
                )
                .Compile();
        }

        /// <summary>
        /// Create a static property setter
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Setter</returns>
        public static Action<object?, object?> CreateStaticPropertySetter2(this PropertyInfo pi)
        {
            Action<object?> setter = CreateStaticPropertySetter(pi);
            return (obj, value) => setter(value);
        }

        /// <summary>
        /// Get a getter delegate
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="pi">Property</param>
        /// <returns>Getter delegate</returns>
        public static Func<object?, T?> GetGetterDelegate<T>(this PropertyInfo pi)
        {
            if (!pi.CanRead) throw new ArgumentException("Property has no accessable getter", nameof(pi));
            if (typeof(T) != pi.PropertyType) throw new ArgumentException($"Property type mismatch ({typeof(T)}/{pi.PropertyType})", nameof(pi));
            return (Func<object?, T?>)(pi.GetMethod!.IsStatic
                ? StaticGetterDelegateMethod2.MakeGenericMethod(
                    pi.PropertyType
                    ).Invoke(
                    obj: null,
                    [
                        pi.GetMethod!.CreateDelegate(typeof(Func<>).MakeGenericType(
                            pi.PropertyType
                            ))
                    ]
                    )!
                : GetterDelegateMethod2.MakeGenericMethod(
                    pi.DeclaringType!,
                    pi.PropertyType
                    ).Invoke(
                    obj: null,
                    [
                        pi.GetMethod!.CreateDelegate(typeof(Func<,>).MakeGenericType(
                            pi.DeclaringType!,
                            pi.PropertyType
                            ))
                    ]
                    )!);
        }

        /// <summary>
        /// Get a getter delegate
        /// </summary>
        /// <typeparam name="T">Return type (casted)</typeparam>
        /// <param name="pi">Property</param>
        /// <returns>Getter delegate</returns>
        public static Func<object?, T?> GetCastedGetterDelegate<T>(this PropertyInfo pi)
        {
            if (!pi.CanRead) throw new ArgumentException("Property has no accessable getter", nameof(pi));
            return (Func<object?, T?>)(pi.GetMethod!.IsStatic
                ? StaticGetterDelegateMethod3.MakeGenericMethod(
                    pi.PropertyType,
                    typeof(T)
                    ).Invoke(
                    obj: null,
                    [
                        pi.GetMethod!.CreateDelegate(typeof(Func<>).MakeGenericType(
                            pi.PropertyType
                            ))
                    ]
                    )!
                : GetterDelegateMethod3.MakeGenericMethod(
                    pi.DeclaringType!,
                    pi.PropertyType,
                    typeof(T)
                    ).Invoke(
                    obj: null,
                    [
                        pi.GetMethod!.CreateDelegate(typeof(Func<,>).MakeGenericType(
                            pi.DeclaringType!,
                            pi.PropertyType
                            ))
                    ]
                    )!);
        }

        /// <summary>
        /// Get a getter delegate
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Getter delegate</returns>
        public static Func<object?, object?> GetGetterDelegate(this PropertyInfo pi)
        {
            if (pi.GetMethod is null) throw new ArgumentException("Property has no accessable getter", nameof(pi));
            return pi.GetMethod.IsStatic
                ? CreateStaticPropertyGetter2(pi)
                : CreateInstancePropertyGetter(pi);
            /*return (Func<object?, object?>)(pi.GetMethod!.IsStatic
                ? StaticGetterDelegateMethod.MakeGenericMethod(
                    pi.PropertyType
                    ).Invoke(
                    obj: null,
                    [
                    pi.GetMethod!.CreateDelegate(typeof(Func<>).MakeGenericType(
                    pi.PropertyType
                    ))
                    ]
                    )!
                : GetterDelegateMethod.MakeGenericMethod(
                    pi.DeclaringType!,
                    pi.PropertyType
                    ).Invoke(
                    obj: null,
                    [
                    pi.GetMethod!.CreateDelegate(typeof(Func<,>).MakeGenericType(
                    pi.DeclaringType!,
                    pi.PropertyType
                    ))
                    ]
                    )!);*/
        }

        /// <summary>
        /// Get a getter delegate
        /// </summary>
        /// <typeparam name="tObject">Object type</typeparam>
        /// <typeparam name="tReturn">Return type</typeparam>
        /// <param name="pi">Property</param>
        /// <returns>Getter delegate</returns>
        public static Func<tObject, tReturn?> GetGetterDelegate<tObject, tReturn>(this PropertyInfo pi)
        {
            if (!pi.CanRead) throw new ArgumentException("Property has no accessable getter", nameof(pi));
            return (Func<tObject, tReturn?>)(pi.GetMethod!.IsStatic
                ? Delegate.CreateDelegate(typeof(Func<tReturn?>), firstArgument: null, pi.GetMethod!)
                : Delegate.CreateDelegate(typeof(Func<tObject, tReturn?>), firstArgument: null, pi.GetMethod!));
        }

        /// <summary>
        /// Get a setter delegate
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Setter delegate</returns>
        public static Action<object?, object?> GetSetterDelegate(this PropertyInfo pi)
        {
            if (pi.SetMethod is null) throw new ArgumentException("Property has no accessable setter", nameof(pi));
            return pi.SetMethod.IsStatic
                ? CreateStaticPropertySetter2(pi)
                : CreateInstancePropertySetter(pi);
            /*return (Action<object?, object?>)(pi.SetMethod!.IsStatic
                ? StaticSetterDelegateMethod.MakeGenericMethod(
                    pi.PropertyType
                    ).Invoke(
                    obj: null,
                    [
                    pi.SetMethod!.CreateDelegate(typeof(Action<>).MakeGenericType(
                    pi.PropertyType
                    ))
                    ]
                    )!
                : SetterDelegateMethod.MakeGenericMethod(
                    pi.DeclaringType!,
                    pi.PropertyType
                    ).Invoke(
                    obj: null,
                    [
                    pi.SetMethod!.CreateDelegate(typeof(Action<,>).MakeGenericType(
                    pi.DeclaringType!,
                    pi.PropertyType
                    ))
                    ]
                    )!);*/
        }

        /// <summary>
        /// Get a setter delegate
        /// </summary>
        /// <typeparam name="tObject">Object type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="pi">Property</param>
        /// <returns>Setter delegate</returns>
        public static Action<tObject, tValue?> GetSetterDelegate<tObject, tValue>(this PropertyInfo pi)
        {
            if (!pi.CanWrite) throw new ArgumentException("Property has no accessable setter", nameof(pi));
            return (Action<tObject, tValue?>)(pi.SetMethod!.IsStatic
                ? Delegate.CreateDelegate(typeof(Action<tValue?>), firstArgument: null, pi.SetMethod!)
                : Delegate.CreateDelegate(typeof(Action<tObject, tValue?>), firstArgument: null, pi.SetMethod!));
        }

        /// <summary>
        /// Getter delegate
        /// </summary>
        /// <typeparam name="tObject">Object type</typeparam>
        /// <typeparam name="tReturn">Return type</typeparam>
        /// <param name="getter">Getter</param>
        /// <returns>Return value</returns>
        private static Func<object?, object?> CreateGetterDelegate<tObject, tReturn>(Func<tObject?, tReturn?> getter) => (obj) => getter((tObject?)obj);

        /// <summary>
        /// Getter delegate
        /// </summary>
        /// <typeparam name="tObject">Object type</typeparam>
        /// <typeparam name="tReturn">Return type</typeparam>
        /// <param name="getter">Getter</param>
        /// <returns>Return value</returns>
        private static Func<object?, tReturn?> CreateGetterDelegate2<tObject, tReturn>(Func<tObject?, tReturn?> getter) => (obj) => getter((tObject?)obj);

        /// <summary>
        /// Getter delegate
        /// </summary>
        /// <typeparam name="tObject">Object type</typeparam>
        /// <typeparam name="tReturn">Return type</typeparam>
        /// <typeparam name="tResult">Casted result type</typeparam>
        /// <param name="getter">Getter</param>
        /// <returns>Return value</returns>
        private static Func<object?, tResult?> CreateGetterDelegate3<tObject, tReturn, tResult>(Func<tObject?, tReturn?> getter)
        {
            if (typeof(IConvertible).IsAssignableFrom(typeof(tReturn)))
            {
                return (obj) => (tResult?)Convert.ChangeType(getter((tObject?)obj), typeof(tResult));
            }
            else
            {
                static tResult? cast(object? obj) => (tResult?)obj;
                return (obj) => cast(getter((tObject?)obj));
            }
        }

        /// <summary>
        /// Getter delegate
        /// </summary>
        /// <typeparam name="tObject">Object type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="setter">Getter</param>
        /// <returns>Return value</returns>
        private static Action<object?, object?> CreateSetterDelegate<tObject, tValue>(Action<tObject?, tValue?> setter) => (obj, value) => setter((tObject?)obj, (tValue?)value);

        /// <summary>
        /// Getter delegate
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="getter">Getter</param>
        /// <returns>Return value</returns>
        private static Func<object?, object?> CreateStaticGetterDelegate<T>(Func<T?> getter) => (obj) => getter();

        /// <summary>
        /// Getter delegate
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="getter">Getter</param>
        /// <returns>Return value</returns>
        private static Func<object?, T?> CreateStaticGetterDelegate2<T>(Func<T?> getter) => (obj) => getter();

        /// <summary>
        /// Getter delegate
        /// </summary>
        /// <typeparam name="tReturn">Return type</typeparam>
        /// <typeparam name="tResult">Casted result type</typeparam>
        /// <param name="getter">Getter</param>
        /// <returns>Return value</returns>
        private static Func<object?, tResult?> CreateStaticGetterDelegate3<tReturn, tResult>(Func<tReturn?> getter)
        {
            if (typeof(IConvertible).IsAssignableFrom(typeof(tReturn)))
            {
                return (obj) => (tResult?)Convert.ChangeType(getter(), typeof(tResult));
            }
            else
            {
                static tResult? cast(object? obj) => (tResult?)obj;
                return (obj) => cast(getter());
            }
        }

        /// <summary>
        /// Getter delegate
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="setter">Getter</param>
        /// <returns>Return value</returns>
        private static Action<object?, object?> CreateStaticSetterDelegate<T>(Action<T?> setter) => (obj, value) => setter((T?)value);

        /// <summary>
        /// Ensure it's possible to create a property getter
        /// </summary>
        /// <param name="pi">Property</param>
        /// <exception cref="ArgumentException">Can't create a property getter</exception>
        private static void EnsureCanCreatePropertyGetter(in PropertyInfo pi)
        {
            if (!CanCreatePropertyGetter(pi)) throw new ArgumentException("Can't create getter for this kind of property", nameof(pi));
        }

        /// <summary>
        /// Ensure it's possible to create a property setter
        /// </summary>
        /// <param name="pi">Property</param>
        /// <exception cref="ArgumentException">Can't create a property setter</exception>
        private static void EnsureCanCreatePropertySetter(in PropertyInfo pi)
        {
            if (!CanCreatePropertySetter(pi)) throw new ArgumentException("Can't create setter for this kind of property", nameof(pi));
        }
    }
}
