using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.Storage;

namespace Payments.Core.Services
{
    public interface IStorageService
    {
        /// <summary>
        /// Returns a unique URL, including Shared Signature, which can be used to upload the file given by fileName
        /// Only valid for a few minutes
        /// </summary>
        Task<SasResponse> GetSharedAccessSignature(string fileName, string containerName);

        Task<string> UploadAttachment(IFormFile file);

        /// <summary>
        /// uploads a number of files to blob storage
        /// </summary>
        Task UploadFiles(params UploadRequest[] files);

        Task<CloudBlob> DownloadFile(string identifier, string containerName);
    }

    public class StorageService : IStorageService
    {
        private readonly StorageSettings _storageSettings;

        public StorageService(IOptions<StorageSettings> storageSettings)
        {
            _storageSettings = storageSettings.Value;
        }

        public async Task<SasResponse> GetSharedAccessSignature(string fileName, string containerName)
        {
            var storageConnectionString = _storageSettings.ConnectionString;

            // Create the storage account with the connection string.
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create the blob client object.
            var blobClient = storageAccount.CreateCloudBlobClient();

            var props = await blobClient.GetServicePropertiesAsync();

            if (!props.Cors.CorsRules.Any())
            {
                //create the default cors access rule
                var cors = new CorsRule();
                cors.AllowedHeaders.Add("*");
                cors.AllowedMethods = CorsHttpMethods.Get | CorsHttpMethods.Put;
                cors.AllowedOrigins.Add("*"); //TODO: only allow certain origins
                cors.ExposedHeaders.Add("x-ms-*");
                cors.MaxAgeInSeconds = 60 * 60 * 24 * 365; //one year
                props.Cors.CorsRules.Clear();
                props.Cors.CorsRules.Add(cors);
                await blobClient.SetServicePropertiesAsync(props);
            }

            // Get a reference to the container for which shared access signature will be created.
            var container = blobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlockBlobReference(fileName);
            var sas = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(20),
                Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Read
            });

            var accessUrl = $"{blob.Uri.AbsoluteUri}{sas}";
            return new SasResponse
            {
                Identifier = fileName,
                AccessUrl  = accessUrl,
                Url        = blob.Uri.AbsoluteUri,
            };
        }

        public async Task<CloudBlob> DownloadFile(string identifier, string containerName)
        {
            var container = await GetContainer(containerName);
            return container.GetBlobReference(identifier);
        }

        public async Task UploadFiles(params UploadRequest[] files)
        {
            foreach (var fileUpload in files)
            {
                var container = await GetContainer(fileUpload.ContainerName);
                var blob = container.GetBlockBlobReference(fileUpload.Identifier);
                blob.Properties.ContentType = fileUpload.ContentType;

                if (!string.IsNullOrWhiteSpace(fileUpload.CacheControl))
                {
                    blob.Properties.CacheControl = fileUpload.CacheControl;
                }

                fileUpload.Data.Seek(0, SeekOrigin.Begin); //go back to beginning of the stream to get all data

                await blob.UploadFromStreamAsync(fileUpload.Data);
                await blob.SetPropertiesAsync();
            }
        }

        private async Task<CloudBlobContainer> GetContainer(string containerName)
        {
            var storageConnectionString = _storageSettings.ConnectionString;

            // Create the storage account with the connection string.
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create the blob client object
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Get a reference to the container for which shared access signature will be created.
            var container = blobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
            return container;
        }

        public async Task<string> UploadAttachment(IFormFile file)
        {
            var name = Path.GetFileNameWithoutExtension(file.FileName) ?? "UnknownFileName";
            var extension = Path.GetExtension(file.FileName);

            //replace anything non-word chars with a dash (-)
            name = Regex.Replace(name, @"[^\w]", "-"); 

            var randomId = Guid.NewGuid().ToString();

            var nameBase = $"{randomId}-{name}{extension}";

            var fileUpload = new UploadRequest
            {
                Identifier  = nameBase,
                ContentType = file.ContentType,
                Data        = file.OpenReadStream(),
                ContainerName = StorageSettings.AttachmentContainerName,
            };

            await UploadFiles(fileUpload);

            return fileUpload.Identifier;
        }
    }
}
