using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// Translation
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="terms">Terms</param>
    /// <param name="locale">Locale</param>
    public sealed class Translation(in IReadOnlyDictionary<string, string> terms, in string? locale = null)
    {
        /// <summary>
        /// Current translation
        /// </summary>
        private static Translation? _Current = null;

        /// <summary>
        /// Current translation
        /// </summary>
        public static Translation? Current
        {
            get => _Current;
            set
            {
                if (value == _Current) return;
                _Current = value;
                OnLocaleChanged?.Invoke(value, new());
            }
        }

        /// <summary>
        /// Empty dummy translation
        /// </summary>
        public static Translation Dummy { get; } = new(new Dictionary<string, string>(), "en-US");

        /// <summary>
        /// Available locales
        /// </summary>
        public static Dictionary<string, Translation> Locales { get; } = [];

        /// <summary>
        /// Require an existing translation?
        /// </summary>
        public static bool RequireTranslation { get; set; }

        /// <summary>
        /// String parser options
        /// </summary>
        public static StringParserOptions? ParserOptions { get; set; }

        /// <summary>
        /// Maximum recursive parser rounds
        /// </summary>
        public static int? MaxParserRounds { get; set; }

        /// <summary>
        /// Placeholder regular expression
        /// </summary>
        public static Regex? ParserRegex { get; set; }

        /// <summary>
        /// Regular expression group to use
        /// </summary>
        public static int? ParserRegexGroup { get; set; }

        /// <summary>
        /// Get a translation
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="values">Values to parse</param>
        /// <returns>Translated term</returns>
        public string this[in string key, params string[] values]
        {
            get
            {
                if (!Terms.TryGetValue(key, out string? term))
                {
                    if (RequireTranslation) throw new ArgumentException("Translation not found", nameof(key));
                    term = key;
                }
                if (values.Length != 0)
                {
                    int len = values.Length;
                    Dictionary<string, string> valuesDict = new(len);
                    for (int i = 0; i < len; valuesDict[i.ToString()] = values[i], i++) ;
                    term = term.Parse(valuesDict, ParserOptions);
                }
                return term;
            }
        }

        /// <summary>
        /// Get a translation
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="values">Values to parse</param>
        /// <returns>Translated term</returns>
        public string this[in string key, in Dictionary<string, string> values]
        {
            get
            {
                if (!Terms.TryGetValue(key, out string? term))
                {
                    if (RequireTranslation) throw new ArgumentException("Translation not found", nameof(key));
                    term = key;
                }
                if (values.Count != 0) term = term.Parse(values, ParserOptions);
                return term;
            }
        }

        /// <summary>
        /// Terms
        /// </summary>
        public IReadOnlyDictionary<string, string> Terms { get; } = terms;

        /// <summary>
        /// Translation terms
        /// </summary>
        public ITranslationTerms? TranslationTerms { get; } = terms as ITranslationTerms;

        /// <summary>
        /// Locale
        /// </summary>
        [RegularExpression(RegularExpressions.LOCALE_WITH_DASH)]
        public string? Locale { get; } = locale;

        /// <summary>
        /// Does support plural?
        /// </summary>
        public bool PluralSupport
        {
            [MemberNotNullWhen(returnValue: true, nameof(TranslationTerms))]
            get => TranslationTerms?.PluralSupport ?? false;
        }

        /// <summary>
        /// Translate a plural term
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="count">Count (for plural translation)</param>
        /// <param name="values">Values to parse</param>
        /// <returns>Translated term</returns>
        public string TranslatePlural(in string key, in int count, params string[] values)
        {
            if (!PluralSupport) throw new NotSupportedException();
            if (RequireTranslation && !TranslationTerms.ContainsKey(key)) throw new ArgumentException("Translation not found", nameof(key));
            string term = TranslationTerms.GetTerm(key, count);
            if (values.Length != 0)
            {
                int len = values.Length;
                Dictionary<string, string> valuesDict = new(len);
                for (int i = 0; i < len; valuesDict[i.ToString()] = values[i], i++) ;
                term = term.Parse(valuesDict, ParserOptions);
            }
            return term;
        }

        /// <summary>
        /// Translate a plural term
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="count">Count (for plural translation)</param>
        /// <param name="values">Values to parse</param>
        /// <returns>Translated term</returns>
        public string TranslatePlural(in string key, in int count, Dictionary<string, string> values)
        {
            if (!PluralSupport) throw new NotSupportedException();
            if (RequireTranslation && !TranslationTerms.ContainsKey(key)) throw new ArgumentException("Translation not found", nameof(key));
            string term = TranslationTerms.GetTerm(key, count);
            if (values.Count != 0) term = term.Parse(values, ParserOptions);
            return term;
        }

        /// <summary>
        /// Translate a term
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="values">Values to parse</param>
        /// <returns>Translated term</returns>
        public static string Translate(in string key, params string[] values)
        {
            if (Current is null) throw new InvalidOperationException("Missing current locale");
            return Current[key, values];
        }

        /// <summary>
        /// Translate a term
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="values">Values to parse</param>
        /// <returns>Translated term</returns>
        public static string Translate(in string key, in Dictionary<string, string> values)
        {
            if (Current is null) throw new InvalidOperationException("Missing current locale");
            return Current[key, values];
        }

        /// <summary>
        /// Translate a term
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="count">Count (for plural translation)</param>
        /// <param name="values">Values to parse</param>
        /// <returns>Translated term</returns>
        public static string Translate(in string key, in int count, params string[] values)
        {
            if (Current is null) throw new InvalidOperationException("Missing current locale");
            return Current.TranslatePlural(key, count, values);
        }

        /// <summary>
        /// Translate a term
        /// </summary>
        /// <param name="key">Term key</param>
        /// <param name="count">Count (for plural translation)</param>
        /// <param name="values">Values to parse</param>
        /// <returns>Translated term</returns>
        public static string Translate(in string key, in int count, in Dictionary<string, string> values)
        {
            if (Current is null) throw new InvalidOperationException("Missing current locale");
            return Current.TranslatePlural(key, count, values);
        }

        /// <summary>
        /// Translate a term
        /// </summary>
        /// <param name="locale">Locale</param>
        /// <param name="key">Term key</param>
        /// <param name="values">Values to parse</param>
        /// <returns>Translated term</returns>
        public static string Localize(in string locale, in string key, params string[] values)
        {
            if (Locales.TryGetValue(locale, out Translation? translation)) return translation[key, values];
            if (RequireTranslation) throw new ArgumentException("Unknown locale", nameof(locale));
            string term = key;
            if (values.Length != 0)
            {
                int len = values.Length;
                Dictionary<string, string> valuesDict = new(len);
                for (int i = 0; i < len; valuesDict[i.ToString()] = values[i], i++) ;
                term = term.Parse(valuesDict, ParserOptions);
            }
            return term;
        }

        /// <summary>
        /// Translate a term
        /// </summary>
        /// <param name="locale">Locale</param>
        /// <param name="key">Term key</param>
        /// <param name="values">Values to parse</param>
        /// <returns>Translated term</returns>
        public static string Localize(in string locale, in string key, in Dictionary<string, string> values)
        {
            if (Locales.TryGetValue(locale, out Translation? translation)) return translation[key, values];
            if (RequireTranslation) throw new ArgumentException("Unknown locale", nameof(locale));
            string term = key;
            if (values.Count != 0) term = term.Parse(values, ParserOptions);
            return term;
        }

        /// <summary>
        /// Translate a term
        /// </summary>
        /// <param name="locale">Locale</param>
        /// <param name="key">Term key</param>
        /// <param name="count">Count (for plural translation)</param>
        /// <param name="values">Values to parse</param>
        /// <returns>Translated term</returns>
        public static string Localize(in string locale, in string key, in int count, params string[] values)
        {
            if (!Locales.TryGetValue(locale, out Translation? translation)) throw new ArgumentException("Unknown locale", nameof(locale));
            return translation.TranslatePlural(key, count, values);
        }

        /// <summary>
        /// Translate a term
        /// </summary>
        /// <param name="locale">Locale</param>
        /// <param name="key">Term key</param>
        /// <param name="count">Count (for plural translation)</param>
        /// <param name="values">Values to parse</param>
        /// <returns>Translated term</returns>
        public static string Localize(in string locale, in string key, in int count, in Dictionary<string, string> values)
        {
            if (!Locales.TryGetValue(locale, out Translation? translation)) throw new ArgumentException("Unknown locale", nameof(locale));
            return translation.TranslatePlural(key, count, values);
        }

        /// <summary>
        /// Get a localized filename (<c>filename.ext</c> -&gt; <c>filename.en-EN.ext</c>)
        /// </summary>
        /// <param name="locale">Locale</param>
        /// <param name="fileName">Filename</param>
        /// <param name="checkExists">Check if the file exists?</param>
        /// <returns>Localized filename (may be the given filename, if the locale is unknown or the file doesn't exist)</returns>
        /// <exception cref="FileNotFoundException">File not found</exception>
        public static string LocalizedFileName(in string locale, in string fileName, in bool checkExists = true)
        {
            if (!EnsureValidLocale(locale, throwOnError: RequireTranslation)) return fileName;
            string res = $"{Path.GetFileNameWithoutExtension(fileName)}.{locale}.{Path.GetExtension(fileName)}";
            if (checkExists && !File.Exists(res))
            {
                if (RequireTranslation) throw new FileNotFoundException(res);
                return fileName;
            }
            return res;
        }

        /// <summary>
        /// Ensure a valid locale
        /// </summary>
        /// <param name="locale">Locale</param>
        /// <param name="checkExists">Check if the locale exists?</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>If the locale is valid (and exists)</returns>
        public static bool EnsureValidLocale(in string locale, in bool checkExists = true, in bool throwOnError = true)
        {
            if (RegularExpressions.RX_LOCALE_WITH_DASH.IsMatch(locale))
            {
                if (!checkExists || Locales.ContainsKey(locale)) return true;
                if (!throwOnError) return false;
                throw new ArgumentException($"Unknown locale \"{locale}\"", nameof(locale));
            }
            if (!throwOnError) return false;
            throw new ArgumentException("Invalid locale", nameof(locale));
        }

        /// <summary>
        /// Delegate for an <see cref="OnLocaleChanged"/> event handler
        /// </summary>
        /// <param name="translation">New translation</param>
        /// <param name="e">Arguments</param>
        public delegate void LocaleChanged_Delegate(Translation? translation, EventArgs e);
        /// <summary>
        /// Raised when the <see cref="Current"/> locale changed
        /// </summary>
        public static event LocaleChanged_Delegate? OnLocaleChanged;
    }
}
