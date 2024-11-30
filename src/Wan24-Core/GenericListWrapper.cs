using System.Collections;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Generic <see cref="IList{T}"/> wrapper
    /// </summary>
    public sealed class GenericListWrapper : IList
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
        /// <see cref="IsReadOnly"/> property
        /// </summary>
        private readonly PropertyInfoExt IsReadOnlyProperty;
        /// <summary>
        /// <see cref="Count"/> property
        /// </summary>
        private readonly PropertyInfoExt CountProperty;
        /// <summary>
        /// <see cref="Add"/> method
        /// </summary>
        private readonly MethodInfoExt AddMethod;
        /// <summary>
        /// <see cref="Clear"/> method
        /// </summary>
        private readonly MethodInfoExt ClearMethod;
        /// <summary>
        /// <see cref="Contains"/> method
        /// </summary>
        private readonly MethodInfoExt ContainsMethod;
        /// <summary>
        /// <see cref="CopyTo"/> method
        /// </summary>
        private readonly MethodInfoExt CopyToMethod;
        /// <summary>
        /// <see cref="GetEnumerator"/> method
        /// </summary>
        private readonly MethodInfoExt GetEnumeratorMethod;
        /// <summary>
        /// <see cref="IndexOf"/> method
        /// </summary>
        private readonly MethodInfoExt IndexOfMethod;
        /// <summary>
        /// <see cref="Insert"/> method
        /// </summary>
        private readonly MethodInfoExt InsertMethod;
        /// <summary>
        /// <see cref="Remove"/> method
        /// </summary>
        private readonly MethodInfoExt RemoveMethod;
        /// <summary>
        /// <see cref="RemoveAt"/> method
        /// </summary>
        private readonly MethodInfoExt RemoveAtMethod;
        /// <summary>
        /// List
        /// </summary>
        private readonly object List;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="list"><see cref="IList{T}"/> type</param>
        /// <exception cref="ArgumentException">Not a generic list</exception>
        public GenericListWrapper(in object list)
        {
            if (list.GetType().FindGenericType(typeof(IList<>)) is not Type genericType)
                throw new ArgumentException("Not a generic list", nameof(list));
            List = list;
            TypeInfoExt genericTypeInfo = TypeInfoExt.From(genericType);
            GenericType = genericTypeInfo;
            IndexProperty = GenericType.GetProperties().First(p => p.Property.GetMethod is not null && p.Property.GetMethod.GetParameterCountCached() > 0).Property;
            IsReadOnlyProperty = GenericType.GetProperties().First(p => p.Name == nameof(IsReadOnly));
            CountProperty = GenericType.GetProperties().First(p => p.Name == nameof(Count));
            AddMethod = GenericType.GetMethods().First(m => m.Name == nameof(Add));
            ClearMethod = GenericType.GetMethods().First(m => m.Name == nameof(Clear));
            ContainsMethod = GenericType.GetMethods().First(m => m.Name == nameof(Contains));
            CopyToMethod = GenericType.GetMethods().First(m => m.Name == nameof(CopyTo));
            GetEnumeratorMethod = GenericType.GetMethods().First(m => m.Name == nameof(GetEnumerator));
            IndexOfMethod = GenericType.GetMethods().First(m => m.Name == nameof(IndexOf));
            InsertMethod = GenericType.GetMethods().First(m => m.Name == nameof(Insert));
            RemoveMethod = GenericType.GetMethods().First(m => m.Name == nameof(Remove));
            RemoveAtMethod = GenericType.GetMethods().First(m => m.Name == nameof(RemoveAt));
        }

        /// <inheritdoc/>
        public object? this[int index]
        {
            get => IndexProperty.GetMethod!.Invoke(List, [index]);
            set => IndexProperty.SetMethod!.Invoke(List, [index, value]);
        }

        /// <summary>
        /// Generic list item type
        /// </summary>
        public Type ItemType => GenericType.FirstGenericArgument ?? throw new InvalidProgramException();

        /// <inheritdoc/>
        public bool IsFixedSize { get; init; }

        /// <inheritdoc/>
        public bool IsReadOnly => (bool)(IsReadOnlyProperty.Getter!(List) ?? throw new InvalidProgramException());

        /// <inheritdoc/>
        public int Count => (int)(CountProperty.Getter!(List) ?? throw new InvalidProgramException());

        /// <inheritdoc/>
        public bool IsSynchronized => false;

        /// <inheritdoc/>
        public object SyncRoot => List;

        /// <inheritdoc/>
        public int Add(object? value)
        {
            int count = Count;
            AddMethod.Invoker!(List, [value]);
            if (Count != count + 1) throw new InvalidOperationException();
            return count;
        }

        /// <inheritdoc/>
        public void Clear() => ClearMethod.Invoker!(List, []);

        /// <inheritdoc/>
        public bool Contains(object? value) => (bool)(ContainsMethod.Invoker!(List, [value]) ?? throw new InvalidProgramException());

        /// <inheritdoc/>
        public void CopyTo(Array array, int index) => CopyToMethod.Invoker!(List, [index]);

        /// <inheritdoc/>
        public IEnumerator GetEnumerator() => (IEnumerator)(GetEnumeratorMethod.Invoker!(List, []) ?? throw new InvalidProgramException());

        /// <inheritdoc/>
        public int IndexOf(object? value) => (int)(IndexOfMethod.Invoker!(List, [value]) ?? throw new InvalidProgramException());

        /// <inheritdoc/>
        public void Insert(int index, object? value) => InsertMethod.Invoker!(List, [index, value]);

        /// <inheritdoc/>
        public void Remove(object? value) => RemoveMethod.Invoker!(List, [value]);

        /// <inheritdoc/>
        public void RemoveAt(int index) => RemoveAtMethod.Invoker!(List, [index]);
    }
}
