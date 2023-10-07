using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Simple dictionary based translation
    /// </summary>
    public class TranslationTerms : ITranslationTerms
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="terms">Terms</param>
        public TranslationTerms(IReadOnlyDictionary<string, string> terms) => Terms = terms;

        /// <summary>
        /// Terms
        /// </summary>
        public IReadOnlyDictionary<string, string> Terms { get; }

        /// <inheritdoc/>
        public virtual bool PluralSupport => false;

        /// <inheritdoc/>
        public string this[string key] => GetTerm(key);

        /// <inheritdoc/>
        public string this[in string key, in int count] => GetTerm(key, count);

        /// <inheritdoc/>
        public IEnumerable<string> Keys => Terms.Keys;

        /// <inheritdoc/>
        public IEnumerable<string> Values => Terms.Values;

        /// <inheritdoc/>
        public int Count => Terms.Count;

        /// <inheritdoc/>
        public virtual string GetTerm(in string key) => TryGetValue(key, out string? term) ? term : key;

        /// <inheritdoc/>
        public virtual string GetTerm(in string key, in int count)
            => PluralSupport ? throw new NotImplementedException() : throw new NotSupportedException();

        /// <inheritdoc/>
        public bool ContainsKey(string key) => Terms.ContainsKey(key);

        /// <inheritdoc/>
        public virtual bool TryGetValue(string key, [MaybeNullWhen(returnValue: false)] out string value) => Terms.TryGetValue(key, out value);

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => Terms.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Terms).GetEnumerator();
    }
}
