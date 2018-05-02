using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Extensions;
using Payments.Mvc.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Payments.Mvc.Models;
using Payments.Mvc.Models.Roles;
using Payments.Mvc.Models.Teams;

namespace Payments.Mvc.Controllers
{
    public class TeamsController : SuperController
    {
        private readonly ApplicationDbContext _context;
        private readonly IFinancialService _financialService;
        private readonly IDirectorySearchService _directorySearchService;
        private readonly UserManager<User> _userManager;

        public TeamsController(ApplicationDbContext context, IFinancialService financialService, IDirectorySearchService directorySearchService, UserManager<User> userManager)
        {
            _context = context;
            _financialService = financialService;
            _directorySearchService = directorySearchService;
            _userManager = userManager;
        }

        // GET: Teams
        public async Task<IActionResult> Index()
        {
            var teams = _context.Teams.AsQueryable();
            if (!User.IsInRole(ApplicationRoleCodes.Admin))
            {
                var teamPermissions = await _context.TeamPermissions.Where(a => a.UserId == CurrentUserId).Select(a => a.TeamId).Distinct().ToArrayAsync();
                teams = teams.Where(a => a.IsActive && teamPermissions.Contains(a.Id));
            }

            return View(await teams.ToListAsync());
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.Include(a => a.Accounts)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (team == null)
            {
                return NotFound();
            }

            var model = new TeamDetailsModel();
            model.Team = team;
            model.Permissions = await _context.TeamPermissions.Include(a => a.Role).Include(a => a.User).Where(a => a.TeamId == team.Id).ToListAsync();


            return View(model);
        }

        // GET: Teams/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teams/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name")] Team team)
        {
            if (ModelState.IsValid)
            {
                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        // GET: Teams/Edit/5
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
            return View(team);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IsActive,Name")] Team team)
        {
            if (id != team.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.Id))
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
            return View(team);
        }

        // GET: Teams/Delete/5
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

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
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


        // GET: FinancialAccounts/Create
        public IActionResult CreateAccount(int id)
        {
            var model = new FinancialAccount();
            model.TeamId = id;
            model.Team = _context.Teams.Single(a => a.Id == id && a.IsActive);
            return View(model);
        }

        // POST: FinancialAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> CreateAccount([Bind("Name,Description,Chart,Account,SubAccount,IsDefault,TeamId")] FinancialAccount financialAccount)
        {
            string kfsResult = null;
            //TODO Kfs look/validation (Maybe on form?)
            try
            {
                kfsResult = await GetAccountInfo(financialAccount.Chart, financialAccount.Account, financialAccount.SubAccount);
            }
            catch (Exception)
            {
                //Log?
            }

            if (string.IsNullOrWhiteSpace(kfsResult))
            {
                ModelState.AddModelError("Account", "Valid Account Not Found.");
            }

            financialAccount.Chart = financialAccount.Chart.SafeToUpper();
            financialAccount.Account = financialAccount.Account.SafeToUpper();
            financialAccount.SubAccount = financialAccount.SubAccount.SafeToUpper();


            if (ModelState.IsValid)
            {
                if (financialAccount.IsDefault)
                {
                    var accountToUpdate =
                        await _context.FinancialAccounts.SingleOrDefaultAsync(a =>
                            a.TeamId == financialAccount.TeamId && a.IsDefault && a.IsActive);
                    if (accountToUpdate != null)
                    {
                        accountToUpdate.IsDefault = false;
                        _context.FinancialAccounts.Update(accountToUpdate);
                    }
                }
                else
                {
                    if (! await _context.FinancialAccounts.AnyAsync(a =>(a.TeamId == financialAccount.TeamId && a.IsDefault && a.IsActive)))
                    {
                        financialAccount.IsDefault = true;
                    }
                }
                _context.Add(financialAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new {id=financialAccount.TeamId});
            }
            financialAccount.Team = _context.Teams.Single(a => a.Id == financialAccount.TeamId);
            return View(financialAccount);
        }

        // GET: FinancialAccounts/Edit/5
        public async Task<IActionResult> EditAccount(int? id, int? teamId)
        {
            if (id == null || teamId == null)
            {
                return NotFound();
            }

            var financialAccount = await _context.FinancialAccounts.SingleOrDefaultAsync(m => m.Id == id && m.TeamId == teamId);
            if (financialAccount == null)
            {
                return NotFound();
            }
            return View(financialAccount);
        }

        // POST: FinancialAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccount(int id, int teamId, FinancialAccount financialAccount)
        {
            if (id != financialAccount.Id || teamId != financialAccount.TeamId)
            {
                return NotFound();
            }
            var financialAccountToUpdate = await _context.FinancialAccounts.SingleOrDefaultAsync(m => m.Id == id && m.TeamId == teamId);
            if (financialAccountToUpdate == null)
            {
                return NotFound();
            }

            financialAccountToUpdate.Name = financialAccount.Name;
            financialAccountToUpdate.Description = financialAccount.Description;
            financialAccountToUpdate.IsDefault = financialAccount.IsDefault;
            financialAccountToUpdate.IsActive = financialAccount.IsActive;

            ModelState.Clear();
            TryValidateModel(financialAccountToUpdate);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(financialAccountToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FinancialAccountExists(financialAccountToUpdate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new {id = teamId});
            }

            return View(financialAccountToUpdate);
        }

        // GET: FinancialAccounts/Details/5
        public async Task<IActionResult> AccountDetails(int? id, int? teamId)
        {
            if (id == null || teamId == null)
            {
                return NotFound();
            }

            var financialAccount = await _context.FinancialAccounts
                .Include(f => f.Team)
                .SingleOrDefaultAsync(m => m.Id == id && m.TeamId == teamId);
            if (financialAccount == null)
            {
                return NotFound();
            }

            return View(financialAccount);
        }

        // GET: FinancialAccounts/Delete/5
        public async Task<IActionResult> DeleteAccount(int? id, int? teamId)
        {
            if (id == null || teamId == null)
            {
                return NotFound();
            }

            var financialAccount = await _context.FinancialAccounts
                .Include(f => f.Team)
                .SingleOrDefaultAsync(m => m.Id == id && m.TeamId == teamId);
            if (financialAccount == null)
            {
                return NotFound();
            }

            return View(financialAccount);
        }

        // POST: FinancialAccounts/DeleteAccount/5
        [HttpPost, ActionName("DeleteAccount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccountConfirmed(int id, int teamId)
        {
            var financialAccount = await _context.FinancialAccounts.SingleOrDefaultAsync(m => m.Id == id && m.TeamId == teamId);
            if (financialAccount == null)
            {
                return NotFound();
            }

            financialAccount.IsActive = false;
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new {id = teamId});
        }

        [HttpGet("financial/info")]
        public async Task<string> GetAccountInfo(string chart, string account, string subAccount)
        {
            var result = await _financialService.GetAccountName(chart, account, subAccount);

            return result;
        }

        private bool FinancialAccountExists(int id)
        {
            return _context.FinancialAccounts.Any(e => e.Id == id);
        }

        public async Task<IActionResult> CreatePermission(int id)
        {
            var model = new TeamPermissionModel();
            model.Team = await _context.Teams.SingleAsync(a => a.Id == id && a.IsActive);
            model.Roles = new SelectList(_context.TeamRoles, "Id", "Name");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePermission(TeamPermissionModel teamPermissionModel)
        {
            //TODO: Permissions to do this, verify team, does the TeamPermission already exist anything else....

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


            if (ModelState.IsValid)
            {
                var teamPermission = new TeamPermission();
                teamPermission.TeamId = teamPermissionModel.Team.Id;
                teamPermission.Role = await _context.TeamRoles.SingleAsync(a => a.Id == teamPermissionModel.SelectedRole);
                teamPermission.UserId = foundUser.Id;
                _context.TeamPermissions.Add(teamPermission);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { id = teamPermissionModel.Team.Id });
            }


            var model = new TeamPermissionModel();

            return View(model);
        }

        // GET: TeamPermissions/Delete/5
        public async Task<IActionResult> DeletePermission(int? id, int? teamId)
        {
            //TODO: Check permissions
            if (id == null || teamId == null)
            {
                return NotFound();
            }

            var model = new TeamPermissionModel();
            model.Team = await _context.Teams.SingleAsync(a => a.Id == teamId && a.IsActive);
            
            model.TeamPermission = await _context.TeamPermissions
                .Include(t => t.Role)
                .Include(t => t.Team)
                .Include(t => t.User)
                .SingleOrDefaultAsync(m => m.Id == id && m.TeamId == teamId);
            if (model.TeamPermission == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: TeamPermissions/Delete/5
        [HttpPost, ActionName("DeletePermission")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermissionConfirmed(int id, int teamId)
        {
            var teamPermission = await _context.TeamPermissions.SingleOrDefaultAsync(m => m.Id == id && m.TeamId == teamId);
            _context.TeamPermissions.Remove(teamPermission);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new {id=teamId});
        }
    }
}
