using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payments.Core.Domain;

namespace Payments.Mvc.Identity
{
    public class ApplicationUserManager : UserManager<User>
    {
        public ApplicationUserManager(
                IUserStore<User> store,
                IOptions<IdentityOptions> optionsAccessor,
                IPasswordHasher<User> passwordHasher,
                IEnumerable<IUserValidator<User>> userValidators,
                IEnumerable<IPasswordValidator<User>> passwordValidators,
                ILookupNormalizer keyNormalizer,
                IdentityErrorDescriber errors,
                IServiceProvider services,
                ILogger<UserManager<User>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override Task<User> FindByIdAsync(string userId)
        {
            return Users
                .Include(u => u.TeamPermissions)
                    .ThenInclude(p => p.Team)
                .Include(u => u.TeamPermissions)
                    .ThenInclude(p => p.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
