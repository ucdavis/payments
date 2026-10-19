using System;
using System.Threading.Tasks;
using AggieEnterpriseApi.Types;
using AggieEnterpriseApi.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.Validation;
using Payments.Core.Services;
using Payments.Mvc.Models.ChartStringApiViewModels;

namespace Payments.Mvc.Controllers
{
    [Route("api/chartstrings")]
    public class ChartStringsApiController : ApiController
    {
        private readonly IAggieEnterpriseService _aggieEnterpriseService;

        public ChartStringsApiController(ApplicationDbContext dbContext, IAggieEnterpriseService aggieEnterpriseService)
            : base(dbContext)
        {
            _aggieEnterpriseService = aggieEnterpriseService;
        }

        /// <summary>
        /// Validate an Aggie Enterprise GL or PPM chart string.
        /// </summary>
        /// <remarks>
        /// Supply the team's API key in the Authorization header. Missing, invalid, or inactive API keys
        /// return 401 Unauthorized without redirecting to login.
        ///
        /// With useRechargeValidation true, direction selects Credit or Debit recharge rules. Recharge
        /// validation replaces the natural account or expenditure type with 775000 for Credit or 770006
        /// for Debit. Use the returned chartString and review any warnings.
        ///
        /// With useRechargeValidation false, the existing Payments fund and revenue-account restrictions
        /// apply. Direction is required but does not affect validation in this mode.
        ///
        /// An evaluated chart string returns 200 with isValid indicating the result, including invalid
        /// or unsupported chart strings. KFS accounts are not supported. Missing or malformed request
        /// fields return 400.
        /// </remarks>
        /// <param name="model">The chart string, Credit or Debit direction, and recharge-validation flag.</param>
        /// <returns>Validity, the resulting chart string, warnings, and validation messages.</returns>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ChartStringValidationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ChartStringValidationResult>> Validate([FromBody] ValidateChartStringRequest model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest("A valid request body is required.");
            }

            if (string.IsNullOrWhiteSpace(model.ChartString))
            {
                return BadRequest("chartString is required.");
            }

            if (!string.Equals(model.Direction, nameof(RechargeAccount.CreditDebit.Credit), StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(model.Direction, nameof(RechargeAccount.CreditDebit.Debit), StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("direction must be Credit or Debit.");
            }

            if (!model.UseRechargeValidation.HasValue)
            {
                return BadRequest("useRechargeValidation is required.");
            }

            var chartType = FinancialChartValidation.GetFinancialChartStringType(model.ChartString);
            if (chartType != FinancialChartStringType.Gl && chartType != FinancialChartStringType.Ppm)
            {
                return ToApiResult(new AccountValidationModel
                {
                    IsValid = false,
                    ChartString = model.ChartString,
                    CoaChartType = FinancialChartStringType.Invalid,
                    Messages = { "Only Aggie Enterprise GL and PPM chart strings are supported." },
                });
            }

            var direction = string.Equals(model.Direction, nameof(RechargeAccount.CreditDebit.Credit), StringComparison.OrdinalIgnoreCase)
                ? RechargeAccount.CreditDebit.Credit
                : RechargeAccount.CreditDebit.Debit;

            var result = model.UseRechargeValidation.Value
                ? await _aggieEnterpriseService.IsRechargeAccountValid(model.ChartString, direction)
                : await _aggieEnterpriseService.IsAccountValid(model.ChartString);

            return ToApiResult(result);
        }

        private static ChartStringValidationResult ToApiResult(AccountValidationModel result)
        {
            return new ChartStringValidationResult
            {
                IsValid = result.IsValid,
                ChartString = result.ChartString,
                Warnings = result.Warnings,
                Message = result.Message,
                Messages = result.Messages,
            };
        }
    }
}
