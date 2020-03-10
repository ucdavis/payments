using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        private readonly User _superuserSettings;

        public DbInitializer(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
            IOptions<User> superuserSettings)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _superuserSettings = superuserSettings.Value;
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
            var adminRole = await FindOrCreateRole(TeamRole.Codes.Admin);
            var financeRole = await FindOrCreateRole(TeamRole.Codes.FinanceOfficer);
            var editorRole = await FindOrCreateRole(TeamRole.Codes.Editor);
            var reportRole = await FindOrCreateRole(TeamRole.Codes.ReportUser);

            // create system user
            if (!_context.Users.Any() && !string.IsNullOrWhiteSpace(_superuserSettings.CampusKerberos))
            {
                await FindOrCreateUser(_superuserSettings);
            }

            await _context.SaveChangesAsync();
        }

        private async Task<TeamRole> FindOrCreateRole(string name)
        {
            var role = await _context.TeamRoles.FirstOrDefaultAsync(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

            if (role != null)
            {
                return role;
            }

            role = new TeamRole { Name = name };
            _context.TeamRoles.Add(role);
            await _context.SaveChangesAsync();

            return role;
        }

        private async Task FindOrCreateUser(User userToCreate)
        {
            var user = await _userManager.FindByNameAsync(userToCreate.Name);
            if (user != null)
            {
                return;
            }

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
