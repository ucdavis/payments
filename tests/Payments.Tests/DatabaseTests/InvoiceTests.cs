using payments.Tests.Helpers;
using Payments.Core.Domain;
using Shouldly;
using System.Collections.Generic;
using TestHelpers.Helpers;
using Xunit;

namespace Payments.Tests.DatabaseTests
{
    [Trait("Category", "DatabaseTests")]
    public class InvoiceTests
    {
        [Fact]
        public void TestAllFieldsInTheDatabaseHaveBeenTested()
        {
            #region Arrange
            var expectedFields = new List<NameAndType>();

            expectedFields.Add(new NameAndType("Account", "Payments.Core.Domain.FinancialAccount", new List<string>()));
            expectedFields.Add(new NameAndType("Attachments", "System.Collections.Generic.IList`1[Payments.Core.Domain.InvoiceAttachment]", new List<string>
            {
                "[Newtonsoft.Json.JsonIgnoreAttribute()]",
            }));
            expectedFields.Add(new NameAndType("CalculatedDiscount", "System.Decimal", new List<string>
            {               
                "[System.ComponentModel.DataAnnotations.DisplayFormatAttribute(DataFormatString = \"{0:C}\")]",
                "[System.ComponentModel.DataAnnotations.Schema.ColumnAttribute(TypeName = \"decimal(18,2)\")]",
                "[System.ComponentModel.DisplayNameAttribute(\"Discount\")]",
                
            }));
            expectedFields.Add(new NameAndType("CalculatedSubtotal", "System.Decimal", new List<string>{
                "[System.ComponentModel.DataAnnotations.DisplayFormatAttribute(DataFormatString = \"{0:C}\")]",
                "[System.ComponentModel.DataAnnotations.Schema.ColumnAttribute(TypeName = \"decimal(18,2)\")]",
                "[System.ComponentModel.DisplayNameAttribute(\"Subtotal\")]",
            }));
            expectedFields.Add(new NameAndType("CalculatedTaxableAmount", "System.Decimal", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayFormatAttribute(DataFormatString = \"{0:C}\")]",
                "[System.ComponentModel.DataAnnotations.Schema.ColumnAttribute(TypeName = \"decimal(18,2)\")]",
                "[System.ComponentModel.DisplayNameAttribute(\"Taxable Amount\")]",
            }));
            expectedFields.Add(new NameAndType("CalculatedTaxAmount", "System.Decimal", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayFormatAttribute(DataFormatString = \"{0:C}\")]",
                "[System.ComponentModel.DataAnnotations.Schema.ColumnAttribute(TypeName = \"decimal(18,2)\")]",
                "[System.ComponentModel.DisplayNameAttribute(\"Tax\")]",                
            }));
            expectedFields.Add(new NameAndType("CalculatedTotal", "System.Decimal", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayFormatAttribute(DataFormatString = \"{0:C}\")]",
                "[System.ComponentModel.DataAnnotations.Schema.ColumnAttribute(TypeName = \"decimal(18,2)\")]",
                "[System.ComponentModel.DisplayNameAttribute(\"Total\")]",
            }));
            expectedFields.Add(new NameAndType("Coupon", "Payments.Core.Domain.Coupon", new List<string>()));
            expectedFields.Add(new NameAndType("CreatedAt", "System.DateTime", new List<string>{
                "[System.ComponentModel.DisplayNameAttribute(\"Created On\")]",
            }));

            expectedFields.Add(new NameAndType("CustomerAddress", "System.String", new List<string>{
                "[System.ComponentModel.DisplayNameAttribute(\"Customer Address\")]",
            }));

            expectedFields.Add(new NameAndType("CustomerCompany", "System.String", new List<string>{
                "[System.ComponentModel.DisplayNameAttribute(\"Customer Company\")]",
            }));

            expectedFields.Add(new NameAndType("CustomerEmail", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.EmailAddressAttribute()]",
                "[System.ComponentModel.DisplayNameAttribute(\"Customer Email\")]",
            }));
            expectedFields.Add(new NameAndType("CustomerName", "System.String", new List<string>{
                "[System.ComponentModel.DisplayNameAttribute(\"Customer Name\")]",
            }));
            expectedFields.Add(new NameAndType("Deleted", "System.Boolean", new List<string>()));
            expectedFields.Add(new NameAndType("DeletedAt", "System.Nullable`1[System.DateTime]", new List<string>
            {
                "[System.ComponentModel.DisplayNameAttribute(\"Deleted On\")]",
            }));

            expectedFields.Add(new NameAndType("DraftCount", "System.Int32", new List<string>()));
            expectedFields.Add(new NameAndType("DueDate", "System.Nullable`1[System.DateTime]", new List<string>()));
            expectedFields.Add(new NameAndType("History", "System.Collections.Generic.IList`1[Payments.Core.Domain.History]", new List<string>
            {
                "[Newtonsoft.Json.JsonIgnoreAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.KeyAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Items", "System.Collections.Generic.IList`1[Payments.Core.Domain.LineItem]", new List<string>()));
            expectedFields.Add(new NameAndType("KfsTrackingNumber", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("LinkId", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("ManualDiscount", "System.Decimal", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayFormatAttribute(DataFormatString = \"{0:C}\")]",
                "[System.ComponentModel.DataAnnotations.Schema.ColumnAttribute(TypeName = \"decimal(18,2)\")]",
            }));
            expectedFields.Add(new NameAndType("Memo", "System.String", new List<string>{
                "[System.ComponentModel.DataAnnotations.DataTypeAttribute((System.ComponentModel.DataAnnotations.DataType)9)]",
            }));
            expectedFields.Add(new NameAndType("Paid", "System.Boolean", new List<string>()));
            expectedFields.Add(new NameAndType("PaidAt", "System.Nullable`1[System.DateTime]", new List<string>
            {
                "[System.ComponentModel.DisplayNameAttribute(\"Paid On\")]",
            }));
            expectedFields.Add(new NameAndType("PaymentEvents", "System.Collections.Generic.IList`1[Payments.Core.Domain.PaymentEvent]", new List<string>
            {
                "[Newtonsoft.Json.JsonIgnoreAttribute()]",
            }));
            expectedFields.Add(new NameAndType("PaymentProcessorId", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("PaymentType", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Refunded", "System.Boolean", new List<string>()));
            expectedFields.Add(new NameAndType("RefundedAt", "System.Nullable`1[System.DateTime]", new List<string>()));
            expectedFields.Add(new NameAndType("Sent", "System.Boolean", new List<string>()));
            expectedFields.Add(new NameAndType("SentAt", "System.Nullable`1[System.DateTime]", new List<string>{
                "[System.ComponentModel.DisplayNameAttribute(\"Sent At\")]",
            }));
            expectedFields.Add(new NameAndType("Status", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("TaxPercent", "System.Decimal", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.DisplayFormatAttribute(DataFormatString = \"{0:P}\")]",
                "[System.ComponentModel.DataAnnotations.Schema.ColumnAttribute(TypeName = \"decimal(18,5)\")]",
                "[System.ComponentModel.DisplayNameAttribute(\"Tax Percentage\")]",
            }));
            expectedFields.Add(new NameAndType("Team", "Payments.Core.Domain.Team", new List<string>
            {
                "[Newtonsoft.Json.JsonIgnoreAttribute()]",
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",                
            }));
            expectedFields.Add(new NameAndType("TeamName", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute()]",
            }));



            #endregion Arrange

            AttributeAndFieldValidation.ValidateFieldsAndAttributes(expectedFields, typeof(Invoice));
        }

        [Fact]
        public void TestInvoiceInitsItems()
        {
            // Arrange
            var invoice = new Invoice();


            // Act


            // Assert		
            invoice.Items.ShouldNotBeNull();
        }

        [Fact]
        public void TestUpdateCalculatedValuesUpdatesSubtotal()
        {
            // Arrange
            var invoice = new Invoice();
            invoice.Items.Add(new LineItem() { Total = 1.23m });
            invoice.Items.Add(new LineItem() { Total = 1.24m });
            invoice.Items.Add(new LineItem() { Total = 1.25m });


            // Act
            invoice.UpdateCalculatedValues();

            // Assert		
            invoice.CalculatedSubtotal.ShouldBe(3.72m);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 0, 0)]
        [InlineData(2, 0, 0)]
        [InlineData(0, 0.15, 0.558)]
        [InlineData(0, 0.10, 0.372)]
        [InlineData(1, 0.15, 0.408)]
        [InlineData(1, 0.10, 0.272)]
        [InlineData(1.5, 0.15, 0.333)]
        [InlineData(1.5, 0.10, 0.222)]
        public void TestUpdateCalculatedValuesUpdatesTaxAmount(decimal discount, decimal taxPercent, decimal expectedValue)
        {
            // Arrange
            var invoice = new Invoice();
            invoice.Items.Add(new LineItem() { Total = 1.23m });
            invoice.Items.Add(new LineItem() { Total = 1.24m });
            invoice.Items.Add(new LineItem() { Total = 1.25m });
            invoice.ManualDiscount = discount;
            invoice.TaxPercent = taxPercent;

            // Act
            invoice.UpdateCalculatedValues();

            // Assert		
            invoice.CalculatedTaxAmount.ShouldBe(expectedValue);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 0, 0)]
        [InlineData(2, 0, 0)]
        [InlineData(0, 0.15, 0.372)]
        [InlineData(0, 0.10, 0.248)]
        [InlineData(1, 0.15, 0.272)]
        [InlineData(1.5, 0.15, 0.222)]
        [InlineData(1.5, 0.10, 0.148)]
        public void TestUpdateCalculatedValuesWithTaxExemptUpdatesTaxAmount(decimal discount, decimal taxPercent, decimal expectedValue)
        {
            // Arrange
            var invoice = new Invoice();
            invoice.Items.Add(new LineItem() { Total = 1.23m });
            invoice.Items.Add(new LineItem() { Total = 1.24m, TaxExempt = true });
            invoice.Items.Add(new LineItem() { Total = 1.25m });
            invoice.ManualDiscount = discount;
            invoice.TaxPercent = taxPercent;

            // Act
            invoice.UpdateCalculatedValues();

            // Assert		
            invoice.CalculatedTaxAmount.ShouldBe(expectedValue);
        }

        [Theory]
        [InlineData(0, 0, 3.72)]
        [InlineData(1, 0, 2.72)]
        [InlineData(2, 0, 1.72)]
        [InlineData(0, 0.15, 4.278)]
        [InlineData(0, 0.10, 4.092)]
        [InlineData(1, 0.15, 3.128)]
        [InlineData(1, 0.10, 2.992)]
        [InlineData(1.5, 0.15, 2.553)]
        [InlineData(1.5, 0.10, 2.442)]
        public void TestUpdateCalculatedValuesUpdatesTotal(decimal discount, decimal taxPercent, decimal expectedValue)
        {
            // Arrange
            var invoice = new Invoice();
            invoice.Items.Add(new LineItem() { Total = 1.23m });
            invoice.Items.Add(new LineItem() { Total = 1.24m });
            invoice.Items.Add(new LineItem() { Total = 1.25m });
            invoice.ManualDiscount = discount;
            invoice.TaxPercent = taxPercent;

            // Act
            invoice.UpdateCalculatedValues();

            // Assert		
            invoice.CalculatedTotal.ShouldBe(expectedValue);
        }

        [Theory]
        [InlineData(0, 0, 3.72)]
        [InlineData(1, 0, 2.72)]
        [InlineData(2, 0, 1.72)]
        [InlineData(0, 0.15, 4.092)]
        [InlineData(0, 0.10, 3.968)]
        [InlineData(1, 0.15, 2.992)]
        [InlineData(1.5, 0.15, 2.442)]
        [InlineData(1.5, 0.10, 2.368)]
        public void TestUpdateCalculatedValuesWithTaxExemptUpdatesTotal(decimal discount, decimal taxPercent, decimal expectedValue)
        {
            // Arrange
            var invoice = new Invoice();
            invoice.Items.Add(new LineItem() { Total = 1.23m });
            invoice.Items.Add(new LineItem() { Total = 1.24m, TaxExempt = true });
            invoice.Items.Add(new LineItem() { Total = 1.25m });
            invoice.ManualDiscount = discount;
            invoice.TaxPercent = taxPercent;

            // Act
            invoice.UpdateCalculatedValues();

            // Assert		
            invoice.CalculatedTotal.ShouldBe(expectedValue);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void TestGetPaymentDictionaryReturnsExpectedValues(int value)
        {
            // Arrange
            var invoice = CreateValidEntities.Invoice(value, value);
            invoice.UpdateCalculatedValues();
            invoice.CustomerEmail.ShouldNotBeNull();
            invoice.CustomerName.ShouldNotBeNull();

            // Act
            var result = invoice.GetPaymentDictionary();

            // Assert		
            result.ShouldBeOfType<Dictionary<string, string>>();
            result.Count.ShouldBe(14 + (2 * invoice.Items.Count));

            result["transaction_type"].ShouldBe("sale");
            result["reference_number"].ShouldBe(invoice.Id.ToString());
            result["amount"].ShouldBe(invoice.CalculatedTotal.ToString("F2"));
            result["currency"].ShouldBe("USD");
            result["transaction_uuid"].ShouldNotBeNull();
            result["signed_date_time"].ShouldNotBeNull();
            result["unsigned_field_names"].ShouldBe("");
            result["locale"].ShouldBe("en");
            result["bill_to_email"].ShouldBe(invoice.CustomerEmail);
            result["bill_to_forename"].ShouldBe(invoice.CustomerName);
            result["bill_to_address_country"].ShouldBe("US");
            result["bill_to_address_state"].ShouldBe("CA");
            result["line_item_count"].ShouldBe(value.ToString());
            result["bill_to_company_name"].ShouldBe(null);

            for (int i = 0; i < value; i++)
            {
                result[$"item_{i}_name"].ShouldBe(invoice.Items[i].Description);
                //result[$"item_{i}_quantity"].ShouldBe(invoice.Items[i].Quantity.ToString()); //This is gone
                result[$"item_{i}_unit_price"].ShouldBe((invoice.Items[i].Amount * invoice.Items[i].Quantity).ToString("F2"));
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void TestGetPaymentDictionaryReturnsExpectedValues2(int value)
        {
            // Arrange
            var invoice = CreateValidEntities.Invoice(value, value);
            invoice.CustomerCompany = "FakeCompany";
            invoice.UpdateCalculatedValues();
            invoice.CustomerEmail.ShouldNotBeNull();
            invoice.CustomerName.ShouldNotBeNull();

            // Act
            var result = invoice.GetPaymentDictionary();

            // Assert		
            result.ShouldBeOfType<Dictionary<string, string>>();
            result.Count.ShouldBe(14 + (2 * invoice.Items.Count));

            result["transaction_type"].ShouldBe("sale");
            result["reference_number"].ShouldBe(invoice.Id.ToString());
            result["amount"].ShouldBe(invoice.CalculatedTotal.ToString("F2"));
            result["currency"].ShouldBe("USD");
            result["transaction_uuid"].ShouldNotBeNull();
            result["signed_date_time"].ShouldNotBeNull();
            result["unsigned_field_names"].ShouldBe("");
            result["locale"].ShouldBe("en");
            result["bill_to_email"].ShouldBe(invoice.CustomerEmail);
            result["bill_to_forename"].ShouldBe(invoice.CustomerName);
            result["bill_to_address_country"].ShouldBe("US");
            result["bill_to_address_state"].ShouldBe("CA");
            result["line_item_count"].ShouldBe(value.ToString());
            result["bill_to_company_name"].ShouldBe("FakeCompany");

            for (int i = 0; i < value; i++)
            {
                result[$"item_{i}_name"].ShouldBe(invoice.Items[i].Description);
                //result[$"item_{i}_quantity"].ShouldBe(invoice.Items[i].Quantity.ToString()); //This is gone
                result[$"item_{i}_unit_price"].ShouldBe((invoice.Items[i].Amount * invoice.Items[i].Quantity).ToString("F2"));
            }
        }

        [Theory]
        [InlineData("123456789 123456789 123456789 123456789X", "123456789 123456789 123456789 123456789X")]
        [InlineData("123456789 123456789 123456789 123456789X1", "123456789 123456789 123456789 123456789X")]
        [InlineData("123", "123")]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData(null, null)]
        public void TestCyberSourceTruncationOfDictionaryFieldCompanyName1(string value, string expectedValue)
        {
            // Arrange
            var invoice = CreateValidEntities.Invoice(1, 1);
            invoice.CustomerCompany = value;
            invoice.UpdateCalculatedValues();
            invoice.CustomerEmail.ShouldNotBeNull();
            invoice.CustomerName.ShouldNotBeNull();

            // Act
            var result = invoice.GetPaymentDictionary();

            // Assert		
            result.ShouldBeOfType<Dictionary<string, string>>();
            result.Count.ShouldBe(14 + (2 * invoice.Items.Count));

            result["transaction_type"].ShouldBe("sale");
            result["reference_number"].ShouldBe(invoice.Id.ToString());
            result["amount"].ShouldBe(invoice.CalculatedTotal.ToString("F2"));
            result["currency"].ShouldBe("USD");
            result["transaction_uuid"].ShouldNotBeNull();
            result["signed_date_time"].ShouldNotBeNull();
            result["unsigned_field_names"].ShouldBe("");
            result["locale"].ShouldBe("en");
            result["bill_to_email"].ShouldBe(invoice.CustomerEmail);
            result["bill_to_forename"].ShouldBe(invoice.CustomerName);
            result["bill_to_address_country"].ShouldBe("US");
            result["bill_to_address_state"].ShouldBe("CA");
            result["line_item_count"].ShouldBe("1");
            if (expectedValue == null)
            {
                result["bill_to_company_name"].ShouldBeNull();
            }
            else
            {
                result["bill_to_company_name"].Length.ShouldBeLessThanOrEqualTo(40);
                result["bill_to_company_name"].ShouldBe(expectedValue);
            }

            for (int i = 0; i < 1; i++)
            {
                result[$"item_{i}_name"].ShouldBe(invoice.Items[i].Description);
                //result[$"item_{i}_quantity"].ShouldBe(invoice.Items[i].Quantity.ToString()); //This is gone
                result[$"item_{i}_unit_price"].ShouldBe((invoice.Items[i].Amount * invoice.Items[i].Quantity).ToString("F2"));
            }
        }

        [Theory]
        [InlineData("123456789 123456789 123456789 123456789 123456789 123456789X", "123456789 123456789 123456789 123456789 123456789 123456789X")]
        [InlineData("123456789 123456789 123456789 123456789 123456789 123456789X1", "123456789 123456789 123456789 123456789 123456789 123456789X")]
        [InlineData("123", "123")]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData(null, null)]
        public void TestCyberSourceTruncationOfDictionaryFieldCustomerName(string value, string expectedValue)
        {
            // Arrange
            var invoice = CreateValidEntities.Invoice(1, 1);
            invoice.CustomerName = value;
            invoice.UpdateCalculatedValues();
            invoice.CustomerEmail.ShouldNotBeNull();
            

            // Act
            var result = invoice.GetPaymentDictionary();

            // Assert		
            result.ShouldBeOfType<Dictionary<string, string>>();
            result.Count.ShouldBe(14 + (2 * invoice.Items.Count));

            result["transaction_type"].ShouldBe("sale");
            result["reference_number"].ShouldBe(invoice.Id.ToString());
            result["amount"].ShouldBe(invoice.CalculatedTotal.ToString("F2"));
            result["currency"].ShouldBe("USD");
            result["transaction_uuid"].ShouldNotBeNull();
            result["signed_date_time"].ShouldNotBeNull();
            result["unsigned_field_names"].ShouldBe("");
            result["locale"].ShouldBe("en");
            result["bill_to_email"].ShouldBe(invoice.CustomerEmail);
            result["bill_to_address_country"].ShouldBe("US");
            result["bill_to_address_state"].ShouldBe("CA");
            result["line_item_count"].ShouldBe("1");
            if (expectedValue == null)
            {
                result["bill_to_forename"].ShouldBeNull();
            }
            else
            {
                result["bill_to_forename"].Length.ShouldBeLessThanOrEqualTo(60);
                result["bill_to_forename"].ShouldBe(expectedValue);
            }

            for (int i = 0; i < 1; i++)
            {
                result[$"item_{i}_name"].ShouldBe(invoice.Items[i].Description);
                //result[$"item_{i}_quantity"].ShouldBe(invoice.Items[i].Quantity.ToString()); //This is gone
                result[$"item_{i}_unit_price"].ShouldBe((invoice.Items[i].Amount * invoice.Items[i].Quantity).ToString("F2"));
            }
        }

        [Fact]
        public void TestStatusCodesHaveExpectedValues()
        {
            var scType = typeof(Invoice.StatusCodes);
            var props = scType.GetFields();
            props.Length.ShouldBe(9);
            
            //props[0].Name.ShouldBe("Draft");
            Invoice.StatusCodes.Draft.ShouldBe("Draft");
            Invoice.StatusCodes.Sent.ShouldBe("Sent");
            Invoice.StatusCodes.Paid.ShouldBe("Paid");
            Invoice.StatusCodes.Completed.ShouldBe("Completed");
            Invoice.StatusCodes.Cancelled.ShouldBe("Cancelled");
            Invoice.StatusCodes.Processing.ShouldBe("Processing");
            Invoice.StatusCodes.Deleted.ShouldBe("Deleted");
            Invoice.StatusCodes.Refunded.ShouldBe("Refunded");
            Invoice.StatusCodes.Refunding.ShouldBe("Refunding");
    }
    }
}
