using System.Collections;

namespace wan24.Core
{
    // Synchronous list reading
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Read an <see cref="IList"/>
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <returns>List</returns>
        public static List<object> ReadList(this Stream stream, in int version, in ListReadingOptions options)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            List<object> res = options.MaxCount < 1
                ? []
                : new(options.MaxCount);
            stream.ReadList(version, res, options);
            return res;
        }

        /// <summary>
        /// Read an <see cref="IList"/> with nullable values
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <returns>List</returns>
        public static List<object?> ReadListNullableValue(this Stream stream, in int version, in ListReadingOptions options)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            List<object?> res = options.MaxCount < 1
                ? []
                : new(options.MaxCount);
            stream.ReadListNullableValue(version, res, options);
            return res;
        }

        /// <summary>
        /// Read an <see cref="IList"/>
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <returns>List</returns>
        public static List<object>? ReadListNullable(this Stream stream, in int version, in ListReadingOptions options)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            List<object> res = options.MaxCount < 1
                ? []
                : new(options.MaxCount);
            return stream.ReadListNullable(version, res, options) < 0
                ? null
                : res;
        }

        /// <summary>
        /// Read an <see cref="IList"/> with nullable values
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <returns>List</returns>
        public static List<object?>? ReadListNullableValueNullable(this Stream stream, in int version, in ListReadingOptions options)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            List<object?> res = options.MaxCount < 1
                ? []
                : new(options.MaxCount);
            return stream.ReadListNullableValuesNullable(version, res, options) < 0
                ? null
                : res;
        }

        /// <summary>
        /// Read an <see cref="IList"/>
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        /// <returns>Number of red items</returns>
        public static int ReadList(this Stream stream, in int version, in IList list, in ListReadingOptions options)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            if (options.IncludesType) stream.ReadOneByte(version);
            int count = stream.ReadNumeric<int>(version);
            if (count < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (options.MaxCount >= 0 && count > options.MaxCount)
                throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {options.MaxCount}");
            if (count < 1) return 0;
            stream.ReadListItems(version, list, count, options);
            return count;
        }

        /// <summary>
        /// Read an <see cref="IList"/> with nullable values
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        /// <returns>Number of red items</returns>
        public static int ReadListNullableValue(this Stream stream, in int version, in IList list, in ListReadingOptions options)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            if (options.IncludesType) stream.ReadOneByte(version);
            int count = stream.ReadNumeric<int>(version);
            if (count < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (options.MaxCount >= 0 && count > options.MaxCount)
                throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {options.MaxCount}");
            if (count < 1) return 0;
            stream.ReadListItemsNullableValues(version, list, count, options);
            return count;
        }

        /// <summary>
        /// Read an <see cref="IList"/>
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        /// <returns>Number of red items or <c>-1</c>, if the list was <see langword="null"/></returns>
        public static int ReadListNullable(this Stream stream, in int version, in IList list, in ListReadingOptions options)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            if (options.IncludesType) stream.ReadOneByte(version);
            int? count = stream.ReadNumericNullable<int>(version);
            if (!count.HasValue) return -1;
            if (count.Value < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (options.MaxCount >= 0 && count.Value > options.MaxCount)
                throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {options.MaxCount}");
            if (count.Value < 1) return 0;
            stream.ReadListItems(version, list, count.Value, options);
            return count.Value;
        }

        /// <summary>
        /// Read an <see cref="IList"/> with nullable values
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        /// <returns>Number of red items or <c>-1</c>, if the list was <see langword="null"/></returns>
        public static int ReadListNullableValuesNullable(this Stream stream, in int version, in IList list, in ListReadingOptions options)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            if (options.IncludesType) stream.ReadOneByte(version);
            int? count = stream.ReadNumericNullable<int>(version);
            if (!count.HasValue) return -1;
            if (count.Value < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (options.MaxCount >= 0 && count.Value > options.MaxCount)
                throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {options.MaxCount}");
            if (count.Value < 1) return 0;
            stream.ReadListItemsNullableValues(version, list, count.Value, options);
            return count.Value;
        }

        /// <summary>
        /// Read an <see cref="IList"/>
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="count">Item count</param>
        /// <param name="options">Options</param>
        public static void ReadListItems(this Stream stream, in int version, in IList list, in int count, ListReadingOptions options)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 0, nameof(count));
            if (count < 1) return;
            stream.ReadListItems(list, StreamHelper.GetReader<object>(version, SerializedObjectTypes.NullableValue, options), count);
        }

        /// <summary>
        /// Read an <see cref="IList"/> with nullable values
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="count">Item count</param>
        /// <param name="options">Options</param>
        public static void ReadListItemsNullableValues(this Stream stream, in int version, in IList list, in int count, ListReadingOptions options)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 0, nameof(count));
            if (count < 1) return;
            stream.ReadListItemsNullableValues(list, StreamHelper.GetNullableReader<object>(version, SerializedObjectTypes.NullableValue, options), count);
        }

        /// <summary>
        /// Read an <see cref="IList"/>
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="reader">Reader</param>
        /// <param name="count">Item count</param>
        public static void ReadListItems(this Stream stream, in IList list, in Func<Stream, object> reader, in int count)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 0, nameof(count));
            if (count < 1) return;
            for (int i = 0; i < count; list.Add(reader(stream)), i++) ;
        }

        /// <summary>
        /// Read an <see cref="IList"/> with nullable values
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="reader">Reader</param>
        /// <param name="count">Item count</param>
        public static void ReadListItemsNullableValues(this Stream stream, in IList list, in Func<Stream, object?> reader, in int count)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 0, nameof(count));
            if (count < 1) return;
            for (int i = 0; i < count; list.Add(reader(stream)), i++) ;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <param name="dummy">Dummy item type value for dynamic calls only</param>
        /// <returns>List</returns>
        public static List<T> ReadGenericList<T>(this Stream stream, in int version, in ListReadingOptions options, in T dummy = default!)
        {
            if (options.MaxCount < 0 && !AllowDangerousBinarySerialization) throw new InvalidOperationException("Limitation required");
            List<T> res = options.MaxCount < 1
                ? []
                : new(options.MaxCount);
            stream.ReadGenericList(version, res, options);
            return res;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <param name="dummy">Dummy item type value for dynamic calls only</param>
        /// <returns>List</returns>
        public static List<T?> ReadGenericListNullableValues<T>(this Stream stream, in int version, in ListReadingOptions options, in T dummy = default!
            )
        {
            if (options.MaxCount < 0 && !AllowDangerousBinarySerialization) throw new InvalidOperationException("Limitation required");
            List<T?> res = options.MaxCount < 1
                ? []
                : new(options.MaxCount);
            stream.ReadGenericListNullableValues(version, res, options);
            return res;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <param name="dummy">Dummy item type value for dynamic calls only</param>
        /// <returns>List</returns>
        public static List<T>? ReadGenericListNullable<T>(this Stream stream, in int version, in ListReadingOptions options, in T dummy = default!
            )
        {
            if (options.MaxCount < 0 && !AllowDangerousBinarySerialization) throw new InvalidOperationException("Limitation required");
            List<T> res = options.MaxCount < 1
                ? []
                : new(options.MaxCount);
            return stream.ReadGenericListNullable(version, res, options) < 0
                ? null
                : res;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <param name="dummy">Dummy item type value for dynamic calls only</param>
        /// <returns>List</returns>
        public static List<T?>? ReadGenericListNullableValuesNullable<T>(this Stream stream, in int version, in ListReadingOptions options, in T dummy = default!)
        {
            if (options.MaxCount < 0 && !AllowDangerousBinarySerialization) throw new InvalidOperationException("Limitation required");
            List<T?> res = options.MaxCount < 1
                ? []
                : new(options.MaxCount);
            return stream.ReadGenericListNullableValuesNullable(version, res, options) < 0
                ? null
                : res;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        /// <returns>Number of red items</returns>
        public static int ReadGenericList<T>(this Stream stream, in int version, in IList<T> list, in ListReadingOptions options)
        {
            Type type = typeof(T),
                serializedType = options.IncludesType
                    ? stream.ReadType(version, options.TypeItemOptions)
                    : type;
            if (type != serializedType && !type.IsAssignableFrom(serializedType))
                throw new InvalidDataException($"Serialized item type {serializedType} is incompatible with requested type {type}");
            int count = stream.ReadNumeric<int>(version);
            if (count < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (options.MaxCount >= 0 && count > options.MaxCount)
                throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {options.MaxCount}");
            if (count < 1) return 0;
            stream.ReadGenericListItems(version, list, count, options);
            return count;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        /// <returns>Number of red items</returns>
        public static int ReadGenericListNullableValues<T>(this Stream stream, in int version, in IList<T?> list, in ListReadingOptions options)
        {
            Type type = typeof(T),
                serializedType = options.IncludesType
                    ? stream.ReadType(version, options.TypeItemOptions)
                    : type;
            if (type != serializedType && !type.IsAssignableFrom(serializedType))
                throw new InvalidDataException($"Serialized item type {serializedType} is incompatible with requested type {type}");
            int count = stream.ReadNumeric<int>(version);
            if (count < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (options.MaxCount >= 0 && count > options.MaxCount)
                throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {options.MaxCount}");
            if (count < 1) return 0;
            stream.ReadGenericListItemsNullableValues(version, list, count, options);
            return count;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        /// <returns>Number of red items or <c>-1</c>, if the list was <see langword="null"/></returns>
        public static int ReadGenericListNullable<T>(this Stream stream, in int version, in IList<T> list, in ListReadingOptions options)
        {
            Type type = typeof(T),
                serializedType = options.IncludesType
                    ? stream.ReadType(version, options.TypeItemOptions)
                    : type;
            if (type != serializedType && !type.IsAssignableFrom(serializedType))
                throw new InvalidDataException($"Serialized item type {serializedType} is incompatible with requested type {type}");
            int? count = stream.ReadNumericNullable<int>(version);
            if (!count.HasValue) return -1;
            if (count.Value < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (options.MaxCount >= 0 && count.Value > options.MaxCount)
                throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {options.MaxCount}");
            if (count.Value < 1) return 0;
            stream.ReadGenericListItems(version, list, count.Value, options);
            return count.Value;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="options">Options</param>
        /// <returns>Number of red items or <c>-1</c>, if the list was <see langword="null"/></returns>
        public static int ReadGenericListNullableValuesNullable<T>(this Stream stream, in int version, in IList<T?> list, in ListReadingOptions options)
        {
            Type type = typeof(T),
                serializedType = options.IncludesType
                    ? stream.ReadType(version, options.TypeItemOptions)
                    : type;
            if (type != serializedType && !type.IsAssignableFrom(serializedType))
                throw new InvalidDataException($"Serialized item type {serializedType} is incompatible with requested type {type}");
            int? count = stream.ReadNumericNullable<int>(version);
            if (!count.HasValue) return -1;
            if (count.Value < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (options.MaxCount >= 0 && count.Value > options.MaxCount)
                throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {options.MaxCount}");
            if (count.Value < 1) return 0;
            stream.ReadGenericListItemsNullableValues(version, list, count.Value, options);
            return count.Value;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="count">Item count</param>
        /// <param name="options">Options</param>
        public static void ReadGenericListItems<T>(this Stream stream, in int version, in IList<T> list, in int count, ListReadingOptions options)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 0, nameof(count));
            if (count < 1) return;
            Type type = typeof(T);
            stream.ReadGenericListItems(list, StreamHelper.GetReader<T>(version, options: options), count);
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="count">Item count</param>
        /// <param name="options">Options</param>
        public static void ReadGenericListItemsNullableValues<T>(this Stream stream, in int version, in IList<T?> list, in int count, ListReadingOptions options)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 0, nameof(count));
            if (count < 1) return;
            Type type = typeof(T);
            stream.ReadGenericListItemsNullableValues(list, StreamHelper.GetNullableReader<T>(version, options: options), count);
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="reader">Reader</param>
        /// <param name="count">Item count</param>
        public static void ReadGenericListItems<T>(this Stream stream, in IList<T> list, in Func<Stream, T> reader, in int count)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 0, nameof(count));
            if (count < 1) return;
            for (int i = 0; i < count; list.Add(reader(stream)), i++) ;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="list">List</param>
        /// <param name="reader">Reader</param>
        /// <param name="count">Item count</param>
        public static void ReadGenericListItemsNullableValues<T>(this Stream stream, in IList<T?> list, in Func<Stream, T?> reader, in int count)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 0, nameof(count));
            if (count < 1) return;
            for (int i = 0; i < count; list.Add(reader(stream)), i++) ;
        }
    }
}
