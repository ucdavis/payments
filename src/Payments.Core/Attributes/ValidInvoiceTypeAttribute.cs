using System;
using System.ComponentModel.DataAnnotations;
using Payments.Core.Domain;

namespace Payments.Core.Attributes
{
    /// <summary>
    /// Validates that the invoice type is one of the valid invoice types defined in Invoice.InvoiceTypes
    /// </summary>
    public class ValidInvoiceTypeAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return false; // Required attribute should handle null values
            }

            var stringValue = value.ToString();

            // Check against the constants defined in Invoice.InvoiceTypes
            return stringValue == Invoice.InvoiceTypes.CreditCard ||
                   stringValue == Invoice.InvoiceTypes.Recharge;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field must be either '{Invoice.InvoiceTypes.CreditCard}' or '{Invoice.InvoiceTypes.Recharge}'.";
        }
    }
}