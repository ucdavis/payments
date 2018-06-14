using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Extensions;
using Payments.Mvc.Models.Roles;
using Payments.Mvc.Services;

namespace Payments.Mvc.Controllers
{
    public class FinancialAccountsController : SuperController
    {
        private readonly ApplicationDbContext _context;
        private readonly IFinancialService _financialService;

        public FinancialAccountsController(ApplicationDbContext context, IFinancialService financialService)
        {
            _context = context;
            _financialService = financialService;
        }

        /// <summary>
        /// GET: FinancialAccounts/Create
        /// </summary>
        /// <param name="id">Team id</param>
        /// <returns></returns>
        public async Task<IActionResult> CreateAccount(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .SingleOrDefaultAsync(m => m.Id == id && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }
            if (!User.IsInRole(ApplicationRoleCodes.Admin) && !await _context.TeamPermissions.AnyAsync(a => a.TeamId == team.Id && a.UserId == CurrentUserId))
            {
                ErrorMessage = "You do not have access to this team.";
                return RedirectToAction("Index", "Teams");
            }

            var model = new FinancialAccount();
            model.TeamId = team.Id;
            model.Team = team;
            return View(model);
        }

        /// <summary>
        /// POST: FinancialAccounts/Create
        /// </summary>
        /// <param name="financialAccount"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ConfirmAccount([Bind("Name,Description,Chart,Account,Object,SubAccount,SubObject,Project,IsDefault,TeamId")] FinancialAccount financialAccount)
        {

            var team = await _context.Teams
                .SingleOrDefaultAsync(m => m.Id == financialAccount.TeamId && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }
            if (!User.IsInRole(ApplicationRoleCodes.Admin) && !await _context.TeamPermissions.AnyAsync(a => a.TeamId == team.Id && a.UserId == CurrentUserId))
            {
                ErrorMessage = "You do not have access to this team.";
                return RedirectToAction("Index", "Teams");
            }


            string kfsResult = null;
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
            financialAccount.Object = financialAccount.Object.SafeToUpper();
            financialAccount.SubObject = financialAccount.SubObject.SafeToUpper();
            financialAccount.Project = financialAccount.Project.SafeToUpper();


            if (ModelState.IsValid)
            {
                financialAccount.Team = team;
                return View(financialAccount);
            }

            financialAccount.Team = team;
            return View("CreateAccount", financialAccount);
        }

        /// <summary>
        /// POST: FinancialAccounts/Create
        /// </summary>
        /// <param name="financialAccount"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateAccount([Bind("Name,Description,Chart,Account,Object,SubAccount,SubObject,Project,IsDefault,TeamId")] FinancialAccount financialAccount, bool confirm)
        {

            var team = await _context.Teams
                .SingleOrDefaultAsync(m => m.Id == financialAccount.TeamId && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }
            if (!User.IsInRole(ApplicationRoleCodes.Admin) && !await _context.TeamPermissions.AnyAsync(a => a.TeamId == team.Id && a.UserId == CurrentUserId))
            {
                ErrorMessage = "You do not have access to this team.";
                return RedirectToAction("Index", "Teams");
            }


            string kfsResult = null;
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
            financialAccount.Object = financialAccount.Object.SafeToUpper();
            financialAccount.SubObject = financialAccount.SubObject.SafeToUpper();
            financialAccount.Project = financialAccount.Project.SafeToUpper();

            if (!confirm)
            {
                financialAccount.Team = team;
                return View("CreateAccount", financialAccount);
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
                return RedirectToAction("Details", "Teams", new { id = financialAccount.TeamId });
            }

            financialAccount.Team = team;
            return View(financialAccount);
        }

        /// <summary>
        /// GET: FinancialAccounts/Edit/5
        /// </summary>
        /// <param name="id">FinancialAccount Id</param>
        /// <param name="teamId">Team Id</param>
        /// <returns></returns>
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

            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Id == teamId && m.IsActive); //Maybe include getting the team with the financial account?
            if (team == null)
            {
                return NotFound();
            }
            if (!User.IsInRole(ApplicationRoleCodes.Admin) && !await _context.TeamPermissions.AnyAsync(a => a.TeamId == team.Id && a.UserId == CurrentUserId))
            {
                ErrorMessage = "You do not have access to this team.";
                return RedirectToAction("Index", "Teams");
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
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Id == teamId && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }
            if (!User.IsInRole(ApplicationRoleCodes.Admin) && !await _context.TeamPermissions.AnyAsync(a => a.TeamId == team.Id && a.UserId == CurrentUserId))
            {
                ErrorMessage = "You do not have access to this team.";
                return RedirectToAction("Index", "Teams");
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
                return RedirectToAction("Details", "Teams", new { id = teamId });
            }

            return View(financialAccountToUpdate);
        }

        /// <summary>
        /// GET: FinancialAccounts/Details/5
        /// </summary>
        /// <param name="id">FinancialAccount Id</param>
        /// <param name="teamId">Team Id</param>
        /// <returns></returns>
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

            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Id == teamId && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }
            if (!User.IsInRole(ApplicationRoleCodes.Admin) && !await _context.TeamPermissions.AnyAsync(a => a.TeamId == team.Id && a.UserId == CurrentUserId))
            {
                ErrorMessage = "You do not have access to this team.";
                return RedirectToAction("Index", "Teams");
            }

            return View(financialAccount);
        }

        /// <summary>
        /// GET: FinancialAccounts/Delete/5
        /// </summary>
        /// <param name="id">FinancialAccount Id</param>
        /// <param name="teamId">Team Id</param>
        /// <returns></returns>
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

            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Id == teamId && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }
            if (!User.IsInRole(ApplicationRoleCodes.Admin) && !await _context.TeamPermissions.AnyAsync(a => a.TeamId == team.Id && a.UserId == CurrentUserId))
            {
                ErrorMessage = "You do not have access to this team.";
                return RedirectToAction("Index", "Teams");
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

            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Id == teamId && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }
            if (!User.IsInRole(ApplicationRoleCodes.Admin) && !await _context.TeamPermissions.AnyAsync(a => a.TeamId == team.Id && a.UserId == CurrentUserId))
            {
                ErrorMessage = "You do not have access to this team.";
                return RedirectToAction("Index", "Teams");
            }
            financialAccount.IsActive = false;
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Teams", new { id = teamId });
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