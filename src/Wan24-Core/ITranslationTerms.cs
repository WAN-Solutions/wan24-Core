using Microsoft.Extensions.Localization;

//TODO Implement IHtmlLocalizer wrapper which HTML encodes parser values (but not the term)

namespace wan24.Core
{
    /// <summary>
    /// Interface for a translation
    /// </summary>
    public interface ITranslationTerms : IReadOnlyDictionary<string, string>, IStringLocalizer
    {
        /// <summary>
        /// Get a term
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="values">Parser values</param>
        /// <returns>Term</returns>
        string this[in string key, params string[] values] { get; }
        /// <summary>
        /// Get a term
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="count">Count (for plural translation)</param>
        /// <param name="values">Parser values</param>
        /// <returns>Term</returns>
        /// <exception cref="NotSupportedException">Plural terms are not supported</exception>
        string this[in string key, in int count, params string[] values] { get; }
        /// <summary>
        /// Does support plural?
        /// </summary>
        bool PluralSupport { get; }
        /// <summary>
        /// Get a term
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="values">Parser values</param>
        /// <returns>Term</returns>
        string GetTerm(in string key, params string[] values);
        /// <summary>
        /// Get a term
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="count">Count (for plural translation)</param>
        /// <param name="values">Parser values</param>
        /// <returns>Term</returns>
        string GetTerm(in string key, in int count, params string[] values);
    }
}
