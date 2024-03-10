using Microsoft.Extensions.Localization;
using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Simple dictionary based translation
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="terms">Terms</param>
    public class TranslationTerms(IReadOnlyDictionary<string, string> terms) : ITranslationTerms
    {
        /// <summary>
        /// Terms
        /// </summary>
        public virtual FrozenDictionary<string, string> Terms { get; } = terms as FrozenDictionary<string, string> ?? terms.ToFrozenDictionary();

        /// <inheritdoc/>
        public virtual bool PluralSupport => false;

        /// <inheritdoc/>
        public virtual string this[in string key, params string[] values] => GetTerm(key, values);

        /// <inheritdoc/>
        public virtual string this[in string key, in int count, params string[] values] => GetTerm(key, count, values);

        /// <inheritdoc/>
        string IReadOnlyDictionary<string, string>.this[string key] => this[key];

        /// <inheritdoc/>
        LocalizedString IStringLocalizer.this[string name, params object[] arguments] => StringLocalizer(name, arguments);

        /// <inheritdoc/>
        LocalizedString IStringLocalizer.this[string name] => StringLocalizer(name, []);

        /// <inheritdoc/>
        public virtual IEnumerable<string> Keys => Terms.Keys;

        /// <inheritdoc/>
        public virtual IEnumerable<string> Values => Terms.Values;

        /// <inheritdoc/>
        public virtual int Count => Terms.Count;

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
            => PluralSupport ? throw new NotImplementedException() : throw new NotSupportedException();

        /// <inheritdoc/>
        public virtual bool ContainsKey(string key) => Terms.ContainsKey(key);

        /// <inheritdoc/>
        public virtual bool TryGetValue(string key, [MaybeNullWhen(returnValue: false)] out string value) => Terms.TryGetValue(key, out value);

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => Terms.GetEnumerator();

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
