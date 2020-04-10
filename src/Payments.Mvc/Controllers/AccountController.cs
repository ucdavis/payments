using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Payments.Core.Domain;
using Payments.Mvc.Identity;
using Payments.Mvc.Models.AccountViewModels;
using Payments.Mvc.Services;

namespace Payments.Mvc.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : SuperController
    {
        private readonly ApplicationUserManager _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IDirectorySearchService _directorySearchService;
        private readonly ILogger _logger;

        public AccountController(
                ApplicationUserManager userManager,
                SignInManager<User> signInManager,
                IDirectorySearchService directorySearchService,
                ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _directorySearchService = directorySearchService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public async Task<IActionResult> LogoutDirect()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // setup claims properly to deal with how CAS represents things
            if (IsUcdLogin(info)) // lgtm [cs/user-controlled-bypass]
            {
                // kerberos comes across in both name and nameidentifier
                var kerb = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

                var ucdPerson = await _directorySearchService.GetByKerberos(kerb);
                if (ucdPerson.IsInvalid)
                {
                    TempData["ErrorMessage"] = ucdPerson.ErrorMessage;
                    return RedirectToAction("Index", "Home");
                }

                var identity = (ClaimsIdentity)info.Principal.Identity;

                identity.AddClaim(new Claim(ClaimTypes.Email, ucdPerson.Person.Mail));
                identity.AddClaim(new Claim(ClaimTypes.GivenName, ucdPerson.Person.GivenName));
                identity.AddClaim(new Claim(ClaimTypes.Surname, ucdPerson.Person.Surname));

                // name and identifier come back as kerb, let's replace them with our found values.
                identity.RemoveClaim(identity.FindFirst(ClaimTypes.NameIdentifier));
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, ucdPerson.Person.Kerberos));
                info.ProviderKey = ucdPerson.Person.Kerberos;

                identity.RemoveClaim(identity.FindFirst(ClaimTypes.Name));
                identity.AddClaim(new Claim(ClaimTypes.Name, ucdPerson.Person.FullName));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true); // lgtm [cs/user-controlled-bypass]

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }

            // If the user does not have an account, create an account.
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;

            // create new user
            var user = new User
            {
                Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                LastName = info.Principal.FindFirstValue(ClaimTypes.Surname),
                Name = info.Principal.FindFirstValue(ClaimTypes.Name),
                CampusKerberos = IsUcdLogin(info) ? info.ProviderKey : string.Empty // lgtm [cs/user-controlled-bypass]
            };

            if (string.IsNullOrWhiteSpace(user.Name))
            {
                user.Name = user.Email;
            }

            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                createResult = await _userManager.AddLoginAsync(user, info); // lgtm [cs/user-controlled-bypass]
                if (createResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                    return RedirectToLocal(returnUrl);
                }
            }

            // TODO: add in error message explaining why login failed
            ErrorMessage = "There was a problem logging you in";

            throw new Exception(createResult.Errors.First().Description);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new EditProfileViewModel()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Name = user.Name,
                Email = user.Email,
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new EditProfileViewModel()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Name = user.Name,
                Email = user.Email,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Name = model.Name;
            user.Email = model.Email;

            await _userManager.UpdateAsync(user);

            Message = "Profile Updated";
            return RedirectToAction(nameof(Index));
        }


        #region Helpers

        private bool IsUcdLogin(ExternalLoginInfo info) {
            return info.LoginProvider.Equals("UCDavis", StringComparison.OrdinalIgnoreCase);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }      

        #endregion
    }
}
