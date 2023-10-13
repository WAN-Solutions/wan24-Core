using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Translation helper (use <c>using static wan24.Core.TranslationHelper;</c> for getting <c>_</c> as translation method, which will help a POEdit parser to find translation 
    /// texts in your source code)
    /// </summary>
    public static class TranslationHelper
    {
        /// <summary>
        /// Translate
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="values">Parser values</param>
        /// <returns>Translated (and parsed) string</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string _(string str, params string[] values) => Translation.Translate(str, values);

        /// <summary>
        /// Translate
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="values">Parser values</param>
        /// <returns>Translated (and parsed) string</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string _(string str, in Dictionary<string, string> values) => Translation.Translate(str, values);

        /// <summary>
        /// Translate
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="count">Count (for plural translation)</param>
        /// <param name="values">Parser values</param>
        /// <returns>Translated (and parsed) string</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string _(string str, in int count, params string[] values) => Translation.Translate(str, count, values);

        /// <summary>
        /// Translate
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="count">Count (for plural translation)</param>
        /// <param name="values">Parser values</param>
        /// <returns>Translated (and parsed) string</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string _(string str, in int count, in Dictionary<string, string> values) => Translation.Translate(str, count, values);
    }
}
