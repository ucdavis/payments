using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
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
            // get all invoices for team
            var query = _dbContext.Invoices
                .AsQueryable()
                .Include(i => i.Items)
                .Where(i => i.Team.Slug == TeamSlug);

            var invoices = query
                .OrderByDescending(i => i.Id)
                .ToList();

            var model = new SearchResultsViewModel()
            {
                Invoices = invoices,
            };

            return View(model);
        }
    }
}
