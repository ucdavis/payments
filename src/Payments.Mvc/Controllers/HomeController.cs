using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Mvc.Identity;

namespace Payments.Mvc.Controllers
{
    public class HomeController : SuperController
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationUserManager userManager, ApplicationDbContext context) : base(userManager)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Error()
        {
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }

        [Authorize]
        public IActionResult Secure() {
            return Content("Logged in");
        }

        [HttpPost]
        public async Task<IActionResult> SetActiveTeam(int teamId)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == teamId);

            // TODO: Check permissions

            // TODO: Save as default team

            if (team == null)
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            return RedirectToAction(nameof(Index), "Home", new { team = team.Slug });
        }
    }
}
