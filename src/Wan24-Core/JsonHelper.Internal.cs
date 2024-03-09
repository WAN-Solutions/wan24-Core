using System.Text.Json;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    // Internal
    public static partial class JsonHelper
    {
        /// <summary>
        /// Non-intending options
        /// </summary>
        private static readonly JsonSerializerOptions NotIntendedOptions = new();
        /// <summary>
        /// Intending options
        /// </summary>
        private static readonly JsonSerializerOptions IntendedOptions = new()
        {
            WriteIndented = true
        };
        /// <summary>
        /// Decoder options
        /// </summary>
        private static readonly JsonSerializerOptions DecoderOptions;
        /// <summary>
        /// Regular expression to match a JSON integer value
        /// </summary>
        private static readonly Regex RxJsonInt = RxJsonInt_Generator();
        /// <summary>
        /// Regular expression to match a JSON float value
        /// </summary>
        private static readonly Regex RxJsonFloat = RxJsonFloat_Generator();
        /// <summary>
        /// Regular expression to match a JSON string value
        /// </summary>
        private static readonly Regex RxJsonString = RxJsonString_Generator();
        /// <summary>
        /// Regular expression to match a JSON object value
        /// </summary>
        private static readonly Regex RxJsonObject = RxJsonObject_Generator();
        /// <summary>
        /// Regular expression to match a JSON array value
        /// </summary>
        private static readonly Regex RxJsonArray = RxJsonArray_Generator();
    }
}
