using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Mvc.Models.InvoiceViewModels;
using Payments.Mvc.Models.ReportViewModels;
using Microsoft.AspNetCore.Authorization;
using Payments.Mvc.Models.Roles;
using static Payments.Core.Domain.Invoice;
using System.Threading.Tasks;
using Payments.Mvc.Services;
using Payments.Core.Domain;
using Payments.Core.Models.History;

namespace Payments.Mvc.Controllers
{
    public class ReportsController : SuperController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDirectorySearchService _directorySearchService;

        public ReportsController(ApplicationDbContext dbContext, IDirectorySearchService directorySearchService)
        {
            _dbContext = dbContext;
            _directorySearchService = directorySearchService;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region team reports
        public IActionResult Activity(string team, int? year = null)
        {
            //If we wanted to include other history actions, we could add them to the where clause
            var invoiceQuery = _dbContext.Invoices
                .Include(i => i.Account)
                .Include(a => a.History.Where(a => a.Type == HistoryActionTypes.PaymentCompleted.TypeCode || a.Type == HistoryActionTypes.InvoiceCreated.TypeCode || a.Type == HistoryActionTypes.InvoiceSent.TypeCode))
                .Where(i => i.Team.Slug == TeamSlug && i.Type == InvoiceTypes.CreditCard);
            if (year.HasValue)
            {
                invoiceQuery = invoiceQuery.Where(i => i.CreatedAt >= new DateTime(year.Value, 1, 1) && i.CreatedAt <= new DateTime(year.Value, 12, 31));
            }
            else
            {
                invoiceQuery = invoiceQuery.Where(i => i.CreatedAt >= DateTime.UtcNow.AddMonths(-12));
            }



            var invoices = invoiceQuery
                .OrderByDescending(i => i.Id)
                .AsNoTracking()
                .ToList();

            var model = new InvoiceListViewModel()
            {
                Invoices = invoices,
                Filter = null,
                Year = year,
            };

            return View(model);
        }

        public IActionResult RechargeActivity(string team, int? year = null)
        {
            //If we wanted to include other history actions, we could add them to the where clause
            var invoiceQuery = _dbContext.Invoices
                .Include(a => a.RechargeAccounts)
                .Include(a => a.History.Where(a => a.Type == HistoryActionTypes.RechargeCompletedInSloth.TypeCode || a.Type == HistoryActionTypes.RechargePaidByCustomer.TypeCode || a.Type == HistoryActionTypes.InvoiceCreated.TypeCode || a.Type == HistoryActionTypes.InvoiceSent.TypeCode))
                .Where(i => i.Team.Slug == TeamSlug && i.Type == InvoiceTypes.Recharge);
            if (year.HasValue)
            {
                invoiceQuery = invoiceQuery.Where(i => i.CreatedAt >= new DateTime(year.Value, 1, 1) && i.CreatedAt <= new DateTime(year.Value, 12, 31));
            }
            else
            {
                invoiceQuery = invoiceQuery.Where(i => i.CreatedAt >= DateTime.UtcNow.AddMonths(-12));
            }

            var invoices = invoiceQuery
                .OrderByDescending(i => i.Id)
                .AsNoTracking()
                .ToList();

            var model = new InvoiceListViewModel()
            {
                Invoices = invoices,
                Filter = null,
                Year = year,
            };

            return View(model);
        }

        /// Shows all unpaid invoices, grouping by customer to show how much is unpaid based on sent date brackets
        public IActionResult Aging(string team)
        {
            // TODO: add in date range filters
            var invoices = _dbContext.Invoices
                .Where(i => i.Team.Slug == TeamSlug)
                .Where(i => !i.Paid && i.Sent) // just show invoices that have been sent but not paid
                .AsNoTracking()
                .ToList();

            var byCustomer = invoices.GroupBy(i => i.CustomerEmail);

            var agingTotals = byCustomer.Select(c => new CustomerAgingTotals
            {
                CustomerEmail = c.Key,
                OneMonth = c.Where(i => 
                    (i.DueDate.HasValue && i.DueDate.Value >= DateTime.UtcNow.AddMonths(-1)) || 
                    (!i.DueDate.HasValue && i.CreatedAt >= DateTime.UtcNow.AddMonths(-1))
                ).Sum(i => i.CalculatedTotal),
                TwoMonths = c.Where(i => 
                    (i.DueDate.HasValue && i.DueDate.Value >= DateTime.UtcNow.AddMonths(-2) && i.DueDate.Value < DateTime.UtcNow.AddMonths(-1)) || 
                    (!i.DueDate.HasValue && i.CreatedAt >= DateTime.UtcNow.AddMonths(-2) && i.CreatedAt < DateTime.UtcNow.AddMonths(-1))
                ).Sum(i => i.CalculatedTotal),
                ThreeMonths = c.Where(i => 
                    (i.DueDate.HasValue && i.DueDate.Value >= DateTime.UtcNow.AddMonths(-3) && i.DueDate.Value < DateTime.UtcNow.AddMonths(-2)) ||
                    (!i.DueDate.HasValue && i.CreatedAt >= DateTime.UtcNow.AddMonths(-3) && i.CreatedAt < DateTime.UtcNow.AddMonths(-2))
                ).Sum(i => i.CalculatedTotal),
                FourMonths = c.Where(i => 
                    (i.DueDate.HasValue && i.DueDate.Value >= DateTime.UtcNow.AddMonths(-4) && i.DueDate.Value < DateTime.UtcNow.AddMonths(-3)) ||
                    (!i.DueDate.HasValue && i.CreatedAt >= DateTime.UtcNow.AddMonths(-4) && i.CreatedAt < DateTime.UtcNow.AddMonths(-3))
                ).Sum(i => i.CalculatedTotal),
                FourToSixMonths = c.Where(i => 
                    (i.DueDate.HasValue && i.DueDate.Value >= DateTime.UtcNow.AddMonths(-6) && i.DueDate.Value < DateTime.UtcNow.AddMonths(-4)) ||
                    (!i.DueDate.HasValue && i.CreatedAt >= DateTime.UtcNow.AddMonths(-6) && i.CreatedAt < DateTime.UtcNow.AddMonths(-4))
                ).Sum(i => i.CalculatedTotal),
                SixToTwelveMonths = c.Where(i => 
                    (i.DueDate.HasValue && i.DueDate.Value >= DateTime.UtcNow.AddMonths(-12) && i.DueDate.Value < DateTime.UtcNow.AddMonths(-6)) ||
                    (!i.DueDate.HasValue && i.CreatedAt >= DateTime.UtcNow.AddMonths(-12) && i.CreatedAt < DateTime.UtcNow.AddMonths(-6))
                ).Sum(i => i.CalculatedTotal),
                OneToTwoYears = c.Where(i => 
                    (i.DueDate.HasValue && i.DueDate.Value >= DateTime.UtcNow.AddYears(-2) && i.DueDate.Value < DateTime.UtcNow.AddYears(-1)) ||
                    (!i.DueDate.HasValue && i.CreatedAt >= DateTime.UtcNow.AddYears(-2) && i.CreatedAt < DateTime.UtcNow.AddYears(-1))
                ).Sum(i => i.CalculatedTotal),
                OverTwoYears = c.Where(i => 
                    (i.DueDate.HasValue && i.DueDate.Value < DateTime.UtcNow.AddYears(-2)) ||
                    (!i.DueDate.HasValue && i.CreatedAt < DateTime.UtcNow.AddYears(-2))
                ).Sum(i => i.CalculatedTotal),
                Total = c.Sum(i => i.CalculatedTotal)
            }).ToArray();

            return View(agingTotals);
        }

        #endregion

        #region system reports
        /* system wide reports should use attribute routes */

        [Authorize(Roles = ApplicationRoleCodes.Admin)]
        public async Task<IActionResult> StuckInProcessing()
        {
            //Look at all paid invoices in the processing status that were paid a week ago.
            var invoices = await _dbContext.Invoices
                .Where(a => a.Paid && (a.Status == StatusCodes.Processing || a.Status == StatusCodes.Paid) && (a.PaidAt == null || a.PaidAt < DateTime.UtcNow.AddDays(-7)))
                .Include(i => i.Team)
                .Include( i => i.Account)
                .AsSplitQuery() //Split it?
                .AsNoTracking()
                .ToListAsync();

            return View(invoices);            
        }

        [Authorize(Roles = ApplicationRoleCodes.Admin)]
        public async Task<IActionResult> PendingRefundRequests()
        {
            //Look at all paid invoices in the processing status that were paid a week ago.
            var invoices = await _dbContext.Invoices
                .Where(a => a.Status == StatusCodes.Refunding)
                .Include(i => i.Team)
                .Include(i => i.Account)
                .AsSplitQuery() //Split it?
                .AsNoTracking()
                .ToListAsync();

            return View(invoices);
        }

        [Authorize(Roles = ApplicationRoleCodes.Admin)]
        public async Task<IActionResult> InactiveUsers()
        {
            var usersWithTeams = await _dbContext.TeamPermissions.AsNoTracking().Select(TeamUsersReportModel.Projection()).ToListAsync();

            var kerbs = usersWithTeams.Select(a => a.Kerb).Distinct().ToArray();

            foreach (var kerb in kerbs)
            {
                var info = await _directorySearchService.GetByKerberos(kerb);
                if (info == null || info.IsInvalid)
                {
                    //set usersWithTeams IsActive to false where kerb matches
                    foreach (var userTeam in usersWithTeams.Where(a => a.Kerb == kerb))
                    {
                        userTeam.IsActive = false;
                    }
                }
            }

            return View(usersWithTeams.OrderBy(a => a.IsActive).ThenBy(a => a.Kerb).ThenBy(a => a.TeamName).ThenBy(a => a.RoleName).ToArray());
        }

        #endregion
    }
}
