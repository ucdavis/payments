﻿using System;
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
            return View();
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
        public async Task<IActionResult> Create([FromBody] CreateInvoiceViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var team = await _dbContext.Teams.FirstAsync();

            // manage multiple customer scenario
            foreach (var customer in model.Customers)
            {
                // create new object, track it
                var invoice = new Invoice
                {
                    Creator         = user,
                    Team            = team,
                    Discount        = model.Discount,
                    TaxPercent      = model.Tax,
                    CustomerAddress = customer.Address,
                    CustomerEmail   = customer.Email,
                    CustomerName    = customer.Name,
                    Memo            = model.Memo,
                    Status          = Invoice.StatusCodes.Sent, // TODO: Set to draft or sent based on actual email status
                };

                // add line items
                var items = model.Items.Select(i => new LineItem()
                {
                    Amount      = i.Amount,
                    Description = i.Description,
                    Quantity    = i.Quantity,
                    Total       = i.Quantity * i.Amount,
                });
                invoice.Items = items.ToList();

                // start tracking for db
                invoice.UpdateCalculatedValues();
                _dbContext.Invoices.Add(invoice);
            }

            _dbContext.SaveChanges();

            return new JsonResult(new
            {
                success = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody]EditInvoiceViewModel model)
        {
            // find item
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // remove old items
            _dbContext.LineItems.RemoveRange(invoice.Items);

            // update invoice
            invoice.CustomerAddress = model.Customer.Address;
            invoice.CustomerEmail   = model.Customer.Email;
            invoice.CustomerName    = model.Customer.Name;
            invoice.Memo            = model.Memo;
            invoice.Discount        = model.Discount;
            invoice.TaxPercent      = model.Tax;

            // add line items
            var items = model.Items.Select(i => new LineItem()
            {
                Amount      = i.Amount,
                Description = i.Description,
                Quantity    = i.Quantity,
                Total       = i.Quantity * i.Amount,
            });
            invoice.Items = items.ToList();

            // save to db
            invoice.UpdateCalculatedValues();
            _dbContext.SaveChanges();

            return new JsonResult(new
            {
                success = true
            });
        }
    }
}
