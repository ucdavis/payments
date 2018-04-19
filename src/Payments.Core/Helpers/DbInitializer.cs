using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Payments.Core.Data;
using Payments.Core.Domain;

namespace Payments.Core.Helpers
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;

        public DbInitializer(ApplicationDbContext context)
        {
            _context = context;
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

            var jason = new User
            {
                Id = "jason1",
                Email = "jason@ucdavis.edu",
                FirstName = "Jason",
                LastName = "Sylvestre",
                Name = "Jason Sylvestre"
            };
            _context.Users.Add(jason);

            var team1 = new Team {
                Name = "Team1"
            };

            team1.Accounts.Add(new FinancialAccount {
                Chart = "3",
                Account = "OTHER",
                Name = "Other Acct"
            });

            _context.Teams.Add(team1);



            await _context.SaveChangesAsync();

        }
    }
}
