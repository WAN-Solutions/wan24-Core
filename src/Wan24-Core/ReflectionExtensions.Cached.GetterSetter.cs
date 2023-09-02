using System.Reflection;

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
                    new object?[] {
                        pi.GetMethod!.CreateDelegate(typeof(Func<>).MakeGenericType(
                            pi.PropertyType
                            ))
                    }
                    )!
                : GetterDelegateMethod2.MakeGenericMethod(
                    pi.DeclaringType!,
                    pi.PropertyType
                    ).Invoke(
                    obj: null,
                    new object?[] {
                        pi.GetMethod!.CreateDelegate(typeof(Func<,>).MakeGenericType(
                            pi.DeclaringType!,
                            pi.PropertyType
                            ))
                    }
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
                    new object?[] {
                        pi.GetMethod!.CreateDelegate(typeof(Func<>).MakeGenericType(
                            pi.PropertyType
                            ))
                    }
                    )!
                : GetterDelegateMethod3.MakeGenericMethod(
                    pi.DeclaringType!,
                    pi.PropertyType,
                    typeof(T)
                    ).Invoke(
                    obj: null,
                    new object?[] {
                        pi.GetMethod!.CreateDelegate(typeof(Func<,>).MakeGenericType(
                            pi.DeclaringType!,
                            pi.PropertyType
                            ))
                    }
                    )!);
        }

        /// <summary>
        /// Get a getter delegate
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Getter delegate</returns>
        public static Func<object?, object?> GetGetterDelegate(this PropertyInfo pi)
        {
            if (!pi.CanRead) throw new ArgumentException("Property has no accessable getter", nameof(pi));
            return (Func<object?, object?>)(pi.GetMethod!.IsStatic
                ? StaticGetterDelegateMethod.MakeGenericMethod(
                    pi.PropertyType
                    ).Invoke(
                    obj: null,
                    new object?[] {
                    pi.GetMethod!.CreateDelegate(typeof(Func<>).MakeGenericType(
                        pi.PropertyType
                        ))
                    }
                    )!
                : GetterDelegateMethod.MakeGenericMethod(
                    pi.DeclaringType!,
                    pi.PropertyType
                    ).Invoke(
                    obj: null,
                    new object?[] {
                    pi.GetMethod!.CreateDelegate(typeof(Func<,>).MakeGenericType(
                        pi.DeclaringType!,
                        pi.PropertyType
                        ))
                    }
                    )!);
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
            if (!pi.CanWrite) throw new ArgumentException("Property has no accessable setter", nameof(pi));
            return (Action<object?, object?>)(pi.SetMethod!.IsStatic
                ? StaticSetterDelegateMethod.MakeGenericMethod(
                    pi.PropertyType
                    ).Invoke(
                    obj: null,
                    new object?[] {
                    pi.SetMethod!.CreateDelegate(typeof(Action<>).MakeGenericType(
                        pi.PropertyType
                        ))
                    }
                    )!
                : SetterDelegateMethod.MakeGenericMethod(
                    pi.DeclaringType!,
                    pi.PropertyType
                    ).Invoke(
                    obj: null,
                    new object?[] {
                    pi.SetMethod!.CreateDelegate(typeof(Action<,>).MakeGenericType(
                        pi.DeclaringType!,
                        pi.PropertyType
                        ))
                    }
                    )!);
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
    }
}
