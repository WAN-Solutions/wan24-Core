using System.Numerics;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="SerializerTypeInformation"/> extensions
    /// </summary>
    public static class SerializerTypeInformationExtensions
    {
        /// <summary>
        /// Get <see cref="SerializerTypeInformation"/> from a <see cref="Type"/>
        /// </summary>
        /// <param name="type"><see cref="Type"/></param>
        /// <param name="options">Options</param>
        /// <returns><see cref="SerializerTypeInformation"/> (may be multiple)</returns>
        public static IEnumerable<SerializerTypeInformation> ToSerializerTypeInformation(this Type type, SerializerOptions? options = null)
        {
            options ??= SerializerOptions.Default;
            /*
             * Try custom serializer types form the settings
             */
            if (SerializerSettings.SerializerTypes.ContainsValue(type))
            {
                yield return SerializerSettings.SerializerTypes.Where(kvp => kvp.Value == type).Select(kvp => (SerializerTypeInformation)kvp.Key).First();
                yield break;
            }
            else if (type.IsGenericType && SerializerSettings.SerializerTypes.ContainsValue(type.GetGenericTypeDefinition()))
            {
                Type gtd = type.GetGenericTypeDefinition();
                yield return SerializerSettings.SerializerTypes.Where(kvp => kvp.Value == gtd).Select(kvp => (SerializerTypeInformation)kvp.Key).First();
                foreach (Type ga in type.GetGenericArgumentsCached())
                    foreach (SerializerTypeInformation sti in ToSerializerTypeInformation(ga, options))
                        yield return sti;
                yield break;
            }
            /*
             * Numeric types
             */
            SerializerNumericTypes snt = type.ToSerializerNumericTypes();
            if (snt != SerializerNumericTypes.None)
            {
                yield return snt.ToSerializerTypeInformation();
                yield break;
            }
            /*
             * Numeric array types
             */
            if (type.IsArray)
            {
                snt = (type.GetElementType() ?? throw new InvalidProgramException($"Failed to get element type of array type {type}")).ToSerializerNumericTypes();
                if (snt != SerializerNumericTypes.None)
                {
                    yield return snt.ToSerializerTypeInformation();
                    yield break;
                }
            }
            /*
             * String and string array
             */
            if (type == typeof(string))
            {
                yield return SerializerTypeInformation.String;
                yield break;
            }
            else if (type == typeof(string[]))
            {
                yield return SerializerTypeInformation.StringArray;
                yield break;
            }
            /*
             * Basic generic types
             */
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                yield return SerializerTypeInformation.List;
                foreach (SerializerTypeInformation sti in ToSerializerTypeInformation(type.GetGenericArgumentsCached()[0], options))
                    yield return sti;
                yield break;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                yield return SerializerTypeInformation.Set;
                foreach (SerializerTypeInformation sti in ToSerializerTypeInformation(type.GetGenericArgumentsCached()[0], options))
                    yield return sti;
                yield break;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                yield return SerializerTypeInformation.Dictionary;
                foreach (Type ga in type.GetGenericArgumentsCached())
                    foreach (SerializerTypeInformation sti in ToSerializerTypeInformation(ga, options))
                        yield return sti;
                yield break;
            }
            /*
             * Other types
             */
            if (type.IsArray)
            {
                yield return SerializerTypeInformation.Array;
                Type et = type;
                SerializerTypeInformation firstSti = SerializerTypeInformation.Array;
                while (firstSti == SerializerTypeInformation.Array)
                {
                    et = type.GetElementType() ?? throw new InvalidProgramException($"Failed to get element type of array type {type}");
                    firstSti = SerializerTypeInformation.Null;
                    foreach (SerializerTypeInformation sti in ToSerializerTypeInformation(et, options))
                    {
                        if (firstSti == SerializerTypeInformation.Null) firstSti = sti;
                        yield return sti;
                    }
                }
                yield break;
            }
            else if (type.IsEnum)
            {
                yield return SerializerTypeInformation.Enum;
                yield break;
            }
            else if (type == typeof(Type))
            {
                yield return SerializerTypeInformation.Type;
                yield break;
            }
            else if(type == typeof(bool))
            {
                yield return SerializerTypeInformation.Boolean;
                yield break;
            }
            else if (typeof(Stream).IsAssignableFrom(type))
            {
                yield return SerializerTypeInformation.Stream;
                yield break;
            }
            /*
             * Type cache
             */
            if (options.UseTypeCache)
            {
                if (options.UseNamedTypeCache && TypeCache.GetTypeByNameHashCode(type.ToString().GetHashCode()) is not null)
                {
                    yield return SerializerTypeInformation.NamedTypeCache;
                    yield break;
                }
                if (TypeCache.GetTypeByHashCode(type.GetHashCode()) is not null)
                {
                    yield return SerializerTypeInformation.UnnamedTypeCache;
                    yield break;
                }
            }
            /*
             * Object type
             */
            SerializerObjectTypes sot = type.ToSerializerObjectTypes();
            if (sot.ContainsClrType())
            {
                yield return SerializerTypeInformation.ObjectType;
                yield break;
            }
            /*
             * Structure
             */
            if (type.IsValueType)
            {
                yield return SerializerTypeInformation.Structure;
                yield break;
            }
            /*
             * Type name
             */
            yield return SerializerTypeInformation.TypeName;
        }

        /// <summary>
        /// Get the <see cref="SerializerNumericTypes"/>
        /// </summary>
        /// <param name="type"><see cref="SerializerTypeInformation"/></param>
        /// <returns><see cref="SerializerNumericTypes"/></returns>
        public static SerializerNumericTypes ToSerializerNumericTypes(this SerializerTypeInformation type)
            => (type & ~SerializerTypeInformation.FLAGS) switch
            {
                SerializerTypeInformation.Byte => SerializerNumericTypes.Byte,
                SerializerTypeInformation.SByte => SerializerNumericTypes.SByte,
                SerializerTypeInformation.UShort => SerializerNumericTypes.UShort,
                SerializerTypeInformation.Short => SerializerNumericTypes.Short,
                SerializerTypeInformation.Half => SerializerNumericTypes.Half,
                SerializerTypeInformation.UInt => SerializerNumericTypes.UInt,
                SerializerTypeInformation.Int => SerializerNumericTypes.Int,
                SerializerTypeInformation.Float => SerializerNumericTypes.Float,
                SerializerTypeInformation.ULong => SerializerNumericTypes.ULong,
                SerializerTypeInformation.Long => SerializerNumericTypes.Long,
                SerializerTypeInformation.Double => SerializerNumericTypes.Double,
                SerializerTypeInformation.Decimal => SerializerNumericTypes.Decimal,
                SerializerTypeInformation.BigInteger => SerializerNumericTypes.BigInteger,
                _ => SerializerNumericTypes.None
            };

        /// <summary>
        /// Determine if a complete CLR type information is included
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If a complete CLR type information is included</returns>
        public static bool ContainsClrType(this SerializerTypeInformation type) => (type & SerializerTypeInformation.IsFullType) == SerializerTypeInformation.IsFullType;

        /// <summary>
        /// Get the CLR type
        /// </summary>
        /// <param name="type"><see cref="SerializerTypeInformation"/></param>
        /// <returns><see cref="Type"/></returns>
        public static Type? ToClrType(this SerializerTypeInformation type)
        {
            Type? res = (type & ~SerializerTypeInformation.Nullable) switch
            {
                SerializerTypeInformation.Byte => typeof(byte),
                SerializerTypeInformation.SByte => typeof(sbyte),
                SerializerTypeInformation.UShort => typeof(ushort),
                SerializerTypeInformation.Short => typeof(short),
                SerializerTypeInformation.Half => typeof(Half),
                SerializerTypeInformation.UInt => typeof(uint),
                SerializerTypeInformation.Int => typeof(int),
                SerializerTypeInformation.Float => typeof(float),
                SerializerTypeInformation.ULong => typeof(ulong),
                SerializerTypeInformation.Long => typeof(long),
                SerializerTypeInformation.Double => typeof(double),
                SerializerTypeInformation.Decimal => typeof(decimal),
                SerializerTypeInformation.BigInteger => typeof(BigInteger),
                SerializerTypeInformation.ByteArray => typeof(byte[]),
                SerializerTypeInformation.IntArray => typeof(int[]),
                SerializerTypeInformation.FloatArray => typeof(float[]),
                SerializerTypeInformation.LongArray => typeof(long[]),
                SerializerTypeInformation.DoubleArray => typeof(double[]),
                SerializerTypeInformation.DecimalArray => typeof(decimal[]),
                SerializerTypeInformation.String => typeof(string),
                SerializerTypeInformation.StringArray => typeof(string[]),
                SerializerTypeInformation.Boolean => typeof(bool),
                SerializerTypeInformation.Stream => typeof(Stream),
                SerializerTypeInformation.Type => typeof(Type),
                _ => null
            };
            return res is not null && (type & SerializerTypeInformation.Nullable) == SerializerTypeInformation.Nullable
                ? typeof(Nullable<>).MakeGenericType(res)
                : res;
        }
    }
}
