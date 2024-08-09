using System.Linq.Expressions;
using System.Reflection;

namespace wan24.Core
{
    // Field getter/setter
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Determine if a field getter can be created (using <see cref="CreateFieldGetter(FieldInfo)"/>)
        /// </summary>
        /// <param name="fi">Field</param>
        /// <returns>If a getter can be created</returns>
        public static bool CanCreateFieldGetter(this FieldInfo fi)
            => fi.DeclaringType is not null && 
                !fi.DeclaringType.IsGenericTypeDefinition && 
                !fi.DeclaringType.ContainsGenericParameters && 
                !fi.FieldType.IsByRef && 
                !fi.FieldType.IsByRefLike && 
                !fi.FieldType.IsPointer;

        /// <summary>
        /// Determine if a field setter can be created (using <see cref="CreateFieldSetter(FieldInfo)"/>)
        /// </summary>
        /// <param name="fi">Field</param>
        /// <returns>If a setter can be created</returns>
        public static bool CanCreateFieldSetter(this FieldInfo fi)
            => fi.DeclaringType is not null &&
                !fi.DeclaringType.IsGenericTypeDefinition &&
                !fi.DeclaringType.ContainsGenericParameters &&
                !fi.IsLiteral && 
                !fi.IsInitOnly && 
                !fi.FieldType.IsByRef && 
                !fi.FieldType.IsByRefLike &&
                !fi.FieldType.IsPointer;

        /// <summary>
        /// Create a field getter
        /// </summary>
        /// <param name="fi">Field</param>
        /// <returns>Getter</returns>
        public static Func<object?, object?> CreateFieldGetter(this FieldInfo fi)
        {
            EnsureCanCreateFieldGetter(fi);
            return fi.IsStatic
                ? CreateStaticFieldGetter2(fi)
                : CreateInstanceFieldGetter(fi);
        }

        /// <summary>
        /// Create an instance field getter
        /// </summary>
        /// <param name="fi">Field</param>
        /// <returns>Getter</returns>
        public static Func<object?, object?> CreateInstanceFieldGetter(this FieldInfo fi)
        {
            EnsureCanCreateFieldGetter(fi);
            if (fi.IsStatic) throw new ArgumentException("Field is static", nameof(fi));
            ParameterExpression objArg = Expression.Parameter(typeof(object), "obj");
            return Expression.Lambda<Func<object?, object?>>(Expression.Convert(Expression.Field(Expression.Convert(objArg, fi.DeclaringType!), fi), typeof(object)), objArg).Compile();
        }

        /// <summary>
        /// Create a typed instance field getter
        /// </summary>
        /// <typeparam name="tObj">Object type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="fi">Field</param>
        /// <returns>Getter</returns>
        public static Func<tObj, tValue?> CreateTypedInstanceFieldGetter<tObj, tValue>(this FieldInfo fi)
        {
            EnsureCanCreateFieldGetter(fi);
            if (!typeof(tObj).IsAssignableFrom(fi.DeclaringType)) throw new ArgumentException($"Object type mismatch ({typeof(tObj)}/{fi.DeclaringType})", nameof(tObj));
            if (!typeof(tValue).IsAssignableFrom(fi.FieldType)) throw new ArgumentException($"Value type mismatch ({typeof(tValue)}/{fi.FieldType})", nameof(tValue));
            if (fi.IsStatic) throw new ArgumentException("Field is static", nameof(fi));
            ParameterExpression objArg = Expression.Parameter(typeof(tObj), "obj");
            MemberExpression getter = Expression.Field(Expression.Convert(objArg, fi.DeclaringType), fi);
            Expression? returnValue = typeof(tValue) == fi.FieldType
                ? null
                : Expression.Convert(getter, typeof(tValue));
            return Expression.Lambda<Func<tObj, tValue?>>(returnValue ?? getter, objArg).Compile();
        }

        /// <summary>
        /// Create a static field getter
        /// </summary>
        /// <param name="fi">Field</param>
        /// <returns>Getter</returns>
        public static Func<object?> CreateStaticFieldGetter(this FieldInfo fi)
        {
            EnsureCanCreateFieldGetter(fi);
            if (!fi.IsStatic) throw new ArgumentException("Field is not static", nameof(fi));
            return Expression.Lambda<Func<object?>>(Expression.Convert(Expression.Field(null, fi), typeof(object))).Compile();
        }

        /// <summary>
        /// Create a static field getter
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="fi">Field</param>
        /// <returns>Getter</returns>
        public static Func<object?> CreateTypedStaticFieldGetter<T>(this FieldInfo fi)
        {
            EnsureCanCreateFieldGetter(fi);
            if (!fi.IsStatic) throw new ArgumentException("Field is not static", nameof(fi));
            return Expression.Lambda<Func<object?>>(Expression.Field(null, fi)).Compile();
        }

        /// <summary>
        /// Create a static field getter
        /// </summary>
        /// <param name="fi">Field</param>
        /// <returns>Getter</returns>
        public static Func<object?, object?> CreateStaticFieldGetter2(this FieldInfo fi)
        {
            EnsureCanCreateFieldGetter(fi);
            Func<object?> getter = CreateStaticFieldGetter(fi);
            return (obj) => getter();
        }

        /// <summary>
        /// Create a field setter
        /// </summary>
        /// <param name="fi">Field</param>
        /// <returns>Setter</returns>
        public static Action<object?, object?> CreateFieldSetter(this FieldInfo fi)
        {
            EnsureCanCreateFieldSetter(fi);
            return fi.IsStatic
                ? CreateStaticFieldSetter2(fi)
                : CreateInstanceFieldSetter(fi);
        }

        /// <summary>
        /// Create an instance field setter
        /// </summary>
        /// <param name="fi">Field</param>
        /// <returns>Setter</returns>
        public static Action<object?, object?> CreateInstanceFieldSetter(this FieldInfo fi)
        {
            EnsureCanCreateFieldSetter(fi);
            if (fi.IsStatic) throw new ArgumentException("Field is static", nameof(fi));
            ParameterExpression objArg = Expression.Parameter(typeof(object), "obj"),
                valueArg = Expression.Parameter(typeof(object), "value");
            return Expression.Lambda<Action<object?, object?>>(
                Expression.Assign(
                    Expression.Field(Expression.Convert(objArg, fi.DeclaringType!), fi),
                    Expression.Convert(valueArg, fi.FieldType)
                    ),
                objArg,
                valueArg
                )
                .Compile();
        }

        /// <summary>
        /// Create an instance field setter
        /// </summary>
        /// <typeparam name="tObj">Object type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="fi">Field</param>
        /// <returns>Setter</returns>
        public static Action<tObj, tValue?> CreateTypedInstanceFieldSetter<tObj, tValue>(this FieldInfo fi)
        {
            EnsureCanCreateFieldSetter(fi);
            if (!typeof(tObj).IsAssignableFrom(fi.DeclaringType)) throw new ArgumentException($"Object type mismatch ({typeof(tObj)}/{fi.DeclaringType})", nameof(tObj));
            if (!fi.FieldType.IsAssignableFrom(typeof(tValue))) throw new ArgumentException($"Value type mismatch ({typeof(tValue)}/{fi.FieldType})", nameof(tValue));
            if (fi.IsStatic) throw new ArgumentException("Field is static", nameof(fi));
            ParameterExpression objArg = Expression.Parameter(typeof(tObj), "obj"),
                valueArg = Expression.Parameter(typeof(tValue), "value");
            return Expression.Lambda<Action<tObj, tValue?>>(
                Expression.Assign(
                    Expression.Field(Expression.Convert(objArg, fi.DeclaringType), fi),
                    valueArg
                    ),
                objArg,
                valueArg
                )
                .Compile();
        }

        /// <summary>
        /// Create a static field setter
        /// </summary>
        /// <param name="fi">Field</param>
        /// <returns>Setter</returns>
        public static Action<object?> CreateStaticFieldSetter(this FieldInfo fi)
        {
            EnsureCanCreateFieldSetter(fi);
            if (!fi.IsStatic) throw new ArgumentException("Field is not static", nameof(fi));
            ParameterExpression valueArg = Expression.Parameter(typeof(object), "value");
            return Expression.Lambda<Action<object?>>(
                Expression.Assign(
                    Expression.Field(null, fi),
                    Expression.Convert(valueArg, fi.FieldType)
                    ),
                valueArg
                )
                .Compile();
        }

        /// <summary>
        /// Create a static field setter
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="fi">Field</param>
        /// <returns>Setter</returns>
        public static Action<T?> CreateTypedStaticFieldSetter<T>(this FieldInfo fi)
        {
            EnsureCanCreateFieldSetter(fi);
            if (!fi.FieldType.IsAssignableFrom(typeof(T))) throw new ArgumentException($"Value type mismatch ({typeof(T)}/{fi.FieldType})", nameof(T));
            if (!fi.IsStatic) throw new ArgumentException("Field is not static", nameof(fi));
            ParameterExpression valueArg = Expression.Parameter(typeof(T), "value");
            return Expression.Lambda<Action<T?>>(Expression.Assign(Expression.Field(null, fi), valueArg), valueArg).Compile();
        }

        /// <summary>
        /// Create a static field setter
        /// </summary>
        /// <param name="fi">Field</param>
        /// <returns>Setter</returns>
        public static Action<object?, object?> CreateStaticFieldSetter2(this FieldInfo fi)
        {
            EnsureCanCreateFieldSetter(fi);
            Action<object?> setter = CreateStaticFieldSetter(fi);
            return (obj, value) => setter(value);
        }

        /// <summary>
        /// Ensure it's possible to create a field getter
        /// </summary>
        /// <param name="fi">Field</param>
        /// <exception cref="ArgumentException">Can't create a field getter</exception>
        private static void EnsureCanCreateFieldGetter(in FieldInfo fi)
        {
            if (!CanCreateFieldGetter(fi)) throw new ArgumentException("Can't create getter for this kind of field", nameof(fi));
        }

        /// <summary>
        /// Ensure it's possible to create a field setter
        /// </summary>
        /// <param name="fi">Field</param>
        /// <exception cref="ArgumentException">Can't create a field setter</exception>
        private static void EnsureCanCreateFieldSetter(in FieldInfo fi)
        {
            if (!CanCreateFieldSetter(fi)) throw new ArgumentException("Can't create setter for this kind of field", nameof(fi));
        }
    }
}
