using Microsoft.AspNetCore.Mvc;
using Payments.Core.Data;
using Payments.Mvc.Identity;

namespace Payments.Mvc.Controllers
{
    public class SearchController : SuperController
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationUserManager userManager, ApplicationDbContext context) : base(userManager)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}