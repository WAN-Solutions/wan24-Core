using Microsoft.Extensions.Localization;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="IStringLocalizerFactory"/> (uses <see cref="Translation.Current"/>)
    /// </summary>
    public class StringLocalizerFactory : IStringLocalizerFactory
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public StringLocalizerFactory() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizer">String localizer</param>
        public StringLocalizerFactory(in IStringLocalizer localizer) => StringLocalizer = localizer;

        /// <summary>
        /// String localizer
        /// </summary>
        public IStringLocalizer? StringLocalizer { get; }

        /// <inheritdoc/>
        public virtual IStringLocalizer Create(Type resourceSource)
            => StringLocalizer ?? Translation.Current ?? throw new InvalidOperationException("No current translation");

        /// <inheritdoc/>
        public virtual IStringLocalizer Create(string baseName, string location)
            => StringLocalizer ?? Translation.Current ?? throw new InvalidOperationException("No current translation");
    }
}
