using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payments.Core.Data;
using Payments.Mvc.Models.TeamApiViewModels;

namespace Payments.Mvc.Controllers
{
    [Route("api/team")]
    public class TeamsApiController : ApiController
    {
        public TeamsApiController(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Fetch the team name and slug associated with the API key.
        /// </summary>
        /// <remarks>
        /// Supply the team's API key in the Authorization header.
        /// </remarks>
        /// <returns>The authorized team's name and slug.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(TeamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TeamResult>> Get()
        {
            var team = await GetAuthorizedTeam();
            if (team == null)
            {
                return NotFound();
            }

            return new TeamResult
            {
                Name = team.Name,
                Slug = team.Slug,
            };
        }
    }
}
