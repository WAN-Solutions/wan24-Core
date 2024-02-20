using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Combines multiple translations
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="terms">Translations to combine</param>
    public class CombinedTranslationTerms(in List<ITranslationTerms> terms) : ITranslationTerms
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="terms">Translations to combine</param>
        public CombinedTranslationTerms(params ITranslationTerms[] terms) : this(terms.ToList()) { }

        /// <summary>
        /// Combined terms
        /// </summary>
        public List<ITranslationTerms> Terms { get; } = terms;

        /// <inheritdoc/>
        public virtual bool PluralSupport => Terms.Any(t => t.PluralSupport);

        /// <inheritdoc/>
        public string this[string key] => GetTerm(key);

        /// <inheritdoc/>
        public string this[in string key, in int count] => GetTerm(key, count);

        /// <inheritdoc/>
        public IEnumerable<string> Keys
            => (from term in Terms
                from key in term.Keys
                select key)
                .Distinct();

        /// <inheritdoc/>
        public IEnumerable<string> Values
            => from term in Terms
               from value in term.Values
               select value;

        /// <inheritdoc/>
        public int Count => Keys.Count();

        /// <inheritdoc/>
        public virtual string GetTerm(in string key) => TryGetValue(key, out string? term) ? term : key;

        /// <inheritdoc/>
        public virtual string GetTerm(in string key, in int count)
        {
            if (!PluralSupport) throw new NotSupportedException();
            foreach (ITranslationTerms term in Terms)
            {
                if (!term.PluralSupport) continue;
                string res = term.GetTerm(key, count);
                if (res != key) return res;
            }
            return key;
        }

        /// <inheritdoc/>
        public bool ContainsKey(string key) => Terms.Any(t => t.ContainsKey(key));

        /// <inheritdoc/>
        public virtual bool TryGetValue(string key, [MaybeNullWhen(returnValue: false)] out string value)
        {
            value = null;
            foreach (ITranslationTerms term in Terms)
                if (term.TryGetValue(key, out value))
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            HashSet<string> seenKeys = [];
            foreach (ITranslationTerms term in Terms)
                foreach (KeyValuePair<string, string> kvp in term)
                {
                    if (seenKeys.Contains(kvp.Key)) continue;
                    seenKeys.Add(kvp.Key);
                    yield return kvp;
                }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Terms).GetEnumerator();
    }
}
