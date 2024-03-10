using Microsoft.Extensions.Localization;
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
        public virtual string this[in string key, params string[] values] => GetTerm(key, values);

        /// <inheritdoc/>
        public virtual string this[in string key, in int count, params string[] values] => GetTerm(key, count, values);

        /// <inheritdoc/>
        string IReadOnlyDictionary<string,string>.this[string key] => this[key];

        /// <inheritdoc/>
        LocalizedString IStringLocalizer.this[string name, params object[] arguments] => new(name,this[name, [.. arguments.Select(a=>a.ToString()??string.Empty)]]);

        /// <inheritdoc/>
        LocalizedString IStringLocalizer.this[string name] => new(name, this[name]);

        /// <inheritdoc/>
        public virtual IEnumerable<string> Keys
            => (from term in Terms
                from key in term.Keys
                select key)
                .Distinct();

        /// <inheritdoc/>
        public virtual IEnumerable<string> Values
            => (from term in Terms
                from value in term.Values
                select value)
                .Distinct();

        /// <inheritdoc/>
        public virtual int Count => Keys.Count();

        /// <inheritdoc/>
        public virtual string GetTerm(in string key, params string[] values)
        {
            string res = TryGetValue(key, out string? term) ? term : key;
            if (values.Length > 0)
            {
                int len = values.Length;
                Dictionary<string, string> valuesDict = new(len);
                for (int i = 0; i < len; valuesDict[i.ToString()] = values[i], i++) ;
                res = res.Parse(valuesDict, Translation.ParserOptions);
            }
            return res;
        }

        /// <inheritdoc/>
        public virtual string GetTerm(in string key, in int count, params string[] values)
        {
            if (!PluralSupport) throw new NotSupportedException();
            string res;
            foreach (ITranslationTerms terms in Terms.Where(t => t.PluralSupport))
            {
                res = terms.GetTerm(key, count, values);
                if (res != key) return res;
            }
            return key;
        }

        /// <inheritdoc/>
        public virtual bool ContainsKey(string key) => Terms.Any(t => t.ContainsKey(key));

        /// <inheritdoc/>
        public virtual bool TryGetValue(string key, [MaybeNullWhen(returnValue: false)] out string value)
        {
            value = null;
            foreach (ITranslationTerms terms in Terms)
                if (terms.TryGetValue(key, out value))
                    return true;
            return false;
        }

        /// <inheritdoc/>
        public virtual IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            HashSet<string> seenKeys = [];
            foreach (ITranslationTerms terms in Terms)
                foreach (KeyValuePair<string, string> kvp in terms)
                {
                    if (seenKeys.Contains(kvp.Key)) continue;
                    seenKeys.Add(kvp.Key);
                    yield return kvp;
                }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        IEnumerable<LocalizedString> IStringLocalizer.GetAllStrings(bool includeParentCultures) => Keys.Select(k => StringLocalizer(k, []));

        /// <summary>
        /// String localizer used for the <see cref="IStringLocalizer"/> implementation
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="arguments">Arguments</param>
        /// <returns>Localized string</returns>
        protected virtual LocalizedString StringLocalizer(string name, object[] arguments)
            => new(name, GetTerm(name, [.. arguments.Select(a => a.ToString() ?? string.Empty)]));
    }
}
