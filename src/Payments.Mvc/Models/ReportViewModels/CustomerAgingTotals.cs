namespace Payments.Mvc.Models.ReportViewModels
{
    public class CustomerAgingTotals
    {
        public string CustomerEmail { get; set; }
        public decimal OneMonth { get; set; }
        public decimal TwoMonths { get; set; }
        public decimal ThreeMonths { get; set; }
        public decimal FourMonths { get; set; }
        public decimal FourToSixMonths { get; set; }
        public decimal SixToTwelveMonths { get; set; }
        public decimal OneToTwoYears { get; set; }
        public decimal OverTwoYears { get; set; }
        public decimal Total { get; set; }
    }
}
