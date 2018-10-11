using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Payments.Core.Domain
{
    public class InvoiceAttachment
    {
        [Key]
        public int Id { get; set; }

        public string Identifier { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public long Size { get; set; }

        [JsonIgnore]
        [Required]
        public Invoice Invoice { get; set; }

        public string GetSizeText()
        {
            if (Size <= 0)
            {
                return "0 B";
            }

            if (Size < 1024)
            {
                return $"{Size} B";
            }

            if (Size < (1024 * 1024))
            {
                return $"{Size / 1024} KB";
            }

            return $"{Size / 1024 / 1024} MB";
        }
    }
}
