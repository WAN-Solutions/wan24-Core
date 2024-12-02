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
        /// <param name="options">Options</param>
        public static void Write<T>(this Stream stream, [NotNull] in T list, in ListWritingOptions? options = null) where T : IList
        {
            if (typeof(T).FindGenericType(typeof(IList<>)) is not null)
            {
                WriteList(stream, (dynamic)list, options);
                return;
            }
            stream.Write(list, StreamHelper.GetWriter<object>(SerializedObjectTypes.NullableValue, options));
        }

        /// <summary>
        /// Write an <see cref="IList"/> with nullable values
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        public static void WriteNullableValue<T>(this Stream stream, [NotNull] in T list, in ListWritingOptions? options = null) where T : IList
        {
            if (typeof(T).FindGenericType(typeof(IList<>)) is not null)
            {
                WriteListNullableValue(stream, (dynamic)list, options);
                return;
            }
            stream.WriteNullableValue(list, StreamHelper.GetNullableWriter<object>(SerializedObjectTypes.NullableValue, options));
        }

        /// <summary>
        /// Write an <see cref="IList"/>
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        public static void WriteNullable<T>(this Stream stream, in T? list, in ListWritingOptions? options = null) where T : IList
        {
            if (typeof(T).FindGenericType(typeof(IList<>)) is not null)
            {
                WriteListNullable(stream, (dynamic?)list, options);
                return;
            }
            stream.WriteNullable(list, StreamHelper.GetWriter<object>(SerializedObjectTypes.NullableValue, options));
        }

        /// <summary>
        /// Write an <see cref="IList"/> with nullable values
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        public static void WriteNullableValueNullable<T>(this Stream stream, in T? list, in ListWritingOptions? options = null) where T : IList
        {
            if (typeof(T).FindGenericType(typeof(IList<>)) is not null)
            {
                WriteListNullableValueNullable(stream, (dynamic?)list, options);
                return;
            }
            stream.WriteNullableValueNullable(list, StreamHelper.GetNullableWriter<object>(SerializedObjectTypes.NullableValue, options));
        }

        /// <summary>
        /// Write an <see cref="IList{T}"/> (adapter of <see cref="Write{T}(Stream, in IList{T}, ListWritingOptions)"/> for dynamic generic calls only)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteList<T>(this Stream stream, IList<T> list, in ListWritingOptions? options = null) => stream.Write(list, options);

        /// <summary>
        /// Write an <see cref="IList{T}"/> with nullable values (adapter of <see cref="Write{T}(Stream, in IList{T}, ListWritingOptions)"/> for dynamic generic 
        /// calls only)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteListNullableValue<T>(this Stream stream, IList<T?> list, in ListWritingOptions? options = null)
            => stream.WriteNullableValue(list, options);

        /// <summary>
        /// Write an <see cref="IList{T}"/> (adapter of <see cref="Write{T}(Stream, in IList{T}, ListWritingOptions)"/> for dynamic generic calls only)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteListNullable<T>(this Stream stream, IList<T>? list, in ListWritingOptions? options = null)
            => stream.WriteNullable(list, options);

        /// <summary>
        /// Write an <see cref="IList{T}"/> with nullable values (adapter of <see cref="Write{T}(Stream, in IList{T}, ListWritingOptions)"/> for dynamic generic calls only)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void WriteListNullableValueNullable<T>(this Stream stream, IList<T?>? list, in ListWritingOptions? options = null)
            => stream.WriteNullableValueNullable(list, options);

        /// <summary>
        /// Write an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        public static void Write<T>(this Stream stream, in IList<T> list, ListWritingOptions? options = null)
        {
            Type type = typeof(T);
            stream.Write(type);
            int len = list.Count;
            stream.WriteNumeric(len);
            if (len < 1) return;
            options ??= ListWritingOptions.DefaultListOptions;
            stream.Write(list, StreamHelper.GetWriter<T>(options: options), options);
        }

        /// <summary>
        /// Write an <see cref="IList{T}"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        public static void WriteNullableValue<T>(this Stream stream, in IList<T?> list, ListWritingOptions? options = null)
        {
            Type type = typeof(T);
            stream.Write(type);
            int len = list.Count;
            stream.WriteNumeric(len);
            if (len < 1) return;
            options ??= ListWritingOptions.DefaultListOptions;
            stream.WriteNullableValue(list, StreamHelper.GetNullableWriter<T>(options: options), options);
        }

        /// <summary>
        /// Write an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        public static void WriteNullable<T>(this Stream stream, in IList<T>? list, ListWritingOptions? options = null)
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
            options ??= ListWritingOptions.DefaultListOptions;
            stream.Write(list, StreamHelper.GetWriter<T>(options: options), options);
        }

        /// <summary>
        /// Write an <see cref="IList{T}"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        public static void WriteNullableValueNullable<T>(this Stream stream, in IList<T?>? list, ListWritingOptions? options = null)
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
            options ??= ListWritingOptions.DefaultListOptions;
            stream.WriteNullableValue(list, StreamHelper.GetNullableWriter<T>(options: options), options);
        }

        /// <summary>
        /// Write an <see cref="IList"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="writer">Writer</param>
        /// <param name="options">Options</param>
        public static void Write<T>(this Stream stream, [NotNull] in T list, in Action<Stream, object> writer, ListWritingOptions? options = null)
            where T : IList
        {
            options ??= ListWritingOptions.DefaultListOptions;
            if (options.IncludeItemType) stream.Write((byte)NumericTypes.None);
            int len = list.Count;
            if (options.IncludeItemCount) stream.WriteNumeric(len);
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
        /// <param name="options">Options</param>
        public static void WriteNullableValue<T>(this Stream stream, [NotNull] in T list, in Action<Stream, object?> writer, ListWritingOptions? options = null)
            where T : IList
        {
            options ??= new();
            if (options.IncludeItemType) stream.Write((byte)NumericTypes.None);
            int len = list.Count;
            if (options.IncludeItemCount) stream.WriteNumeric(len);
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
        /// <param name="options">Options</param>

        public static void WriteNullable<T>(this Stream stream, in T? list, in Action<Stream, object> writer, ListWritingOptions? options = null)
             where T : IList
        {
            options ??= ListWritingOptions.DefaultListOptions;
            if (options.IncludeItemCount) stream.Write((byte)NumericTypes.None);
            if (list is null)
            {
                stream.Write((byte)NumericTypes.None);
                return;
            }
            int len = list.Count;
            if (options.IncludeItemCount) stream.WriteNumeric(len);
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
        /// <param name="options">Options</param>
        public static void WriteNullableValueNullable<T>(this Stream stream, in T? list, in Action<Stream, object?> writer, ListWritingOptions? options = null)
            where T : IList
        {
            options ??= ListWritingOptions.DefaultListOptions;
            if (options.IncludeItemType) stream.Write((byte)NumericTypes.None);
            if (list is null)
            {
                stream.Write((byte)NumericTypes.None);
                return;
            }
            int len = list.Count;
            if (options.IncludeItemCount) stream.WriteNumeric(len);
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
        /// <param name="options">Options</param>
        public static void Write<T>(this Stream stream, in IList<T> list, in Action<Stream, T> writer, ListWritingOptions? options = null)
        {
            options ??= ListWritingOptions.DefaultListOptions;
            if (options.IncludeItemType) stream.Write(typeof(T));
            int len = list.Count;
            if (options.IncludeItemCount) stream.WriteNumeric(len);
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
        /// <param name="options">Options</param>
        public static void WriteNullableValue<T>(this Stream stream, in IList<T?> list, in Action<Stream, T?> writer, ListWritingOptions? options = null)
        {
            options ??= ListWritingOptions.DefaultListOptions;
            if (options.IncludeItemType) stream.Write(typeof(T));
            int len = list.Count;
            if (options.IncludeItemCount) stream.WriteNumeric(len);
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
        /// <param name="options">Options</param>
        public static void WriteNullable<T>(this Stream stream, in IList<T>? list, in Action<Stream, T> writer, ListWritingOptions? options = null)
        {
            options ??= ListWritingOptions.DefaultListOptions;
            if (options.IncludeItemType) stream.Write(typeof(T));
            if (list is null)
            {
                stream.Write((byte)NumericTypes.None);
                return;
            }
            int len = list.Count;
            if (options.IncludeItemCount) stream.WriteNumeric(len);
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
        /// <param name="options">Options</param>
        public static void WriteNullableValueNullable<T>(this Stream stream, in IList<T?>? list, in Action<Stream, T?> writer, ListWritingOptions? options = null)
        {
            options ??= ListWritingOptions.DefaultListOptions;
            if (options.IncludeItemType) stream.Write(typeof(T));
            if (list is null)
            {
                stream.Write((byte)NumericTypes.None);
                return;
            }
            int len = list.Count;
            if (options.IncludeItemCount) stream.WriteNumeric(len);
            if (len < 1) return;
            for (int i = 0; i < len; writer(stream, list[i]), i++) ;
        }
    }
}
