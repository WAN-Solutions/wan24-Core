using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // List
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Write an <see cref="IList"/>
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="useInterfaces">If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="useItemInterfaces">If to use supported interfaces for items (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        public static void Write<T>(this Stream stream, in T list, in bool? useInterfaces = null, in bool? useItemInterfaces = null) where T : IList
        {
            if (typeof(T).FindGenericType(typeof(IList<>)) is not null)
            {
                WriteList(stream, (dynamic)list, useInterfaces, useItemInterfaces);
                return;
            }
            stream.Write(list, static (stream, item) => { });//TODO Write any object
        }

        /// <summary>
        /// Write an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="useInterfaces">If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="useItemInterfaces">If to use supported interfaces for items (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteList<T>(this Stream stream, IList<T> list, in bool? useInterfaces = null, in bool? useItemInterfaces = null)
            => stream.Write(list, useInterfaces, useItemInterfaces);

        /// <summary>
        /// Write an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="useInterfaces">If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="useItemInterfaces">If to use supported interfaces for items (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        public static void Write<T>(this Stream stream, in IList<T> list, in bool? useInterfaces = null, in bool? useItemInterfaces = null)
        {
            Type type = typeof(T);
            stream.Write(type);
            int len = list.Count;
            stream.WriteNumeric(len);
            if (len < 1) return;
            SerializedObjectTypes objType = type.GetSerializedType(useInterfaces ?? type.IsInterface);
            using RentedMemory<byte>? buffer = objType == SerializedObjectTypes.SerializeBinary
                ? new(type.GetMaxStructureSize() ?? Settings.BufferSize, clean: false)
                : null;
            stream.Write(
                list,
                StreamHelper.GetWriter<T>(objType, useInterfaces, useItemInterfaces, buffer),
                includeItemType: false,
                includeCount: false
                );
        }

        /// <summary>
        /// Write an <see cref="IList"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="writer">Writer</param>
        /// <param name="includeItemType">If to include the item type</param>
        /// <param name="includeCount">If to include the count</param>
        public static void Write<T>(this Stream stream, in T list, in Action<Stream, object> writer, in bool includeItemType = true, in bool includeCount = true)
            where T : IList
        {
            if (includeItemType) stream.Write((byte)NumericTypes.None);
            int len = list.Count;
            if (includeCount) stream.WriteNumeric(len);
            if (len < 1) return;
            string listArgName = nameof(list);
            for (int i = 0; i < len; writer(stream, EnsureNonNullValue(list[i], i, listArgName)), i++) ;
        }

        /// <summary>
        /// Write an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="writer">Writer</param>
        /// <param name="includeItemType">If to include the item type</param>
        /// <param name="includeCount">If to include the count</param>
        public static void Write<T>(this Stream stream, in IList<T> list, in Action<Stream, T> writer, in bool includeItemType = true, in bool includeCount = true)
        {
            if (includeItemType) stream.Write(typeof(T));
            int len = list.Count;
            if (includeCount) stream.WriteNumeric(len);
            if (len < 1) return;
            string listArgName = nameof(list);
            for (int i = 0; i < len; writer(stream, EnsureNonNullValue(list[i], i, listArgName)), i++) ;
        }

        /// <summary>
        /// Ensure having a non-<see langword="null"/> value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="index">Index</param>
        /// <param name="arg">Argument name</param>
        /// <returns>Non-<see langword="null"/> value</returns>
        /// <exception cref="ArgumentException">Unexpected <see langword="null"/> value</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        private static T EnsureNonNullValue<T>(in T? value, in object index, in string arg)
            => value ?? throw new ArgumentException($"Unexpected NULL value at index {index.ToString()?.ToQuotedLiteral()}", arg);
    }
}
