using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Extensions;
using Payments.Mvc.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Payments.Mvc.Identity;
using Payments.Mvc.Models;
using Payments.Mvc.Models.Roles;
using Payments.Mvc.Models.Teams;
using Payments.Mvc.Models.TeamViewModels;

namespace Payments.Mvc.Controllers
{
    public class TeamsController : SuperController
    {
        private readonly ApplicationDbContext _context;
        private readonly IDirectorySearchService _directorySearchService;

        public TeamsController(ApplicationDbContext context, IDirectorySearchService directorySearchService, ApplicationUserManager userManager)
            : base(userManager)
        {
            _context = context;
            _directorySearchService = directorySearchService;
        }

        // GET: Teams
        [Authorize(Policy = "TeamEditor")]
        public async Task<IActionResult> Index()
        {
            List<Team> teams;
            if (User.IsInRole(ApplicationRoleCodes.Admin))
            {
                teams = _context.Teams.ToList();
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                teams = user.GetTeams().ToList();
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
            if (await _context.Teams.AnyAsync(a => a.Slug == model.Slug && a.IsActive))
            {
                ModelState.AddModelError("Slug", "Team Slug already exists for an active Team");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }            

            var team = new Team()
            {
                Name = model.Name,
                Slug = model.Slug,
            };
            _context.Add(team);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET: Teams/Details/5
        /// </summary>
        /// <param name="id">Team Id</param>
        /// <returns></returns>
        [Authorize(Policy = "TeamEditor")]
        public async Task<IActionResult> Details(int? id)
        {
            Team team = null;
            if (id != null && User.IsInRole(ApplicationRoleCodes.Admin))
            {
                team = await _context.Teams.Include(a => a.Accounts)
                    .SingleOrDefaultAsync(m => m.Id == id);
            }
            else
            {

                team = await _context.Teams.Include(a => a.Accounts)
                    .SingleOrDefaultAsync(m => m.Slug == TeamSlug);
            }

            if (team == null)
            {
                return NotFound();
            }

            if (!User.IsInRole(ApplicationRoleCodes.Admin) && !await _context.TeamPermissions.AnyAsync(a => a.TeamId == team.Id && a.UserId == CurrentUserId))
            {
                ErrorMessage = "You do not have access to this team.";
                return RedirectToAction("Index");
            }

            var model = new TeamDetailsModel();
            model.Team = team;
            model.Permissions = await _context.TeamPermissions.Include(a => a.Role).Include(a => a.User).Where(a => a.TeamId == team.Id).ToListAsync();


            return View(model);
        }



        /// <summary>
        /// GET: Teams/Edit/5
        /// </summary>
        /// <param name="id">Team Id</param>
        /// <returns></returns>
        [Authorize(Roles = ApplicationRoleCodes.Admin)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Id == id);
            if (team == null)
            {
                return NotFound();
            }

            var model = new EditTeamViewModel()
            {
                Name = team.Name,
                Slug = team.Slug,
                IsActive = team.IsActive,
            };

            return View(model);
        }

        /// <summary>
        /// POST: Teams/Edit/5
        /// </summary>
        /// <param name="id">Team Id</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = ApplicationRoleCodes.Admin)]
        public async Task<IActionResult> Edit(int id, EditTeamViewModel model)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            if (model.IsActive && await _context.Teams.AnyAsync(a => a.Id != team.Id && a.IsActive && a.Slug == model.Slug))
            {
                ModelState.AddModelError("Slug", "That Team Slug exists for a different active Team.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            team.Name = model.Name;
            team.Slug = model.Slug;
            team.IsActive = model.IsActive;
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET: Teams/Delete/5
        /// </summary>
        /// <param name="id">Team Id</param>
        /// <returns></returns>
        [Authorize(Roles = ApplicationRoleCodes.Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .SingleOrDefaultAsync(m => m.Id == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        /// <summary>
        /// POST: Teams/Delete/5
        /// </summary>
        /// <param name="id">Team Id</param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = ApplicationRoleCodes.Admin)]        
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Id == id);
            if (team == null)
            {
                return NotFound();
            }
            team.IsActive = false;
            //_context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Team Id</param>
        /// <returns></returns>
        [Authorize(Policy = "TeamAdmin")]
        public async Task<IActionResult> CreatePermission()
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var model = new TeamPermissionModel();
            model.Team = team;
            model.Roles = new SelectList(_context.TeamRoles, "Id", "Name");
            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Team Id</param>
        /// <param name="teamPermissionModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "TeamAdmin")]
        public async Task<IActionResult> CreatePermission(int id, TeamPermissionModel teamPermissionModel)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Id == id && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            teamPermissionModel.Team =
                await _context.Teams.SingleAsync(a => a.Id == teamPermissionModel.Team.Id && a.IsActive);


            var foundUser = await _context.Users.SingleOrDefaultAsync(a =>
                a.CampusKerberos == teamPermissionModel.UserLookup.ToLower() ||
                a.NormalizedEmail == teamPermissionModel.UserLookup.SafeToUpper());

            if (foundUser == null)
            {
                Person user = null;
                //lets do a lookup and create user!
                if (teamPermissionModel.UserLookup.Contains("@"))
                {
                    user = await _directorySearchService.GetByEmail(teamPermissionModel.UserLookup.ToLower());
                }
                else
                {
                    var directoryUser = await _directorySearchService.GetByKerberos(teamPermissionModel.UserLookup.ToLower());
                    if (directoryUser != null && !directoryUser.IsInvalid)
                    {
                        user = directoryUser.Person;
                    }
                }

                if (user != null)
                {
                    //Create the user
                    var userToCreate = new User();
                    userToCreate.Email = user.Mail;
                    userToCreate.UserName = user.Mail;
                    userToCreate.CampusKerberos = user.Kerberos;
                    userToCreate.Name = user.FullName;

                    var userPrincipal = new ClaimsPrincipal();
                    userPrincipal.AddIdentity(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userToCreate.CampusKerberos),
                        new Claim(ClaimTypes.Name, userToCreate.Name)
                    }));
                    var loginInfo = new ExternalLoginInfo(userPrincipal, "UCDavis", userToCreate.CampusKerberos, null);

                    var createResult = await _userManager.CreateAsync(userToCreate);
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddLoginAsync(userToCreate, loginInfo);
                    }

                    foundUser = userToCreate;
                }
            }
            ModelState.Clear();
            TryValidateModel(teamPermissionModel);


            if (foundUser == null)
            { 
                ModelState.AddModelError("UserLookup", "User Not Found");
            }
            else
            {
                if (await _context.TeamPermissions.AnyAsync(a =>
                    a.TeamId == teamPermissionModel.Team.Id && a.UserId == foundUser.Id &&
                    a.RoleId == teamPermissionModel.SelectedRole))
                {
                    ModelState.AddModelError("UserLookup", "User with selected role already exists.");
                }
            }


            if (ModelState.IsValid)
            {
                var teamPermission = new TeamPermission();
                teamPermission.TeamId = teamPermissionModel.Team.Id;
                teamPermission.Role = await _context.TeamRoles.SingleAsync(a => a.Id == teamPermissionModel.SelectedRole);
                teamPermission.UserId = foundUser.Id;
                _context.TeamPermissions.Add(teamPermission);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { team = teamPermissionModel.Team.Slug });
            }


            var model = new TeamPermissionModel();
            model.Team = team;
            model.UserLookup = teamPermissionModel.UserLookup;
            model.SelectedRole = teamPermissionModel.SelectedRole;
            model.Roles = new SelectList(_context.TeamRoles, "Id", "Name");
            return View(model);
        }

        // GET: TeamPermissions/Delete/5
        [Authorize(Policy = "TeamAdmin")]
        public async Task<IActionResult> DeletePermission(int? id)
        {
            //TODO: Check permissions
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var model = new TeamPermissionModel();
            model.Team = team;
            
            model.TeamPermission = await _context.TeamPermissions
                .Include(t => t.Role)
                .Include(t => t.Team)
                .Include(t => t.User)
                .SingleOrDefaultAsync(m => m.Id == id && m.TeamId == team.Id);
            if (model.TeamPermission == null)
            {
                return NotFound();
            }

            if (model.TeamPermission.UserId == CurrentUserId)
            {
                Message = "Warning! This is your own permission. If you remove it you may remove your access to the team.";
            }

            return View(model);
        }

        // POST: TeamPermissions/Delete/5
        [HttpPost, ActionName("DeletePermission")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "TeamAdmin")]
        public async Task<IActionResult> DeletePermissionConfirmed(int id, int teamId)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Id == teamId && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var teamPermission = await _context.TeamPermissions.SingleOrDefaultAsync(m => m.Id == id && m.TeamId == teamId);
            _context.TeamPermissions.Remove(teamPermission);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new {id=team.Slug});
        }
    }
}
