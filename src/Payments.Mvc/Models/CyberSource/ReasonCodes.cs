namespace Payments.Mvc.Models.CyberSource
{
    public static class ReasonCodes
    {
        /*
         * Errors are programatic/configuration
         * Failures are card/account
         * */

        //Successful transaction.
        public const int Success = 100;

        //One or more fields in the request contain invalid data.
        //Possible action: see the reply fields invalid_fields for which fields are invalid. Resend the request with the correct information.
        public const int BadRequestError = 102;

        //The access_key and transaction_uuid fields for this authorization request matches the access_key and transaction_uuid of another authorization request that you sent within the past 15 minutes.
        //Possible action: resend the request with a unique access_key and transaction_uuid fields.
        public const int DuplicateRequestError = 104;

        //Only a partial amount was approved.
        public const int PartialApproveError = 110;

        //The authorization request was approved by the issuing bank but declined by CyberSource because it did not pass the Address Verification System (AVS) check.
        //Possible action: you can capture the authorization, but consider reviewing the order for the possibility of fraud.
        public const int AvsFailure = 200;

        //The issuing bank has questions about the request. You do not receive an authorization code programmatically, but you might receive one verbally by calling the processor.
        //Possible action: call your processor to possibly receive a verbal authorization. For contact phone numbers, refer to your merchant bank information.
        public const int BankFollowupRequired = 201;

        //Expired card. You might also receive this value if the expiration date you provided does not match the date the issuing bank has on file.
        //Possible action: request a different card or other form of payment.
        public const int CareExpiredFailure = 202;

        //General decline of the card. No other information was provided by the issuing bank.
        //Possible action: request a different card or other form of payment.
        public const int CardDeclinedFailure = 203;

        //Insufficient funds in the account.
        //Possible action: request a different card or other form of payment.
        public const int InsufficientFundsFailure = 204;

        //Stolen or lost card.
        //Possible action: review this transaction manually to ensure that you submitted the correct information.
        public const int CardStolenFailure = 205;

        //Issuing bank unavailable.
        //Possible action: wait a few minutes and resend the request.
        public const int BankTimeoutError = 207;

        //Inactive card or card not authorized for card-not-present transactions.
        //Possible action: request a different card or other form of payment.
        public const int CardInactiveFailure = 208;

        //The card has reached the credit limit.
        //Possible action: request a different card or other form of payment.
        public const int CardLimitFailure = 210;

        //Invalid CVN.
        //Possible action: request a different card or other form of payment.
        public const int CvsFailure = 211;

        //The customer matched an entry on the processor’s negative file.
        //Possible action: review the order and contact the payment processor.
        public const int CardBannedFailure = 221;

        //Account frozen.
        public const int AccountFrozenFailure = 222;

        //The authorization request was approved by the issuing bank but declined by CyberSource because it did not pass the CVN check.
        //Possible action: you can capture the authorization, but consider reviewing the order for the possibility of fraud.
        public const int CardSuspiciousFailure = 230;

        //Invalid account number.
        //Possible action: request a different card or other form of payment.
        public const int CardNotFouondFailure = 231;

        //The card type is not accepted by the payment processor.
        //Possible action: contact your merchant bank to confirm that your account is set up to receive the card in question.
        public const int UnacceptedCardTypeFailure = 232;

        //General decline by the processor.
        //Possible action: request a different card or other form of payment.
        public const int GeneralCardFailure = 233;

        //There is a problem with the information in your CyberSource account.
        //Possible action: do not resend the request. Contact CyberSource Customer Support to correct the information in your account.
        public const int MerchantAccountError = 234;

        //Processor failure.
        //Possible action: wait a few minutes and resend the request.
        public const int ProcessorTimeoutError = 236;

        //The card type sent is invalid or does not correlate with the credit card number.
        //Possible action: confirm that the card type correlates with the credit card number specified in the request, then resend the request.
        public const int CardTypeMismatchError = 240;

        //The cardholder is enrolled for payer authentication.
        //Possible action: authenticate cardholder before proceeding.
        public const int PayerAuthRequired = 475;

        //Payer authentication could not be authenticated.
        public const int PayerNotAuthFailure = 476;

        //The authorization request was approved by the issuing bank but declined by CyberSource based on your Decision Manager settings.
        //Possible action: review the authorization request.
        public const int PayerAuthSuspiciouosFailure = 520;
    }
}