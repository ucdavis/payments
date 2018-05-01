using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;

namespace Payments.Mvc.Controllers
{
    public class TeamPermissionsController : SuperController
    {
        private readonly ApplicationDbContext _context;

        public TeamPermissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TeamPermissions
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TeamPermissions.Include(t => t.Role).Include(t => t.Team).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TeamPermissions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teamPermission = await _context.TeamPermissions
                .Include(t => t.Role)
                .Include(t => t.Team)
                .Include(t => t.User)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (teamPermission == null)
            {
                return NotFound();
            }

            return View(teamPermission);
        }

        // GET: TeamPermissions/Create
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.TeamRoles, "Id", "Name");
            ViewData["TeamId"] = new SelectList(_context.Teams.Where(a => a.IsActive), "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Name");
            return View();
        }

        // POST: TeamPermissions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamPermission teamPermission)
        {
            teamPermission.Team = _context.Teams.Single(a => a.Id == teamPermission.TeamId);
            teamPermission.Role = _context.TeamRoles.Single(a => a.Id == teamPermission.RoleId);
            teamPermission.User = _context.Users.Single(a => a.Id == teamPermission.UserId);
            ModelState.Clear();
            TryValidateModel(teamPermission);


            if (ModelState.IsValid)
            {
                _context.Add(teamPermission);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.TeamRoles, "Id", "Name", teamPermission.RoleId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Name", teamPermission.TeamId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Name", teamPermission.UserId);
            return View(teamPermission);
        }

        // GET: TeamPermissions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teamPermission = await _context.TeamPermissions.SingleOrDefaultAsync(m => m.Id == id);
            if (teamPermission == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.TeamRoles, "Id", "Name", teamPermission.RoleId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Name", teamPermission.TeamId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", teamPermission.UserId);
            return View(teamPermission);
        }

        // POST: TeamPermissions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TeamId,UserId,RoleId")] TeamPermission teamPermission)
        {
            if (id != teamPermission.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teamPermission);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamPermissionExists(teamPermission.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.TeamRoles, "Id", "Name", teamPermission.RoleId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Name", teamPermission.TeamId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", teamPermission.UserId);
            return View(teamPermission);
        }

        // GET: TeamPermissions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teamPermission = await _context.TeamPermissions
                .Include(t => t.Role)
                .Include(t => t.Team)
                .Include(t => t.User)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (teamPermission == null)
            {
                return NotFound();
            }

            return View(teamPermission);
        }

        // POST: TeamPermissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teamPermission = await _context.TeamPermissions.SingleOrDefaultAsync(m => m.Id == id);
            _context.TeamPermissions.Remove(teamPermission);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamPermissionExists(int id)
        {
            return _context.TeamPermissions.Any(e => e.Id == id);
        }
    }
}
