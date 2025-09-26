using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Payments.Mvc.Identity;
using Payments.Mvc.Models;
using Payments.Mvc.Models.Roles;
using Payments.Mvc.Models.TeamViewModels;

namespace Payments.Mvc.Controllers
{
    public class TeamsController : SuperController
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationUserManager _userManager;

        public TeamsController(ApplicationDbContext context, ApplicationUserManager userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Teams
        [Authorize(Policy = PolicyCodes.TeamEditor)]
        public async Task<IActionResult> Index()
        {
            List<Team> teams;
            if (User.IsInRole(ApplicationRoleCodes.Admin))
            {
                // fetch all teams
                teams = _context.Teams.ToList();
            }
            else
            {
                // fetch users teams
                var user = await _userManager.GetUserAsync(User);
                teams = user.GetTeams().Where(a => a.IsActive).ToList();
            }

            return View(teams);
        }

        // GET: Teams/Create
        [Authorize(Roles = ApplicationRoleCodes.Admin)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teams/Create
        [HttpPost]
        [Authorize(Roles = ApplicationRoleCodes.Admin)]
        public async Task<IActionResult> Create(CreateTeamViewModel model)
        {
            if (await _context.Teams.AnyAsync(a => a.Slug == model.Slug))
            {
                ModelState.AddModelError("Slug", "Team Slug already used.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var team = new Team()
            {
                Name = model.Name,
                Slug = model.Slug,
                ContactName = model.ContactName,
                ContactEmail = model.ContactEmail,
                ContactPhoneNumber = model.ContactPhoneNumber,
                IsActive = true,
                ApiKey = Guid.NewGuid().ToString("N").ToUpper(),
                WebHookApiKey = model.WebHookApiKey,
                AllowedInvoiceType = model.AllowedInvoiceType,
            };

            // add user to team
            var user = await _userManager.GetUserAsync(User);
            var role = await _context.TeamRoles.FirstOrDefaultAsync(r => r.Name == TeamRole.Codes.Admin);
            team.AddPermission(user, role);

            _context.Add(team);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Settings", new { Team = team.Slug });
        }
    }
}
