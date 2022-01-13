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
                        var transaction = await _slothService.GetTransactionsByProcessorId(invoice.PaymentProcessorId);
                        if (transaction == null)
                        {
                            log.Warning("No reconciliation found for invoice id: {id} Paid Date: {PaidAt}", invoice.Id, invoice.PaidAt);
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

                        // transaction found, bank reconcile was successful
                        invoice.KfsTrackingNumber = transaction.KfsTrackingNumber;
                        invoice.Status = Invoice.StatusCodes.Processing;

                        // calculate fees
                        var feeAmount = Math.Round(invoice.CalculatedTotal * FeeSchedule.StandardRate, 2);
                        var incomeAmount = invoice.CalculatedTotal - feeAmount;

                        var incomeAccountChart = team.DefaultAccount.Chart;
                        var incomeAccount = team.DefaultAccount.Account;
                        var incomeSubAccount = team.DefaultAccount.SubAccount;

                        if (invoice.Account != null && !string.IsNullOrWhiteSpace(invoice.Account.Account) && invoice.Account.IsActive) {
                            // the invoice has a specified account, use it instead of the team's default
                            incomeAccountChart = invoice.Account.Chart;
                            incomeAccount = invoice.Account.Account;
                            incomeSubAccount = invoice.Account.SubAccount;
                        }

                        // create transfers
                        var debitHolding = new CreateTransfer()
                        {
                            Amount      = invoice.CalculatedTotal,
                            Direction   = Transfer.CreditDebit.Debit,
                            Chart       = _financeSettings.ClearingChart,
                            Account     = _financeSettings.ClearingAccount,
                            ObjectCode  = ObjectCodes.Income,
                            Description = $"Funds Distribution INV {invoice.GetFormattedId()}".SafeTruncate(40)
                        };

                        var feeCredit = new CreateTransfer()
                        {
                            Amount      = feeAmount,
                            Direction   = Transfer.CreditDebit.Credit,
                            Chart       = _financeSettings.FeeChart,
                            Account     = _financeSettings.FeeAccount,
                            ObjectCode  = ObjectCodes.Income,
                            Description = $"Processing Fee INV {invoice.GetFormattedId()}".SafeTruncate(40)
                        };

                        var incomeCredit = new CreateTransfer()
                        {
                            Amount      = incomeAmount,
                            Direction   = Transfer.CreditDebit.Credit,
                            Chart       = incomeAccountChart,
                            Account     = incomeAccount,
                            SubAccount  = incomeSubAccount,
                            ObjectCode  = ObjectCodes.Income,
                            Description = $"Funds Distribution INV {invoice.GetFormattedId()}".SafeTruncate(40)
                        };

                        // setup transaction
                        var merchantUrl = $"https://payments.ucdavis.edu/{invoice.Team.Slug}/invoices/details/{invoice.Id}";

                        var response = await _slothService.CreateTransaction(new CreateTransaction()
                        {
                            AutoApprove            = true,
                            MerchantTrackingNumber = transaction.MerchantTrackingNumber,
                            MerchantTrackingUrl    = merchantUrl,
                            KfsTrackingNumber      = transaction.KfsTrackingNumber,
                            TransactionDate        = DateTime.UtcNow,
                            Transfers              = new List<CreateTransfer>()
                            {
                                debitHolding,
                                feeCredit,
                                incomeCredit,
                            },
                            Source     = "Payments",
                            SourceType = "CyberSource",
                        });

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
                        var distribution = transactions?.FirstOrDefault(t =>
                            string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                            && t.Transfers.Any(r => string.Equals(r.Account, _financeSettings.FeeAccount)));

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
