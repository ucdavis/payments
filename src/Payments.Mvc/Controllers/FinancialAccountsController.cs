using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Extensions;
using Payments.Core.Models.Configuration;
using Payments.Core.Services;
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
        private readonly IAggieEnterpriseService _aggieEnterpriseService;
        private readonly FinanceSettings _financeSettings;

        public FinancialAccountsController(ApplicationDbContext context, IFinancialService financialService, ApplicationUserManager userManager, IOptions<FinanceSettings> financeSettings, IAggieEnterpriseService aggieEnterpriseService)
        {
            _context = context;
            _financialService = financialService;
            _userManager = userManager;
            _aggieEnterpriseService = aggieEnterpriseService;
            _financeSettings = financeSettings.Value;
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

            if (_financeSettings.UseCoa)
            {
                if(team.Accounts.Any(a => a.IsActive && string.IsNullOrWhiteSpace(a.FinancialSegmentString)))
                {
                    Message = $"Warning!!!! Update all accounts to have a COA!!! {Message}";
                }
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
                UserCanEdit        = userCanEdit,
                ShowCoa            = _financeSettings.ShowCoa,
            };

            return View(model);
        }

        /// <summary>
        /// GET: FinancialAccounts/Create
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CreateAccount()
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            var model = new FinancialAccountModel
            {
                Team = team,
                UseCoa = _financeSettings.UseCoa,
                ShowCoa = _financeSettings.ShowCoa,
            };

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
            financialAccount.Team = team;
            financialAccount.UseCoa = _financeSettings.UseCoa;
            financialAccount.ShowCoa = _financeSettings.ShowCoa;

            if (!_financeSettings.UseCoa)
            {
                if (String.IsNullOrEmpty(financialAccount.Account))
                {
                    ModelState.AddModelError("Account", "KFS Account is required");
                }
                if (String.IsNullOrEmpty(financialAccount.Chart))
                {
                    ModelState.AddModelError("Chart", "KFS Chart is required");
                }
                if (!ModelState.IsValid)
                {
                    return View("CreateAccount", financialAccount);
                }
                
                financialAccount.Chart = financialAccount.Chart.SafeToUpper();
                financialAccount.Account = financialAccount.Account.SafeToUpper();
                financialAccount.SubAccount = financialAccount.SubAccount.SafeToUpper();
                financialAccount.Project = financialAccount.Project.SafeToUpper();

                if (!await _financialService.IsAccountValid(financialAccount.Chart, financialAccount.Account, financialAccount.SubAccount))
                {
                    ModelState.AddModelError("Account", "Valid Account Not Found.");
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
                    //OK, it is valid, so lookup display values
                    financialAccount.KfsAccount = accountLookup;
                }
            }
            else
            {
                financialAccount.KfsAccount = new KfsAccount();
                if (String.IsNullOrWhiteSpace(financialAccount.FinancialSegmentString))
                {
                    ModelState.AddModelError("FinancialSegmentString", "Financial Segment String is required.");
                }
            }
            if (!String.IsNullOrWhiteSpace(financialAccount.FinancialSegmentString) )
            {
                financialAccount.AeValidationModel = await _aggieEnterpriseService.IsAccountValid(financialAccount.FinancialSegmentString);
                if (!financialAccount.AeValidationModel.IsValid)
                {
                    ModelState.AddModelError("FinancialSegmentString", $"Financial Segment String is not valid. {financialAccount.AeValidationModel.Message}");
                }
            }



            if (ModelState.IsValid)
            {
                if (!_financeSettings.UseCoa)
                {
                    if (!string.IsNullOrWhiteSpace(financialAccount.Project))
                    {
                        financialAccount.KfsAccount.ProjectName = await _financialService.GetProjectName(financialAccount.Project);
                    }

                    if (!string.IsNullOrWhiteSpace(financialAccount.SubAccount))
                    {
                        financialAccount.KfsAccount.SubAccountName = await _financialService.GetSubAccountName(financialAccount.Chart, financialAccount.Account, financialAccount.SubAccount);
                    }

                }
                return View(financialAccount);
            }

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

            financialAccountModel.Team = team;
            financialAccountModel.UseCoa = _financeSettings.UseCoa;
            financialAccountModel.ShowCoa = _financeSettings.ShowCoa;

            var financialAccount = new FinancialAccount
            {
                Name = financialAccountModel.Name,
                Description = financialAccountModel.Description,
                Chart = financialAccountModel.Chart,
                Account = financialAccountModel.Account,
                SubAccount = financialAccountModel.SubAccount,
                Project = financialAccountModel.Project,
                IsDefault = financialAccountModel.IsDefault,
                FinancialSegmentString = financialAccountModel.FinancialSegmentString,
                TeamId = team.Id
            };


            financialAccount.Chart = financialAccount.Chart.SafeToUpper();
            financialAccount.Account = financialAccount.Account.SafeToUpper();
            financialAccount.SubAccount = financialAccount.SubAccount.SafeToUpper();
            financialAccount.Project = financialAccount.Project.SafeToUpper();

            if (!_financeSettings.UseCoa)
            {
                if (!await _financialService.IsAccountValid(financialAccount.Chart, financialAccount.Account, financialAccount.SubAccount))
                {
                    ModelState.AddModelError("Account", "Valid Account Not Found.");
                }

                if (!string.IsNullOrWhiteSpace(financialAccount.Project) && !await _financialService.IsProjectValid(financialAccount.Project))
                {
                    ModelState.AddModelError("Project", "Project Not Valid.");
                }

                if (ModelState.IsValid)
                {
                    var accountLookup = await _financialService.GetAccount(financialAccount.Chart, financialAccount.Account);
                    if (!accountLookup.IsValidIncomeAccount)
                    {
                        ModelState.AddModelError("Account", "Not An Income Account.");
                    }
                }
            }
            else
            {
                if (String.IsNullOrWhiteSpace(financialAccount.FinancialSegmentString))
                {
                    ModelState.AddModelError("FinancialSegmentString", "Financial Segment String is required.");
                }
            }
            if (!String.IsNullOrWhiteSpace(financialAccount.FinancialSegmentString))
            {
                //TODO: Extra payments account validation (income, etc.)
                var validationResult = await _aggieEnterpriseService.IsAccountValid(financialAccount.FinancialSegmentString);
                if (!validationResult.IsValid)
                {
                    ModelState.AddModelError("FinancialSegmentString", $"Financial Segment String is not valid. {validationResult.Message}");
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

            var model = new FinancialAccountEditModel { FinancialAccount = financialAccount, ShowCoa = _financeSettings.ShowCoa, UseCoa = _financeSettings.UseCoa };
            model.AeValidationModel = await _aggieEnterpriseService.IsAccountValid(financialAccount.FinancialSegmentString);
            model.AllowCoaEdit = string.IsNullOrWhiteSpace( financialAccount.FinancialSegmentString);

            return View(model);
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

            var model = new FinancialAccountEditModel { ShowCoa = _financeSettings.ShowCoa, UseCoa = _financeSettings.UseCoa };
            model.AllowCoaEdit = string.IsNullOrWhiteSpace(financialAccountToUpdate.FinancialSegmentString);

            ModelState.Clear();
            if (String.IsNullOrWhiteSpace(financialAccountToUpdate.FinancialSegmentString)) //If the original is empty
            {
                if (!_financeSettings.UseCoa && string.IsNullOrWhiteSpace(financialAccount.FinancialSegmentString))
                {
                    //Allow an empty string to be saved if the COA is not being used.
                }
                else
                {
                    financialAccountToUpdate.FinancialSegmentString = financialAccount.FinancialSegmentString; //Only allow it to be updated once if it is empty.
                    model.AeValidationModel = await _aggieEnterpriseService.IsAccountValid(financialAccount.FinancialSegmentString);
                    if (!model.AeValidationModel.IsValid)
                    {
                        ModelState.AddModelError("FinancialAccount.FinancialSegmentString", $"Financial Segment String is not valid. {model.AeValidationModel.Message}");
                    }
                }
            }

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
            model.FinancialAccount = financialAccountToUpdate;

            return View(model);
        }

        /// <summary>
        /// GET: FinancialAccounts/Details/5
        /// </summary>
        /// <param name="id">FinancialAccount Id</param>
        /// <returns></returns>
        public async Task<IActionResult> AccountDetails(int? id)
        {
            // check for team access
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Slug == TeamSlug && m.IsActive);
            if (team == null)
            {
                return NotFound();
            }

            if (id == null)
            {
                return NotFound();
            }

            // fetch db details
            var financialAccount = await _context.FinancialAccounts
                .Include(f => f.Team)
                .SingleOrDefaultAsync(m => m.Id == id && m.TeamId == team.Id);
            if (financialAccount == null)
            {
                return NotFound();
            }

            var model = new FinancialAccountDetailsModel
            {
                FinancialAccount = financialAccount,
                ShowCoa = _financeSettings.ShowCoa, 
            };


            if (_financeSettings.UseCoa || _financeSettings.ShowCoa)
            {
                model.KfsAccount = new KfsAccount();
                model.ShowKfsAccount = false;
                model.AeValidationModel = await _aggieEnterpriseService.IsAccountValid(financialAccount.FinancialSegmentString);
                model.IsAeAccountValid = model.AeValidationModel.IsValid;
                model.AeValidationMessage = model.AeValidationModel.Message;
            }
            if(!_financeSettings.UseCoa)
            {
                model.ShowKfsAccount = true;
                if (string.IsNullOrWhiteSpace( financialAccount.Account))
                {
                    model.IsAccountValid = false;
                    model.KfsAccount = new KfsAccount();
                }
                else
                {
                    // fetch kfs details
                    model.KfsAccount = await _financialService.GetAccount(financialAccount.Chart, financialAccount.Account);
                    if (!string.IsNullOrWhiteSpace(financialAccount.SubAccount))
                    {
                        //Populate subaccount info
                        model.KfsAccount.SubAccountName = await _financialService.GetSubAccountName(financialAccount.Chart, financialAccount.Account, financialAccount.SubAccount);
                    }

                    // check if account is valid
                    model.IsAccountValid = await _financialService.IsAccountValid(financialAccount.Chart, financialAccount.Account, financialAccount.SubAccount);

                    // check if project is valid
                    if (!string.IsNullOrWhiteSpace(financialAccount.Project))
                    {
                        model.IsProjectValid = await _financialService.IsProjectValid(financialAccount.Project);
                    }
                }
            }


            return View(model);
        }

        /// <summary>
        /// GET: FinancialAccounts/Delete/5
        /// </summary>
        /// <param name="id">FinancialAccount Id</param>
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
