using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Base class for an enumeration
    /// </summary>
    public abstract class EnumerationBase : IEnumeration
    {
        /// <summary>
        /// Hash code
        /// </summary>
        protected readonly int HashCode;
        /// <summary>
        /// Lower case name
        /// </summary>
        protected readonly string LcName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Numeric value</param>
        /// <param name="name">Name</param>
        protected EnumerationBase(in int value, in string name)
        {
            if (name.Length == 0) throw new ArgumentException("Name required", nameof(name));
            if (!typeof(EnumerationBase<>).MakeGenericType(GetType()).IsAssignableFrom(GetType()))
                throw new InvalidProgramException($"{GetType()} must extend {typeof(EnumerationBase<>).MakeGenericType(GetType())}");
            LcName = name.ToLower();
            Value = value;
            Name = name;
            HashCode = GetType().GetHashCode() ^ value.GetHashCode();
        }

        /// <inheritdoc/>
        public int Value { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public abstract int CompareTo(object? obj);

        /// <inheritdoc/>
        public sealed override string ToString() => Name;
    }

    /// <summary>
    /// Base class for an enumeration
    /// </summary>
    /// <typeparam name="T">Final type (should be sealed with a private constructor!)</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="value">Numeric value</param>
    /// <param name="name">Name</param>
    public abstract class EnumerationBase<T>(in int value, in string name) : EnumerationBase(value, name), IComparable<T>, IEquatable<T> where T : EnumerationBase<T>
    {
        /// <summary>
        /// All values
        /// </summary>
        private static FrozenSet<T>? _AllValues = null;
        /// <summary>
        /// All values
        /// </summary>
        private static FrozenSet<IEnumeration>? _AllEnumerationValues = null;
        /// <summary>
        /// Keys of values
        /// </summary>
        private static FrozenDictionary<int, string>? _ValueKeys = null;
        /// <summary>
        /// Values of keys
        /// </summary>
        private static FrozenDictionary<string, int>? _KeyValues = null;

        /// <summary>
        /// Constructor
        /// </summary>
        static EnumerationBase()
        {
            if (!typeof(T).IsSealed) throw new InvalidProgramException($"{typeof(T)} must be sealed");
            if (typeof(T).GetConstructorsCached(BindingFlags.Public).Length != 0) throw new InvalidProgramException($"{typeof(T)} must use private construction");
        }

        /// <summary>
        /// All values
        /// </summary>
        public static IReadOnlyCollection<T> AllValues
        {
            get
            {
                if (_AllValues is null) Init();
                return _AllValues;
            }
        }

        /// <inheritdoc/>
        public static IReadOnlyCollection<IEnumeration> AllEnumerationValues
        {
            get
            {
                if (_AllEnumerationValues is null) Init();
                return _AllEnumerationValues;
            }
        }

        /// <inheritdoc/>
        public static IReadOnlyDictionary<int, string> ValueKeys
        {
            get
            {
                if (_ValueKeys is null) Init();
                return _ValueKeys;
            }
        }

        /// <inheritdoc/>
        public static IReadOnlyDictionary<string, int> KeyValues
        {
            get
            {
                if (_KeyValues is null) Init();
                return _KeyValues;
            }
        }

        /// <inheritdoc/>
        public sealed override int CompareTo(object? obj) => obj is T e && typeof(T) == e.GetType() ? Value.CompareTo(e.Value) : 1;

        /// <inheritdoc/>
        public int CompareTo(T? other) => other is not null ? Value.CompareTo(other.Value) : 1;

        /// <inheritdoc/>
        public sealed override bool Equals(object? obj) => obj is T e && typeof(T) == e.GetType() && Value == e.Value;

        /// <inheritdoc/>
        public bool Equals(T? other) => Value == other?.Value;

        /// <inheritdoc/>
        public sealed override int GetHashCode() => HashCode;

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="e">Enumeration value</param>
        public static implicit operator int(in EnumerationBase<T> e) => e.Value;

        /// <summary>
        /// Cast as name
        /// </summary>
        /// <param name="e">Enumeration value</param>
        public static implicit operator string(in EnumerationBase<T> e) => e.Name;

        /// <summary>
        /// Equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If equal</returns>
        public static bool operator ==(in EnumerationBase<T> a, in EnumerationBase<T> b) => a.Equals(b);

        /// <summary>
        /// Not equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If not equal</returns>
        public static bool operator !=(in EnumerationBase<T> a, in EnumerationBase<T> b) => !a.Equals(b);

        /// <summary>
        /// Lower than
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If lower than</returns>
        public static bool operator <(in EnumerationBase<T> a, in EnumerationBase<T> b) => a.CompareTo(b) < 0;

        /// <summary>
        /// Greater than
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If greater than</returns>
        public static bool operator >(in EnumerationBase<T> a, in EnumerationBase<T> b) => a.CompareTo(b) > 0;

        /// <summary>
        /// Lower or equal to
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If lower or equal to</returns>
        public static bool operator <=(in EnumerationBase<T> a, in EnumerationBase<T> b) => a.CompareTo(b) <= 0;

        /// <summary>
        /// Greater or equal to
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>If greater or equal to</returns>
        public static bool operator >=(in EnumerationBase<T> a, in EnumerationBase<T> b) => a.CompareTo(b) >= 0;

        /// <summary>
        /// Parse an enumeration value
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Enumeration value</returns>
        public static T Parse(in string name)
        {
            string? n = name.ToLower();
            return AllValues.FirstOrDefault(v => v.LcName == n) ?? throw new ArgumentException("Unknown key", nameof(name));
        }

        /// <summary>
        /// Try parsing an enumeration value
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="result">Enumeration value</param>
        /// <returns>If succeed</returns>
        public static bool TryParse(in string name, [NotNullWhen(returnValue: true)] out T? result)
        {
            string? n = name.ToLower();
            result = AllValues.FirstOrDefault(v => v.LcName == n);
            return result is not null;
        }

        /// <summary>
        /// Get an enumeration value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Enumeration value</returns>
        public static T Get(int value) => AllValues.FirstOrDefault(v => v.Value == value) ?? throw new ArgumentException("Unknown value", nameof(value));

        /// <summary>
        /// Try parsing an enumeration value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="result">Enumeration value</param>
        /// <returns>If succeed</returns>
        public static bool TryGet(int value, [NotNullWhen(returnValue: true)] out T? result)
        {
            result = AllValues.FirstOrDefault(v => v.Value == value);
            return result is not null;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [MemberNotNull(nameof(_AllValues), nameof(_AllEnumerationValues), nameof(_KeyValues), nameof(_ValueKeys))]
        private static void Init()
        {
            List<T> allValues = [];
            T value;
            foreach(FieldInfoExt fi in from fi in typeof(T).GetFieldsCached(BindingFlags.Static | BindingFlags.Public)
                                    where fi.FieldType == typeof(T) && 
                                        fi.Getter is not null
                                    select fi)
            {
                value = fi.Getter!(null) as T ?? throw new InvalidProgramException($"{typeof(T)}.{fi.Name} value is NULL");
                if (value.Name != fi.Name) throw new InvalidProgramException($"Field {typeof(T)}.{fi.Name} enumeration value name mismatch");
                allValues.Add(value);
            }
            if (allValues.Count == 0) throw new InvalidProgramException($"Empty enumeration class {typeof(T)}");
            _AllValues = allValues.ToFrozenSet();
            _AllEnumerationValues = _AllValues.Cast<IEnumeration>().ToFrozenSet();
            _ValueKeys = new Dictionary<int, string>(allValues.Select(v => new KeyValuePair<int, string>(v.Value, v.Name))).ToFrozenDictionary();
            _KeyValues = new Dictionary<string, int>(allValues.Select(v => new KeyValuePair<string, int>(v.Name, v.Value))).ToFrozenDictionary();
            if (_ValueKeys.Count != allValues.Count) throw new InvalidProgramException("Found double enumeration values");
            if (_KeyValues.Count != allValues.Count) throw new InvalidProgramException("Found double enumeration names");
        }
    }
}
