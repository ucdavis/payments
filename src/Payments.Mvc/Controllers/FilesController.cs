using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payments.Core.Data;
using Payments.Core.Services;
using Payments.Mvc.Models.Roles;

namespace Payments.Mvc.Controllers
{
    [Authorize(Policy = PolicyCodes.TeamEditor)]
    public class FilesController : SuperController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IStorageService _storageService;

        public FilesController(ApplicationDbContext dbContext, IStorageService storageService)
        {
            _dbContext = dbContext;
            _storageService = storageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFile(string id, string filename)
        {
            var blob = await _storageService.DownloadFile(id);

            var stream = await blob.OpenReadAsync();

            return File(stream, blob.Properties.ContentType, filename);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var identifier = await _storageService.UploadFile(file);
            return new JsonResult(new
            {
                success = true,
                identifier
            });
        }
    }
}
