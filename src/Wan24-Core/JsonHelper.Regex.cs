using System.Text.RegularExpressions;

namespace wan24.Core
{
    // Regular expressions
    public static partial class JsonHelper
    {
        /// <summary>
        /// Regular expression to match a JSON integer value
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"^\s*\d+\s*$", RegexOptions.Singleline | RegexOptions.Compiled, 3000)]
        private static partial Regex RxJsonInt_Generator();

        /// <summary>
        /// Regular expression to match a JSON float value
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"^\s*\d+(\.\d+)?\s*$", RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RxJsonFloat_Generator();

        /// <summary>
        /// Regular expression to match a JSON string value
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex("^\\s*\\\"[^\\n]*\\\"\\s*$", RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RxJsonString_Generator();

        /// <summary>
        /// Regular expression to match a JSON object value
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"^\s*\{.*\}\s*$", RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RxJsonObject_Generator();

        /// <summary>
        /// Regular expression to match a JSON array value
        /// </summary>
        /// <returns>Regular expression</returns>
        [GeneratedRegex(@"^\s*\[.*\]\s*$", RegexOptions.Compiled | RegexOptions.Singleline, 3000)]
        private static partial Regex RxJsonArray_Generator();
    }
}
