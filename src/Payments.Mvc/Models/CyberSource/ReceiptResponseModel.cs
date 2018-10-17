﻿using System;
using System.Globalization;

namespace Payments.Mvc.Models.CyberSource
{
    /*
     * Posted Model from CyberSource
     * Req_ Fields are returned request values. 
     * These values were generated by the payment request and are returned for information
     * */
    public class ReceiptResponseModel
    {
        /// <summary>
        /// The Base64 signature returned by the server.
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Reference number that you use to reconcile your CyberSource reports with your processor reports.
        /// </summary>
        public string Auth_Trans_Ref_No { get; set; }

        /// <summary>
        /// Numeric value corresponding to the result of the credit card request.
        /// </summary>
        public int Reason_Code { get; set; }

        /// <summary>
        /// Amount that was authorized.
        /// </summary>
        public string Auth_Amount { get; set; }

        /// <summary>
        /// For most processors, this is the error message sent directly from the bank. Returned only if a value is returned by the processor.
        /// </summary>
        public string Auth_Response { get; set; }

        /// <summary>
        /// Reference number that you use to reconcile your CyberSource reports with your processor reports.
        /// </summary>
        public string Bill_Trans_Ref_No { get; set; }

        /// <summary>
        /// Request token data created by CyberSource for each reply. This field is an encoded string that contains no confidential information.
        /// Atos
        /// You must store the request token value so that you can retrieve and send it in follow-on requests.
        /// </summary>
        public string Request_Token { get; set; }

        /// <summary>
        /// Time of authorization in UTC.
        /// </summary>
        public string Auth_Time { get; set; }

        public DateTime AuthorizationDateTime
        {
            get
            {
                DateTime date;
                if (DateTime.TryParseExact(Auth_Time, "yyyy-MM-ddTHHmmssZ", new DateTimeFormatInfo(), DateTimeStyles.None, out date))
                {
                    return date.ToUniversalTime();
                }

                return DateTime.UtcNow;
            }
        }
        /// <summary>
        /// Total amount for the order. Must be greater than or equal to zero.
        /// </summary>
        public string Req_Amount { get; set; }

        /// <summary>
        /// Customer email address.
        /// </summary>
        public string Req_Bill_To_Email { get; set; }

        /// <summary>
        /// The transaction identifier returned from the payment gateway.
        /// </summary>
        public string Transaction_Id { get; set; }

        /// <summary>
        /// Request Currancy Code, "USD"
        /// </summary>
        public string Req_Currency { get; set; }

        /// <summary>
        /// The result of your request. Possible values:
        ///     ACCEPT
        ///     DECLINE
        ///     REVIEW
        ///     ERROR
        ///     CANCEL
        /// </summary>
        public string Decision { get; set; }

        /// <summary>
        /// Reply message from the payment gateway.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// A comma-separated list of response data that was signed by the server. All fields within this list should be used to generate a signature that can then be compared to the response signature to verify the response.
        /// </summary>
        public string Signed_Field_Names { get; set; }

        public string Auth_Avs_Code { get; set; }

        /// <summary>
        /// Authorization code. Returned only if a value is returned by the processor.
        /// </summary>
        public string Auth_Code { get; set; }

        /// <summary>
        /// Unique merchant-generated order reference or tracking number for each transaction.
        /// (Order Id)
        /// </summary>
        public int Req_Reference_Number { get; set; }

        /// <summary>
        /// The date and time of when the signature was generated by the server.
        /// Format: YYYY-MM-DDThh:mm:ssZ
        /// Example 2016-08-11T22:47:57Z equals August 11, 2016, at 22:47:57 (10:47:57 p.m.). The T separates the date and the time. The Z indicates UTC.
        /// </summary>
        public string Signed_Date_Time { get; set; }
    }
}
