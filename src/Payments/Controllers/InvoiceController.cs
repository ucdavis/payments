using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core;
using Payments.Core.Models;
using Payments.Models;

namespace Payments.Controllers
{
    public class InvoiceController : ApplicationController
    {
        private readonly PaymentsContext _context;
        private readonly IMapper _mapper;

        public InvoiceController(PaymentsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            var invoices = _context.Invoices.ToArray();
            return View(invoices);
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
            
            // add new invoice mapped from user input
            _context.Invoices.Add(_mapper.Map<Invoice>(model));            
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Index");
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

            if (!ModelState.IsValid)
                return View(model);

            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null)
                return NotFound();

            // update
            _mapper.Map(model, invoice);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
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
