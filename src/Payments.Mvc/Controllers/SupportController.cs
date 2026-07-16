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
            ViewBag.TeamSlug = TeamSlug;
            ViewBag.Version = typeof(SupportController).Assembly.GetName().Version?.ToString(3) ?? "Unavailable";
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
