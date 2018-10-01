using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Payments.Mvc.Authorization
{
    public class VerifyApiKeyRequirement : IAuthorizationRequirement
    {
    }
}
