namespace wan24.Core
{
    /// <summary>
    /// Interface for a translation
    /// </summary>
    public interface ITranslationTerms : IReadOnlyDictionary<string, string>
    {
        /// <summary>
        /// Get a term
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="count">Count (for plural translation)</param>
        /// <returns>Term</returns>
        string this[in string key, in int count] { get; }
        /// <summary>
        /// Does support plural?
        /// </summary>
        bool PluralSupport { get; }
        /// <summary>
        /// Get a term
        /// </summary>
        /// <param name="key">Term key</param>
        /// <returns>Term</returns>
        string GetTerm(in string key);
        /// <summary>
        /// Get a term
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="count">Count (for plural translation)</param>
        /// <returns>Term</returns>
        string GetTerm(in string key, in int count);
    }
}
