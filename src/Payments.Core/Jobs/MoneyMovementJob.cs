using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
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
                invoice.Status = Invoice.StatusCodes.Completed;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
