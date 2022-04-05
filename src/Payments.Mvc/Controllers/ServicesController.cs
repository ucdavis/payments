using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Mvc.Models.Roles;

namespace Payments.Mvc.Controllers
{
    [Authorize(Policy = PolicyCodes.ServiceKey)]
    public abstract class ServicesController : Controller
    {
    }
}
