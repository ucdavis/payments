using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Mvc.Models.InvoiceViewModels;


namespace Payments.Mvc.Controllers
{
    public class InvoicesController : SuperController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public InvoicesController(ApplicationDbContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            var invoices = _dbContext.Invoices
                .AsQueryable()
                .Take(100)
                .OrderByDescending(i => i.Id);


            return View(invoices);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var invoice = new Invoice();
            return View("Edit", invoice);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            return View(invoice);
        }

        [HttpPost]
        public async Task<IActionResult> Save(int id, [FromBody]EditInvoiceViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var team = await _dbContext.Teams.FirstAsync();

            // setup model target
            Invoice invoice;
            if (id == 0)
            {
                // create new object, track it
                invoice = new Invoice()
                {
                    Creator = user,
                    Team = team,
                };
                _dbContext.Invoices.Add(invoice);
            }
            else
            {
                // find item
                invoice = await _dbContext.Invoices
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null)
                {
                    return NotFound();
                }

                // remove old items
                _dbContext.LineItems.RemoveRange(invoice.Items);
            }

            // update invoice
            invoice.CustomerAddress = model.CustomerAddress;
            invoice.CustomerEmail   = model.CustomerEmail;
            invoice.CustomerName    = model.CustomerName;
            invoice.Discount        = model.Discount;
            invoice.TaxPercent      = model.Tax;

            // add line items
            var items = model.Items.Select(i => new LineItem()
            {
                Amount      = i.Amount,
                Description = i.Description,
                Quantity    = i.Quantity,
            });
            invoice.Items = items.ToList();

            // save to db
            _dbContext.SaveChanges();

            return new JsonResult(new
            {
                success = true
            });
        }
    }
}
