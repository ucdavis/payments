using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Payments.Mvc.Identity;

namespace Payments.Mvc.Controllers
{
    public class SettingsController : SuperController
    {
        public SettingsController(ApplicationUserManager userManager) : base(userManager)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
