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

        private const string TempDataMessageKey = "Message";
        private const string TempDataErrorMessageKey = "ErrorMessage";

        public string Message
        {
            get => TempData[TempDataMessageKey] as string;
            set => TempData[TempDataMessageKey] = value;
        }

        public string ErrorMessage
        {
            get => TempData[TempDataErrorMessageKey] as string;
            set => TempData[TempDataErrorMessageKey] = value;
        }

        public string TeamSlug => ControllerContext.RouteData.Values["team"] as string;
    }
}
