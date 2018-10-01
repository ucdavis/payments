using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Payments.Mvc.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [AutoValidateAntiforgeryToken]
    [Authorize]
    public abstract class SuperController : Controller
    {
        protected SuperController()
        {
        }

        public string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


        [TempData(Key = "Message")]
        public string Message { get; set; }

        [TempData(Key = "ErrorMessage")]
        public string ErrorMessage { get;set; }

        public string TeamSlug => ControllerContext.RouteData.Values["team"] as string;
    }
}
