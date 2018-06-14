using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Mvc.Identity;

namespace Payments.Mvc.Controllers
{
    public class HomeController : SuperController
    {
        public HomeController(ApplicationUserManager userManager) : base(userManager)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }

        [Authorize]
        public IActionResult Secure() {
            return Content("Logged in");
        }
    }
}
