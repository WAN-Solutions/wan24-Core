using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Object dictionary
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectDictionary<T> : Dictionary<T, object?> where T : notnull
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectDictionary() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">capacity</param>
        public ObjectDictionary(in int capacity) : base(capacity) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items</param>
        public ObjectDictionary(in IEnumerable<KeyValuePair<T, object?>> items) : base(items) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items</param>
        public ObjectDictionary(in IDictionary<T, object?> items) : base(items) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparer">Comparer</param>
        public ObjectDictionary(in IEqualityComparer<T> comparer) : base(comparer) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="comparer">Comparer</param>
        public ObjectDictionary(in int capacity, in IEqualityComparer<T> comparer) : base(capacity, comparer) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="comparer">Comparer</param>
        public ObjectDictionary(in IEnumerable<KeyValuePair<T, object?>> items, in IEqualityComparer<T> comparer) : base(items, comparer) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="comparer">Comparer</param>
        public ObjectDictionary(in IDictionary<T, object?> items, in IEqualityComparer<T> comparer) : base(items, comparer) { }

        /// <summary>
        /// Get an item (value type must match!)
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        /// <exception cref="NullReferenceException">Key exists, but its value was <see langword="null"/></exception>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        /// <exception cref="KeyNotFoundException">Key doesn't exist</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual tValue Get<tValue>(in T key)
            => TryGetValue(key, out object? res) 
                ? (tValue)(res ?? throw new NullReferenceException())
                : throw new KeyNotFoundException();

        /// <summary>
        /// Get an item (value type must match!)
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Value type</param>
        /// <returns>Value</returns>
        /// <exception cref="NullReferenceException">Key exists, but its value was <see langword="null"/></exception>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        /// <exception cref="KeyNotFoundException">Key doesn't exist</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual object Get(in T key, in Type type)
            => TryGetValue(key, out object? res)
                ? res is null
                    ? throw new NullReferenceException()
                    : type.IsAssignableFromExt(res.GetType())
                        ? res
                        : throw new InvalidCastException()
                : throw new KeyNotFoundException();

        /// <summary>
        /// Get an item (value type must match!)
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        /// <exception cref="KeyNotFoundException">Key doesn't exist</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual tValue? GetNullable<tValue>(in T key) where tValue : class
            => TryGetValue(key, out object? res) 
                ? res is null
                    ? default
                    : (tValue)res 
                : throw new KeyNotFoundException();

        /// <summary>
        /// Get an item (value type must match!)
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        /// <exception cref="KeyNotFoundException">Key doesn't exist</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual tValue? GetNullableStruct<tValue>(in T key) where tValue: struct
            => TryGetValue(key, out object? res)
                ? res is null
                    ? new Nullable<tValue>()
                    : (tValue)res
                : throw new KeyNotFoundException();

        /// <summary>
        /// Get an item (value type must match!)
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="exists">If the key exists</param>
        /// <returns>Value</returns>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual tValue? GetNullable<tValue>(in T key, out bool exists) where tValue : class
            => (exists = TryGetValue(key, out object? res))
                ? res is null
                    ? default
                    : (tValue)res
                : default;

        /// <summary>
        /// Get an item (value type must match!)
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="exists">If the key exists</param>
        /// <returns>Value</returns>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual tValue? GetNullableStruct<tValue>(in T key, out bool exists) where tValue : struct
            => (exists = TryGetValue(key, out object? res))
                ? res is null
                    ? new Nullable<tValue>()
                    : (tValue)res
                : new Nullable<tValue>();

        /// <summary>
        /// Get an item (value type must match!)
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Value type</param>
        /// <returns>Value</returns>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        /// <exception cref="KeyNotFoundException">Key doesn't exist</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual object? GetNullable(in T key, in Type type)
            => TryGetValue(key, out object? res)
                ? res is null || type.IsAssignableFromExt(res.GetType())
                    ? res
                    : throw new InvalidCastException()
                : throw new KeyNotFoundException();

        /// <summary>
        /// Get an item (value type must match!)
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Value type</param>
        /// <param name="exists">If the key exists</param>
        /// <returns>Value</returns>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual object? GetNullable(in T key, in Type type, out bool exists)
            => (exists = TryGetValue(key, out object? res))
                ? res is null || type.IsAssignableFromExt(res.GetType())
                    ? res
                    : throw new InvalidCastException()
                : default;

        /// <summary>
        /// Get an item or the default value (value type must match!)
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Value</returns>
        /// <exception cref="NullReferenceException">Key exists, but its value was <see langword="null"/></exception>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual tValue ValueOrDefaultGeneric<tValue>(in T key, in tValue defaultValue)
            => TryGetValue(key, out object? res) 
                ? (tValue)(res ?? throw new NullReferenceException())
                : defaultValue;

        /// <summary>
        /// Get an item or the default value (value type must match!)
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="exists">If the key exists</param>
        /// <returns>Value</returns>
        /// <exception cref="NullReferenceException">Key exists, but its value was <see langword="null"/></exception>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual tValue ValueOrDefaultGeneric<tValue>(in T key, in tValue defaultValue, out bool exists)
            => (exists = TryGetValue(key, out object? res))
                ? (tValue)(res ?? throw new NullReferenceException())
                : defaultValue;

        /// <summary>
        /// Get an item or the default value (value type must match!)
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Value type</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Value</returns>
        /// <exception cref="NullReferenceException">Key exists, but its value was <see langword="null"/></exception>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual object ValueOrDefault(in T key, in Type type, in object defaultValue)
            => TryGetValue(key, out object? res)
                ? res is null
                    ? throw new NullReferenceException()
                    : type.IsAssignableFromExt(res.GetType())
                        ? res
                        : throw new InvalidCastException()
                : defaultValue;

        /// <summary>
        /// Get an item or the default value (value type must match!)
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Value type</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="exists">If the key exists</param>
        /// <returns>Value</returns>
        /// <exception cref="NullReferenceException">Key exists, but its value was <see langword="null"/></exception>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual object ValueOrDefault(in T key, in Type type, in object defaultValue, out bool exists)
            => (exists = TryGetValue(key, out object? res))
                ? res is null
                    ? throw new NullReferenceException()
                    : type.IsAssignableFromExt(res.GetType())
                        ? res
                        : throw new InvalidCastException()
                : defaultValue;

        /// <summary>
        /// Get an item or the default value (value type must match!)
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Value</returns>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual tValue? ValueOrDefaultNullableGeneric<tValue>(in T key, in tValue? defaultValue = default) where tValue: class
            => TryGetValue(key, out object? res) 
                ? res is null
                    ? default
                    : (tValue)res
                : defaultValue;

        /// <summary>
        /// Get an item or the default value (value type must match!)
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Value</returns>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual tValue? StructValueOrDefaultNullableGeneric<tValue>(in T key, in tValue? defaultValue = default) where tValue : struct
            => TryGetValue(key, out object? res)
                ? res is null
                    ? new Nullable<tValue>()
                    : (tValue)res
                : defaultValue;

        /// <summary>
        /// Get an item or the default value (value type must match!)
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="exists">If the key exists</param>
        /// <returns>Value</returns>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual tValue? ValueOrDefaultNullableGeneric<tValue>(in T key, out bool exists, in tValue? defaultValue = default) where tValue: class
            => (exists = TryGetValue(key, out object? res))
                ? res is null
                    ? default
                    : (tValue)res
                : defaultValue;

        /// <summary>
        /// Get an item or the default value (value type must match!)
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="exists">If the key exists</param>
        /// <returns>Value</returns>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual tValue? StructValueOrDefaultNullableGeneric<tValue>(in T key, out bool exists, in tValue? defaultValue = default) where tValue: struct
            => (exists = TryGetValue(key, out object? res))
                ? res is null
                    ? default
                    : (tValue)res
                : defaultValue;

        /// <summary>
        /// Get an item or the default value (value type must match!)
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Value type</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Value</returns>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual object? ValueOrDefaultNullable(in T key, in Type type, in object? defaultValue = default)
            => TryGetValue(key, out object? res)
                ? res is null || type.IsAssignableFromExt(res.GetType())
                    ? res
                    : throw new InvalidCastException()
                : defaultValue;

        /// <summary>
        /// Get an item or the default value (value type must match!)
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Value type</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="exists">If the key exists</param>
        /// <returns>Value</returns>
        /// <exception cref="InvalidCastException">Key exists, but its value doesn't match the given value type</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual object? ValueOrDefaultNullable(in T key, in Type type, out bool exists, in object? defaultValue = default)
            => (exists = TryGetValue(key, out object? res))
                ? res is null || type.IsAssignableFromExt(res.GetType())
                    ? res
                    : throw new InvalidCastException()
                : defaultValue;

        /// <summary>
        /// Determine if a value of a type is contained (and not <see langword="null"/>)
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>If the value is contained and from the given type (and not <see langword="null"/>)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual bool ContainsValueOfType<tValue>(in T key)
            => TryGetValue(key, out object? res) && res is not null && typeof(tValue).IsAssignableFromExt(res.GetType());

        /// <summary>
        /// Determine if a value of a type is contained
        /// </summary>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>If the value is contained and from the given type (and not <see langword="null"/>)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual bool ContainsValueOfTypeNullable<tValue>(in T key)
            => TryGetValue(key, out object? res) && (res is null || typeof(tValue).IsAssignableFromExt(res.GetType()));

        /// <summary>
        /// Determine if a value of a type is contained (and not <see langword="null"/>)
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Value type</param>
        /// <returns>If the value is contained and from the given type (and not <see langword="null"/>)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual bool ContainsValueOfType(in T key, in Type type)
            => TryGetValue(key, out object? res) && res is not null && type.IsAssignableFromExt(res.GetType());

        /// <summary>
        /// Determine if a value of a type is contained
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Value type</param>
        /// <returns>If the value is contained and from the given type (and not <see langword="null"/>)</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public virtual bool ContainsValueOfTypeNullable(in T key, in Type type)
            => TryGetValue(key, out object? res) && (res is null || type.IsAssignableFromExt(res.GetType()));

        /// <summary>
        /// Cast as count value
        /// </summary>
        /// <param name="dict"><see cref="ObjectDictionary{T}"/></param>
        [TargetedPatchingOptOut("Tiny method")]
        public static implicit operator int(in ObjectDictionary<T> dict) => dict.Count;
    }
}
