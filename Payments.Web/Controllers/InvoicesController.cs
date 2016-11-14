using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core;
using Payments.Core.Models;
using Payments.Web.ViewModels;


namespace Payments.Web.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly PaymentsContext _context;
        private readonly IMapper _mapper;

        public InvoicesController(PaymentsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(InvoiceEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // create and track
            var invoice = new Invoice();
            _context.Invoices.Add(invoice);

            // update and save
            _mapper.Map(model, invoice);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { invoice.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null)
                return NotFound();

            var model = _mapper.Map<Invoice, InvoiceEditViewModel>(invoice);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int? id, InvoiceEditViewModel model)
        {
            if (id == null)
                return BadRequest();

            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null)
                return NotFound();

            // update
            _mapper.Map(model, invoice);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new {invoice.Id});
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();

            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null)
                return NotFound();

            return View(invoice);
        }
    }
}
