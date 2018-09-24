using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Mvc.Identity;

namespace Payments.Mvc.Controllers
{
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
