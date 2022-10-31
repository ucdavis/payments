using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;
using AggieEnterpriseApi.Types;
using AggieEnterpriseApi.Validation;
using Microsoft.Extensions.Options;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.Validation;
using System.Threading.Tasks;


namespace Payments.Core.Services
{
    public interface IAggieEnterpriseService
    {
        Task<AccountValidationModel> IsAccountValid(string financialSegmentString, bool validateCVRs = true);
    }
    public class AggieEnterpriseService : IAggieEnterpriseService
    {
        private readonly IAggieEnterpriseClient _aggieClient;

        public AggieEnterpriseService(IOptions<AggieEnterpriseOptions> options)
        {
            _aggieClient = GraphQlClient.Get(options.Value.GraphQlUrl, options.Value.Token);
        }

        //TODO: Change this to return invalid reasons

        public async Task<AccountValidationModel> IsAccountValid(string financialSegmentString, bool validateCVRs = true)
        {
            var rtValue = new AccountValidationModel();

            var segmentStringType = FinancialChartValidation.GetFinancialChartStringType(financialSegmentString);

            if (segmentStringType == FinancialChartStringType.Gl)
            {
                var result = await _aggieClient.GlValidateChartstring.ExecuteAsync(financialSegmentString, validateCVRs);

                var data = result.ReadData();

                rtValue.IsValid = data.GlValidateChartstring.ValidationResponse.Valid;
                if (!rtValue.IsValid)
                {
                    foreach(var err in data.GlValidateChartstring.ValidationResponse.ErrorMessages)
                    {
                        rtValue.Messages.Add(err);
                    }
                }

                if (rtValue.IsValid)
                {
                    //Is fund valid?
                    var fund = data.GlValidateChartstring.Segments.Fund;
                    if ("13U00,13U01,13U02".Contains(fund) )//TODO: Make a configurable list of valid funds
                    {
                        //These three are excluded
                        rtValue.IsValid = false;
                        rtValue.Messages.Add("Fund is not valid. Can't be one of 13U00,13U01,13U02");
                    }
                    else 
                    {
                        var funds = await _aggieClient.FundParents.ExecuteAsync(fund);
                        var dataFunds = funds.ReadData();
                        if (DoesFundRollUp.Fund(dataFunds.ErpFund, 2, "1200C") || DoesFundRollUp.Fund(dataFunds.ErpFund, 2, "1300C") || DoesFundRollUp.Fund(dataFunds.ErpFund, 2, "5000C"))
                        {
                            //isValid = true; //Avoid setting to true if it might be false
                        }
                        else
                        {
                            rtValue.IsValid = false;
                            rtValue.Messages.Add("Fund is not valid. Must roll up to 1200C, 1300C, or 5000C");
                        }
                    }
                }
                if (rtValue.IsValid)
                {
                    //Does Natural Account roll up to 41000D or 44000D? (It can't be either of those values)
                    var naturalAcct = data.GlValidateChartstring.Segments.Account;
                    var accountParents = await _aggieClient.ErpAccountRollup.ExecuteAsync(naturalAcct);
                    var dataAccountParents = accountParents.ReadData();
                    if (DoesNaturalAccountRollUp.NaturalAccount(dataAccountParents.ErpAccount, "41000D") || DoesNaturalAccountRollUp.NaturalAccount(dataAccountParents.ErpAccount, "44000D"))
                    {
                        //isValid = true;
                    }
                    else
                    {
                        rtValue.IsValid = false;
                        rtValue.Messages.Add("Natural Account is not valid. Must roll up to 41000D or 44000D");
                    }
                }

                return rtValue;
            }

            if (segmentStringType == FinancialChartStringType.Ppm)
            {
                var result = await _aggieClient.PpmStringSegmentsValidate.ExecuteAsync(financialSegmentString);

                var data = result.ReadData();

                rtValue.IsValid = data.PpmStringSegmentsValidate.ValidationResponse.Valid;
                if (!rtValue.IsValid)
                {
                    foreach (var err in data.PpmStringSegmentsValidate.ValidationResponse.ErrorMessages)
                    {
                        rtValue.Messages.Add(err);
                    }
                }

                if (rtValue.IsValid)
                {
                    var ppmSegments = FinancialChartValidation.GetPpmSegments(financialSegmentString);
                    
                    var checkFundCode = await _aggieClient.PpmTaskByProjectNumberAndTaskNumber.ExecuteAsync(ppmSegments.Project, ppmSegments.Task);
                    var checkFundCodeData = checkFundCode.ReadData();
                    if (checkFundCodeData == null)
                    {
                        rtValue.IsValid = false;
                        rtValue.Messages.Add("Unable to check Task's funding code.");
                    }
                    else
                    {
                        var fundCode = checkFundCodeData.PpmTaskByProjectNumberAndTaskNumber.GlPostingFundCode;

                        if (fundCode == null)
                        {
                            rtValue.IsValid = false;
                            rtValue.Messages.Add("GlPostingFundCode is null for this Task.");
                        }
                        else
                        {

                            if ("13U00,13U01,13U02".Contains(fundCode))//TODO: Make a configurable list of valid funds
                            {
                                //These three are excluded
                                rtValue.IsValid = false;
                                rtValue.Messages.Add("GlPostingFundCode is not valid. Can't be one of 13U00,13U01,13U02");
                            }
                            else
                            {
                                var funds = await _aggieClient.FundParents.ExecuteAsync(fundCode);
                                var dataFunds = funds.ReadData();
                                if (DoesFundRollUp.Fund(dataFunds.ErpFund, 2, "1200C") || DoesFundRollUp.Fund(dataFunds.ErpFund, 2, "1300C") || DoesFundRollUp.Fund(dataFunds.ErpFund, 2, "5000C"))
                                {
                                    //isValid = true; //Avoid setting to true if it might be false
                                }
                                else
                                {
                                    rtValue.IsValid = false;
                                    rtValue.Messages.Add("GlPostingFundCode is not valid. Must roll up to 1200C, 1300C, or 5000C");
                                }
                            }
                        }

                        var ppmNaturalAccount = ppmSegments.ExpenditureType;

                        //Does Natural Account roll up to 41000D or 44000D? (It can't be either of those values)

                        var accountParents = await _aggieClient.ErpAccountRollup.ExecuteAsync(ppmNaturalAccount);
                        var dataAccountParents = accountParents.ReadData();
                        if (DoesNaturalAccountRollUp.NaturalAccount(dataAccountParents.ErpAccount, "41000D") || DoesNaturalAccountRollUp.NaturalAccount(dataAccountParents.ErpAccount, "44000D"))
                        {
                            //isValid = true;
                        }
                        else
                        {
                            rtValue.IsValid = false;
                            rtValue.Messages.Add("ExpenditureType (Natural Account) is not valid. Must roll up to 41000D or 44000D");
                        }


                    }
                }

                return rtValue;
            }

          

            return rtValue;
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
