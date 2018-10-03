using System;

namespace Payments.Core.Models.Configuration
{
    public class StorageSettings
    {
        public string ConnectionString { get; set; }

        public string ContainerName { get; set; }

        public string UrlBase { get; set; }
    }
}
