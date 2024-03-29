﻿using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Translation helper (use <c>using static wan24.Core.TranslationHelper.Ext;</c> for getting <c>_</c> as translation method, which will help a keyword extractor to find 
    /// translation texts in your source code)
    /// </summary>
    public static class TranslationHelper
    {
        /// <summary>
        /// Dummy translate method for a keyword extractor
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static string __(string str) => str;

        /// <summary>
        /// Extended helper (NOTE: using these will disable the discard operator <c>_</c> of C#)
        /// </summary>
        public static class Ext
        {
            /// <summary>
            /// Translate
            /// </summary>
            /// <param name="str">String</param>
            /// <param name="values">Parser values</param>
            /// <returns>Translated (and parsed) string</returns>
            [TargetedPatchingOptOut("Just a method adapter")]
            public static string _(string str, params string[] values) => Translation.Translate(str, values);

            /// <summary>
            /// Translate
            /// </summary>
            /// <param name="str">String</param>
            /// <param name="values">Parser values</param>
            /// <returns>Translated (and parsed) string</returns>
            [TargetedPatchingOptOut("Just a method adapter")]
            public static string _(string str, in Dictionary<string, string> values) => Translation.Translate(str, values);

            /// <summary>
            /// Translate
            /// </summary>
            /// <param name="str">String</param>
            /// <param name="count">Count (for plural translation)</param>
            /// <param name="values">Parser values</param>
            /// <returns>Translated (and parsed) string</returns>
            [TargetedPatchingOptOut("Just a method adapter")]
            public static string _(string str, in int count, params string[] values) => Translation.Translate(str, count, values);

            /// <summary>
            /// Translate
            /// </summary>
            /// <param name="str">String</param>
            /// <param name="count">Count (for plural translation)</param>
            /// <param name="values">Parser values</param>
            /// <returns>Translated (and parsed) string</returns>
            [TargetedPatchingOptOut("Just a method adapter")]
            public static string _(string str, in int count, in Dictionary<string, string> values) => Translation.Translate(str, count, values);
        }
    }
}
