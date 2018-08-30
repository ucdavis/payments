using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Payments.Mvc.Controllers
{
    public class SearchController : SuperController
    {
        public IActionResult Query(string q)
        {
            return View();
        }
    }
}
