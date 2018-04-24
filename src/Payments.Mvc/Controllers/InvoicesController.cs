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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]EditInvoiceViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var team = await _dbContext.Teams.FirstAsync();

            // create line items
            var items = model.Items.Select(i => new LineItem()
            {
                Description = i.Description,
                Amount = i.Amount,
                Quantity = i.Quantity,
            });

            // create invoice
            var invoice = new Invoice()
            {
                Creator = user,
                CustomerAddress = model.CustomerAddress,
                CustomerEmail = model.CustomerEmail,
                CustomerName = model.CustomerName,
                Items = items.ToList(),
                Team = team,
            };

            // save to db
            _dbContext.Invoices.Add(invoice);
            _dbContext.SaveChanges();

            return new JsonResult(new
            {
                id = invoice.Id,
                success = true
            });
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var invoice = _dbContext.Invoices.Find(id);
            return View(invoice);
        }

        [HttpPost]
        public IActionResult Edit(string id, EditInvoiceViewModel model)
        {
            var invoice = _dbContext.Invoices.Find(id);

            // update invoice
            invoice.CustomerAddress = model.CustomerAddress;
            invoice.CustomerEmail = model.CustomerEmail;
            invoice.CustomerName = model.CustomerName;

            // remove old items
            _dbContext.LineItems.RemoveRange(invoice.Items);

            // add new items
            var items = model.Items.Select(i => new LineItem()
            {
                Description = i.Description,
                Amount = i.Amount,
                Quantity = i.Quantity,
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
