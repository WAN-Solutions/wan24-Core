using Microsoft.Extensions.Localization;

namespace wan24.Core
{
    /// <summary>
    /// Generic translation
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="translation">Base translation</param>
    public class GenericTranslation<T>(in Translation? translation = null) : IStringLocalizer<T>
    {
        /// <inheritdoc/>
        public virtual LocalizedString this[string name] => ((IStringLocalizer)Translation)[name];

        /// <inheritdoc/>
        public virtual LocalizedString this[string name, params object[] arguments] => ((IStringLocalizer)Translation)[name,arguments];

        /// <summary>
        /// Base translation
        /// </summary>
        public Translation Translation { get; } = translation ?? Translation.Current ?? throw new InvalidOperationException("No current translation");

        /// <inheritdoc/>
        public virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => ((IStringLocalizer)Translation).GetAllStrings(includeParentCultures);
    }
}
