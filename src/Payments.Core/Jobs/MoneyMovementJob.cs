using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
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

        public MoneyMovementJob(ApplicationDbContext dbContext, ISlothService slothService)
        {
            _dbContext = dbContext;
            _slothService = slothService;
        }

        public async Task FindBankReconcileTransactions(ILogger log)
        {
            // get all invoices that are waiting for reconcile
            var invoices = _dbContext.Invoices
                .Where(i => i.Status == Invoice.StatusCodes.Paid)
                .Include(i => i.Payment)
                .ToList();

            foreach (var invoice in invoices)
            {
                var transaction = await _slothService.GetTransactionsByProcessorId(invoice.Payment.Transaction_Id);
                if (transaction == null) continue;

                // transaction found, bank reconcile was successful
                invoice.Status = Invoice.StatusCodes.Processing;

                // calculate fees and taxes
                var taxAmount = invoice.TaxAmount;
                var feeAmount = invoice.Total * FeeSchedule.StandardRate;
                var incomeAmount = invoice.Total - feeAmount - taxAmount;

                // create transfers
                var debitHolding = new CreateTransfer()
                {
                    Amount = invoice.Total,
                    Direction = Transfer.CreditDebit.Debit,
                };

                var feeCredit = new CreateTransfer()
                {
                    Amount = feeAmount,
                    Direction = Transfer.CreditDebit.Credit,
                };

                var taxCredit = new CreateTransfer()
                {
                    Amount = taxAmount,
                    Direction = Transfer.CreditDebit.Credit,
                };

                var incomeCredit = new CreateTransfer()
                {
                    Amount = incomeAmount,
                    Direction = Transfer.CreditDebit.Credit,
                };

                var response = await _slothService.CreateTransaction(new CreateTransaction()
                {
                    AutoApprove            = false,
                    MerchantTrackingNumber = transaction.MerchantTrackingNumber,
                    TransactionDate        = DateTime.UtcNow,
                    Transfers = new List<CreateTransfer>()
                    {
                        debitHolding,
                        feeCredit,
                        taxCredit,
                        incomeCredit,
                    },
                });
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
