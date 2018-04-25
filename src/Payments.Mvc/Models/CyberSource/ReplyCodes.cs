namespace Payments.Mvc.Models.CyberSource
{
    public class ReplyCodes
    {
        //Successful Transaction.
        //Note See reason codes 100 and 110.
        public const string Accept = "ACCEPT";

        //Transaction declined. Please review payment details.
        //Note See reason codes 200, 201, 230 and 520.
        public const string Review = "REVIEW";

        //Transaction was declined.
        //Note See reason codes 102, 200, 202, 203, 204, 205, 207, 208, 210, 211, 221, 222, 230, 231, 232, 233, 234, 236, 240, 475 and 476.
        public const string Decline = "DECLINE";

        //Access denied, page not found, or internal server error.
        //Note See reason codes 102 and 104.
        public const string Error = "ERROR";

        //The consumer did not accept the service fee conditions.
        //The consumer cancelled the transaction.
        public const string Cancel = "CANCEL";
    }
}