using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Core.Migrations
{
    public partial class Recharges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllowedInvoiceType",
                table: "Teams",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "CC");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Invoices",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "CC");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Type",
                table: "Invoices",
                column: "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_Type",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "AllowedInvoiceType",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Invoices");
        }
    }
}
