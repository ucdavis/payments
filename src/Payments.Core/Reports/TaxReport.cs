using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;

namespace Payments.Core.Reports
{
    public class TaxReport
    {
        private readonly ApplicationDbContext _dbContext;

        public TaxReport(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Invoice> RunForFiscalYear(int year)
        {
            var query = GetBaseQuery();

            // for this fiscal year
            var fiscalStart = new DateTime(year - 1, 7, 1);
            var fiscalEnd = new DateTime(year, 7, 1);

            return query.Where(i =>
                   i.PaidAt >= fiscalStart
                && i.PaidAt <= fiscalEnd);
        }

        public IQueryable<Invoice> RunForMonth(DateTime startMonth)
        {
            // get all invoices for team
            var query = GetBaseQuery();

            // force set datetime to start of month
            startMonth =  new DateTime(startMonth.Year, startMonth.Month, 1);
            var endMonth = startMonth.AddMonths(1);

            return query.Where(i =>
                i.PaidAt >= startMonth
                && i.PaidAt <= endMonth);
        }

        private IQueryable<Invoice> GetBaseQuery()
        {
            return _dbContext.Invoices
                .AsQueryable()
                .Include(i => i.Account)
                .Include(i => i.Items)
                .Include(i => i.Team)
                .Where(i => i.Paid)
                .Where(i => i.CalculatedTaxAmount > 0);
        }
    }
}
