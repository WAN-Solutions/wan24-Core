using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Type converter
    /// </summary>
    public static class TypeConverter
    {
        /// <summary>
        /// Registered type converters
        /// </summary>
        public static readonly ConcurrentDictionary<TypeConverterKey, Convert_Delegate> Registered = new(new EqualityComparer());

        /// <summary>
        /// Determine if a type can be converted to a target type
        /// </summary>
        /// <param name="source">Source type</param>
        /// <param name="target">Target type</param>
        /// <returns>If the source type can be converted to the target type</returns>
        public static bool CanConvert(in Type source, in Type target)
            => typeof(ITypeConverter<>).MakeGenericType(target).IsAssignableFrom(source) || Registered.ContainsKey(new(source, target));

        /// <summary>
        /// Convert an object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="target">Target type</param>
        /// <returns>Converted object</returns>
        /// <exception cref="InvalidCastException"></exception>
        public static object Convert(in object obj, in Type target)
        {
            TypeConverterKey key = new(obj.GetType(), target);
            if (!Registered.TryGetValue(key, out Convert_Delegate? converter))
            {
                if(obj is IConvertible convertible)
                    try
                    {
                        object res = convertible.ToType(target, provider: null);
                        if (target.IsAssignableFrom(res.GetType())) return res;
                        res.TryDispose();
                    }
                    catch (InvalidCastException)
                    {
                    }
                throw new InvalidCastException("Type can't be converted");
            }
            return converter(obj);
        }

        /// <summary>
        /// Convert an object
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Converted object</returns>
        /// <exception cref="InvalidCastException"></exception>
        public static T Convert<T>(in object obj)
        {
            TypeConverterKey key = new(obj.GetType(), typeof(T));
            if (!Registered.TryGetValue(key, out Convert_Delegate? converter))
            {
                if (obj is IConvertible convertible)
                    try
                    {
                        object res = convertible.ToType(typeof(T), provider: null);
                        if (typeof(T).IsAssignableFrom(res.GetType())) return (T)res;
                        res.TryDispose();
                    }
                    catch (InvalidCastException)
                    {
                    }
                throw new InvalidCastException("Type can't be converted");
            }
            return (T)converter(obj);
        }

        /// <summary>
        /// Convert to the target type
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Converted object</returns>
        public static T ConvertType<T>(this ITypeConverter<T> obj) => Convert<T>(obj);

        /// <summary>
        /// Type converter delegate
        /// </summary>
        /// <param name="obj">Object to convert</param>
        /// <returns>Converted object</returns>
        public delegate object Convert_Delegate(object obj);

        /// <summary>
        /// Type converter key for <see cref="Registered"/>
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public readonly record struct TypeConverterKey
        {
            /// <summary>
            /// Source type hash code
            /// </summary>
            [FieldOffset(0)]
            public readonly int SourceType;
            /// <summary>
            /// Target type hash code
            /// </summary>
            [FieldOffset(sizeof(int))]
            public readonly int TargetType;
            /// <summary>
            /// Hash code
            /// </summary>
            [FieldOffset(sizeof(int) << 1)]
            public readonly int HashCode;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="sourceType">Source type hash code</param>
            /// <param name="targetType">Target type hash code</param>
            public TypeConverterKey(in int sourceType, in int targetType)
            {
                SourceType = sourceType;
                TargetType = targetType;
                HashCode = SourceType ^ TargetType;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="source">Source type</param>
            /// <param name="target">Target type</param>
            public TypeConverterKey(in Type source, in Type target)
            {
                SourceType = source.GetHashCode();
                TargetType = target.GetHashCode();
                HashCode = SourceType ^ TargetType;
            }

            /// <inheritdoc/>
            public override int GetHashCode() => HashCode;
        }

        /// <summary>
        /// Equality comparer
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        private sealed class EqualityComparer() : IEqualityComparer<TypeConverterKey>
        {
            /// <inheritdoc/>
            public bool Equals(TypeConverterKey x, TypeConverterKey y) => x.SourceType == y.SourceType && x.TargetType == y.TargetType;

            /// <inheritdoc/>
            public int GetHashCode([DisallowNull] TypeConverterKey obj) => obj.HashCode;
        }
    }
}
