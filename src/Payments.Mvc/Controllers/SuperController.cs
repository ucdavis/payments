using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Payments.Mvc.Controllers
{
    [AutoValidateAntiforgeryToken]
    [Authorize]
    public class SuperController : Controller
    {
    }
}
