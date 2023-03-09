using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Extensions;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.Notifications;
using Payments.Core.Models.Sloth;
using Payments.Core.Resources;
using Payments.Core.Services;
using Serilog;

namespace Payments.Core.Jobs
{
    public class MoneyMovementJob
    {
        public static string JobName = "MoneyMovement";

        private readonly ApplicationDbContext _dbContext;

        private readonly ISlothService _slothService;
        private readonly INotificationService _notificationService;
        private readonly FinanceSettings _financeSettings;

        public MoneyMovementJob(ApplicationDbContext dbContext, ISlothService slothService, INotificationService notificationService, IOptions<FinanceSettings> financeSettings)
        {
            _dbContext = dbContext;
            _slothService = slothService;
            _notificationService = notificationService;

            _financeSettings = financeSettings.Value;
        }

        public async Task FindBankReconcileTransactions(ILogger log)
        {
            using (var ts = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    // get all invoices that are waiting for reconcile
                    var invoices = _dbContext.Invoices
                        .Where(i => i.Status == Invoice.StatusCodes.Paid)
                        .Include(i => i.Account)
                        .Include(i => i.Team)
                        .ThenInclude(t => t.Accounts)
                        .ToList();

                    log.Information("{count} invoices found expecting reconciliation", invoices.Count);

                    foreach (var invoice in invoices)
                    {
                        //Eventually, this call may be changed to return a list of transactions
                        var transaction = await _slothService.GetTransactionsByProcessorId(invoice.PaymentProcessorId);
                        if (transaction == null)
                        {
                            log.Warning("No reconciliation found for invoice id: {id} Paid Date: {PaidAt}", invoice.Id, invoice.PaidAt);
                            continue;
                        }
                        if (transaction.Status != "Completed")
                        {
                            log.Warning("No completed reconciliation found for invoice id: {id} Paid Date: {PaidAt} Status: {status}", invoice.Id, invoice.PaidAt, transaction.Status);
                            continue;
                        }

                        log.Information("Invoice {id} reconciliation found with transaction: {transactionId}",
                            invoice.Id, transaction.Id);

                        // get team account info
                        var team = invoice.Team;
                        if (team.DefaultAccount == null)
                        {
                            log.Warning("Team {team} has no default account for payments", team.Name);
                            continue;
                        }
                        if (_financeSettings.UseCoa)
                        {
                            if (String.IsNullOrWhiteSpace(team.DefaultAccount.FinancialSegmentString))
                            {
                                log.Warning("Team {team} has no financial segment string for payments", team.Name);
                                continue;
                            }
                        }

                        // transaction found, bank reconcile was successful
                        invoice.KfsTrackingNumber = transaction.KfsTrackingNumber;
                        invoice.Status = Invoice.StatusCodes.Processing;

                        // calculate fees
                        var feeAmount = Math.Round(invoice.CalculatedTotal * FeeSchedule.StandardRate, 2);
                        var incomeAmount = invoice.CalculatedTotal - feeAmount;

                        //Setup transfers with base values
                        var debitHolding = new CreateTransfer()
                        {
                            Amount = invoice.CalculatedTotal,
                            Direction = Transfer.CreditDebit.Debit,
                            Description = "Funds Distribution",
                        };
                        var feeCredit = new CreateTransfer()
                        {
                            Amount = feeAmount,
                            Direction = Transfer.CreditDebit.Credit,
                            Description = "Processing Fee"
                        };
                        var incomeCredit = new CreateTransfer()
                        {
                            Amount = incomeAmount,
                            Direction = Transfer.CreditDebit.Credit,
                            Description = "Funds Distribution"
                        };

                        if (_financeSettings.UseCoa)
                        {
                            var incomeAeAccount = team.DefaultAccount.FinancialSegmentString;

                            if (invoice.Account != null && !string.IsNullOrWhiteSpace(invoice.Account.FinancialSegmentString) && invoice.Account.IsActive)
                            {
                                //Validate? here and if invalid use the team default?                            
                                // the invoice has a specified account, use it instead of the team's default
                                incomeAeAccount = invoice.Account.FinancialSegmentString;
                            }

                            // Populate transfers with financial segment strings
                            debitHolding.FinancialSegmentString = _financeSettings.ClearingFinancialSegmentString;
                            feeCredit.FinancialSegmentString = _financeSettings.FeeFinancialSegmentString;
                            incomeCredit.FinancialSegmentString = incomeAeAccount;
                        }
                        else
                        {

                            var incomeAccountChart = team.DefaultAccount.Chart;
                            var incomeAccount = team.DefaultAccount.Account;
                            var incomeSubAccount = team.DefaultAccount.SubAccount;

                            if (invoice.Account != null && !string.IsNullOrWhiteSpace(invoice.Account.Account) && invoice.Account.IsActive)
                            {
                                // the invoice has a specified account, use it instead of the team's default
                                incomeAccountChart = invoice.Account.Chart;
                                incomeAccount = invoice.Account.Account;
                                incomeSubAccount = invoice.Account.SubAccount;
                            }

                            // populate transfers with KFS values
                            debitHolding.Chart = _financeSettings.ClearingChart;
                            debitHolding.Account = _financeSettings.ClearingAccount;
                            debitHolding.ObjectCode = ObjectCodes.Income;

                            feeCredit.Chart = _financeSettings.FeeChart;
                            feeCredit.Account = _financeSettings.FeeAccount;
                            feeCredit.ObjectCode = ObjectCodes.Income;

                            incomeCredit.Chart = incomeAccountChart;
                            incomeCredit.Account = incomeAccount;
                            incomeCredit.SubAccount = incomeSubAccount;
                            incomeCredit.ObjectCode = ObjectCodes.Income;
                            //Guess we never did project code... No sense messing with it now.
                        }



                        // setup transaction
                        var merchantUrl = $"https://payments.ucdavis.edu/{invoice.Team.Slug}/invoices/details/{invoice.Id}";

                        var slothTransaction = new CreateTransaction()
                        {
                            AutoApprove = true,
                            ValidateFinancialSegmentStrings = false,
                            MerchantTrackingNumber = transaction.MerchantTrackingNumber,
                            MerchantTrackingUrl = merchantUrl,
                            KfsTrackingNumber = transaction.KfsTrackingNumber,
                            TransactionDate = DateTime.UtcNow,
                            Description = $"Funds Distribution INV {invoice.GetFormattedId()}",
                            Transfers = new List<CreateTransfer>()
                            {
                                debitHolding,
                                feeCredit,
                                incomeCredit,
                            },
                            Source = "Payments",
                            SourceType = "CyberSource",
                        };
                        try
                        {
                            slothTransaction.AddMetadata("Team Name", team.Name);
                            slothTransaction.AddMetadata("Team Slug", team.Slug);
                            slothTransaction.AddMetadata("Invoice", invoice.GetFormattedId());
                        }
                        catch
                        {
                            log.Warning("Error parsing invoice meta data for invoice {id}", invoice.Id);
                        }

                        var response = await _slothService.CreateTransaction(slothTransaction);


                        //TODO: If there was a problem with the response, set the status back to paid?
                        log.Information("Transaction created with ID: {id}", response.Id);

                        // send notifications
                        try
                        {
                            var notification = new ReconcileNotification()
                            {
                                InvoiceId = invoice.Id,
                            };
                            await _notificationService.SendReconcileNotification(notification);
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "Error while sending notification");
                        }
                    }

                    log.Information("Finishing Job");
                    await _dbContext.SaveChangesAsync();
                    ts.Commit();
                }
                catch (Exception ex)
                {
                    log.Error(ex, ex.Message);
                    ts.Rollback();
                    throw;
                }
            }
        }

        public async Task FindIncomeTransactions(ILogger log)
        {
            using (var ts = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    // get all invoices are in processing
                    var invoices = _dbContext.Invoices
                        .Where(i => i.Status == Invoice.StatusCodes.Processing)
                        .Include(i => i.Team)
                        .ToList();

                    log.Information("{count} invoices found expecting completion", invoices.Count);

                    foreach (var invoice in invoices)
                    {
                        if (string.IsNullOrWhiteSpace(invoice.KfsTrackingNumber))
                        {
                            log.Warning("Invoice {id} has no kfs tarcking number.", invoice.Id);
                            continue;
                        }

                        var transactions = await _slothService.GetTransactionsByKfsKey(invoice.KfsTrackingNumber);

                        // look for transfers into the fees account that have completed
                        // TODO: Use the sloth transaction.Description to identify these? It depends on what we write there
                        var distribution = transactions?.FirstOrDefault(t =>
                            string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                            && t.Transfers.Any(r => string.Equals(r.Account, _financeSettings.FeeAccount) || string.Equals(r.FinancialSegmentString, _financeSettings.FeeFinancialSegmentString)));

                        if (distribution == null)
                        {
                            log.Warning("No reconciliation found for invoice id: {id} Paid Date: {PaidAt}", invoice.Id, invoice.PaidAt);
                            continue;
                        }

                        log.Information("Invoice {id} distribution found with transaction: {transactionId}",
                            invoice.Id, distribution.Id);

                        // transaction found, bank reconcile was successful
                        invoice.Status = Invoice.StatusCodes.Completed;
                    }

                    log.Information("Finishing Job");
                    await _dbContext.SaveChangesAsync();
                    ts.Commit();
                }
                catch (Exception ex)
                {
                    log.Error(ex, ex.Message);
                    ts.Rollback();
                    throw;
                }
            }
        }
    }
}
