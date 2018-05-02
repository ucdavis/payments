using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
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

        public async Task RecreateAndInitialize()
        {
            await _context.Database.EnsureDeletedAsync();

            await Initialize();
        }

        public async Task Initialize()
        {
            _context.Database.EnsureCreated();
            //TODO: Revisit if users and roles change

            // create roles
            await _roleManager.CreateAsync(new IdentityRole(ApplicationRoleCodes.Admin));



            var jason = new User
            {
                Email = "jsylvestre@ucdavis.edu",
                UserName = "jsylvestre@ucdavis.edu",  
                CampusKerberos = "jsylvest",
                FirstName = "Jason",
                LastName = "Sylvestre",
                Name = "Jason Sylvestre"
            };
            var john = new User
            {
                Email = "jpknoll@ucdavis.edu",
                UserName = "jpknoll@ucdavis.edu",
                FirstName = "John",
                LastName = "Knoll",
                Name = "John Knoll",
                CampusKerberos = "jpknoll",
            };
            var scott = new User
            {
                Email = "srkirkland@ucdavis.edu",
                UserName = "srkirkland@ucdavis.edu",
                FirstName = "Scott",
                LastName = "Kirkland",
                Name = "Scott Kirkland",
                CampusKerberos = "postit",
            };
            await MakeUser(jason);
            await MakeUser(john);
            await MakeUser(scott);


            var teamRole = new TeamRole();
            teamRole.Name = TeamRole.Codes.Admin;
            _context.TeamRoles.Add(teamRole);

            teamRole = new TeamRole();
            teamRole.Name = TeamRole.Codes.Editor;
            _context.TeamRoles.Add(teamRole);


            var team1 = new Team
            {
                Name = "Team1",
            };

            team1.Accounts.Add(new FinancialAccount
            {
                Chart = "3",
                Account = "OTHER",
                Name = "Other Acct",
                IsDefault = true
            });

            _context.Teams.Add(team1);

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
