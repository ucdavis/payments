using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Mvc.Models.ReportViewModels;

namespace Payments.Mvc.Controllers
{
    public class ReportsController : SuperController
    {
        private readonly ApplicationDbContext _dbContext;

        public ReportsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region team reports
        /* system wide reports should use attribute routes */

        
        #endregion

        #region system reports
        /* system wide reports should use attribute routes */

        
        #endregion
    }
}
