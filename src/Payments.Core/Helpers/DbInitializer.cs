using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Mvc.Models.Roles;

namespace Payments.Core.Helpers
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Recreate()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }

        public async Task Initialize()
        {
            // create identity roles
            if (!_context.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole(ApplicationRoleCodes.Admin));
            }

            // create team roles
            if (!_context.TeamRoles.Any())
            {
                var adminRole = new TeamRole { Name = TeamRole.Codes.Admin };
                _context.TeamRoles.Add(adminRole);

                var editorRole = new TeamRole { Name = TeamRole.Codes.Editor };
                _context.TeamRoles.Add(editorRole);
            }

            // create system users
            if (!_context.Users.Any())
            {
                var jason = new User
                {
                    Email          = "jsylvestre@ucdavis.edu",
                    UserName       = "jsylvestre@ucdavis.edu",
                    CampusKerberos = "jsylvest",
                    FirstName      = "Jason",
                    LastName       = "Sylvestre",
                    Name           = "Jason Sylvestre",
                };
                await MakeUser(jason);

                var john = new User
                {
                    Email          = "jpknoll@ucdavis.edu",
                    UserName       = "jpknoll@ucdavis.edu",
                    FirstName      = "John",
                    LastName       = "Knoll",
                    Name           = "John Knoll",
                    CampusKerberos = "jpknoll",
                };
                await MakeUser(john);

                var scott = new User
                {
                    Email          = "srkirkland@ucdavis.edu",
                    UserName       = "srkirkland@ucdavis.edu",
                    FirstName      = "Scott",
                    LastName       = "Kirkland",
                    Name           = "Scott Kirkland",
                    CampusKerberos = "postit",
                };
                await MakeUser(scott);

                var cal = new User
                {
                    Email          = "cydoval@ucdavis.edu",
                    UserName       = "cydoval@ucdavis.edu",
                    FirstName      = "Calvin",
                    LastName       = "Doval",
                    Name           = "Calvin Y Doval",
                    CampusKerberos = "cydoval",
                };
                await MakeUser(cal);
            }

            await _context.SaveChangesAsync();
        }

        private async Task MakeUser(User userToCreate)
        {
            var userPrincipal = new ClaimsPrincipal();
            userPrincipal.AddIdentity(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userToCreate.CampusKerberos),
                new Claim(ClaimTypes.Name, userToCreate.Name)
            }));
            var loginInfo = new ExternalLoginInfo(userPrincipal, "UCDavis", userToCreate.CampusKerberos, null);
            await _userManager.CreateAsync(userToCreate);
            await _userManager.AddLoginAsync(userToCreate, loginInfo);
            await _userManager.AddToRoleAsync(userToCreate, ApplicationRoleCodes.Admin);
        }
    }
}
