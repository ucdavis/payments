namespace Payments.Core.Models
{
    public class Account : DomainObject
    {
        public string Chart { get; set; }
        public string AccountNumber { get; set; }
        public string ObjectCode { get; set; }
        public Team Team { get; set; }    
    }
}