using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Payments.Core.Models;
using Payments.Web.ViewModels;


namespace Payments.Web.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly PaymentsContext _context;

        public InvoicesController(PaymentsContext context)
        {
            _context = context;
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
        public IActionResult Create(InvoiceViewModel invoice)
        {
            if (!ModelState.IsValid)
                return View(invoice);

            var target = new Invoice();
            _context.Invoices.Add(target);

            return RedirectToAction("details", new {target.Id});
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return NotFound();

            return View(invoice);
        }
    }
}
