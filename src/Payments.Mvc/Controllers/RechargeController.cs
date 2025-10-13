using Microsoft.AspNetCore.Mvc;
using Payments.Core.Services;
using System.Threading.Tasks;
using static Payments.Core.Domain.RechargeAccount;

namespace Payments.Mvc.Controllers
{
    public class RechargeController : SuperController
    {
        private IAggieEnterpriseService _aggieEnterpriseService;

        public RechargeController(IAggieEnterpriseService aggieEnterpriseService) 
        {
            _aggieEnterpriseService = aggieEnterpriseService;
        }

        [HttpGet]
        [Route("api/recharge/validate")]
        public async Task<IActionResult> ValidateChartString(string chartString, CreditDebit direction)
        {
            var result = await _aggieEnterpriseService.IsRechargeAccountValid(chartString, direction);

            //This may not need to return the entire result object

            return new JsonResult(result);
        }

        public async Task<IActionResult> Pay(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}
