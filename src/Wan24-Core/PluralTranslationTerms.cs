using Microsoft.Extensions.Localization;
using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Simple dictionary based translation with plural support
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="terms">Terms</param>
    public class PluralTranslationTerms(IReadOnlyDictionary<string, string[]> terms) : ITranslationTerms
    {
        /// <summary>
        /// Terms
        /// </summary>
        public virtual FrozenDictionary<string, string[]> Terms { get; } = terms as FrozenDictionary<string, string[]> ?? terms.ToFrozenDictionary();

        /// <inheritdoc/>
        public virtual bool PluralSupport => true;

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
        public virtual IEnumerable<string> Values => Terms.Values.Select(v => v.Length > 0 ? v[0] : string.Empty);

        /// <inheritdoc/>
        public virtual int Count => Terms.Count;

        /// <inheritdoc/>
        public virtual string GetTerm(in string key, params string[] values)
            => TranslationTerms.ParseTerm(TryGetValue(key, out string? term) ? term : key, values);

        /// <inheritdoc/>
        public virtual string GetTerm(in string key, in int count, params string[] values)
        {
            string[] terms = Terms.TryGetValue(key, out string[]? term) ? term : [key];
            int plural = Math.Abs(count);
            string res = plural >= terms.Length
                ? terms.Length > 0
                    ? terms[^1]
                    : string.Empty
                : terms[plural];
            return TranslationTerms.ParseTerm(res, values);
        }

        /// <inheritdoc/>
        public virtual bool ContainsKey(string key) => Terms.ContainsKey(key);

        /// <inheritdoc/>
        public virtual bool TryGetValue(string key, [MaybeNullWhen(returnValue: false)] out string value)
        {
            value = Terms.TryGetValue(key, out string[]? terms)
                ? terms.Length > 0
                    ? terms[0]
                    : string.Empty
                : null;
            return value is not null;
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            => Terms.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.Length > 0 ? kvp.Value[0] : string.Empty)).GetEnumerator();

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
            => new(name, GetTerm(name, arguments.Length > 0 ? [.. arguments.Select(a => a.ToString() ?? string.Empty)] : []));
    }
}
