using System;
using Microsoft.AspNetCore.Mvc;

namespace Payments.Mvc.Controllers
{
    public class ReportsController : SuperController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
