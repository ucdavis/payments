using System.Collections.Generic;

namespace Payments.Mvc.Models.ChartStringApiViewModels
{
    public class ChartStringValidationResult
    {
        /// <summary>
        /// Whether the chart string passed validation.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// The resulting chart string, including any recharge adjustments.
        /// </summary>
        public string ChartString { get; set; }

        /// <summary>
        /// Validation warnings, including any chart-string adjustments.
        /// </summary>
        public List<KeyValuePair<string, string>> Warnings { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// Validation messages combined into a single string.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Individual validation messages.
        /// </summary>
        public List<string> Messages { get; set; } = new List<string>();
    }
}
