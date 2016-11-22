using Microsoft.AspNetCore.Mvc;
using Serilog;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Payments.Controllers
{
    public class SystemController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SendTestLog()
        {
            Log.Information("Test Log");
            return Content("success");
        }
    }
}
