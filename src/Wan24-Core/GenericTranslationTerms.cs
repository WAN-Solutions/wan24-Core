using Microsoft.Extensions.Localization;

namespace wan24.Core
{
    /// <summary>
    /// Generic translation terms
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="terms">Base translation terms</param>
    public class GenericTranslationTerms<T>(in ITranslationTerms terms) : IStringLocalizer<T>
    {
        /// <inheritdoc/>
        public virtual LocalizedString this[string name] => ((IStringLocalizer)Terms)[name];

        /// <inheritdoc/>
        public virtual LocalizedString this[string name, params object[] arguments] => ((IStringLocalizer)Terms)[name,arguments];

        /// <summary>
        /// Base translation terms
        /// </summary>
        public ITranslationTerms Terms { get; } = terms;

        /// <inheritdoc/>
        public virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Terms.GetAllStrings(includeParentCultures);
    }
}
