﻿using System;
using Microsoft.AspNetCore.Authorization;

namespace Payments.Mvc.Authorization
{
    public class VerifyApiKeyRequirement : IAuthorizationRequirement
    {
    }
}
