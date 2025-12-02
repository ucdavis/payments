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
        public async Task<IActionResult> GetFile([FromRoute(Name = "id")] string dbIdOrBlobIdentifier)
        {
            var team = await _dbContext.Teams.FirstOrDefaultAsync(t => t.Slug == TeamSlug);

            string identifier;
            string contentType;
            string fileName;

            // Try to parse as int (legacy ID-based approach)
            if (int.TryParse(dbIdOrBlobIdentifier, out var attachmentId))
            {
                var attachment = await _dbContext.InvoiceAttachments
                    .FirstOrDefaultAsync(a => a.Id == attachmentId && a.Invoice.Team.Id == team.Id);
                if (attachment == null)
                {
                    return NotFound();
                }

                identifier = attachment.Identifier;
                contentType = attachment.ContentType;
                fileName = attachment.FileName;
            }
            else
            {
                // Treat as direct blob identifier
                identifier = dbIdOrBlobIdentifier;
                contentType = "application/octet-stream"; // Default content type
                fileName = identifier; // Use identifier as filename
            }

            // get file
            var blob = await _storageService.DownloadFile(identifier, StorageSettings.AttachmentContainerName);
            if (blob == null || !await blob.ExistsAsync())
            {
                return NotFound();
            }

            var stream = await blob.OpenReadAsync();
            
            // ship it
            return File(stream, contentType, fileName);
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
