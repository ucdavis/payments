using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Extensions;
using Payments.Mvc.Identity;
using Payments.Mvc.Models.FinancialModels;
using Payments.Mvc.Models.Roles;
using Payments.Mvc.Models.TeamViewModels;
using Payments.Mvc.Services;

namespace Payments.Mvc.Controllers
{
    [Authorize(Policy = PolicyCodes.TeamEditor)]
    public class FinancialAccountsController : SuperController
    {
        private readonly ApplicationDbContext _context;
        private readonly IFinancialService _financialService;
        private readonly ApplicationUserManager _userManager;

        public FinancialAccountsController(ApplicationDbContext context, IFinancialService financialService, ApplicationUserManager userManager)
        {
            _context = context;
            _financialService = financialService;
            _userManager = userManager;
        }

        [Authorize(Policy = "TeamEditor")]
        public async Task<IActionResult> Index()
        {
            // admins can look at inactive teams
            Team team;
            if (User.IsInRole(ApplicationRoleCodes.Admin))
            {
                team = await _context.Teams
                    .Include(a => a.Accounts)
                    .SingleOrDefaultAsync(m => m.Slug == TeamSlug);
            }
            else
            {

                team = await _context.Teams
                    .Include(a => a.Accounts)
                    .SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            }

            if (team == null)
            {
                return NotFound();
            }

            var defaultCount = team.Accounts.Count(a => a.IsActive && a.IsDefault);
            if (defaultCount == 0)
            {
                Message = "Warning! There is no active default account. Please set one account as your default";
            }
            else if (defaultCount > 1)
            {
                Message = "Warning! There are multiple active default accounts. Please set only one as your default.";
            }

            var user = await _userManager.GetUserAsync(User);
            var userCanEdit = User.IsInRole(ApplicationRoleCodes.Admin)
                              || user.TeamPermissions.Any(a => a.TeamId == team.Id && a.Role.Name == TeamRole.Codes.Admin);

            var model = new TeamDetailsModel
            {
                Name               = team.Name,
                Slug               = team.Slug,
                ContactName        = team.ContactName,
                ContactEmail       = team.ContactEmail,
                ContactPhoneNumber = team.ContactPhoneNumber,
                IsActive           = team.IsActive,
                Accounts           = team.Accounts,
                UserCanEdit        = userCanEdit
            };

            return View(model);
        }

        /// <summary>
        /// GET: FinancialAccounts/Create
        /// </summary>
        /// <param name="id">Team id</param>
        /// <returns></returns>
        public async Task<IActionResult> CreateAccount()
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var model = new FinancialAccountModel();
            model.Team = team;
            return View(model);
        }

        /// <summary>
        /// POST: FinancialAccounts/Create
        /// </summary>
        /// <param name="financialAccount"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ConfirmAccount(FinancialAccountModel financialAccount)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            financialAccount.Chart = financialAccount.Chart.SafeToUpper();
            financialAccount.Account = financialAccount.Account.SafeToUpper();
            financialAccount.SubAccount = financialAccount.SubAccount.SafeToUpper();
            financialAccount.Object = financialAccount.Object.SafeToUpper();
            financialAccount.SubObject = financialAccount.SubObject.SafeToUpper();
            financialAccount.Project = financialAccount.Project.SafeToUpper();

            if (!await _financialService.IsAccountValid(financialAccount.Chart, financialAccount.Account, financialAccount.SubAccount))
            {
                ModelState.AddModelError("Account", "Valid Account Not Found.");
            }

            if (!await _financialService.IsObjectValid(financialAccount.Chart, financialAccount.Object))
            {
                ModelState.AddModelError("Object", "Object Not Valid.");
            }

            if (!string.IsNullOrWhiteSpace(financialAccount.SubObject) && !await _financialService.IsSubObjectValid(financialAccount.Chart, financialAccount.Account, financialAccount.Object, financialAccount.SubObject))
            {
                ModelState.AddModelError("SubObject", "SubObject Not Valid.");
            }

            if (!string.IsNullOrWhiteSpace(financialAccount.Project) && ! await _financialService.IsProjectValid(financialAccount.Project))
            {
                ModelState.AddModelError("Project", "Project Not Valid.");
            }

            var accountLookup = new KfsAccount();
            if (ModelState.IsValid)
            {
                accountLookup = await _financialService.GetAccount(financialAccount.Chart, financialAccount.Account);
                if (!accountLookup.IsValidIncomeAccount)
                {
                    ModelState.AddModelError("Account", "Not An Income Account.");
                }
            }


            if (ModelState.IsValid)
            {
                //OK, it is valid, so lookup display values
                financialAccount.KfsAccount = accountLookup;
                if (!string.IsNullOrWhiteSpace(financialAccount.Project))
                {
                    financialAccount.KfsAccount.ProjectName = await _financialService.GetProjectName(financialAccount.Project);
                }

                if (!string.IsNullOrWhiteSpace(financialAccount.SubAccount))
                {
                    financialAccount.KfsAccount.SubAccountName = await _financialService.GetSubAccountName(financialAccount.Chart, financialAccount.Account, financialAccount.SubAccount);
                }
                financialAccount.KfsAccount.ObjectName = await _financialService.GetObjectName(financialAccount.Chart, financialAccount.Object);
                if (!string.IsNullOrWhiteSpace(financialAccount.SubObject))
                {
                    financialAccount.KfsAccount.SubObjectName = await _financialService.GetSubObjectName(financialAccount.Chart, financialAccount.Account, financialAccount.Object, financialAccount.SubObject);
                }
                financialAccount.Team = team;
                return View(financialAccount);
            }

            financialAccount.Team = team;
            return View("CreateAccount", financialAccount);
        }


        [HttpPost]
        public async Task<IActionResult> CreateAccount(FinancialAccountModel financialAccountModel, bool confirm)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var financialAccount = new FinancialAccount();
            financialAccount.Name = financialAccountModel.Name;
            financialAccount.Description = financialAccountModel.Description;
            financialAccount.Chart = financialAccountModel.Chart;
            financialAccount.Account = financialAccountModel.Account;
            financialAccount.Object = financialAccountModel.Object;
            financialAccount.SubAccount = financialAccountModel.SubAccount;
            financialAccount.SubObject = financialAccountModel.SubObject;
            financialAccount.Project = financialAccountModel.Project;
            financialAccount.IsDefault = financialAccountModel.IsDefault;
            financialAccount.TeamId = team.Id;



            financialAccount.Chart = financialAccount.Chart.SafeToUpper();
            financialAccount.Account = financialAccount.Account.SafeToUpper();
            financialAccount.SubAccount = financialAccount.SubAccount.SafeToUpper();
            financialAccount.Object = financialAccount.Object.SafeToUpper();
            financialAccount.SubObject = financialAccount.SubObject.SafeToUpper();
            financialAccount.Project = financialAccount.Project.SafeToUpper();

            if (!await _financialService.IsAccountValid(financialAccount.Chart, financialAccount.Account, financialAccount.SubAccount))
            {
                ModelState.AddModelError("Account", "Valid Account Not Found.");
            }

            if (!await _financialService.IsObjectValid(financialAccount.Chart, financialAccount.Object))
            {
                ModelState.AddModelError("Object", "Object Not Valid.");
            }

            if (!string.IsNullOrWhiteSpace(financialAccount.SubObject) && !await _financialService.IsSubObjectValid(financialAccount.Chart, financialAccount.Account, financialAccount.Object, financialAccount.SubObject))
            {
                ModelState.AddModelError("SubObject", "SubObject Not Valid.");
            }

            if (!string.IsNullOrWhiteSpace(financialAccount.Project) && !await _financialService.IsProjectValid(financialAccount.Project))
            {
                ModelState.AddModelError("Project", "Project Not Valid.");
            }

            var accountLookup = new KfsAccount();
            if (ModelState.IsValid)
            {
                accountLookup = await _financialService.GetAccount(financialAccount.Chart, financialAccount.Account);
                if (!accountLookup.IsValidIncomeAccount)
                {
                    ModelState.AddModelError("Account", "Not An Income Account.");
                }
            }

            if (!confirm)
            {
                financialAccountModel.Team = team;
                return View("CreateAccount", financialAccountModel);
            }


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
                    if (!await _context.FinancialAccounts.AnyAsync(a => (a.TeamId == financialAccount.TeamId && a.IsDefault && a.IsActive)))
                    {
                        financialAccount.IsDefault = true;
                    }
                }
                _context.Add(financialAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "FinancialAccounts", new { id = financialAccount.TeamId });
            }

            financialAccount.Team = team;
            return View(financialAccountModel);
        }

        /// <summary>
        /// GET: FinancialAccounts/Edit/5
        /// </summary>
        /// <param name="id">FinancialAccount Id</param>
        /// <param name="teamId">Team Id</param>
        /// <returns></returns>
        public async Task<IActionResult> EditAccount(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var financialAccount = await _context.FinancialAccounts.SingleOrDefaultAsync(m => m.Id == id && m.TeamId == team.Id);
            if (financialAccount == null)
            {
                return NotFound();
            }

            return View(financialAccount);
        }

        /// <summary>
        /// POST: FinancialAccounts/Edit/5
        /// </summary>
        /// <param name="id">FinancialAccount Id</param>
        /// <param name="teamId">Team Id</param>
        /// <param name="financialAccount"></param>
        /// <returns></returns>
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
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Id == teamId && m.Slug == TeamSlug && m.IsActive);
            if (team == null)
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
                return RedirectToAction("Index", "FinancialAccounts", new { id = teamId });
            }

            return View(financialAccountToUpdate);
        }

        /// <summary>
        /// GET: FinancialAccounts/Details/5
        /// </summary>
        /// <param name="id">FinancialAccount Id</param>
        /// <param name="teamId">Team Id</param>
        /// <returns></returns>
        public async Task<IActionResult> AccountDetails(int? id)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }
            if (id == null)
            {
                return NotFound();
            }

            var financialAccount = await _context.FinancialAccounts
                .Include(f => f.Team)
                .SingleOrDefaultAsync(m => m.Id == id && m.TeamId == team.Id);
            if (financialAccount == null)
            {
                return NotFound();
            }

            var model = new FinancialAccountDetailsModel();
            model.FinancialAccount = financialAccount;


            model.IsAccountValid = await _financialService.IsAccountValid(financialAccount.Chart, financialAccount.Account, financialAccount.SubAccount);

            model.IsObjectValid = await _financialService.IsObjectValid(financialAccount.Chart, financialAccount.Object);
            
            if (!string.IsNullOrWhiteSpace(financialAccount.SubObject))
            {
                model.IsSubObjectValid = await _financialService.IsSubObjectValid(financialAccount.Chart, financialAccount.Account, financialAccount.Object, financialAccount.SubObject);
            }

            if (!string.IsNullOrWhiteSpace(financialAccount.Project))
            {
                model.IsProjectValid = await _financialService.IsProjectValid(financialAccount.Project);
            }
            
            if (model.IsAccountValid && model.IsObjectValid)
            {
                model.KfsAccount = await _financialService.GetAccount(financialAccount.Chart, financialAccount.Account);
                if (model.IsProjectValid.HasValue && model.IsProjectValid.Value)
                {
                    model.KfsAccount.ProjectName = await _financialService.GetProjectName(financialAccount.Project);
                }

                if (!string.IsNullOrWhiteSpace(financialAccount.SubAccount))
                {
                    model.KfsAccount.SubAccountName = await _financialService.GetSubAccountName(financialAccount.Chart, financialAccount.Account, financialAccount.SubAccount);
                }
                model.KfsAccount.ObjectName = await _financialService.GetObjectName(financialAccount.Chart, financialAccount.Object);

                if (model.IsSubObjectValid.HasValue && model.IsSubObjectValid.Value)
                {
                    model.KfsAccount.SubObjectName = await _financialService.GetSubObjectName(financialAccount.Chart, financialAccount.Account, financialAccount.Object, financialAccount.SubObject);
                }
            }

            return View(model);
        }

        /// <summary>
        /// GET: FinancialAccounts/Delete/5
        /// </summary>
        /// <param name="id">FinancialAccount Id</param>
        /// <param name="teamId">Team Id</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteAccount(int? id)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }
            if (id == null)
            {
                return NotFound();
            }

            var financialAccount = await _context.FinancialAccounts
                .Include(f => f.Team)
                .SingleOrDefaultAsync(m => m.Id == id && m.TeamId == team.Id);
            if (financialAccount == null)
            {
                return NotFound();
            }


            return View(financialAccount);
        }

        /// <summary>
        /// POST: FinancialAccounts/DeleteAccount/5
        /// </summary>
        /// <param name="id">FinancialAccount Id</param>
        /// <param name="teamId">Team Id</param>
        /// <returns></returns>
        [HttpPost, ActionName("DeleteAccount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccountConfirmed(int id, int teamId)
        {
            var financialAccount = await _context.FinancialAccounts.SingleOrDefaultAsync(m => m.Id == id && m.TeamId == teamId);
            if (financialAccount == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Id == teamId && m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            financialAccount.IsActive = false;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "FinancialAccounts", new { id = teamId });
        }

        private bool FinancialAccountExists(int id)
        {
            return _context.FinancialAccounts.Any(e => e.Id == id);
        }

        [HttpGet("financial/info")]
        public async Task<string> GetAccountInfo(string chart, string account, string subAccount)
        {
            var result = await _financialService.GetAccountName(chart, account, subAccount);

            return result;
        }
    }
}