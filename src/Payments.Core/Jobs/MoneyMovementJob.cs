using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.Configuration;
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
        private readonly FinanceSettings _financeSettings;

        public MoneyMovementJob(ApplicationDbContext dbContext, ISlothService slothService, IOptions<FinanceSettings> financeSettings)
        {
            _dbContext = dbContext;
            _slothService = slothService;

            _financeSettings = financeSettings.Value;
        }

        public async Task FindBankReconcileTransactions(ILogger log)
        {
            try
            {
                // get all invoices that are waiting for reconcile
                var invoices = _dbContext.Invoices
                    .Where(i => i.Status == Invoice.StatusCodes.Paid)
                    .Include(i => i.Payment)
                    .Include(i => i.Team)
                        .ThenInclude(t => t.Accounts)
                    .ToList();

                log.Information("{count} invoices found expecting reconciliation", invoices.Count);

                foreach (var invoice in invoices)
                {
                    var transaction = await _slothService.GetTransactionsByProcessorId(invoice.Payment.Transaction_Id);
                    if (transaction == null)
                    {
                        log.Warning("No reconcilation found for invoice id: {id}", invoice.Id);
                        continue;
                    };

                    log.Information("Invoice {id} reconciliation found with transaction: {transactionId}", invoice.Id, transaction.Id);

                    // get team account info
                    var team = invoice.Team;
                    if (team.DefaultAccount == null)
                    {
                        log.Warning("Team {team} has no default account for payments", team.Name);
                        continue;
                    }

                    // transaction found, bank reconcile was successful
                    invoice.Status = Invoice.StatusCodes.Processing;

                    // calculate fees
                    var feeAmount = invoice.Total * FeeSchedule.StandardRate;
                    var incomeAmount = invoice.Total - feeAmount;

                    // create transfers
                    var debitHolding = new CreateTransfer()
                    {
                        Amount    = invoice.Total,
                        Direction = Transfer.CreditDebit.Debit,
                        Chart     = _financeSettings.ClearingChart,
                        Account   = _financeSettings.ClearingAccount,
                    };

                    var feeCredit = new CreateTransfer()
                    {
                        Amount    = feeAmount,
                        Direction = Transfer.CreditDebit.Credit,
                        Chart     = _financeSettings.FeeChart,
                        Account   = _financeSettings.FeeAccount,
                    };

                    var incomeCredit = new CreateTransfer()
                    {
                        Amount        = incomeAmount,
                        Direction     = Transfer.CreditDebit.Credit,
                        Chart         = team.DefaultAccount.Chart,
                        Account       = team.DefaultAccount.Account,
                        SubAccount    = team.DefaultAccount.SubAccount,
                        ObjectCode    = team.DefaultAccount.Object,
                        SubObjectCode = team.DefaultAccount.SubObject,
                    };

                    var response = await _slothService.CreateTransaction(new CreateTransaction()
                    {
                        AutoApprove            = false,
                        MerchantTrackingNumber = transaction.MerchantTrackingNumber,
                        TransactionDate        = DateTime.UtcNow,
                        Transfers              = new List<CreateTransfer>()
                        {
                            debitHolding,
                            feeCredit,
                            incomeCredit,
                        },
                    });

                    log.Information("Transaction created with ID: {id}", response.Id);
                }

                log.Information("Finishing Job");
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
                throw;
            }
        }
    }
}
