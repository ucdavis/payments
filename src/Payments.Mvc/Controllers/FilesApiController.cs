using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Models.Configuration;
using Payments.Core.Services;

namespace Payments.Mvc.Controllers
{
    [Route("api/files")]
    public class FilesApiController : ApiController
    {
        private readonly IStorageService _storageService;

        public FilesApiController(ApplicationDbContext dbContext, IStorageService storageService)
            : base(dbContext)
        {
            _storageService = storageService;
        }

        /// <summary>
        /// Get Attachment
        /// </summary>
        /// <param name="id">Identifier for file</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFile(int id)
        {
            var team = await GetAuthorizedTeam();

            var attachment = await _dbContext.InvoiceAttachments
                .FirstOrDefaultAsync(a => a.Id == id && a.Invoice.Team.Id == team.Id);
            if (attachment == null)
            {
                return NotFound();
            }

            // fetch file and ship
            var blob = await _storageService.DownloadFile(attachment.Identifier, StorageSettings.AttachmentContainerName);
            var stream = await blob.OpenReadAsync();

            return File(stream, attachment.ContentType, attachment.FileName);
        }

        /// <summary>
        /// Upload Attachment
        /// </summary>
        /// <param name="file">File to upload</param>
        /// <returns>Identifier used to attach to an invoice or retrieve</returns>
        [HttpPost]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UploadAttachment(IFormFile file)
        {
            // upload file to azure
            var identifier = await _storageService.UploadAttachment(file);

            return new JsonResult(new
            {
                success = true,
                identifier,
                file.FileName,
                file.ContentType,
                size = file.Length
            });
        }
    }
}
