using Payments.Core.Domain;
using Payments.Core.Models.Validation;

namespace Payments.Mvc.Models.FinancialModels
{
    public class FinancialAccountEditModel
    {
        public FinancialAccount FinancialAccount { get; set; }
        public AccountValidationModel AeValidationModel { get; set; } //Has details to display to user

        public bool ShowCoa { get; set; }
        public bool UseCoa { get; set; }
    }
}
