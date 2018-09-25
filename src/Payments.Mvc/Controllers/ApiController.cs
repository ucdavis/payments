using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Mvc.Models.Roles;

namespace Payments.Mvc.Controllers
{
    [Authorize(Policy = PolicyCodes.ApiKey)]
    public abstract class ApiController : ControllerBase
    {
        protected readonly ApplicationDbContext _dbContext;

        public ApiController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [NonAction]
        public async Task<Team> GetAuthorizedTeam()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid);
            if (idClaim == null || string.IsNullOrWhiteSpace(idClaim.Value))
            {
                return null;
            }

            if (!int.TryParse(idClaim.Value, out var id))
            {
                return null;
            }

            // fetch team and users
            var team = await _dbContext.Teams
                .Include(t => t.Accounts)
                .Include(t => t.Permissions)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.Id == id);
            return team;
        }
    }
}
