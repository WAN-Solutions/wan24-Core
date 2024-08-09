using System.Linq.Expressions;
using System.Reflection;

namespace wan24.Core
{
    // Property/field getter/setter
    public static partial class ReflectionExtensions
    {
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
                !pi.PropertyType.IsPointer &&
                pi.GetMethod.GetParameters().Length == 0;

        /// <summary>
        /// Determine if a property getter can be created (using <see cref="CreatePropertySetter(PropertyInfo)"/>)
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
                !pi.PropertyType.IsPointer &&
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
            return Expression.Lambda<Func<object?, object?>>(Expression.Convert(Expression.Property(Expression.Convert(objArg, pi.DeclaringType!), pi), typeof(object)), objArg).Compile();
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
            if (!typeof(tObj).IsAssignableFrom(pi.DeclaringType)) throw new ArgumentException($"Object type mismatch ({typeof(tObj)}/{pi.DeclaringType})", nameof(tObj));
            if (!typeof(tValue).IsAssignableFrom(pi.PropertyType)) throw new ArgumentException($"Value type mismatch ({typeof(tValue)}/{pi.PropertyType})", nameof(tValue));
            if (pi.GetMethod!.IsStatic) throw new ArgumentException("Non-static property required", nameof(pi));
            ParameterExpression objArg = Expression.Parameter(typeof(tObj), "obj");
            MemberExpression getter = Expression.Property(Expression.Convert(objArg, pi.DeclaringType), pi);
            Expression? returnValue = typeof(tValue) == pi.PropertyType
                ? null
                : Expression.Convert(getter, typeof(tValue));
            return Expression.Lambda<Func<tObj, tValue?>>(returnValue ?? getter, objArg).Compile();
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
            return Expression.Lambda<Func<object?>>(Expression.Convert(Expression.Property(null, pi), typeof(object))).Compile();
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
            if (!typeof(T).IsAssignableFrom(pi.DeclaringType)) throw new ArgumentException($"Object type mismatch ({typeof(T)}/{pi.DeclaringType})", nameof(T));
            if (!pi.GetMethod!.IsStatic) throw new ArgumentException("Static property required", nameof(pi));
            MemberExpression getter = Expression.Property(null, pi);
            Expression? returnValue = typeof(T) == pi.PropertyType
                ? null
                : Expression.Convert(getter, typeof(T));
            return Expression.Lambda<Func<T>>(returnValue ?? getter).Compile();
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
                    Expression.Property(Expression.Convert(objArg, pi.DeclaringType!),pi),
                    Expression.Convert(valueArg, pi.PropertyType)
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
            if (!typeof(tObj).IsAssignableFrom(pi.DeclaringType)) throw new ArgumentException($"Object type mismatch ({typeof(tObj)}/{pi.DeclaringType})", nameof(tObj));
            if (!pi.PropertyType.IsAssignableFrom(typeof(tValue))) throw new ArgumentException($"Value type mismatch ({typeof(tValue)}/{pi.PropertyType})", nameof(tValue));
            if (pi.SetMethod!.IsStatic) throw new ArgumentException("Non-static property required", nameof(pi));
            ParameterExpression objArg = Expression.Parameter(typeof(tObj), "obj"),
                valueArg = Expression.Parameter(typeof(tValue), "value");
            return Expression.Lambda<Action<tObj, tValue?>>(
                Expression.Assign(
                    Expression.Property(Expression.Convert(objArg, pi.DeclaringType), pi),
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
            return Expression.Lambda<Action<object?>>(Expression.Assign(Expression.Property(null, pi), Expression.Convert(valueArg, pi.PropertyType)), valueArg).Compile();
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
            if (!pi.PropertyType.IsAssignableFrom(typeof(T))) throw new ArgumentException($"Value type mismatch ({typeof(T)}/{pi.PropertyType})", nameof(T));
            if (!pi.SetMethod!.IsStatic) throw new ArgumentException("Static property required", nameof(pi));
            ParameterExpression valueArg = Expression.Parameter(typeof(T), "value");
            return Expression.Lambda<Action<T?>>(Expression.Assign(Expression.Property(null, pi), valueArg), valueArg).Compile();
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
