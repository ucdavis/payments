using System.ComponentModel.DataAnnotations;

namespace Payments.Mvc.Models.ChartStringApiViewModels
{
    public class ValidateChartStringRequest
    {
        /// <summary>
        /// The Aggie Enterprise GL or PPM chart string to validate. KFS accounts are not supported.
        /// </summary>
        [Required]
        public string ChartString { get; set; }

        /// <summary>
        /// Credit or Debit (case-insensitive). Required in both modes; affects recharge validation only.
        /// </summary>
        /// <example>Debit</example>
        [Required]
        [RegularExpression("^([Cc][Rr][Ee][Dd][Ii][Tt]|[Dd][Ee][Bb][Ii][Tt])$")]
        public string Direction { get; set; }

        /// <summary>
        /// True applies recharge validation; false applies the existing Payments fund and revenue-account rules.
        /// </summary>
        /// <example>true</example>
        [Required]
        public bool? UseRechargeValidation { get; set; }
    }
}
