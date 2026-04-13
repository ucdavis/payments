using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Core.Migrations
{
    public partial class PaymentEventInvoiceCoveringIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_PaymentEvents_InvoiceId'
      AND object_id = OBJECT_ID(N'[dbo].[PaymentEvents]')
)
DROP INDEX [IX_PaymentEvents_InvoiceId] ON [dbo].[PaymentEvents];

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'nci_msft_1_PaymentEvents_99E8BA0523EFE2FDCB09326BF709CC4D'
      AND object_id = OBJECT_ID(N'[dbo].[PaymentEvents]')
)
CREATE NONCLUSTERED INDEX [nci_msft_1_PaymentEvents_99E8BA0523EFE2FDCB09326BF709CC4D]
ON [dbo].[PaymentEvents] ([InvoiceId])
INCLUDE ([Amount], [BillingCity], [BillingCompany], [BillingCountry], [BillingEmail], [BillingFirstName], [BillingLastName], [BillingPhone], [BillingPostalCode], [BillingState], [BillingStreet1], [BillingStreet2], [CardExpiry], [CardNumber], [CardType], [Decision], [OccuredAt], [Processor], [ProcessorId], [ReturnedResults]);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'nci_msft_1_PaymentEvents_99E8BA0523EFE2FDCB09326BF709CC4D'
      AND object_id = OBJECT_ID(N'[dbo].[PaymentEvents]')
)
DROP INDEX [nci_msft_1_PaymentEvents_99E8BA0523EFE2FDCB09326BF709CC4D] ON [dbo].[PaymentEvents];

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_PaymentEvents_InvoiceId'
      AND object_id = OBJECT_ID(N'[dbo].[PaymentEvents]')
)
CREATE INDEX [IX_PaymentEvents_InvoiceId] ON [dbo].[PaymentEvents] ([InvoiceId]);
");
        }
    }
}
