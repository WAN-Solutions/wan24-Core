using System.Collections;

namespace wan24.Core
{
    // List
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Write a list
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        public static void Write<T>(this Stream stream, in T list) where T : IList
        {
            if (
                TypeInfoExt.From(list.GetType())
                    .GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && typeof(IList<>).IsAssignableFrom(i.GetGenericTypeDefinition())) is Type genericType
                    )
            {
                typeof(StreamExtensions)
                    .GetMethodsCached()
                    .Where(
                        m => m.Name == nameof(Write) &&
                            m.IsGenericMethod &&
                            m.GenericArgumentCount == 1 &&
                            m.ParameterCount == 2 &&
                            m[1].ParameterType.IsGenericType &&
                            m[1].ParameterType.GetGenericTypeDefinition() == typeof(IList<>)
                        )
                    .First()
                    .MakeGenericMethod(TypeInfoExt.From(genericType).FirstGenericArgument ?? throw new InvalidProgramException())
                    .Invoker!(null, [stream, list]);
                return;
            }
            stream.Write((byte)NumericTypes.None);
            int len = list.Count;
            stream.WriteNumeric(len);
            if (len == 0) return;
            object item;
            for (int i = 0; i < len; i++)
            {
                item = list[i] ?? throw new ArgumentException($"List contains NULL item at index #{i}", nameof(list));
                //TODO Write any object
            }
        }
        /// <summary>
        /// Write a list
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        public static void Write<T>(this Stream stream, in IList<T> list)
        {
            stream.Write(typeof(T));
            int len = list.Count;
            stream.WriteNumeric(len);
            if (len == 0) return;
            T item;
            if (!typeof(T).CanConstruct())
            {
                for (int i = 0; i < len; i++)
                {
                    item = list[i] ?? throw new ArgumentException($"List contains NULL item at index #{i}", nameof(list));
                    //TODO Write any object
                }
                return;
            }
            bool isSealedType = typeof(T).IsSealed;
            SerializedObjectTypes objType = typeof(T).GetSerializedType();
            switch (objType)
            {
                case SerializedObjectTypes.Numeric:
                    for (int i = 0; i < len; i++)
                    {
                        item = list[i] ?? throw new ArgumentException($"List contains NULL item at index #{i}", nameof(list));
                        //TODO Write numeric value
                    }
                    break;
                case SerializedObjectTypes.String:
                    for (int i = 0; i < len; i++)
                    {
                        item = list[i] ?? throw new ArgumentException($"List contains NULL item at index #{i}", nameof(list));
                        stream.Write(item.CastType<string>());
                    }
                    break;
                case SerializedObjectTypes.Boolean:
                    for (int i = 0; i < len; i++)
                    {
                        item = list[i] ?? throw new ArgumentException($"List contains NULL item at index #{i}", nameof(list));
                        stream.Write(item.CastType<bool>());
                    }
                    break;
                case SerializedObjectTypes.Type:
                    for (int i = 0; i < len; i++)
                    {
                        item = list[i] ?? throw new ArgumentException($"List contains NULL item at index #{i}", nameof(list));
                        stream.Write(item.CastType<Type>());
                    }
                    break;
                case SerializedObjectTypes.Array:
                    for (int i = 0; i < len; i++)
                    {
                        item = list[i] ?? throw new ArgumentException($"List contains NULL item at index #{i}", nameof(list));
                        stream.Write((IList)item);
                    }
                    break;
                case SerializedObjectTypes.Dictionary:
                    for (int i = 0; i < len; i++)
                    {
                        item = list[i] ?? throw new ArgumentException($"List contains NULL item at index #{i}", nameof(list));
                        //TODO Write dictionary value
                    }
                    break;
                case SerializedObjectTypes.Stream:
                    for (int i = 0; i < len; i++)
                    {
                        item = list[i] ?? throw new ArgumentException($"List contains NULL item at index #{i}", nameof(list));
                        stream.Write(item.CastType<Stream>());
                    }
                    break;
                case SerializedObjectTypes.Serializable:
                    for (int i = 0; i < len; i++)
                    {
                        item = list[i] ?? throw new ArgumentException($"List contains NULL item at index #{i}", nameof(list));
                        if (!isSealedType) stream.Write(item.GetType());
                        ((ISerializeStream)item).SerializeTo(stream);
                    }
                    break;
                case SerializedObjectTypes.Enumerable:
                    for (int i = 0; i < len; i++)
                    {
                        item = list[i] ?? throw new ArgumentException($"List contains NULL item at index #{i}", nameof(list));
                        //TODO Write enumerable value
                    }
                    break;
                case SerializedObjectTypes.Json:
                    for (int i = 0; i < len; i++)
                    {
                        item = list[i] ?? throw new ArgumentException($"List contains NULL item at index #{i}", nameof(list));
                        if (!isSealedType) stream.Write(item.GetType());
                        stream.WriteJson(item);
                    }
                    break;
                default:
                    throw new InvalidProgramException($"Failed to determine valid serialized object type ({objType}) for {typeof(T)}");
            }
        }
    }
}
