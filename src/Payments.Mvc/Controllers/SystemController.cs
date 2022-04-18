using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Payments.Core.Domain;
using Payments.Mvc.Models.Roles;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Mvc.Identity;
using Payments.Mvc.Models.SystemViewModels;
using Payments.Core.Extensions;
using Payments.Mvc.Models;
using Payments.Mvc.Services;
using System.Security.Claims;

namespace Payments.Mvc.Controllers
{
    [Authorize(Roles = ApplicationRoleCodes.Admin)]
    public class SystemController : SuperController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ApplicationUserManager _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IDirectorySearchService _directorySearchService;

        public SystemController(ApplicationDbContext dbContext, ApplicationUserManager userManager, SignInManager<User> signInManager, IDirectorySearchService directorySearchService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _directorySearchService = directorySearchService;
        }

        public async Task<IActionResult> Index()
        {
            var admins = await _userManager.GetUsersInRoleAsync(ApplicationRoleCodes.Admin);
            return View(admins);
        }

        [HttpGet]
        public IActionResult AddAdmin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin(CreateSystemAdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // find possible user from db
            var foundUser = await _dbContext.Users.SingleOrDefaultAsync(a =>
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

            var roles = await _userManager.GetRolesAsync(foundUser);
            if (roles != null)
            {
                if (roles.Any(a => a.Contains(ApplicationRoleCodes.Admin)))
                {
                    ModelState.AddModelError("UserLookup", "Already in role");
                    return View(model);
                }
            }

            // check model
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _userManager.AddToRoleAsync(foundUser, ApplicationRoleCodes.Admin);

            await _dbContext.SaveChangesAsync();
          

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> RemoveAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, ApplicationRoleCodes.Admin);
            }
            await _dbContext.SaveChangesAsync();
            Message = $"{user.CampusKerberos} removed from Admin role.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Emulate(string id)
        {
            var user = await _userManager.FindByNameAsync(id);

            if (user == null) return NotFound();

            await _signInManager.SignOutAsync(); // sign out current user

            await _signInManager.SignInAsync(user, false); // sign in new user

            Message = $"Signed in as {id}";

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> UpdateCalculatedValues()
        {
            var invoices = await _dbContext.Invoices
                .Include(i => i.Coupon)
                .Include(i => i.Items)
                .ToListAsync();

            foreach (var invoice in invoices)
            {
                invoice.UpdateCalculatedValues();
            }

            await _dbContext.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
            });
        }
    }
}
