using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Mvc.Helpers;
using Payments.Mvc.Identity;

namespace Payments.Mvc.Controllers
{
    public class HomeController : SuperController
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationUserManager userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // look for users teams, redirect if there's only one
            var user = await _userManager.GetUserAsync(User);
            var teams = user.GetTeams().Where(a => a.IsActive)
                .ToList();

            if (teams.Count == 1)
            {
                var team = teams[0];
                return RedirectToAction(nameof(TeamIndex), "Home", new { team = team.Slug });
            }

            // TODO, figure out the last used team and just redirect there


            return View(teams);
        }

        public async Task<IActionResult> TeamIndex()
        {
            return View();
        }

        [ResponseCache(Duration = 600)]
        public async Task<IActionResult> TeamIndexStats()
        {
            if (string.IsNullOrWhiteSpace(TeamSlug))
            {
                return NotFound();
            }

            var totalInvoiceCount = await _context.Invoices
                .AsQueryable()
                .Where(i => i.Team.Slug == TeamSlug)
                .CountAsync();

            var lastMonday = DateTime.UtcNow.StartOfWeek(DayOfWeek.Monday).Date;
            var newInvoiceCount = await _context.Invoices
                .AsQueryable()
                .Where(i => i.Team.Slug == TeamSlug)
                .Where(i => i.CreatedAt > lastMonday)
                .CountAsync();

            var lastSixMonths = DateTime.UtcNow.AddMonths(-6).Date;
            var lastSixMonthsAmount = await _context.Invoices
                .AsQueryable()
                .Where(i => i.Team.Slug == TeamSlug)
                .Where(i => i.CreatedAt > lastSixMonths)
                .SumAsync(i => i.Total);

            return new JsonResult(new
            {
                totalInvoiceCount,
                newInvoiceCount,
                lastSixMonthsAmount = lastSixMonthsAmount.ToString("C"),
            });
        }

        public IActionResult About()
        {
            return View();
        }

        [AllowAnonymous]
        [Route("error/404")]
        public IActionResult Error404()
        {
            return View("NotFound");
        }

        [AllowAnonymous]
        [Route("error/{code:int}")]
        public IActionResult Error(int code)
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
                return RedirectToAction(nameof(Index), "Home", new { team = "" });
            }

            return RedirectToAction(nameof(TeamIndex), "Home", new { team = team.Slug });

        }
    }
}
