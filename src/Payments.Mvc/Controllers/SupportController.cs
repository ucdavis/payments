using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Payments.Mvc.Controllers
{
    public class SupportController : SuperController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Faqs()
        {
            return View();
        }

        public IActionResult WebHooks()
        {
            return View();
        }
    }
}
