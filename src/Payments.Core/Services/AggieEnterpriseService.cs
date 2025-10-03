using AggieEnterpriseApi;
using AggieEnterpriseApi.Extensions;
using AggieEnterpriseApi.Types;
using AggieEnterpriseApi.Validation;
using Microsoft.Extensions.Options;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.Validation;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Payments.Core.Domain.RechargeAccount;


namespace Payments.Core.Services
{
    public interface IAggieEnterpriseService
    {
        Task<AccountValidationModel> IsAccountValid(string financialSegmentString, bool validateCVRs = true);

        Task<AccountValidationModel> IsRechargeAccountValid(string financialSegmentString, CreditDebit direction, bool validateCVRs = true);
    }
    public class AggieEnterpriseService : IAggieEnterpriseService
    {
        private readonly AggieEnterpriseOptions _aeSettings;
        private IAggieEnterpriseClient _aggieClient;
        private const string FiscalOfficer = "Fiscal Officer Approver";

        public AggieEnterpriseService(IOptions<AggieEnterpriseOptions> aeSettings)
        {
            _aeSettings = aeSettings.Value;
            //_aggieClient = GraphQlClient.Get(options.Value.GraphQlUrl, options.Value.Token);
            _aggieClient = GraphQlClient.Get(_aeSettings.GraphQlUrl, _aeSettings.TokenEndpoint, _aeSettings.ConsumerKey, _aeSettings.ConsumerSecret, $"{_aeSettings.ScopeApp}-{_aeSettings.ScopeEnv}");
            
        } 

        //TODO: Change this to return invalid reasons

        private async Task<AccountValidationModel> CommonAccountValidation(AccountValidationModel accountValidationModel, string financialSegmentString, bool validateCVRs = true)
        {            
            if (string.IsNullOrWhiteSpace(financialSegmentString))
            {
                accountValidationModel.IsValid = false;
                accountValidationModel.Messages.Add("Invalid Financial Chart String format");
                accountValidationModel.CoaChartType = FinancialChartStringType.Invalid;
                return accountValidationModel;
            }
            accountValidationModel.CoaChartType = FinancialChartValidation.GetFinancialChartStringType(financialSegmentString);
            accountValidationModel.ChartString = financialSegmentString; //We may replace natural account later.

            if (accountValidationModel.CoaChartType == FinancialChartStringType.Invalid)
            {
                accountValidationModel.CoaChartType = FinancialChartStringType.Invalid;
                accountValidationModel.IsValid = false;
                accountValidationModel.Messages.Add("Invalid Financial Chart String format");
                return accountValidationModel;
            }

            if(accountValidationModel.CoaChartType == FinancialChartStringType.Gl)
            {
                accountValidationModel.GlSegments = FinancialChartValidation.GetGlSegments(financialSegmentString);

                var result = await _aggieClient.GlValidateChartstring.ExecuteAsync(financialSegmentString, validateCVRs);

                var data = result.ReadData();

                accountValidationModel.IsValid = data.GlValidateChartstring.ValidationResponse.Valid;
                if (!accountValidationModel.IsValid)
                {
                    foreach (var err in data.GlValidateChartstring.ValidationResponse.ErrorMessages)
                    {
                        accountValidationModel.Messages.Add(err);
                    }
                }
                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Entity", $"{data.GlValidateChartstring.SegmentNames.EntityName} ({data.GlValidateChartstring.Segments.Entity})"));
                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Fund", $"{data.GlValidateChartstring.SegmentNames.FundName} ({data.GlValidateChartstring.Segments.Fund})"));
                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Department", $"{data.GlValidateChartstring.SegmentNames.DepartmentName} ({data.GlValidateChartstring.Segments.Department})"));
                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Account", $"{data.GlValidateChartstring.SegmentNames.AccountName} ({data.GlValidateChartstring.Segments.Account})"));
                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Purpose", $"{data.GlValidateChartstring.SegmentNames.PurposeName} ({data.GlValidateChartstring.Segments.Purpose})"));
                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Project", $"{data.GlValidateChartstring.SegmentNames.ProjectName} ({data.GlValidateChartstring.Segments.Project})"));
                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Program", $"{data.GlValidateChartstring.SegmentNames.ProgramName} ({data.GlValidateChartstring.Segments.Program})"));
                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Activity", $"{data.GlValidateChartstring.SegmentNames.ActivityName} ({data.GlValidateChartstring.Segments.Activity})"));

                if (data.GlValidateChartstring.Warnings != null)
                {
                    foreach (var warn in data.GlValidateChartstring.Warnings)
                    {
                        accountValidationModel.Warnings.Add(new KeyValuePair<string, string>(warn.SegmentName, warn.Warning));
                    }
                }

                //These should be the same, if they are not my code changes need to be fixed.
                if(data.GlValidateChartstring.Segments.Fund != accountValidationModel.GlSegments.Fund)
                {
                    Log.Error("Internal error: Fund segment mismatch");
                    throw new System.Exception("Internal error: Fund segment mismatch");
                }
                if(data.GlValidateChartstring.Segments.Account != accountValidationModel.GlSegments.Account)
                {
                    Log.Error("Internal error: Account segment mismatch");
                    throw new System.Exception("Internal error: Account segment mismatch");
                }
            }
            if (accountValidationModel.CoaChartType == FinancialChartStringType.Ppm)
            {
                accountValidationModel.CoaChartType = FinancialChartStringType.Ppm;
                accountValidationModel.PpmSegments = FinancialChartValidation.GetPpmSegments(financialSegmentString);

                var result = await _aggieClient.PpmSegmentStringValidate.ExecuteAsync(financialSegmentString);

                var data = result.ReadData();

                accountValidationModel.IsValid = data.PpmSegmentStringValidate.ValidationResponse.Valid;
                if (!accountValidationModel.IsValid)
                {
                    foreach (var err in data.PpmSegmentStringValidate.ValidationResponse.ErrorMessages)
                    {
                        accountValidationModel.Messages.Add(err);
                    }
                }

                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Project", data.PpmSegmentStringValidate.Segments.Project));
                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Task", data.PpmSegmentStringValidate.Segments.Task));
                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Organization", data.PpmSegmentStringValidate.Segments.Organization));
                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Expenditure Type", data.PpmSegmentStringValidate.Segments.ExpenditureType));
                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Award", data.PpmSegmentStringValidate.Segments.Award));
                accountValidationModel.Details.Add(new KeyValuePair<string, string>("Funding Source", data.PpmSegmentStringValidate.Segments.FundingSource));

                if (data.PpmSegmentStringValidate.Warnings != null)
                {
                    foreach (var warn in data.PpmSegmentStringValidate.Warnings)
                    {
                        accountValidationModel.Warnings.Add(new KeyValuePair<string, string>(warn.SegmentName, warn.Warning));
                    }
                }

                

                await GetPpmAccountManager(accountValidationModel);
            }


            return accountValidationModel;
        }

        private async Task<AccountValidationModel> CommonGlFundChecks(AccountValidationModel accountValidationModel)
        {
            var fund = accountValidationModel.GlSegments.Fund;
            if ("13U00,13U01,13U02".Contains(fund))//TODO: Make a configurable list of valid funds
            {
                //These three are excluded
                accountValidationModel.IsValid = false;
                accountValidationModel.Messages.Add("Fund is not valid. Can't be one of 13U00,13U01,13U02");
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
                    accountValidationModel.IsValid = false;
                    accountValidationModel.Messages.Add("Fund is not valid. Must roll up to 1200C, 1300C, or 5000C");
                }
            }

            return accountValidationModel;
        }

        private async Task<AccountValidationModel> CommonPpmFundChecks(AccountValidationModel accountValidationModel)
        {
            var ppmSegments = accountValidationModel.PpmSegments;
            var checkFundCode = await _aggieClient.PpmTaskByProjectNumberAndTaskNumber.ExecuteAsync(ppmSegments.Project, ppmSegments.Task);
            var checkFundCodeData = checkFundCode.ReadData();
            if (checkFundCodeData == null)
            {
                accountValidationModel.IsValid = false;
                accountValidationModel.Messages.Add("Unable to check Task's funding code.");
            }
            else
            {
                var fundCode = checkFundCodeData.PpmTaskByProjectNumberAndTaskNumber.GlPostingFundCode;
                if (fundCode == null)
                {
                    accountValidationModel.IsValid = false;
                    accountValidationModel.Messages.Add("GlPostingFundCode is null for this Task.");
                }
                else
                {
                    if ("13U00,13U01,13U02".Contains(fundCode))//TODO: Make a configurable list of valid funds
                    {
                        //These three are excluded
                        accountValidationModel.IsValid = false;
                        accountValidationModel.Messages.Add("GlPostingFundCode is not valid. Can't be one of 13U00,13U01,13U02");
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
                            accountValidationModel.IsValid = false;
                            accountValidationModel.Messages.Add("GlPostingFundCode is not valid. Must roll up to 1200C, 1300C, or 5000C");
                        }
                    }
                }
            }
            return accountValidationModel;
        }

        public async Task<AccountValidationModel> IsAccountValid(string financialSegmentString, bool validateCVRs = true)
        {
            var rtValue = new AccountValidationModel();

            rtValue = await CommonAccountValidation(rtValue, financialSegmentString, validateCVRs);

            if (rtValue.CoaChartType == FinancialChartStringType.Gl)
            {

                if (rtValue.IsValid)
                {
                    //Is fund valid?
                    rtValue = await CommonGlFundChecks(rtValue);
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
                    var naturalAcct = rtValue.GlSegments.Account;
                    var accountParents = await _aggieClient.ErpAccountRollup.ExecuteAsync(naturalAcct);
                    var dataAccountParents = accountParents.ReadData();
                    if (DoesNaturalAccountRollUp.NaturalAccount(dataAccountParents.ErpAccount, "41000D") || DoesNaturalAccountRollUp.NaturalAccount(dataAccountParents.ErpAccount, "44000D"))
                    {
                        //isValid = true;
                    }
                    else
                    {
                        rtValue.IsValid = false;
                        rtValue.Messages.Add("Natural Account is not valid. Must roll up to 41000D or 44000D. For example, use 410000 for sales of rate based goods and services.");
                    }
                }


                return rtValue;
            }

            if (rtValue.CoaChartType == FinancialChartStringType.Ppm)
            {

                if (rtValue.IsValid)
                {
                    var ppmSegments = rtValue.PpmSegments;

                    var checkFundCode = await _aggieClient.PpmTaskByProjectNumberAndTaskNumber.ExecuteAsync(ppmSegments.Project, ppmSegments.Task);
                    var checkFundCodeData = checkFundCode.ReadData();
                    if (checkFundCodeData == null)
                    {
                        rtValue.IsValid = false;
                        rtValue.Messages.Add("Unable to check Task's funding code.");
                    }
                    else
                    {
                        rtValue = await CommonPpmFundChecks(rtValue);

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
                            rtValue.Messages.Add("ExpenditureType (Natural Account) is not valid. Must roll up to 41000D or 44000D. For example, use 410000 for sales of rate based goods and services.");
                        }


                    }
                }

                return rtValue;
            }

            return rtValue;
        }

        public async Task<AccountValidationModel> IsRechargeAccountValid(string financialSegmentString, CreditDebit direction, bool validateCVRs = true)
        {
            var rtValue = new AccountValidationModel();

            var newChartString = ReplaceNaturalAccount(financialSegmentString, direction == CreditDebit.Credit ? "775000" : "770006");
            var chartStringUpdated = newChartString != financialSegmentString;
            financialSegmentString = newChartString;

            rtValue = await CommonAccountValidation(rtValue, financialSegmentString, validateCVRs);

            
            if(!rtValue.IsValid) 
            {
                //early return if the account is not valid in general
                if(chartStringUpdated)
                {
                    //Add a warning that we changed the natural account for them. Otherwise, they may not notice if it would have been valid except for the change
                    rtValue.Warnings.Add(new KeyValuePair<string, string>("Natural Account", direction == CreditDebit.Credit ? "The Natural Account/Expenditure Type segment has been changed to 775000." : "The Natural Account/Expenditure Type segment has been changed to 770006."));
                }
                return rtValue;
            }

            if (direction == CreditDebit.Credit)
            {

                //The big unknown for now if the rollup stuff/parent stuff.                               

                if (rtValue.CoaChartType == FinancialChartStringType.Gl)
                {
                    //Is fund valid?
                    rtValue = await CommonGlFundChecks(rtValue);
                    if (!rtValue.GlSegments.Account.StartsWith("775"))
                    {
                        rtValue.IsValid = false;
                        rtValue.Messages.Add("For Recharge Credit entries, the Natural Account must start with 775 (e.g. 775000)");
                    }
                    if(chartStringUpdated)
                    {
                        //Add a warning that we changed the natural account for them.
                        rtValue.Warnings.Add(new KeyValuePair<string, string>("Natural Account", "The Natural Account segment has been changed to 775000."));
                    }
                    if (rtValue.IsValid)
                    {
                        var result = await _aggieClient.ErpDepartmentApprovers.ExecuteAsync(rtValue.GlSegments.Department);
                        var data = result.ReadData();
                        if (data != null)
                        {
                            if (data.ErpFinancialDepartment != null && data.ErpFinancialDepartment.Approvers != null)
                            {
                                foreach (var approver in data.ErpFinancialDepartment.Approvers.Where(a => a.ApproverType == FiscalOfficer))
                                {
                                    rtValue.Approvers.Add(new Approver
                                    {
                                        FirstName = approver.FirstName,
                                        LastName = approver.LastName,
                                        Email = approver.EmailAddress,
                                    });
                                }
                            }
                        }
                    }
                }
                if(rtValue.CoaChartType == FinancialChartStringType.Ppm)
                {
                    rtValue = await CommonPpmFundChecks(rtValue);

                    if (!rtValue.PpmSegments.ExpenditureType.StartsWith("775"))
                    {
                        rtValue.IsValid = false;
                        rtValue.Messages.Add("For Recharge Credit entries, the Expenditure Type must start with 775 (e.g. 775000)");
                    }
                    if (chartStringUpdated)
                    {
                        //Add a warning that we changed the natural account for them.
                        rtValue.Warnings.Add(new KeyValuePair<string, string>("Expenditure Type", "The Expenditure Type segment has been changed to 775000."));
                    }
                }
            }
            if(direction  == CreditDebit.Debit) 
            {
                if(rtValue.CoaChartType == FinancialChartStringType.Gl)
                {
                    if (!rtValue.GlSegments.Account.StartsWith("770"))
                    {
                        rtValue.IsValid = false;
                        rtValue.Messages.Add("For Recharge Debit entries, the Natural Account must start with 770 (e.g. 770000)");
                    }
                    if(chartStringUpdated)
                    {
                        //Add a warning that we changed the natural account for them.
                        rtValue.Warnings.Add(new KeyValuePair<string, string>("Natural Account", "The Natural Account segment has been changed to 770006."));
                    }
                }   
                if(rtValue.CoaChartType == FinancialChartStringType.Ppm)
                {
                    if (!rtValue.PpmSegments.ExpenditureType.StartsWith("770"))
                    {
                        rtValue.IsValid = false;
                        rtValue.Messages.Add("For Recharge Debit entries, the Expenditure Type must start with 770 (e.g. 770000)");
                    }
                    if (chartStringUpdated)
                    {
                        //Add a warning that we changed the natural account for them.
                        rtValue.Warnings.Add(new KeyValuePair<string, string>("Expenditure Type", "The Expenditure Type segment has been changed to 770006."));
                    }
                }
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


        private string ReplaceNaturalAccount(string financialSegmentString, string naturalAccountToUse)
        {
            if(string.IsNullOrWhiteSpace(naturalAccountToUse))
            {
                return financialSegmentString;
            }
            var chartType = FinancialChartValidation.GetFinancialChartStringType(financialSegmentString);
            if (chartType == FinancialChartStringType.Gl)
            {
                var segments = FinancialChartValidation.GetGlSegments(financialSegmentString);
                if (segments == null)
                {
                    throw new System.Exception("Internal error: GlSegments is null");
                }
                segments.Account = naturalAccountToUse;
                return segments.ToSegmentString(); 
            }
            if(chartType == FinancialChartStringType.Ppm)
            {
                var segments = FinancialChartValidation.GetPpmSegments(financialSegmentString);
                if (segments == null)
                {
                    throw new System.Exception("Internal error: PpmSegments is null");
                }
                segments.ExpenditureType = naturalAccountToUse;
                return segments.ToSegmentString();
            }
            return financialSegmentString; //Invalid...
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
