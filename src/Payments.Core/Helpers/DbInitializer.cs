using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
            try
            {
                await _context.Database.EnsureDeletedAsync();

                await Initialize();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task Initialize()
        {
            _context.Database.EnsureCreated();

            // Order matters here:

            // We need roles and teams first before we can assign users to them
            await CreateRoles();

            await CreateSampleTeam();

            await CreateTeamRoles();

            // We need users here so we can make them creatores of invoices
            await CreateUsers();

            await CreateSampleInvoices();
        }

        

        private async Task CreateRoles()
        {
            await _roleManager.CreateAsync(new IdentityRole(ApplicationRoleCodes.Admin));
            await _context.SaveChangesAsync();
        }

        private async Task CreateSampleTeam()
        {
            var team1 = new Team
            {
                Name = "CRU Sample Team",
            };

            team1.Accounts.Add(new FinancialAccount
            {
                Chart = "3",
                Account = "OTHER",
                Object = "0060",
                Name = "Other Acct",
                IsDefault = true
            });

            _context.Teams.Add(team1);

            await _context.SaveChangesAsync();
        }

        private async Task CreateTeamRoles() {
            var adminRole = new TeamRole { Name = TeamRole.Codes.Admin };
            _context.TeamRoles.Add(adminRole);

            var editorRole = new TeamRole { Name = TeamRole.Codes.Editor };
            _context.TeamRoles.Add(editorRole);

            await _context.SaveChangesAsync();
        }

        private async Task CreateUsers()
        {
            var team = await _context.Teams.FirstAsync(t => t.Name == "CRU Sample Team");
            var adminRole = await _context.TeamRoles.FirstAsync(r => r.Name == TeamRole.Codes.Admin);
            var editorRole = await _context.TeamRoles.FirstAsync(r => r.Name == TeamRole.Codes.Editor);

            var jason = new User
            {
                Email = "jsylvestre@ucdavis.edu",
                UserName = "jsylvestre@ucdavis.edu",
                CampusKerberos = "jsylvest",
                FirstName = "Jason",
                LastName = "Sylvestre",
                Name = "Jason Sylvestre",
                TeamPermissions = new List<TeamPermission>() {new TeamPermission() { Team = team, Role = adminRole }}
            };
            await MakeUser(jason);

            var john = new User
            {
                Email = "jpknoll@ucdavis.edu",
                UserName = "jpknoll@ucdavis.edu",
                FirstName = "John",
                LastName = "Knoll",
                Name = "John Knoll",
                CampusKerberos = "jpknoll",
                TeamPermissions = new List<TeamPermission>() {new TeamPermission() { Team = team, Role = adminRole }}
            };
            await MakeUser(john);

            var scott = new User
            {
                Email = "srkirkland@ucdavis.edu",
                UserName = "srkirkland@ucdavis.edu",
                FirstName = "Scott",
                LastName = "Kirkland",
                Name = "Scott Kirkland",
                CampusKerberos = "postit",
                TeamPermissions = new List<TeamPermission>() {new TeamPermission() { Team = team, Role = adminRole }}
            };
            await MakeUser(scott);

            var cal = new User
            {
                Email = "cydoval@ucdavis.edu",
                UserName = "cydoval@ucdavis.edu",
                FirstName = "Calvin",
                LastName = "Doval",
                Name = "Calvin Y Doval",
                CampusKerberos = "cydoval",
                TeamPermissions = new List<TeamPermission>() {new TeamPermission() { Team = team, Role = editorRole } }
            };
            await MakeUser(cal);

            await _context.SaveChangesAsync();
        }

        private async Task CreateSampleInvoices()
        {
            var creator = await _context.Users.FirstAsync(u => u.Email == "jpknoll@ucdavis.edu");
            var team = await _context.Teams.Include(t => t.Accounts).FirstAsync(t => t.Name == "CRU Sample Team");
            var account = team.DefaultAccount;

            var invoice1 = new Invoice()
            {
                LinkId        = "TESTKEY001",
                Account       = account,
                Creator       = creator,
                Team          = team,
                CustomerEmail = "jpknoll@ucdavis.edu",
                Discount      = 2,
                TaxPercent    = new decimal(0.05),
                Status        = Invoice.StatusCodes.Sent,
                Sent          = true,
                SentAt        = DateTime.UtcNow.AddDays(-1),
                Memo          = "Sample Memo Goes Here",
                Items = new List<LineItem>()
                {
                    new LineItem()
                    {
                        Description = "Item 1",
                        Amount      = new decimal(5.50),
                        Quantity    = 1,
                        Total       = new decimal(5.50)
                    },
                    new LineItem()
                    {
                        Description = "Item 2",
                        Amount      = new decimal(3.30),
                        Quantity    = 2,
                        Total       = new decimal(6.60)
                    },
                },
                CreatedAt = DateTime.UtcNow.AddDays(-2),
            };
            invoice1.UpdateCalculatedValues();
            _context.Invoices.Add(invoice1);

            var invoice2 = new Invoice()
            {
                LinkId        = "TESTKEY002",
                Account       = account,
                Creator       = creator,
                Team          = team,
                CustomerEmail = "jpknoll@ucdavis.edu",
                Discount      = 2,
                TaxPercent    = new decimal(0.08275),
                Status        = Invoice.StatusCodes.Paid,
                Memo          = "Sample Memo Goes Here",
                Items = new List<LineItem>()
                {
                    new LineItem()
                    {
                        Description = "Item 1",
                        Amount      = new decimal(5.45),
                        Quantity    = 1,
                        Total       = new decimal(5.45)
                    },
                    new LineItem()
                    {
                        Description = "Item 2",
                        Amount      = new decimal(3.35),
                        Quantity    = 2,
                        Total       = new decimal(6.70)
                    },
                },
                Sent = true,
                SentAt = DateTime.UtcNow.AddDays(-1),
                Payment = new PaymentEvent()
                {
                    Transaction_Id = "5252879470916001003524",
                    Decision = "ACCEPT",
                    Reason_Code = 100,
                    OccuredAt = DateTime.UtcNow,
                },
                CreatedAt = DateTime.UtcNow.AddDays(-2),
            };
            invoice2.UpdateCalculatedValues();
            invoice2.Payment.Auth_Amount = invoice2.Total.ToString("F2");

            _context.Invoices.Add(invoice2);

            await _context.SaveChangesAsync();

            // add payment event
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
