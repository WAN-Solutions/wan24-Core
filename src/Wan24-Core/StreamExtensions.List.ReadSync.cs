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
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit)</param>
        /// <returns>List</returns>
        public static List<object> ReadList(this Stream stream, in int version, in int maxCount)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            List<object> res = maxCount < 1
                ? []
                : new(maxCount);
            stream.ReadList(version, res, maxCount);
            return res;
        }

        /// <summary>
        /// Read an <see cref="IList"/> with nullable values
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit)</param>
        /// <returns>List</returns>
        public static List<object?> ReadListNullableValue(this Stream stream, in int version, in int maxCount)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            List<object?> res = maxCount < 1
                ? []
                : new(maxCount);
            stream.ReadListNullableValue(version, res, maxCount);
            return res;
        }

        /// <summary>
        /// Read an <see cref="IList"/>
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit)</param>
        /// <returns>List</returns>
        public static List<object>? ReadListNullable(this Stream stream, in int version, in int maxCount)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            List<object> res = maxCount < 1
                ? []
                : new(maxCount);
            return stream.ReadListNullable(version, res, maxCount) < 0
                ? null
                : res;
        }

        /// <summary>
        /// Read an <see cref="IList"/> with nullable values
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit)</param>
        /// <returns>List</returns>
        public static List<object?>? ReadListNullableValueNullable(this Stream stream, in int version, in int maxCount)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            List<object?> res = maxCount < 1
                ? []
                : new(maxCount);
            return stream.ReadListNullableValuesNullable(version, res, maxCount) < 0
                ? null
                : res;
        }

        /// <summary>
        /// Read an <see cref="IList"/>
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit; excluding the given lists current item count)</param>
        /// <param name="includesType">If the item type is included</param>
        /// <returns>Number of red items</returns>
        public static int ReadList(this Stream stream, in int version, in IList list, in int maxCount, in bool includesType = true)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            if (includesType) stream.ReadOneByte(version);
            int count = stream.ReadNumeric<int>(version);
            if (count < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (maxCount >= 0 && count > maxCount) throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {maxCount}");
            if (count < 1) return 0;
            stream.ReadListItems(version, list, count);
            return count;
        }

        /// <summary>
        /// Read an <see cref="IList"/> with nullable values
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit; excluding the given lists current item count)</param>
        /// <param name="includesType">If the item type is included</param>
        /// <returns>Number of red items</returns>
        public static int ReadListNullableValue(this Stream stream, in int version, in IList list, in int maxCount, in bool includesType = true)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            if (includesType) stream.ReadOneByte(version);
            int count = stream.ReadNumeric<int>(version);
            if (count < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (maxCount >= 0 && count > maxCount) throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {maxCount}");
            if (count < 1) return 0;
            stream.ReadListItemsNullableValues(version, list, count);
            return count;
        }

        /// <summary>
        /// Read an <see cref="IList"/>
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit; excluding the given lists current item count)</param>
        /// <param name="includesType">If the item type is included</param>
        /// <returns>Number of red items or <c>-1</c>, if the list was <see langword="null"/></returns>
        public static int ReadListNullable(this Stream stream, in int version, in IList list, in int maxCount, in bool includesType = true)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            if (includesType) stream.ReadOneByte(version);
            int? count = stream.ReadNumericNullable<int>(version);
            if (!count.HasValue) return -1;
            if (count.Value < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (maxCount >= 0 && count.Value > maxCount) throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {maxCount}");
            if (count.Value < 1) return 0;
            stream.ReadListItems(version, list, count.Value);
            return count.Value;
        }

        /// <summary>
        /// Read an <see cref="IList"/> with nullable values
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit; excluding the given lists current item count)</param>
        /// <param name="includesType">If the item type is included</param>
        /// <returns>Number of red items or <c>-1</c>, if the list was <see langword="null"/></returns>
        public static int ReadListNullableValuesNullable(this Stream stream, in int version, in IList list, in int maxCount, in bool includesType = true)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            if (includesType) stream.ReadOneByte(version);
            int? count = stream.ReadNumericNullable<int>(version);
            if (!count.HasValue) return -1;
            if (count.Value < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (maxCount >= 0 && count.Value > maxCount) throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {maxCount}");
            if (count.Value < 1) return 0;
            stream.ReadListItemsNullableValues(version, list, count.Value);
            return count.Value;
        }

        /// <summary>
        /// Read an <see cref="IList"/>
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="count">Item count</param>
        public static void ReadListItems(this Stream stream, in int version, in IList list, in int count)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 0, nameof(count));
            if (count < 1) return;
            //TODO Read the items
        }

        /// <summary>
        /// Read an <see cref="IList"/> with nullable values
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="count">Item count</param>
        public static void ReadListItemsNullableValues(this Stream stream, in int version, in IList list, in int count)
        {
            if (!AllowDangerousBinarySerialization) throw new InvalidOperationException("Abstract types are not allowed");
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 0, nameof(count));
            if (count < 1) return;
            //TODO Read the items
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
        /// Read an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit)</param>
        /// <param name="dummy">Dummy item type value for dynamic calls only</param>
        /// <returns>List</returns>
        public static List<T> ReadGenericList<T>(this Stream stream, in int version, in int maxCount, in T dummy = default!)
        {
            if (maxCount < 0 && !AllowDangerousBinarySerialization) throw new InvalidOperationException("Limitation required");
            List<T> res = maxCount < 1
                ? []
                : new(maxCount);
            stream.ReadGenericList(version, res, maxCount);
            return res;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit)</param>
        /// <param name="dummy">Dummy item type value for dynamic calls only</param>
        /// <returns>List</returns>
        public static List<T?> ReadGenericListNullableValues<T>(this Stream stream, in int version, in int maxCount, in T dummy = default!)
        {
            if (maxCount < 0 && !AllowDangerousBinarySerialization) throw new InvalidOperationException("Limitation required");
            List<T?> res = maxCount < 1
                ? []
                : new(maxCount);
            stream.ReadGenericListNullableValues(version, res, maxCount);
            return res;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit)</param>
        /// <param name="dummy">Dummy item type value for dynamic calls only</param>
        /// <returns>List</returns>
        public static List<T>? ReadGenericListNullable<T>(this Stream stream, in int version, in int maxCount, in T dummy = default!)
        {
            if (maxCount < 0 && !AllowDangerousBinarySerialization) throw new InvalidOperationException("Limitation required");
            List<T> res = maxCount < 1
                ? []
                : new(maxCount);
            return stream.ReadGenericListNullable(version, res, maxCount) < 0
                ? null
                : res;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit)</param>
        /// <param name="dummy">Dummy item type value for dynamic calls only</param>
        /// <returns>List</returns>
        public static List<T?>? ReadGenericListNullableValuesNullable<T>(this Stream stream, in int version, in int maxCount, in T dummy = default!)
        {
            if (maxCount < 0 && !AllowDangerousBinarySerialization) throw new InvalidOperationException("Limitation required");
            List<T?> res = maxCount < 1
                ? []
                : new(maxCount);
            return stream.ReadGenericListNullableValuesNullable(version, res, maxCount) < 0
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
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit; excluding the given lists current item count)</param>
        /// <param name="includesType">If the item type is included</param>
        /// <returns>Number of red items</returns>
        public static int ReadGenericList<T>(this Stream stream, in int version, in IList<T> list, in int maxCount, in bool includesType = true)
        {
            Type type = typeof(T),
                serializedType = includesType
                    ? stream.ReadType(version)
                    : type;
            if (type != serializedType && !type.IsAssignableFrom(serializedType))
                throw new InvalidDataException($"Serialized item type {serializedType} is incompatible with requested type {type}");
            int count = stream.ReadNumeric<int>(version);
            if (count < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (maxCount >= 0 && count > maxCount) throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {maxCount}");
            if (count < 1) return 0;
            stream.ReadGenericListItems(version, list, count);
            return count;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/> with nullable values
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit; excluding the given lists current item count)</param>
        /// <param name="includesType">If the item type is included</param>
        /// <returns>Number of red items</returns>
        public static int ReadGenericListNullableValues<T>(this Stream stream, in int version, in IList<T?> list, in int maxCount, in bool includesType = true)
        {
            Type type = typeof(T),
                serializedType = includesType
                    ? stream.ReadType(version)
                    : type;
            if (type != serializedType && !type.IsAssignableFrom(serializedType))
                throw new InvalidDataException($"Serialized item type {serializedType} is incompatible with requested type {type}");
            int count = stream.ReadNumeric<int>(version);
            if (count < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (maxCount >= 0 && count > maxCount) throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {maxCount}");
            if (count < 1) return 0;
            stream.ReadGenericListItemsNullableValues(version, list, count);
            return count;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit; excluding the given lists current item count)</param>
        /// <param name="includesType">If the item type is included</param>
        /// <returns>Number of red items or <c>-1</c>, if the list was <see langword="null"/></returns>
        public static int ReadGenericListNullable<T>(this Stream stream, in int version, in IList<T> list, in int maxCount, in bool includesType = true)
        {
            Type type = typeof(T),
                serializedType = includesType
                    ? stream.ReadType(version)
                    : type;
            if (type != serializedType && !type.IsAssignableFrom(serializedType))
                throw new InvalidDataException($"Serialized item type {serializedType} is incompatible with requested type {type}");
            int? count = stream.ReadNumericNullable<int>(version);
            if (!count.HasValue) return -1;
            if (count.Value < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (maxCount >= 0 && count.Value > maxCount) throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {maxCount}");
            if (count.Value < 1) return 0;
            stream.ReadGenericListItems(version, list, count.Value);
            return count.Value;
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="maxCount">Maximum item count (<c>-1</c> for no limit; excluding the given lists current item count)</param>
        /// <param name="includesType">If the item type is included</param>
        /// <returns>Number of red items or <c>-1</c>, if the list was <see langword="null"/></returns>
        public static int ReadGenericListNullableValuesNullable<T>(this Stream stream, in int version, in IList<T?> list, in int maxCount, in bool includesType = true)
        {
            Type type = typeof(T),
                serializedType = includesType
                    ? stream.ReadType(version)
                    : type;
            if (type != serializedType && !type.IsAssignableFrom(serializedType))
                throw new InvalidDataException($"Serialized item type {serializedType} is incompatible with requested type {type}");
            int? count = stream.ReadNumericNullable<int>(version);
            if (!count.HasValue) return -1;
            if (count.Value < 0) throw new InvalidDataException($"Invalid list item count of {count}");
            if (maxCount >= 0 && count.Value > maxCount) throw new OutOfMemoryException($"List item count of {count} exceeds the max. item count of {maxCount}");
            if (count.Value < 1) return 0;
            stream.ReadGenericListItemsNullableValues(version, list, count.Value);
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
        public static void ReadGenericListItems<T>(this Stream stream, in int version, in IList<T> list, in int count)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 0, nameof(count));
            if (count < 1) return;
            //TODO Read the items
        }

        /// <summary>
        /// Read an <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="list">List</param>
        /// <param name="count">Item count</param>
        public static void ReadGenericListItemsNullableValues<T>(this Stream stream, in int version, in IList<T?> list, in int count)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(count, other: 0, nameof(count));
            if (count < 1) return;
            //TODO Read the items
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
    }
}
