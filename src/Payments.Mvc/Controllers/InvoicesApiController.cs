﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.History;
using Payments.Mvc.Models.InvoiceViewModels;

namespace Payments.Mvc.Controllers
{
    [Route("api/invoices")]
    public class InvoicesApiController : ApiController
    {

        public InvoicesApiController(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Invoice), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Invoice>> Get(int id)
        {
            if (id <= 0)
            {

            }

            var team = await GetAuthorizedTeam();

            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id && i.Team.Id == team.Id);

            if (invoice == null)
            {
                return NotFound(new { });
            }

            return invoice;
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceViewModel model)
        {
            var team = await GetAuthorizedTeam();

            // find account
            var account = team.Accounts.FirstOrDefault(a => a.Id == model.AccountId);
            if (account == null)
            {
                ModelState.AddModelError("AccountId", "Account Id not found for this team.");
            }
            else if (!account.IsActive)
            {
                ModelState.AddModelError("AccountId", "Account is inactive.");
            }

            // validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage = "Errors found in request",
                    modelState = ModelState
                });
            }

            // manage multiple customer scenario
            var invoices = new List<Invoice>();
            foreach (var customer in model.Customers)
            {
                // create new object, track it
                var invoice = new Invoice
                {
                    Account         = account,
                    Team            = team,
                    Discount        = model.Discount,
                    TaxPercent      = model.TaxPercent,
                    DueDate         = model.DueDate,
                    CustomerAddress = customer.Address,
                    CustomerEmail   = customer.Email,
                    CustomerName    = customer.Name,
                    Memo            = model.Memo,
                    Status          = Invoice.StatusCodes.Draft,
                    Sent            = false,
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

                // record action
                var action = new History()
                {
                    Type = HistoryActionTypes.InvoiceCreated.TypeCode,
                    ActionDateTime = DateTime.UtcNow,
                    Actor = "API",
                };
                invoice.History.Add(action);

                // start tracking for db
                invoice.UpdateCalculatedValues();
                _dbContext.Invoices.Add(invoice);

                invoices.Add(invoice);
            }

            _dbContext.SaveChanges();

            return new JsonResult(new
            {
                success = true,
                ids = invoices.Select(i => i.Id),
            });
        }
    }
}
