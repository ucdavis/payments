using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Models.Configuration;
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
        public async Task<IActionResult> GetFile(int id)
        {
            var team = await _dbContext.Teams.FirstOrDefaultAsync(t => t.Slug == TeamSlug);

            var attachment = await _dbContext.InvoiceAttachments
                .FirstOrDefaultAsync(a => a.Id == id && a.Invoice.Team.Id == team.Id);
            if (attachment == null)
            {
                return NotFound();
            }

            // get file
            var blob = await _storageService.DownloadFile(attachment.Identifier, StorageSettings.AttachmentContainerName);
            var stream = await blob.OpenReadAsync();
            
            // ship it
            return File(stream, attachment.ContentType, attachment.FileName);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            // upload file to azure
            var identifier = await _storageService.UploadAttachment(file);

            return new JsonResult(new
            {
                success = true,
                identifier,
            });
        }
    }
}
