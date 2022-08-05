using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;
using AggieEnterpriseApi.Types;
using AggieEnterpriseApi.Validation;
using Microsoft.Extensions.Options;
using Payments.Core.Models.Configuration;
using System.Threading.Tasks;


namespace Payments.Core.Services
{
    public interface IAggieEnterpriseService
    {
        Task<bool> IsAccountValid(string financialSegmentString, bool validateCVRs = true);
    }
    public class AggieEnterpriseService : IAggieEnterpriseService
    {
        private readonly IAggieEnterpriseClient _aggieClient;

        public AggieEnterpriseService(IOptions<AggieEnterpriseOptions> options)
        {
            _aggieClient = GraphQlClient.Get(options.Value.GraphQlUrl, options.Value.Token);
        }

        public async Task<bool> IsAccountValid(string financialSegmentString, bool validateCVRs = true)
        {
            var segmentStringType = FinancialChartValidation.GetFinancialChartStringType(financialSegmentString);

            if (segmentStringType == FinancialChartStringType.Gl)
            {
                var result = await _aggieClient.GlValidateChartstring.ExecuteAsync(financialSegmentString, validateCVRs);

                var data = result.ReadData();

                var isValid = data.GlValidateChartstring.ValidationResponse.Valid;

                if (isValid)
                {
                    //Is fund valid?
                    var fund = data.GlValidateChartstring.Segments.Fund;
                    if ("13U00,13U01,13U02".Contains(fund) )//TODO: Make a configurable list of valid funds
                    {
                        //These three are excluded
                        isValid = false;                       
                    }
                    else 
                    {
                        var funds = await _aggieClient.FundParents.ExecuteAsync(fund);
                        var dataFunds = funds.ReadData();
                        if (DoesFundRollUp.Fund(dataFunds.ErpFund, 2, "1200C") || DoesFundRollUp.Fund(dataFunds.ErpFund, 2, "1300C") || DoesFundRollUp.Fund(dataFunds.ErpFund, 2, "5000C"))
                        {
                            isValid = true;
                        }
                        else
                        {
                            isValid = false;
                        }
                    }
                }

                return isValid;
            }

            if (segmentStringType == FinancialChartStringType.Ppm)
            {
                // there is no validate ppm string, but we can validate by segments
                var segments = FinancialChartValidation.GetPpmSegments(financialSegmentString);

                var result = await _aggieClient.PpmSegmentsValidate.ExecuteAsync(ConvertToPpmSegmentInput(segments), accountingDate: null);

                var data = result.ReadData();

                var isValid = data.PpmSegmentsValidate.ValidationResponse.Valid;

                //TODO: Extra validation for PPM strings?

                return isValid;
            }

          

            return false;
        }

        private PpmSegmentInput ConvertToPpmSegmentInput(PpmSegments segments)
        {
            return new PpmSegmentInput
            {
                Award = segments.Award,
                Organization = segments.Organization,
                Project = segments.Project,
                Task = segments.Task,
                ExpenditureType = segments.ExpenditureType,
                FundingSource = segments.FundingSource,
            };
        }
    }
}
