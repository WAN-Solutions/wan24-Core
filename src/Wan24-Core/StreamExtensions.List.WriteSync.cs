using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Synchronous list writing
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
        public static void Write<T>(this Stream stream, [NotNull] in T list, in bool? useInterfaces = null, in bool? useItemInterfaces = null) where T : IList
        {
            if (typeof(T).FindGenericType(typeof(IList<>)) is not null)
            {
                WriteList(stream, (dynamic)list, useInterfaces, useItemInterfaces);
                return;
            }
            stream.Write(list, StreamHelper.GetWriter<object>(SerializedObjectTypes.NullableValue, useInterfaces, useItemInterfaces));
        }

        /// <summary>
        /// Write an <see cref="IList"/> with nullable values
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="useInterfaces">If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="useItemInterfaces">If to use supported interfaces for items (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        public static void WriteNullableValue<T>(this Stream stream, [NotNull] in T list, in bool? useInterfaces = null, in bool? useItemInterfaces = null) where T : IList
        {
            if (typeof(T).FindGenericType(typeof(IList<>)) is not null)
            {
                WriteListNullableValue(stream, (dynamic)list, useInterfaces, useItemInterfaces);
                return;
            }
            stream.WriteNullableValue(list, StreamHelper.GetNullableWriter<object>(SerializedObjectTypes.NullableValue, useInterfaces, useItemInterfaces));
        }

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
        public static void WriteNullable<T>(this Stream stream, in T? list, in bool? useInterfaces = null, in bool? useItemInterfaces = null) where T : IList
        {
            if (typeof(T).FindGenericType(typeof(IList<>)) is not null)
            {
                WriteListNullable(stream, (dynamic?)list, useInterfaces, useItemInterfaces);
                return;
            }
            stream.WriteNullable(list, StreamHelper.GetWriter<object>(SerializedObjectTypes.NullableValue, useInterfaces, useItemInterfaces));
        }

        /// <summary>
        /// Write an <see cref="IList"/> with nullable values
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="useInterfaces">If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="useItemInterfaces">If to use supported interfaces for items (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        public static void WriteNullableValueNullable<T>(this Stream stream, in T? list, in bool? useInterfaces = null, in bool? useItemInterfaces = null) where T : IList
        {
            if (typeof(T).FindGenericType(typeof(IList<>)) is not null)
            {
                WriteListNullableValueNullable(stream, (dynamic?)list, useInterfaces, useItemInterfaces);
                return;
            }
            stream.WriteNullableValueNullable(list, StreamHelper.GetNullableWriter<object>(SerializedObjectTypes.NullableValue, useInterfaces, useItemInterfaces));
        }

        /// <summary>
        /// Write an <see cref="IList{T}"/> (adapter of <see cref="Write{T}(Stream, in IList{T}, in bool?, in bool?)"/> for dynamic generic calls only)
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
        /// Write an <see cref="IList{T}"/> with nullable values (adapter of <see cref="Write{T}(Stream, in IList{T}, in bool?, in bool?)"/> for dynamic generic calls only)
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
        public static void WriteListNullableValue<T>(this Stream stream, IList<T?> list, in bool? useInterfaces = null, in bool? useItemInterfaces = null)
            => stream.WriteNullableValue(list, useInterfaces, useItemInterfaces);

        /// <summary>
        /// Write an <see cref="IList{T}"/> (adapter of <see cref="Write{T}(Stream, in IList{T}, in bool?, in bool?)"/> for dynamic generic calls only)
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
        public static void WriteListNullable<T>(this Stream stream, IList<T>? list, in bool? useInterfaces = null, in bool? useItemInterfaces = null)
            => stream.WriteNullable(list, useInterfaces, useItemInterfaces);

        /// <summary>
        /// Write an <see cref="IList{T}"/> with nullable values (adapter of <see cref="Write{T}(Stream, in IList{T}, in bool?, in bool?)"/> for dynamic generic calls only)
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
        public static void WriteListNullableValueNullable<T>(this Stream stream, IList<T?>? list, in bool? useInterfaces = null, in bool? useItemInterfaces = null)
            => stream.WriteNullableValueNullable(list, useInterfaces, useItemInterfaces);

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
        /// Write an <see cref="IList{T}"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="useInterfaces">If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="useItemInterfaces">If to use supported interfaces for items (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        public static void WriteNullableValue<T>(this Stream stream, in IList<T?> list, in bool? useInterfaces = null, in bool? useItemInterfaces = null)
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
            stream.WriteNullableValue(
                list,
                StreamHelper.GetNullableWriter<T>(objType, useInterfaces, useItemInterfaces, buffer),
                includeItemType: false,
                includeCount: false
                );
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
        public static void WriteNullable<T>(this Stream stream, in IList<T>? list, in bool? useInterfaces = null, in bool? useItemInterfaces = null)
        {
            Type type = typeof(T);
            stream.Write(type);
            if (list is null)
            {
                stream.Write((byte)NumericTypes.None);
                return;
            }
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
        /// Write an <see cref="IList{T}"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="useInterfaces">If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        /// <param name="useItemInterfaces">If to use supported interfaces for items (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, 
        /// <see cref="IDictionary{TKey, TValue}"/>, <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)</param>
        public static void WriteNullableValueNullable<T>(this Stream stream, in IList<T?>? list, in bool? useInterfaces = null, in bool? useItemInterfaces = null)
        {
            Type type = typeof(T);
            stream.Write(type);
            if (list is null)
            {
                stream.Write((byte)NumericTypes.None);
                return;
            }
            int len = list.Count;
            stream.WriteNumeric(len);
            if (len < 1) return;
            SerializedObjectTypes objType = type.GetSerializedType(useInterfaces ?? type.IsInterface);
            using RentedMemory<byte>? buffer = objType == SerializedObjectTypes.SerializeBinary
                ? new(type.GetMaxStructureSize() ?? Settings.BufferSize, clean: false)
                : null;
            stream.WriteNullableValue(
                list,
                StreamHelper.GetNullableWriter<T>(objType, useInterfaces, useItemInterfaces, buffer),
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
        public static void Write<T>(this Stream stream, [NotNull] in T list, in Action<Stream, object> writer, in bool includeItemType = true, in bool includeCount = true)
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
        /// Write an <see cref="IList"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="writer">Writer</param>
        /// <param name="includeItemType">If to include the item type</param>
        /// <param name="includeCount">If to include the count</param>
        public static void WriteNullableValue<T>(
            this Stream stream,
            [NotNull] in T list,
            in Action<Stream, object?> writer,
            in bool includeItemType = true,
            in bool includeCount = true
            )
            where T : IList
        {
            if (includeItemType) stream.Write((byte)NumericTypes.None);
            int len = list.Count;
            if (includeCount) stream.WriteNumeric(len);
            if (len < 1) return;
            for (int i = 0; i < len; writer(stream, list[i]), i++) ;
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
        public static void WriteNullable<T>(this Stream stream, in T? list, in Action<Stream, object> writer, in bool includeItemType = true, in bool includeCount = true)
            where T : IList
        {
            if (includeItemType) stream.Write((byte)NumericTypes.None);
            if (list is null)
            {
                stream.Write((byte)NumericTypes.None);
                return;
            }
            int len = list.Count;
            if (includeCount) stream.WriteNumeric(len);
            if (len < 1) return;
            string listArgName = nameof(list);
            for (int i = 0; i < len; writer(stream, EnsureNonNullValue(list[i], i, listArgName)), i++) ;
        }

        /// <summary>
        /// Write an <see cref="IList"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="writer">Writer</param>
        /// <param name="includeItemType">If to include the item type</param>
        /// <param name="includeCount">If to include the count</param>
        public static void WriteNullableValueNullable<T>(
            this Stream stream,
            in T? list,
            in Action<Stream, object?> writer,
            in bool includeItemType = true,
            in bool includeCount = true
            )
            where T : IList
        {
            if (includeItemType) stream.Write((byte)NumericTypes.None);
            if (list is null)
            {
                stream.Write((byte)NumericTypes.None);
                return;
            }
            int len = list.Count;
            if (includeCount) stream.WriteNumeric(len);
            if (len < 1) return;
            for (int i = 0; i < len; writer(stream, list[i]), i++) ;
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
        public static void Write<T>(
            this Stream stream,
            in IList<T> list,
            in Action<Stream, T> writer,
            in bool includeItemType = true,
            in bool includeCount = true
            )
        {
            if (includeItemType) stream.Write(typeof(T));
            int len = list.Count;
            if (includeCount) stream.WriteNumeric(len);
            if (len < 1) return;
            string listArgName = nameof(list);
            for (int i = 0; i < len; writer(stream, EnsureNonNullValue(list[i], i, listArgName)), i++) ;
        }

        /// <summary>
        /// Write an <see cref="IList{T}"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="writer">Writer</param>
        /// <param name="includeItemType">If to include the item type</param>
        /// <param name="includeCount">If to include the count</param>
        public static void WriteNullableValue<T>(
            this Stream stream,
            in IList<T?> list,
            in Action<Stream, T?> writer,
            in bool includeItemType = true,
            in bool includeCount = true
            )
        {
            if (includeItemType) stream.Write(typeof(T));
            int len = list.Count;
            if (includeCount) stream.WriteNumeric(len);
            if (len < 1) return;
            for (int i = 0; i < len; writer(stream, list[i]), i++) ;
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
        public static void WriteNullable<T>(
            this Stream stream,
            in IList<T>? list,
            in Action<Stream, T> writer,
            in bool includeItemType = true,
            in bool includeCount = true
            )
        {
            if (includeItemType) stream.Write(typeof(T));
            if (list is null)
            {
                stream.Write((byte)NumericTypes.None);
                return;
            }
            int len = list.Count;
            if (includeCount) stream.WriteNumeric(len);
            if (len < 1) return;
            string listArgName = nameof(list);
            for (int i = 0; i < len; writer(stream, EnsureNonNullValue(list[i], i, listArgName)), i++) ;
        }

        /// <summary>
        /// Write an <see cref="IList{T}"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="writer">Writer</param>
        /// <param name="includeItemType">If to include the item type</param>
        /// <param name="includeCount">If to include the count</param>
        public static void WriteNullableValueNullable<T>(
            this Stream stream,
            in IList<T?>? list,
            in Action<Stream, T?> writer,
            in bool includeItemType = true,
            in bool includeCount = true
            )
        {
            if (includeItemType) stream.Write(typeof(T));
            if (list is null)
            {
                stream.Write((byte)NumericTypes.None);
                return;
            }
            int len = list.Count;
            if (includeCount) stream.WriteNumeric(len);
            if (len < 1) return;
            for (int i = 0; i < len; writer(stream, list[i]), i++) ;
        }
    }
}
