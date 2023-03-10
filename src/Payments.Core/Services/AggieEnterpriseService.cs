using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;
using AggieEnterpriseApi.Types;
using AggieEnterpriseApi.Validation;
using Microsoft.Extensions.Options;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.Validation;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Payments.Core.Services
{
    public interface IAggieEnterpriseService
    {
        Task<AccountValidationModel> IsAccountValid(string financialSegmentString, bool validateCVRs = true);
    }
    public class AggieEnterpriseService : IAggieEnterpriseService
    {
        private readonly AggieEnterpriseOptions _aeSettings;
        private IAggieEnterpriseClient _aggieClient;

        public AggieEnterpriseService(IOptions<AggieEnterpriseOptions> aeSettings)
        {
            _aeSettings = aeSettings.Value;
            //_aggieClient = GraphQlClient.Get(options.Value.GraphQlUrl, options.Value.Token);
            _aggieClient = GraphQlClient.Get(_aeSettings.GraphQlUrl, _aeSettings.TokenEndpoint, _aeSettings.ConsumerKey, _aeSettings.ConsumerSecret, $"{_aeSettings.ScopeApp}-{_aeSettings.ScopeEnv}");
            
        }

        //TODO: Change this to return invalid reasons

        public async Task<AccountValidationModel> IsAccountValid(string financialSegmentString, bool validateCVRs = true)
        {
            var rtValue = new AccountValidationModel();
            if (string.IsNullOrWhiteSpace(financialSegmentString))
            {
                rtValue.IsValid = false;
                rtValue.Messages.Add("Invalid Financial Chart String format");
                rtValue.CoaChartType = FinancialChartStringType.Invalid;
                return rtValue;
            }

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
                rtValue.Details.Add(new KeyValuePair<string, string>("Entity", $"{data.GlValidateChartstring.SegmentNames.EntityName} ({data.GlValidateChartstring.Segments.Entity})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Fund", $"{data.GlValidateChartstring.SegmentNames.FundName} ({data.GlValidateChartstring.Segments.Fund})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Department", $"{data.GlValidateChartstring.SegmentNames.DepartmentName} ({data.GlValidateChartstring.Segments.Department})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Account", $"{data.GlValidateChartstring.SegmentNames.AccountName} ({data.GlValidateChartstring.Segments.Account})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Purpose", $"{data.GlValidateChartstring.SegmentNames.PurposeName} ({data.GlValidateChartstring.Segments.Purpose})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Project", $"{data.GlValidateChartstring.SegmentNames.ProjectName} ({data.GlValidateChartstring.Segments.Project})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Program", $"{data.GlValidateChartstring.SegmentNames.ProgramName} ({data.GlValidateChartstring.Segments.Program})"));
                rtValue.Details.Add(new KeyValuePair<string, string>("Activity", $"{data.GlValidateChartstring.SegmentNames.ActivityName} ({data.GlValidateChartstring.Segments.Activity})"));

                if (data.GlValidateChartstring.Warnings != null)
                {
                    foreach (var warn in data.GlValidateChartstring.Warnings)
                    {
                        rtValue.Warnings.Add(new KeyValuePair<string, string>(warn.SegmentName, warn.Warning));
                    }
                }

                rtValue.GlSegments = FinancialChartValidation.GetGlSegments(financialSegmentString);

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
#if DEBUG
                //Just for testing
                //rtValue.Warnings.Add(new KeyValuePair<string, string>("Fund", "Fund is not valid. Must roll up to 1200C, 1300C, or 5000C"));
                //rtValue.Warnings.Add(new KeyValuePair<string, string>("asdfasd", "Fund is not valid. Must roll up to 1200C, 1300C, or 5000C"));
                //rtValue.Warnings.Add(new KeyValuePair<string, string>("Fusadfsadfnd", "Fund is not valid. Must roll up to 1200C, 1300C, or 5000C"));
#endif
                
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

                rtValue.Details.Add(new KeyValuePair<string, string>("Project", data.PpmStringSegmentsValidate.Segments.Project));
                rtValue.Details.Add(new KeyValuePair<string, string>("Task", data.PpmStringSegmentsValidate.Segments.Task));
                rtValue.Details.Add(new KeyValuePair<string, string>("Organization", data.PpmStringSegmentsValidate.Segments.Organization));
                rtValue.Details.Add(new KeyValuePair<string, string>("Expenditure Type", data.PpmStringSegmentsValidate.Segments.ExpenditureType));
                rtValue.Details.Add(new KeyValuePair<string, string>("Award", data.PpmStringSegmentsValidate.Segments.Award));
                rtValue.Details.Add(new KeyValuePair<string, string>("Funding Source", data.PpmStringSegmentsValidate.Segments.FundingSource));

                if (data.PpmStringSegmentsValidate.Warnings != null)
                {
                    foreach (var warn in data.PpmStringSegmentsValidate.Warnings)
                    {
                        rtValue.Warnings.Add(new KeyValuePair<string, string>(warn.SegmentName, warn.Warning));
                    }
                }

                rtValue.PpmSegments = FinancialChartValidation.GetPpmSegments(financialSegmentString);

                await GetPpmAccountManager(rtValue);

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

            if (segmentStringType == FinancialChartStringType.Invalid)
            {
                rtValue.IsValid = false;
                rtValue.Messages.Add("Invalid Financial Chart String format");
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

        private async Task GetPpmAccountManager(AccountValidationModel rtValue)
        {
            var result = await _aggieClient.PpmProjectManager.ExecuteAsync(rtValue.PpmSegments.Project);

            var data = result.ReadData();

            if (data.PpmProjectByNumber?.ProjectNumber == rtValue.PpmSegments.Project)
            {
                rtValue.AccountManager = data.PpmProjectByNumber.PrimaryProjectManagerName;
                rtValue.AccountManagerEmail = data.PpmProjectByNumber.PrimaryProjectManagerEmail;
            }
            return;
        }
    }
}
