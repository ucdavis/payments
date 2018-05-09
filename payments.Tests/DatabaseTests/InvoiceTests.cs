﻿using System;
using System.Collections.Generic;
using System.Text;
using Payments.Core.Domain;
using Shouldly;
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
            expectedFields.Add(new NameAndType("Creator", "Payments.Core.Domain.User", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
            }));
            expectedFields.Add(new NameAndType("CustomerAddress", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("CustomerEmail", "System.String", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.EmailAddressAttribute()]",
            }));
            expectedFields.Add(new NameAndType("CustomerName", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Discount", "System.Decimal", new List<string>()));
            expectedFields.Add(new NameAndType("Id", "System.Int32", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.KeyAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Items", "System.Collections.Generic.List`1[Payments.Core.Domain.LineItem]", new List<string>()));
            expectedFields.Add(new NameAndType("Memo", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Payment", "Payments.Core.Domain.PaymentEvent", new List<string>()));
            expectedFields.Add(new NameAndType("Status", "System.String", new List<string>()));
            expectedFields.Add(new NameAndType("Subtotal", "System.Decimal", new List<string>()));
            expectedFields.Add(new NameAndType("TaxAmount", "System.Decimal", new List<string>()));
            expectedFields.Add(new NameAndType("TaxPercent", "System.Decimal", new List<string>()));
            expectedFields.Add(new NameAndType("Team", "Payments.Core.Domain.Team", new List<string>
            {
                "[System.ComponentModel.DataAnnotations.RequiredAttribute()]",
            }));
            expectedFields.Add(new NameAndType("Total", "System.Decimal", new List<string>()));

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
            invoice.Subtotal.ShouldBe(3.72m);
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
            invoice.Discount = discount;
            invoice.TaxPercent = taxPercent;

            // Act
            invoice.UpdateCalculatedValues();

            // Assert		
            invoice.TaxAmount.ShouldBe(expectedValue);
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
            invoice.Discount = discount;
            invoice.TaxPercent = taxPercent;

            // Act
            invoice.UpdateCalculatedValues();

            // Assert		
            invoice.Total.ShouldBe(expectedValue);
        }
    }
}
