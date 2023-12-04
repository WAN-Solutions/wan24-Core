using System.Collections.ObjectModel;

namespace wan24.Core
{
    /// <summary>
    /// CLI arguments (an argument key for a following value starts with <c>--</c> (double dashes), a flag argument with <c>-</c> (single dash))
    /// </summary>
    public partial class CliArguments
    {
        /// <summary>
        /// Backslash
        /// </summary>
        protected const string BACKSLASH = @"\";
        /// <summary>
        /// Single quote
        /// </summary>
        protected const string ESCAPED_BACKSLASH = @"\\";
        /// <summary>
        /// String parameter name
        /// </summary>
        protected const string STR_PARAMETER_NAME = "str";

        /// <summary>
        /// Arguments
        /// </summary>
        protected readonly OrderedDictionary<string, ReadOnlyCollection<string>> _Arguments = [];
        /// <summary>
        /// Read-only arguments
        /// </summary>
        protected OrderedDictionary<string, ReadOnlyCollection<string>>? ReadOnlyArguments = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="args">Arguments (an argument key for a following value starts with <c>--</c> (double dashes), a flag argument with <c>-</c> (single dash))</param>
        public CliArguments(params string[] args) : this(args.AsSpan()) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="args">Arguments (an argument key for a following value starts with <c>--</c> (double dashes), a flag argument with <c>-</c> (single dash))</param>
        public CliArguments(in IEnumerable<string> args) : this(args.ToArray().AsSpan()) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="args">Arguments (an argument key for a following value starts with <c>--</c> (double dashes), a flag argument with <c>-</c> (single dash))</param>
        public CliArguments(in ReadOnlySpan<string> args) => Initialize(args);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="args">Arguments</param>
        public CliArguments(in IEnumerable<KeyValuePair<string, ReadOnlyCollection<string>>> args) => _Arguments.AddRange(args);

        /// <summary>
        /// Get if an argument was given
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="requireValues">Is the key required to have values (not be a boolean)?</param>
        /// <returns>Argument was given (and has values)?</returns>
        public bool this[in string key, in bool requireValues = false] => _Arguments.ContainsKey(key) && (!requireValues || _Arguments[key].Count != 0);

        /// <summary>
        /// Arguments
        /// </summary>
        public OrderedDictionary<string, ReadOnlyCollection<string>> Arguments => ReadOnlyArguments ??= _Arguments.AsReadOnly();

        /// <summary>
        /// Keyless arguments
        /// </summary>
        public ReadOnlyCollection<string> KeyLessArguments { get; internal set; } = null!;

        /// <summary>
        /// Number of given arguments
        /// </summary>
        public int Count => _Arguments.Count;

        /// <summary>
        /// Get a single argument value
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public string Single(in string key)
        {
            if (!_Arguments.ContainsKey(key)) throw new ArgumentException($"Unknown argument \"{key}\"", nameof(key));
            ReadOnlyCollection<string> values = _Arguments[key];
            if (values.Count == 0) throw new InvalidOperationException($"Given argument \"{key}\" is a boolean");
            return values[0];
        }

        /// <summary>
        /// Get a single JSON decoded argument value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public T SingleJson<T>(in string key)
            => JsonHelper.Decode<T>(Single(key)) ?? throw new InvalidDataException($"Failed to JSON decode the value of \"{key}\" as {typeof(T)}");

        /// <summary>
        /// Get a single JSON decoded argument value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Value type</param>
        /// <returns>Value</returns>
        public object SingleJson(in string key, in Type type)
            => JsonHelper.DecodeObject(type, Single(key)) ?? throw new InvalidDataException($"Failed to JSON decode the value of \"{key}\" as {type}");

        /// <summary>
        /// Get a single JSON decoded nullable argument value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public T? SingleJsonNullable<T>(in string key) => JsonHelper.Decode<T>(Single(key));

        /// <summary>
        /// Get a single JSON decoded nullable argument value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Value type</param>
        /// <returns>Value</returns>
        public object? SingleJsonNullable(in string key, in Type type) => JsonHelper.DecodeObject(type, Single(key));

        /// <summary>
        /// Get all argument values
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Values</returns>
        public ReadOnlyCollection<string> All(in string key)
        {
            if (!_Arguments.ContainsKey(key)) throw new ArgumentException($"Unknown argument \"{key}\"", nameof(key));
            ReadOnlyCollection<string> values = _Arguments[key];
            if (values.Count == 0) throw new InvalidOperationException($"Given argument \"{key}\" is a boolean");
            return values;
        }

        /// <summary>
        /// Get all JSON decoded argument values
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Values</returns>
        public IEnumerable<T> AllJson<T>(string key)
        {
            if (!_Arguments.ContainsKey(key)) throw new ArgumentException($"Unknown argument \"{key}\"", nameof(key));
            ReadOnlyCollection<string> values = _Arguments[key];
            if (values.Count == 0) throw new InvalidOperationException($"Given argument \"{key}\" is a boolean");
            for (int i = 0, len = values.Count; i < len; i++)
                yield return JsonHelper.Decode<T>(values[i])
                    ?? throw new InvalidDataException($"Failed to JSON decode value #{i} of \"{key}\"");
        }

        /// <summary>
        /// Get all JSON decoded argument values
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Value type</param>
        /// <returns>Values</returns>
        public IEnumerable<object> AllJson(string key, Type type)
        {
            if (!_Arguments.ContainsKey(key)) throw new ArgumentException($"Unknown argument \"{key}\"", nameof(key));
            ReadOnlyCollection<string> values = _Arguments[key];
            if (values.Count == 0) throw new InvalidOperationException($"Given argument \"{key}\" is a boolean");
            for (int i = 0, len = values.Count; i < len; i++)
                yield return JsonHelper.DecodeObject(type, values[i])
                    ?? throw new InvalidDataException($"Failed to JSON decode value #{i} of \"{key}\" as {type}");
        }

        /// <summary>
        /// Get all JSON decoded argument values
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Values</returns>
        public IEnumerable<T?> AllJsonNullable<T>(string key)
        {
            if (!_Arguments.ContainsKey(key)) throw new ArgumentException($"Unknown argument \"{key}\"", nameof(key));
            ReadOnlyCollection<string> values = _Arguments[key];
            if (values.Count == 0) throw new InvalidOperationException($"Given argument \"{key}\" is a boolean");
            for (int i = 0, len = values.Count; i < len; i++)
                yield return JsonHelper.Decode<T>(values[i]);
        }

        /// <summary>
        /// Get all JSON decoded argument values
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="type">Value type</param>
        /// <returns>Values</returns>
        public IEnumerable<object?> AllJsonNullable(string key, Type type)
        {
            if (!_Arguments.ContainsKey(key)) throw new ArgumentException($"Unknown argument \"{key}\"", nameof(key));
            ReadOnlyCollection<string> values = _Arguments[key];
            if (values.Count == 0) throw new InvalidOperationException($"Given argument \"{key}\" is a boolean");
            for (int i = 0, len = values.Count; i < len; i++)
                yield return JsonHelper.DecodeObject(type, values[i]);
        }

        /// <summary>
        /// Determine if a given argument is a flag
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Is a flag (boolean)?</returns>
        public bool IsBoolean(in string key) => _Arguments.ContainsKey(key) && _Arguments[key].Count == 0;

        /// <summary>
        /// Determine if a given argument has values
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Is has values?</returns>
        public bool HasValues(in string key) => _Arguments.ContainsKey(key) && _Arguments[key].Count != 0;

        /// <summary>
        /// Get the value count for a key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Number of values or <c>-1</c>, if the argument wasn't given</returns>
        public int ValueCount(in string key) => _Arguments.ContainsKey(key) ? _Arguments[key].Count : -1;

        /// <summary>
        /// Get an existing key
        /// </summary>
        /// <param name="key">Key (case insensitive handling)</param>
        /// <returns>Existing case sensitive key or <see langword="null"/>, if the key wasn't found</returns>
        public string? GetExistingKey(string key)
            => (from k in _Arguments.Keys
                where k.Equals(key, StringComparison.CurrentCultureIgnoreCase)
                select k)
                .FirstOrDefault();
    }
}
