using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payments.Core.Data;
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFile(string id, string filename)
        {
            var blob = await _storageService.DownloadFile(id);

            var stream = await blob.OpenReadAsync();

            return File(stream, blob.Properties.ContentType, filename);
        }

        /// <summary>
        /// Upload Attachment
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Identifier used to attach to an invoice or retrieve</returns>
        [HttpPost]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UploadAttachment(IFormFile file)
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
