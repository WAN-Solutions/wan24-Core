using System.Collections;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Generic <see cref="IDictionary{TKey, TValue}"/> wrapper
    /// </summary>
    public sealed class GenericDictionaryWrapper : IDictionary
    {
        /// <summary>
        /// Generic list type
        /// </summary>
        private readonly TypeInfoExt GenericType;
        /// <summary>
        /// Index property
        /// </summary>
        private readonly PropertyInfo IndexProperty;
        /// <summary>
        /// <see cref="Keys"/> property
        /// </summary>
        private readonly PropertyInfoExt KeysProperty;
        /// <summary>
        /// <see cref="Values"/> property
        /// </summary>
        private readonly PropertyInfoExt ValuesProperty;
        /// <summary>
        /// <see cref="IsReadOnly"/> property
        /// </summary>
        private readonly PropertyInfoExt IsReadOnlyProperty;
        /// <summary>
        /// <see cref="Count"/> property
        /// </summary>
        private readonly PropertyInfoExt CountProperty;
        /// <summary>
        /// <see cref="KeyValuePair{TKey, TValue}.Key"/> property
        /// </summary>
        private readonly PropertyInfoExt KeyProperty;
        /// <summary>
        /// <see cref="KeyValuePair{TKey, TValue}.Value"/> property
        /// </summary>
        private readonly PropertyInfoExt ValueProperty;
        /// <summary>
        /// <see cref="Add"/> method
        /// </summary>
        private readonly MethodInfoExt AddMethod;
        /// <summary>
        /// <see cref="Clear"/> method
        /// </summary>
        private readonly MethodInfoExt ClearMethod;
        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.ContainsKey(TKey)"/> method
        /// </summary>
        private readonly MethodInfoExt ContainsKeyMethod;
        /// <summary>
        /// <see cref="CopyTo"/> method
        /// </summary>
        private readonly MethodInfoExt CopyToMethod;
        /// <summary>
        /// <see cref="GetEnumerator"/> method
        /// </summary>
        private readonly MethodInfoExt GetEnumeratorMethod;
        /// <summary>
        /// <see cref="Remove"/> method
        /// </summary>
        private readonly MethodInfoExt RemoveMethod;
        /// <summary>
        /// Dictionary
        /// </summary>
        private readonly object Dict;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dict"><see cref="IDictionary{TKey, TValue}"/> type</param>
        /// <exception cref="ArgumentException">Not a generic dictionary</exception>
        public GenericDictionaryWrapper(in object dict)
        {
            if (dict.GetType().FindGenericType(typeof(IDictionary<,>)) is not Type genericType)
                throw new ArgumentException("Not a generic dictionary", nameof(dict));
            Dict = dict;
            TypeInfoExt genericTypeInfo = TypeInfoExt.From(genericType);
            GenericType = genericTypeInfo;
            IndexProperty = GenericType.GetProperties().First(p => p.Property.GetMethod is not null && p.Property.GetMethod.GetParameterCountCached() > 0).Property;
            IsReadOnlyProperty = GenericType.GetProperties().First(p => p.Name == nameof(IsReadOnly));
            CountProperty = GenericType.GetProperties().First(p => p.Name == nameof(Count));
            KeysProperty = GenericType.GetProperties().First(p => p.Name == nameof(Keys));
            ValuesProperty = GenericType.GetProperties().First(p => p.Name == nameof(Values));
            AddMethod = GenericType.GetMethods().First(m => m.Name == nameof(Add));
            ClearMethod = GenericType.GetMethods().First(m => m.Name == nameof(Clear));
            ContainsKeyMethod = GenericType.GetMethods().First(m => m.Name == nameof(IDictionary<object, object>.ContainsKey));
            CopyToMethod = GenericType.GetMethods().First(m => m.Name == nameof(CopyTo));
            GetEnumeratorMethod = GenericType.GetMethods().First(m => m.Name == nameof(GetEnumerator));
            RemoveMethod = GenericType.GetMethods().First(m => m.Name == nameof(Remove));
            TypeInfoExt kvp = TypeInfoExt.From(typeof(KeyValuePair<,>)).MakeGenericType(KeyType, ValueType);
            KeyProperty = kvp.GetProperties().First(p => p.Name == nameof(KeyValuePair<object, object>.Key));
            ValueProperty = kvp.GetProperties().First(p => p.Name == nameof(KeyValuePair<object, object>.Value));
        }

        /// <summary>
        /// Key type
        /// </summary>
        public Type KeyType => GenericType.FirstGenericArgument ?? throw new InvalidProgramException();

        /// <summary>
        /// Value type
        /// </summary>
        public Type ValueType => GenericType.GetGenericArguments()[1];

        /// <inheritdoc/>
        public object? this[object key]
        {
            get => IndexProperty.GetMethod!.Invoke(Dict, [key]);
            set => IndexProperty.SetMethod!.Invoke(Dict, [key, value]);
        }

        /// <inheritdoc/>
        public bool IsFixedSize { get; init; }

        /// <inheritdoc/>
        public bool IsReadOnly => (bool)(IsReadOnlyProperty.Getter!(Dict) ?? throw new InvalidProgramException());

        /// <inheritdoc/>
        public ICollection Keys => new List<object>([.. (IEnumerable)(KeysProperty.Getter!(Dict) ?? throw new InvalidProgramException())]);

        /// <inheritdoc/>
        public ICollection Values => new List<object?>([.. (IEnumerable)(ValuesProperty.Getter!(Dict) ?? throw new InvalidProgramException())]);

        /// <inheritdoc/>
        public int Count => (int)(CountProperty.Getter!(Dict) ?? throw new InvalidProgramException());

        /// <inheritdoc/>
        public bool IsSynchronized => false;

        /// <inheritdoc/>
        public object SyncRoot => Dict;

        /// <inheritdoc/>
        public void Add(object key, object? value) => AddMethod.Invoker!(Dict, [key, value]);

        /// <inheritdoc/>
        public void Clear() => ClearMethod.Invoker!(Dict, []);

        /// <inheritdoc/>
        public bool Contains(object key) => (bool)(ContainsKeyMethod.Invoker!(Dict, [key]) ?? throw new InvalidProgramException());

        /// <inheritdoc/>
        public void CopyTo(Array array, int index) => CopyToMethod.Invoker!(Dict, [array, index]);

        /// <inheritdoc/>
        public IDictionaryEnumerator GetEnumerator() => new Enumerator(this);

        /// <inheritdoc/>
        public void Remove(object key) => RemoveMethod.Invoker!(Dict, [key]);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Enumerator
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="wrapper">Wrapper</param>
        private sealed class Enumerator(in GenericDictionaryWrapper wrapper) : IDictionaryEnumerator
        {
            /// <summary>
            /// Wrapper
            /// </summary>
            private readonly GenericDictionaryWrapper Wrapper = wrapper;
            /// <summary>
            /// Dictionary enumerator
            /// </summary>
            private readonly IEnumerator DictEnumerator = ((IEnumerable)wrapper.Dict).GetEnumerator();

            /// <inheritdoc/>
            public DictionaryEntry Entry { get; private set; }

            /// <inheritdoc/>
            public object Key => Entry.Key;

            /// <inheritdoc/>
            public object? Value => Entry.Value;

            /// <inheritdoc/>
            public object Current => Entry;

            /// <inheritdoc/>
            public bool MoveNext()
            {
                if (!DictEnumerator.MoveNext()) return false;
                object current = DictEnumerator.Current;
                Entry = new(Wrapper.KeyProperty.Getter!(current) ?? throw new InvalidProgramException(), Wrapper.ValueProperty.Getter!(current));
                return true;
            }

            /// <inheritdoc/>
            public void Reset() => DictEnumerator.Reset();
        }
    }
}
