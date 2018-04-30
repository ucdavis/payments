﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Payments.Core.Data;
using Payments.Core.Domain;

namespace Payments.Core.Helpers
{
    public interface IDbInitializationService
    {
        Task Initialize();
        Task RecreateAndInitialize();
    }

    public class DbInitializer : IDbInitializationService
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

            //var jason = new User
            //{
            //    Id = "jason1",
            //    Email = "jason@ucdavis.edu",
            //    FirstName = "Jason",
            //    LastName = "Sylvestre",
            //    Name = "Jason Sylvestre"
            //};
            //_context.Users.Add(jason);

            var john = new User
            {
                Email = "jpknoll@ucdavis.edu",
                UserName = "jpknoll@ucdavis.edu",
                FirstName = "John",
                LastName = "Knoll",
                Name = "John Knoll",
                CampusKerberos = "jpknoll",
            };
            _context.Users.Add(john);

            var team1 = new Team {
                Name = "Team1",
            };

            team1.Accounts.Add(new FinancialAccount {
                Chart = "3",
                Account = "OTHER",
                Name = "Other Acct",
                IsDefault = true
            });

            _context.Teams.Add(team1);

            await _context.SaveChangesAsync();
        }
    }
}
