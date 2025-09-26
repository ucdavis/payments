using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Extensions;
using Payments.Core.Services;
using Payments.Emails;
using Payments.Mvc.Identity;
using Payments.Mvc.Models;
using Payments.Mvc.Models.Roles;
using Payments.Mvc.Models.TeamViewModels;
using Payments.Mvc.Models.WebHookModels;
using Payments.Mvc.Services;
using Serilog;

namespace Payments.Mvc.Controllers
{
    public class SettingsController : SuperController
    {
        private readonly ApplicationDbContext _context;
        private readonly IDirectorySearchService _directorySearchService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly ApplicationUserManager _userManager;

        public SettingsController(
            ApplicationDbContext context,
            IDirectorySearchService directorySearchService,
            INotificationService notificationService,
            IEmailService emailService,
            ApplicationUserManager userManager)
        {
            _context = context;
            _directorySearchService = directorySearchService;
            _notificationService = notificationService;
            _emailService = emailService;
            _userManager = userManager;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = PolicyCodes.TeamEditor)]
        public async Task<IActionResult> Index()
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug);
            if (team == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userCanEdit = User.IsInRole(ApplicationRoleCodes.Admin)
                              || user.TeamPermissions.Any(a => a.TeamId == team.Id && a.Role.Name == TeamRole.Codes.Admin);

            var model = new TeamDetailsModel
            {
                Name = team.Name,
                Slug = team.Slug,
                ContactName = team.ContactName,
                ContactEmail = team.ContactEmail,
                ContactPhoneNumber = team.ContactPhoneNumber,
                ApiKey = userCanEdit ? team.ApiKey : "",
                IsActive = team.IsActive,
                UserCanEdit = userCanEdit,
                WebHookApiKey = team.WebHookApiKey,
                AllowedInvoiceType = team.AllowedInvoiceType
            };

            return View(model);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        public async Task<IActionResult> Edit()
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug);
            if (team == null)
            {
                return NotFound();
            }

            var model = new EditTeamViewModel()
            {
                Name = team.Name,
                Slug = team.Slug,
                ContactName = team.ContactName,
                ContactEmail = team.ContactEmail,
                ContactPhoneNumber = team.ContactPhoneNumber,
                IsActive = team.IsActive,
                WebHookApiKey = team.WebHookApiKey,
                AllowedInvoiceType = team.AllowedInvoiceType
            };

            return View(model);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        public async Task<IActionResult> Edit(EditTeamViewModel model)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug);
            if (team == null)
            {
                return NotFound();
            }

            if (model.IsActive && await _context.Teams.AnyAsync(a => a.Id != team.Id && a.Slug == model.Slug))
            {
                ModelState.AddModelError("Slug", "Team Slug already used.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            team.Name = model.Name;
            team.Slug = model.Slug;
            team.ContactName = model.ContactName;
            team.ContactEmail = model.ContactEmail;
            team.ContactPhoneNumber = model.ContactPhoneNumber;
            team.WebHookApiKey = model.WebHookApiKey;
            

            // only admins can change active
            if (User.IsInRole(ApplicationRoleCodes.Admin))
            {
                team.IsActive = model.IsActive;
                team.AllowedInvoiceType = model.AllowedInvoiceType;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { team = team.Slug });
        }

        [Authorize(Policy = PolicyCodes.TeamEditor)]
        public async Task<IActionResult> Roles()
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var permissions = await _context.TeamPermissions
                .Include(a => a.Role)
                .Include(a => a.User)
                .Where(a => a.TeamId == team.Id)
                .ToListAsync();

            var user = await _userManager.GetUserAsync(User);
            var userCanEdit = User.IsInRole(ApplicationRoleCodes.Admin)
                              || user.TeamPermissions.Any(a => a.TeamId == team.Id && a.Role.Name == TeamRole.Codes.Admin);

            var model = new TeamDetailsModel
            {
                Name = team.Name,
                Slug = team.Slug,
                ContactName = team.ContactName,
                ContactEmail = team.ContactEmail,
                ContactPhoneNumber = team.ContactPhoneNumber,
                IsActive = team.IsActive,
                Permissions = permissions,
                UserCanEdit = userCanEdit
            };

            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        public async Task<IActionResult> CreatePermission()
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var roles = await _context.TeamRoles.ToListAsync();

            var model = new CreateTeamPermissionViewModel
            {
                TeamName = team.Name,
                Roles = roles
                            .Select(r => new SelectListItem(r.Name.Humanize(LetterCasing.Title), r.Id.ToString()))
                            .ToList()
            };

            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        public async Task<IActionResult> CreatePermission(CreateTeamPermissionViewModel model)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            // find possible user from db
            var foundUser = await _context.Users.SingleOrDefaultAsync(a =>
                a.CampusKerberos == model.UserLookup.ToLower()
                || a.NormalizedEmail == model.UserLookup.SafeToUpper());

            // otherwise, create the user
            if (foundUser == null)
            {
                // find user
                Person user = null;
                if (model.UserLookup.Contains("@"))
                {
                    user = await _directorySearchService.GetByEmail(model.UserLookup.ToLower());
                }
                else
                {
                    var directoryUser = await _directorySearchService.GetByKerberos(model.UserLookup.ToLower());
                    if (directoryUser != null && !directoryUser.IsInvalid)
                    {
                        user = directoryUser.Person;
                    }
                }

                // create user
                if (user != null)
                {
                    var userToCreate = new User
                    {
                        Email = user.Mail,
                        UserName = user.Mail,
                        CampusKerberos = user.Kerberos,
                        Name = user.FullName
                    };

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
                        foundUser = userToCreate;
                    }
                }
            }

            ModelState.Clear();
            TryValidateModel(model);

            // find or create failed
            if (foundUser == null)
            {
                ModelState.AddModelError("UserLookup", "User Not Found");
                return View(model);
            }

            // look for existing permissions
            if (await _context.TeamPermissions.AnyAsync(a =>
                a.TeamId == team.Id
                && a.UserId == foundUser.Id
                && a.RoleId == model.SelectedRole))
            {
                ModelState.AddModelError("UserLookup", "User with selected role already exists.");
                return View(model);
            }

            // check model
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // create permission and save it
            var role = await _context.TeamRoles.SingleAsync(a => a.Id == model.SelectedRole);
            var teamPermission = new TeamPermission
            {
                TeamId = team.Id,
                Role = role,
                UserId = foundUser.Id
            };

            _context.TeamPermissions.Add(teamPermission);
            await _context.SaveChangesAsync();

            // send user notification
            try
            {
                await _emailService.SendNewTeamMemberNotice(team, foundUser, role);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "New Member Notice email failed.");
            }

            return RedirectToAction(nameof(Roles), new { team = team.Slug });
        }

        // GET: TeamPermissions/Delete/5
        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        public async Task<IActionResult> DeletePermission(int? id)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            //TODO: Check permissions
            if (id == null)
            {
                return NotFound();
            }

            var permission = await _context.TeamPermissions
                .Include(t => t.Role)
                .Include(t => t.Team)
                .Include(t => t.User)
                .SingleOrDefaultAsync(m => m.Id == id && m.TeamId == team.Id);
            if (permission == null)
            {
                return NotFound();
            }

            if (permission.UserId == CurrentUserId)
            {
                Message = "Warning! This is your own permission. If you remove it you may remove your access to the team.";
            }

            return View(permission);
        }

        // POST: TeamPermissions/Delete/5
        [HttpPost, ActionName("DeletePermission")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        public async Task<IActionResult> DeletePermissionConfirmed(int id)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var teamPermission = await _context.TeamPermissions.SingleOrDefaultAsync(m => m.Id == id && m.TeamId == team.Id);
            if (teamPermission == null)
            {
                return NotFound();
            }

            _context.TeamPermissions.Remove(teamPermission);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Roles), new { team = team.Slug });
        }

        [HttpGet]
        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        public async Task<IActionResult> WebHooks()
        {
            var team = await _context.Teams
                .Include(t => t.WebHooks)
                .SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var hooks = team.WebHooks;

            return View(hooks);
        }

        [HttpGet]
        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        public IActionResult CreateWebHook()
        {
            var model = new CreateWebHookViewModel()
            {
                IsActive = true
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        public async Task<IActionResult> CreateWebHook(CreateWebHookViewModel model)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            // check model
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // create webhook and add it
            var webHook = new WebHook()
            {
                Team = team,
                Url = model.Url,
                ContentType = model.ContentType,
                IsActive = model.IsActive,
                TriggerOnPaid = model.TriggerOnPaid,
                TriggerOnReconcile = model.TriggerOnReconcile,
            };

            team.WebHooks.Add(webHook);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(WebHooks), new { team = team.Slug });
        }

        [HttpGet]
        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        public async Task<IActionResult> EditWebHook(int id)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var webHook = await _context.WebHooks
                .SingleOrDefaultAsync(m => m.Id == id && m.TeamId == team.Id);
            if (webHook == null)
            {
                return NotFound();
            }

            var model = new EditWebHookViewModel()
            {
                Id = webHook.Id,
                Url = webHook.Url,
                IsActive = webHook.IsActive,
                TriggerOnPaid = webHook.TriggerOnPaid,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        public async Task<IActionResult> EditWebHook(int id, EditWebHookViewModel model)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var webHook = await _context.WebHooks
                .SingleOrDefaultAsync(m => m.Id == id && m.TeamId == team.Id);
            if (webHook == null)
            {
                return NotFound();
            }

            // check model
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // update model
            webHook.Url = model.Url;
            webHook.ContentType = model.ContentType;
            webHook.IsActive = model.IsActive;
            webHook.TriggerOnPaid = model.TriggerOnPaid;
            webHook.TriggerOnReconcile = model.TriggerOnReconcile;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(WebHooks), new { team = team.Slug });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = PolicyCodes.TeamAdmin)]
        public async Task<IActionResult> TestWebHook(int id)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var webHook = await _context.WebHooks.Include(a => a.Team)
                .SingleOrDefaultAsync(m => m.Id == id && m.TeamId == team.Id);
            if (webHook == null)
            {
                return NotFound();
            }

            await _notificationService.TestWebHook(webHook, webHook.Team.WebHookApiKey);

            Message = "Test Notification Sent";

            return RedirectToAction(nameof(WebHooks), new { team = team.Slug });
        }
    }
}
