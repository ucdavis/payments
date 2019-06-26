using System;
using System.IO;

namespace Payments.Core.Models.Storage
{
    public class UploadRequest
    {
        public string Identifier { get; set; }

        public string ContainerName { get; set; }

        public string ContentType { get; set; }

        public string CacheControl { get; set; }

        public Stream Data { get; set; }
    }
}
