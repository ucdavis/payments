using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Helpers;

namespace Payments.Mvc.Controllers
{
    public class SystemController : SuperController
    {
        private readonly IDbInitializationService _dbInitializationService;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;
        private UserManager<User> _userManager;
        public SystemController(IDbInitializationService dbInitializationService, ApplicationDbContext context, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _dbInitializationService = dbInitializationService;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
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


        public async Task<IActionResult> ResetDb()
        {
#if DEBUG
            await _dbInitializationService.RecreateAndInitialize();
#endif
            return RedirectToAction("LogoutDirect", "Account");
        }
    }
}