using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Mvc.Models.SearchViewModels;

namespace Payments.Mvc.Controllers
{
    public class SearchController : SuperController
    {
        private readonly ApplicationDbContext _dbContext;

        public SearchController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Query(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                ViewBag.ErrorMessage = "No search value entered";
                return View(new SearchResultsViewModel(){Invoices = new List<Invoice>()});
            }

            q = q.Trim();
            // get all invoices for team
            var query = _dbContext.Invoices
                .AsQueryable()
                .Include(i => i.Items)
                .Where(i => i.Team.Slug == TeamSlug);

            if (q.Contains("@"))
            {
                query = query.Where(a => a.CustomerEmail.Equals(q, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                var temp = q.Split('-');
                var id = 0;
                Int32.TryParse(temp.FirstOrDefault(), out id);
                query = query.Where(a => a.Id == id);

            }

            var invoices = query
                .OrderByDescending(i => i.Id)
                .ToList();
            if (invoices.Count == 0)
            {
                ViewBag.Message = "No Invoioces found for your search criteria (Invoice # or email).";
            }

            var model = new SearchResultsViewModel()
            {
                Invoices = invoices,
            };

            return View(model);
        }
    }
}
