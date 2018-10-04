using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Services;
using Payments.Mvc.Models.InvoiceViewModels;
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

            //// filter by team
            //var team = await _dbContext.Teams
            //    .Include(t => t.Accounts)
            //    .FirstOrDefaultAsync(t => t.Slug == TeamSlug);

            //var invoiceAttachment = await _dbContext.InvoiceAttachments
            //    .FirstOrDefaultAsync(a => a.Id == id && a.TeamId == team.Id);

            //if (invoiceAttachment == null)
            //{
            //    return NotFound(null);
            //}

            //var url = await _storageService.GetSharedAccessSignature(invoiceAttachment.Identifier);
            //return Redirect(url.AccessUrl);
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
