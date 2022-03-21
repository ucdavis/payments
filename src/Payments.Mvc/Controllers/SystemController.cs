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

namespace Payments.Mvc.Controllers
{
    [Authorize(Roles = ApplicationRoleCodes.Admin)]
    public class SystemController : SuperController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ApplicationUserManager _userManager;
        private readonly SignInManager<User> _signInManager;

        public SystemController(ApplicationDbContext dbContext, ApplicationUserManager userManager, SignInManager<User> signInManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public Microsoft.AspNetCore.Mvc.ActionResult Index()
        {
            return View();
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
